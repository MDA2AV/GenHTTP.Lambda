using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Secrets;

/// <summary>
/// Where the secrets of the lambdas are kept, and where a running lambda
/// reads its own.
/// </summary>
/// <remarks>
/// Addressed by the ids of a lambda and a feature rather than by keys, because
/// it is asked by the platform itself - the compiled lambda, the features, the
/// data service - once whoever called has been checked. What an owner or an
/// agent calls is <see cref="ISecretService"/>.
///
/// A lambda reads its secrets while it serves requests, so what it reads is
/// held in memory, sealed, per lambda and feature, and opened on each read;
/// every change goes through here and makes the next read load them again.
/// The code of the lambda is handed a function to read with rather than the
/// values, so a secret changed in the editor is what the next request reads,
/// without the lambda being built again.
/// </remarks>
public sealed partial class SecretVault(IDbContextFactory<LambdaDbContext> databases, SecretCipher cipher, ILogger<SecretVault> logger)
{

    /// <summary>
    /// How many secrets a lambda may have.
    /// </summary>
    public const int MaxSecrets = 100;

    /// <summary>
    /// How long a name may be.
    /// </summary>
    public const int MaxName = 128;

    /// <summary>
    /// How large a value may be, in bytes: room for a private key or the JSON
    /// of a service account, not for a file.
    /// </summary>
    public const int MaxValue = 32 * 1024;

    private const string FingerprintSetting = "secrets-key";

    private readonly ConcurrentDictionary<Scope, Snapshot> _snapshots = [];

    /// <summary>
    /// How often the secrets of a lambda changed, so a snapshot loaded while
    /// one did is known to be old.
    /// </summary>
    private readonly ConcurrentDictionary<long, long> _generations = [];

    private int _fingerprinted;

    #region Reading, from inside a lambda

    /// <summary>
    /// What a compiled lambda is handed to read its secrets with.
    /// </summary>
    /// <remarks>
    /// Called with a name and whether the caller insists on it: insisting,
    /// it throws with what to do about a secret that is not there; not, it
    /// answers null - which is what <c>Secret.Exists</c> turns into false,
    /// for code that works with and without a value.
    /// </remarks>
    public Func<string, bool, string?> ReaderFor(long lambdaId, long? featureId)
        => (name, required) => Read(new Scope(lambdaId, featureId), name, required);

    private string? Read(Scope scope, string name, bool required)
    {
        var snapshot = Current(scope);

        if (!snapshot.Enabled)
        {
            return required ? throw new InvalidOperationException(DataKinds.SecretsOff) : null;
        }

        if (name == null || !snapshot.Values.TryGetValue(name, out var sealedValue))
        {
            if (!required)
            {
                return null;
            }

            var where = scope.FeatureId == null ? "this lambda" : "the copy of the secrets this feature works on";

            throw new KeyNotFoundException($"There is no secret called '{name}' in {where}. Set it under Data > Secrets in the editor, "
                                         + "with set_secret (MCP) or with PUT /api/v1/lambdas/{privateKey}/secrets/{name}. Names are case sensitive.");
        }

        return cipher.Open(snapshot.Key!, name, sealedValue);
    }

    private Snapshot Current(Scope scope)
    {
        var generation = _generations.GetValueOrDefault(scope.LambdaId);

        if (_snapshots.TryGetValue(scope, out var known) && known.Generation == generation)
        {
            return known;
        }

        // taken with the generation read before it, so a change landing while
        // it loads leaves it old, and the next read loads again
        var loaded = Load(scope, generation);

        _snapshots[scope] = loaded;

        return loaded;
    }

    /// <summary>
    /// Reads the sealed secrets of a lambda or feature, synchronously: a lambda
    /// reads a secret with a plain call, from wherever it happens to be.
    /// </summary>
    private Snapshot Load(Scope scope, long generation)
    {
        using var database = databases.CreateDbContext();

        var enabled = database.DataStores.AsNoTracking()
                              .Where(s => s.LambdaId == scope.LambdaId && s.Kind == DataKinds.SecretsId)
                              .Select(s => (bool?)s.Enabled)
                              .FirstOrDefault() ?? DataKinds.Secrets.Default;

        if (!enabled)
        {
            return new Snapshot(generation, false, null, new Dictionary<string, byte[]>());
        }

        var salt = database.Lambdas.AsNoTracking()
                           .Where(l => l.Id == scope.LambdaId)
                           .Select(l => l.SecretSalt)
                           .FirstOrDefault();

        var values = database.Secrets.AsNoTracking()
                             .Where(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId)
                             .ToDictionary(s => s.Name, s => s.Value, StringComparer.Ordinal);

        if (values.Count > 0)
        {
            CheckFingerprint(database);
        }

        return new Snapshot(generation, true, salt != null && values.Count > 0 ? cipher.KeyOf(salt) : null, values);
    }

    #endregion

    #region Keeping them

    /// <summary>
    /// What is stored for the lambda, or for a feature's copy, by name.
    /// </summary>
    public IReadOnlyList<SecretEntity> List(long lambdaId, long? featureId)
    {
        using var database = databases.CreateDbContext();

        return database.Secrets.AsNoTracking()
                       .Where(s => s.LambdaId == lambdaId && s.FeatureId == featureId)
                       .OrderBy(s => s.Name)
                       .ToList();
    }

    /// <summary>
    /// How many secrets the lambda has, or a feature's copy of them.
    /// </summary>
    public int Count(long lambdaId, long? featureId)
    {
        using var database = databases.CreateDbContext();

        return database.Secrets.Count(s => s.LambdaId == lambdaId && s.FeatureId == featureId);
    }

    /// <summary>
    /// Stores a value under a name, replacing the one there.
    /// </summary>
    /// <remarks>
    /// Whether the lambda has secrets switched on is the caller's to check;
    /// the demos are set up here with the check deliberately left out.
    /// </remarks>
    public SecretEntity Store(long lambdaId, long? featureId, string name, string value)
    {
        Validate(name, value);

        using var database = databases.CreateDbContext();

        var lambda = database.Lambdas.FirstOrDefault(l => l.Id == lambdaId)
                  ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        // the lambda's half of its key, made the first time it needs one
        lambda.SecretSalt ??= SecretCipher.NewSalt();

        CheckFingerprint(database);

        var sealedValue = cipher.Seal(cipher.KeyOf(lambda.SecretSalt), name, value);

        var existing = database.Secrets.FirstOrDefault(s => s.LambdaId == lambdaId && s.FeatureId == featureId && s.Name == name);

        var now = DateTime.UtcNow;

        if (existing == null)
        {
            if (database.Secrets.Count(s => s.LambdaId == lambdaId && s.FeatureId == featureId) >= MaxSecrets)
            {
                throw LambdaException.Conflict($"A lambda may keep {MaxSecrets} secrets. Delete one it no longer needs first.");
            }

            existing = new SecretEntity { LambdaId = lambdaId, FeatureId = featureId, Name = name, Value = sealedValue, Created = now, Changed = now };

            database.Secrets.Add(existing);
        }
        else
        {
            existing.Value = sealedValue;
            existing.Changed = now;
        }

        database.SaveChanges();

        Invalidate(lambdaId);

        return existing;
    }

    /// <summary>
    /// Removes a secret, and says whether there was one.
    /// </summary>
    public bool Remove(long lambdaId, long? featureId, string name)
    {
        using var database = databases.CreateDbContext();

        var removed = database.Secrets.Where(s => s.LambdaId == lambdaId && s.FeatureId == featureId && s.Name == name)
                              .ExecuteDelete();

        Invalidate(lambdaId);

        return removed > 0;
    }

    /// <summary>
    /// Replaces a feature's copy of the secrets with the lambda's own.
    /// </summary>
    /// <remarks>
    /// Copied sealed: the copy is the lambda's, under the lambda's key, so
    /// nothing is opened to make it.
    /// </remarks>
    public void Copy(long lambdaId, long featureId)
    {
        using var database = databases.CreateDbContext();

        database.Secrets.Where(s => s.LambdaId == lambdaId && s.FeatureId == featureId).ExecuteDelete();

        var own = database.Secrets.AsNoTracking()
                          .Where(s => s.LambdaId == lambdaId && s.FeatureId == null)
                          .ToList();

        database.Secrets.AddRange(own.Select(s => new SecretEntity
        {
            LambdaId = lambdaId,
            FeatureId = featureId,
            Name = s.Name,
            Value = s.Value,
            Created = s.Created,
            Changed = s.Changed
        }));

        database.SaveChanges();

        Invalidate(lambdaId);
    }

    /// <summary>
    /// Deletes every secret of a lambda, its features' copies included, and
    /// its half of the key with them - secrets stored later are sealed with a
    /// new one.
    /// </summary>
    public void Clear(long lambdaId)
    {
        using var database = databases.CreateDbContext();

        database.Secrets.Where(s => s.LambdaId == lambdaId).ExecuteDelete();

        database.Lambdas.Where(l => l.Id == lambdaId)
                      .ExecuteUpdate(u => u.SetProperty(l => l.SecretSalt, (byte[]?)null));

        Invalidate(lambdaId);
    }

    /// <summary>
    /// Forgets what is held in memory for a lambda, so the next read loads
    /// what is stored - after a change, a switch, or the lambda being deleted.
    /// </summary>
    public void Invalidate(long lambdaId)
    {
        _generations.AddOrUpdate(lambdaId, 1, (_, was) => was + 1);

        foreach (var scope in _snapshots.Keys.Where(s => s.LambdaId == lambdaId).ToList())
        {
            _snapshots.TryRemove(scope, out _);
        }
    }

    #endregion

    #region Names

    /// <summary>
    /// Whether a name is one a secret may have: letters, digits and underscores,
    /// not starting with a digit - the names an environment variable may have,
    /// which is where an exported lambda reads its secrets from.
    /// </summary>
    public static bool IsName(string? name) => name is { Length: > 0 and <= MaxName } && NamePattern().IsMatch(name);

    private static void Validate(string name, string value)
    {
        if (!IsName(name))
        {
            throw LambdaException.Invalid($"'{name}' is not a name a secret may have: letters, digits and underscores, not starting with a digit, "
                                        + $"up to {MaxName} characters - STRIPE_KEY, for example. It is the name of an environment variable in an exported project.");
        }

        if (string.IsNullOrEmpty(value))
        {
            throw LambdaException.Invalid("A secret needs a value. To remove one, delete it.");
        }

        if (System.Text.Encoding.UTF8.GetByteCount(value) > MaxValue)
        {
            throw LambdaException.Invalid($"A secret may be {MaxValue / 1024} KB at most. A file belongs in the workspace.");
        }
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex NamePattern();

    /// <summary>
    /// The names code reads with <c>Secret.Read</c> or <c>Secret.Exists</c>,
    /// written as a literal.
    /// </summary>
    /// <remarks>
    /// Read off the text rather than asked of the compiler: a name built at
    /// runtime is not found, and that is fine - this is there to tell the owner
    /// which values the code is waiting for, not to be a proof.
    /// </remarks>
    /// <returns>Each name, and whether it is asked about with Exists - which is what code does that works without it</returns>
    public static IEnumerable<(string Name, bool Checked)> ReadBy(string code)
        => UsePattern().Matches(code).Select(m => (m.Groups[2].Value, m.Groups[1].Value == "Exists"));

    [GeneratedRegex("""\bSecret\s*\.\s*(Read|Exists)\s*\(\s*"([A-Za-z_][A-Za-z0-9_]*)"\s*\)""")]
    private static partial Regex UsePattern();

    #endregion

    #region Key

    /// <summary>
    /// Remembers which installation key sealed what the database holds, and
    /// says so loudly when the server runs with another one.
    /// </summary>
    /// <remarks>
    /// Without this, a server moved without its key would find out one secret
    /// at a time, in the logs of the lambdas that read them.
    /// </remarks>
    private void CheckFingerprint(LambdaDbContext database)
    {
        if (Interlocked.Exchange(ref _fingerprinted, 1) == 1)
        {
            return;
        }

        try
        {
            var mine = cipher.Fingerprint();

            var stored = database.Settings.AsNoTracking().FirstOrDefault(s => s.Key == FingerprintSetting);

            if (stored == null)
            {
                using var writer = databases.CreateDbContext();

                writer.Settings.Add(new SettingEntity { Key = FingerprintSetting, Value = mine });
                writer.SaveChanges();
            }
            else if (stored.Value != mine)
            {
                logger.LogError("The secrets in this database were sealed with another key than the one this server has (LAMBDA_SECRETS_KEY or secrets.key), "
                              + "so none of them can be read. Start the server with the key the database was written with.");
            }
        }
        catch (Exception e)
        {
            // the check is a courtesy; reading the secrets says the same thing later
            logger.LogWarning(e, "Failed to check which key the secrets were sealed with");
        }
    }

    #endregion

    private readonly record struct Scope(long LambdaId, long? FeatureId);

    /// <param name="Key">The key of the lambda, while it has anything to open with it</param>
    private sealed record Snapshot(long Generation, bool Enabled, byte[]? Key, IReadOnlyDictionary<string, byte[]> Values);

}
