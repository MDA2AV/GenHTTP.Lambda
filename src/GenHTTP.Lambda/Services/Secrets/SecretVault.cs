using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Secrets;

/// <inheritdoc cref="ISecretVault" />
/// <remarks>
/// A lambda asks for a secret while it answers a request, from code that is
/// not asynchronous, and may ask on every request. So what it needs - whether
/// secrets are on, its key, and its secrets still sealed - is read once into a
/// snapshot per lambda or feature and kept until something changes it; opening
/// a value is left to each call, so no value lives in memory longer than it is
/// used. Everything here that changes a lambda's secrets or whether it has them
/// drops its snapshots afterwards, and a snapshot read while that happened is
/// not kept.
/// </remarks>
public sealed partial class SecretVault(IDbContextFactory<LambdaDbContext> databases, SecretCipher cipher, LambdaOptions options,
                                        ILogger<SecretVault> logger) : ISecretVault
{

    /// <summary>
    /// How long the name of a secret may be.
    /// </summary>
    public const int MaxName = 64;

    private readonly ConcurrentDictionary<(long Lambda, long? Feature), Snapshot> _snapshots = [];

    /// <summary>
    /// Counts what invalidated the snapshots, so one that was being read while
    /// that happened is recognised as stale.
    /// </summary>
    private long _generation;

    /// <summary>
    /// Takes turns over what changes one lambda's secrets, one lambda at a time.
    /// </summary>
    /// <remarks>
    /// A limit checked and a row added are two steps, and two sets of the same
    /// name would otherwise both find room, or both find none.
    /// </remarks>
    private readonly SemaphoreSlim[] _stripes = [.. Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1))];

    #region Functionality

    public async ValueTask<IReadOnlyList<SecretInfo>> ListAsync(long lambdaId, long? featureId = null, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.Secrets.AsNoTracking()
                             .Where(s => s.LambdaId == lambdaId && s.FeatureId == featureId)
                             .OrderBy(s => s.Name)
                             .Select(s => new SecretInfo(s.Name, s.Created, s.Updated))
                             .ToListAsync(cancellation);
    }

    public async ValueTask<int> CountAsync(long lambdaId, long? featureId = null, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.Secrets.CountAsync(s => s.LambdaId == lambdaId && s.FeatureId == featureId, cancellation);
    }

    public async ValueTask<SecretInfo> SetAsync(long lambdaId, long? featureId, string name, string value, CancellationToken cancellation = default)
    {
        name = ValidName(name);

        value = ValidValue(value);

        using var turn = await TakeTurnAsync(lambdaId, cancellation);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.FirstOrDefaultAsync(l => l.Id == lambdaId, cancellation)
                  ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        var row = await database.Secrets.FirstOrDefaultAsync(s => s.LambdaId == lambdaId && s.FeatureId == featureId && s.Name == name, cancellation);

        if (row == null && await database.Secrets.CountAsync(s => s.LambdaId == lambdaId && s.FeatureId == featureId, cancellation) >= options.MaxSecrets)
        {
            throw LambdaException.Conflict($"A lambda may keep {options.MaxSecrets} secrets. Delete one that is no longer used before adding another.");
        }

        var now = DateTime.UtcNow;

        // the lambda's half of the key, made the first time it has a secret
        if (lambda.SecretSalt == null)
        {
            lambda.SecretSalt = SecretCipher.NewSalt();
        }

        var sealedValue = SecretCipher.Seal(cipher.KeyFor(lambda.SecretSalt), name, value);

        if (row == null)
        {
            row = new SecretEntity { LambdaId = lambdaId, FeatureId = featureId, Name = name, Value = sealedValue, Created = now, Updated = now };

            database.Secrets.Add(row);
        }
        else
        {
            row.Value = sealedValue;
            row.Updated = now;
        }

        lambda.Modified = now;

        await database.SaveChangesAsync(cancellation);

        Forget(lambdaId);

        return new SecretInfo(row.Name, row.Created, row.Updated);
    }

    public async ValueTask<bool> DeleteAsync(long lambdaId, long? featureId, string name, CancellationToken cancellation = default)
    {
        using var turn = await TakeTurnAsync(lambdaId, cancellation);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var removed = await database.Secrets.Where(s => s.LambdaId == lambdaId && s.FeatureId == featureId && s.Name == name.Trim())
                                    .ExecuteDeleteAsync(cancellation);

        Forget(lambdaId);

        return removed > 0;
    }

    public async ValueTask CopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default)
    {
        using var turn = await TakeTurnAsync(lambdaId, cancellation);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var mine = await database.Secrets.AsNoTracking()
                                 .Where(s => s.LambdaId == lambdaId && s.FeatureId == null)
                                 .ToListAsync(cancellation);

        var had = await database.Secrets.Where(s => s.LambdaId == lambdaId && s.FeatureId == featureId).ToListAsync(cancellation);

        database.Secrets.RemoveRange(had);

        // the sealed values are copied as they are: the copy belongs to the
        // same lambda, so it opens with the same key, under the same names
        foreach (var secret in mine)
        {
            database.Secrets.Add(new SecretEntity
            {
                LambdaId = lambdaId,
                FeatureId = featureId,
                Name = secret.Name,
                Value = secret.Value,
                Created = secret.Created,
                Updated = secret.Updated
            });
        }

        await database.SaveChangesAsync(cancellation);

        Forget(lambdaId);
    }

    public async ValueTask ClearAsync(long lambdaId, CancellationToken cancellation = default)
    {
        using var turn = await TakeTurnAsync(lambdaId, cancellation);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        await database.Secrets.Where(s => s.LambdaId == lambdaId).ExecuteDeleteAsync(cancellation);

        Forget(lambdaId);
    }

    public void Forget(long lambdaId)
    {
        Interlocked.Increment(ref _generation);

        foreach (var slot in _snapshots.Keys.Where(k => k.Lambda == lambdaId).ToList())
        {
            _snapshots.TryRemove(slot, out _);
        }
    }

    public SecretAccess AccessFor(long lambdaId, long? featureId = null)
        => new(() => SnapshotOf(lambdaId, featureId).Enabled, name => Read(lambdaId, featureId, name));

    #endregion

    #region Reading

    /// <summary>
    /// Opens one secret for the code that asked for it.
    /// </summary>
    private string? Read(long lambdaId, long? featureId, string name)
    {
        var snapshot = SnapshotOf(lambdaId, featureId);

        if (!snapshot.Enabled)
        {
            throw new InvalidOperationException(DataKinds.SecretsOff);
        }

        var wanted = name?.Trim() ?? string.Empty;

        if (!snapshot.Values.TryGetValue(wanted, out var sealedValue) || snapshot.Key == null)
        {
            return null;
        }

        var opened = SecretCipher.Open(snapshot.Key, wanted, sealedValue);

        if (opened == null)
        {
            logger.LogError("The secret {Name} of lambda {LambdaId} cannot be opened: it was stored with another key than this installation has", wanted, lambdaId);

            throw new InvalidOperationException($"The secret '{wanted}' cannot be opened. It was stored under another LAMBDA_SECRETS_KEY than this installation has - "
                                              + "its owner has to set it again.");
        }

        return opened;
    }

    /// <summary>
    /// What is known of a lambda's secrets, read now if it is not known yet.
    /// </summary>
    /// <remarks>
    /// Synchronous on purpose: the code that asks is, and it asks from inside a
    /// request that is already being answered, so there is nothing to wait for
    /// that a blocked thread would hold up.
    /// </remarks>
    private Snapshot SnapshotOf(long lambdaId, long? featureId)
    {
        if (_snapshots.TryGetValue((lambdaId, featureId), out var known))
        {
            return known;
        }

        var generation = Interlocked.Read(ref _generation);

        using var database = databases.CreateDbContext();

        var enabled = database.DataStores.AsNoTracking()
                              .Where(s => s.LambdaId == lambdaId && s.Kind == DataKinds.SecretsId)
                              .Select(s => (bool?)s.Enabled)
                              .FirstOrDefault() ?? DataKinds.Secrets.Default;

        Snapshot read;

        if (!enabled)
        {
            read = new Snapshot(false, null, []);
        }
        else
        {
            var salt = database.Lambdas.AsNoTracking().Where(l => l.Id == lambdaId).Select(l => l.SecretSalt).FirstOrDefault();

            var values = database.Secrets.AsNoTracking()
                                 .Where(s => s.LambdaId == lambdaId && s.FeatureId == featureId)
                                 .ToDictionary(s => s.Name, s => s.Value);

            read = new Snapshot(true, salt != null ? cipher.KeyFor(salt) : null, values);
        }

        // read while something changed: right for this call, but not to be kept
        if (generation == Interlocked.Read(ref _generation))
        {
            _snapshots[(lambdaId, featureId)] = read;
        }

        return read;
    }

    private sealed record Snapshot(bool Enabled, byte[]? Key, Dictionary<string, byte[]> Values);

    #endregion

    #region Rules

    /// <summary>
    /// The name of a secret, or why it is not one.
    /// </summary>
    /// <remarks>
    /// The shape of the name of an environment variable, because that is what
    /// a secret is called in the project a lambda is exported as, and a name
    /// that is not one there would be a secret that cannot be moved.
    /// </remarks>
    public static string ValidName(string? name)
    {
        var trimmed = name?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            throw LambdaException.Invalid("A secret needs a name, such as STRIPE_API_KEY.");
        }

        if (!NamePattern().IsMatch(trimmed))
        {
            throw LambdaException.Invalid($"The name of a secret is letters, digits and underscores, must not start with a digit and may be {MaxName} characters long "
                                        + "- the shape of the name of an environment variable, which is what it is called when the lambda is exported. "
                                        + "STRIPE_API_KEY is one; 'stripe key' is not.");
        }

        return trimmed;
    }

    /// <summary>
    /// A value as it is kept: without the line break a paste brings along, and
    /// not empty or too long.
    /// </summary>
    private string ValidValue(string? value)
    {
        var kept = value?.TrimEnd('\r', '\n');

        if (string.IsNullOrEmpty(kept))
        {
            throw LambdaException.Invalid("A secret needs a value. To remove one, delete it.");
        }

        if (kept.Length > options.MaxSecretLength)
        {
            throw LambdaException.Invalid($"A secret may be {options.MaxSecretLength:N0} characters long. A larger file belongs in the workspace.");
        }

        return kept;
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]{0,63}$")]
    private static partial Regex NamePattern();

    private async ValueTask<IDisposable> TakeTurnAsync(long lambdaId, CancellationToken cancellation)
    {
        var stripe = _stripes[(int)((ulong)lambdaId % (ulong)_stripes.Length)];

        await stripe.WaitAsync(cancellation);

        return new Turn(stripe);
    }

    private sealed class Turn(SemaphoreSlim stripe) : IDisposable
    {
        public void Dispose() => stripe.Release();
    }

    #endregion

}
