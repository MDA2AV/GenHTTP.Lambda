using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Secrets;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;

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
/// held in memory per lambda and feature, as the secrets integration hands it
/// over; every change goes through here and makes the next read load them
/// again. The code of the lambda is handed a function to read with rather than
/// the values, so a secret changed in the editor is what the next request
/// reads, without the lambda being built again.
///
/// What is decided here is the platform's: whether the lambda has secrets
/// switched on, what a name may be, how large a value and how many of them.
/// Where they are kept is the integration's (<see cref="ISecretProvider"/>).
/// </remarks>
public sealed partial class SecretVault(IDbContextFactory<LambdaDbContext> databases, ISecretProvider provider) : ISecretVault
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

    private readonly ConcurrentDictionary<DataScope, Snapshot> _snapshots = [];

    /// <summary>
    /// How often the secrets of a lambda changed, so a snapshot loaded while
    /// one did is known to be old.
    /// </summary>
    private readonly ConcurrentDictionary<long, long> _generations = [];

    #region Reading, from inside a lambda

    /// <summary>
    /// What a compiled lambda is handed to read its secrets with.
    /// </summary>
    /// <remarks>
    /// Called with a name and whether the caller insists on it: insisting,
    /// it throws with what to do about a secret that is not there; not, it
    /// answers null - which is what <c>Secret.Exists</c> turns into false,
    /// for code that works with and without a value. The plain call goes
    /// through <see cref="Synchronous"/>, which waits for nothing where the
    /// integration answers at once.
    /// </remarks>
    public SecretReader ReaderFor(long lambdaId, long? featureId)
    {
        var scope = new DataScope(lambdaId, featureId);

        return new SecretReader((name, required) => Synchronous.Run(() => ReadAsync(scope, name, required, CancellationToken.None)),
                                (name, required, cancellation) => ReadAsync(scope, name, required, cancellation));
    }

    private async ValueTask<string?> ReadAsync(DataScope scope, string name, bool required, CancellationToken cancellation)
    {
        var snapshot = await CurrentAsync(scope, cancellation);

        if (snapshot.Values == null)
        {
            return required ? throw new InvalidOperationException(DataKinds.SecretsOff) : null;
        }

        if ((name == null ? null : snapshot.Values.Read(name)) is not { } value)
        {
            if (!required)
            {
                return null;
            }

            var where = scope.FeatureId == null ? "this lambda" : "the copy of the secrets this feature works on";

            throw new KeyNotFoundException($"There is no secret called '{name}' in {where}. Set it under Secrets in the editor, "
                                         + "with set_secret (MCP) or with PUT /api/v1/lambdas/{privateKey}/secrets/{name}. Names are case sensitive.");
        }

        return value;
    }

    /// <summary>
    /// What a lambda reads its secrets from now - from memory, but after a
    /// change.
    /// </summary>
    private ValueTask<Snapshot> CurrentAsync(DataScope scope, CancellationToken cancellation)
    {
        var generation = _generations.GetValueOrDefault(scope.LambdaId);

        if (_snapshots.TryGetValue(scope, out var known) && known.Generation == generation)
        {
            return ValueTask.FromResult(known);
        }

        return LoadAsync(scope, generation, cancellation);
    }

    /// <summary>
    /// Reads the secrets of a lambda or feature and keeps them, taken with
    /// the generation read before - so a change landing while they load
    /// leaves them old, and the next read loads again.
    /// </summary>
    private async ValueTask<Snapshot> LoadAsync(DataScope scope, long generation, CancellationToken cancellation)
    {
        var loaded = new Snapshot(generation, Enabled(scope.LambdaId) ? await provider.LoadAsync(scope, cancellation) : null);

        _snapshots[scope] = loaded;

        return loaded;
    }

    /// <summary>
    /// Whether the owner left the secrets of a lambda on.
    /// </summary>
    private bool Enabled(long lambdaId)
    {
        using var database = databases.CreateDbContext();

        return database.DataStores.AsNoTracking()
                       .Where(s => s.LambdaId == lambdaId && s.Kind == DataKinds.SecretsId)
                       .Select(s => (bool?)s.Enabled)
                       .FirstOrDefault() ?? DataKinds.Secrets.Default;
    }

    #endregion

    #region Keeping them

    /// <summary>
    /// What is stored for the lambda, or for a feature's copy, by name.
    /// </summary>
    public async ValueTask<IReadOnlyList<SecretEntry>> ListAsync(long lambdaId, long? featureId, CancellationToken cancellation = default)
        => [.. (await provider.ListAsync(new DataScope(lambdaId, featureId), cancellation)).OrderBy(s => s.Name, StringComparer.Ordinal)];

    /// <summary>
    /// How many secrets the lambda has, or a feature's copy of them.
    /// </summary>
    public async ValueTask<int> CountAsync(long lambdaId, long? featureId, CancellationToken cancellation = default)
        => (await provider.ListAsync(new DataScope(lambdaId, featureId), cancellation)).Count;

    /// <summary>
    /// Stores a value under a name, replacing the one there.
    /// </summary>
    /// <remarks>
    /// Whether the lambda has secrets switched on is the caller's to check;
    /// the demos are set up here with the check deliberately left out.
    /// </remarks>
    public async ValueTask<SecretEntry> StoreAsync(long lambdaId, long? featureId, string name, string value, CancellationToken cancellation = default)
    {
        Validate(name, value);

        var scope = new DataScope(lambdaId, featureId);

        var kept = await provider.ListAsync(scope, cancellation);

        if (kept.Count >= MaxSecrets && !kept.Any(s => s.Name == name))
        {
            throw LambdaException.Conflict($"A lambda may keep {MaxSecrets} secrets. Delete one it no longer needs first.");
        }

        var stored = await provider.StoreAsync(scope, name, value, cancellation);

        Invalidate(lambdaId);

        return stored;
    }

    /// <summary>
    /// Removes a secret, and says whether there was one.
    /// </summary>
    public async ValueTask<bool> RemoveAsync(long lambdaId, long? featureId, string name, CancellationToken cancellation = default)
    {
        var removed = await provider.DeleteAsync(new DataScope(lambdaId, featureId), name, cancellation);

        Invalidate(lambdaId);

        return removed;
    }

    /// <summary>
    /// Replaces a feature's copy of the secrets with the lambda's own.
    /// </summary>
    public async ValueTask CopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default)
    {
        await provider.CopyAsync(new DataScope(lambdaId), new DataScope(lambdaId, featureId), cancellation);

        Invalidate(lambdaId);
    }

    /// <summary>
    /// Removes a feature's copy of the secrets.
    /// </summary>
    public async ValueTask RemoveCopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default)
    {
        await provider.DeleteAsync(new DataScope(lambdaId, featureId), cancellation);

        Invalidate(lambdaId);
    }

    /// <summary>
    /// Deletes every secret of a lambda, its features' copies included -
    /// secrets stored later start afresh.
    /// </summary>
    public async ValueTask ClearAsync(long lambdaId, CancellationToken cancellation = default)
    {
        await provider.DeleteAllAsync(lambdaId, cancellation);

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
    /// The names code reads with <c>Secret.Read</c> or <c>Secret.Exists</c> -
    /// or their asynchronous forms - written as a literal.
    /// </summary>
    /// <remarks>
    /// Read off the text rather than asked of the compiler: a name built at
    /// runtime is not found, and that is fine - this is there to tell the owner
    /// which values the code is waiting for, not to be a proof.
    /// </remarks>
    /// <returns>Each name, and whether it is asked about with Exists - which is what code does that works without it</returns>
    public static IEnumerable<(string Name, bool Checked)> ReadBy(string code)
        => UsePattern().Matches(code).Select(m => (m.Groups[2].Value, m.Groups[1].Value == "Exists"));

    [GeneratedRegex("""\bSecret\s*\.\s*(Read|Exists)(?:Async)?\s*\(\s*"([A-Za-z_][A-Za-z0-9_]*)"\s*[,)]""")]
    private static partial Regex UsePattern();

    #endregion

    /// <param name="Values">What the lambda reads its secrets from, or nothing while its secrets are switched off</param>
    private sealed record Snapshot(long Generation, ISecretSet? Values);

}
