using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Data;
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
    /// for code that works with and without a value.
    /// </remarks>
    public Func<string, bool, string?> ReaderFor(long lambdaId, long? featureId)
        => (name, required) => Read(new DataScope(lambdaId, featureId), name, required);

    private string? Read(DataScope scope, string name, bool required)
    {
        var snapshot = Current(scope);

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

    private Snapshot Current(DataScope scope)
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
    /// Reads the secrets of a lambda or feature, synchronously: a lambda reads
    /// a secret with a plain call, from wherever it happens to be.
    /// </summary>
    private Snapshot Load(DataScope scope, long generation)
    {
        using var database = databases.CreateDbContext();

        var enabled = database.DataStores.AsNoTracking()
                              .Where(s => s.LambdaId == scope.LambdaId && s.Kind == DataKinds.SecretsId)
                              .Select(s => (bool?)s.Enabled)
                              .FirstOrDefault() ?? DataKinds.Secrets.Default;

        return new Snapshot(generation, enabled ? provider.Load(scope) : null);
    }

    #endregion

    #region Keeping them

    /// <summary>
    /// What is stored for the lambda, or for a feature's copy, by name.
    /// </summary>
    public IReadOnlyList<SecretEntry> List(long lambdaId, long? featureId)
        => [.. provider.List(new DataScope(lambdaId, featureId)).OrderBy(s => s.Name, StringComparer.Ordinal)];

    /// <summary>
    /// How many secrets the lambda has, or a feature's copy of them.
    /// </summary>
    public int Count(long lambdaId, long? featureId) => provider.List(new DataScope(lambdaId, featureId)).Count;

    /// <summary>
    /// Stores a value under a name, replacing the one there.
    /// </summary>
    /// <remarks>
    /// Whether the lambda has secrets switched on is the caller's to check;
    /// the demos are set up here with the check deliberately left out.
    /// </remarks>
    public SecretEntry Store(long lambdaId, long? featureId, string name, string value)
    {
        Validate(name, value);

        var scope = new DataScope(lambdaId, featureId);

        var kept = provider.List(scope);

        if (kept.Count >= MaxSecrets && !kept.Any(s => s.Name == name))
        {
            throw LambdaException.Conflict($"A lambda may keep {MaxSecrets} secrets. Delete one it no longer needs first.");
        }

        var stored = provider.Store(scope, name, value);

        Invalidate(lambdaId);

        return stored;
    }

    /// <summary>
    /// Removes a secret, and says whether there was one.
    /// </summary>
    public bool Remove(long lambdaId, long? featureId, string name)
    {
        var removed = provider.Delete(new DataScope(lambdaId, featureId), name);

        Invalidate(lambdaId);

        return removed;
    }

    /// <summary>
    /// Replaces a feature's copy of the secrets with the lambda's own.
    /// </summary>
    public void Copy(long lambdaId, long featureId)
    {
        provider.Copy(new DataScope(lambdaId), new DataScope(lambdaId, featureId));

        Invalidate(lambdaId);
    }

    /// <summary>
    /// Removes a feature's copy of the secrets.
    /// </summary>
    public void RemoveCopy(long lambdaId, long featureId)
    {
        provider.Delete(new DataScope(lambdaId, featureId));

        Invalidate(lambdaId);
    }

    /// <summary>
    /// Deletes every secret of a lambda, its features' copies included -
    /// secrets stored later start afresh.
    /// </summary>
    public void Clear(long lambdaId)
    {
        provider.DeleteAll(lambdaId);

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

    /// <param name="Values">What the lambda reads its secrets from, or nothing while its secrets are switched off</param>
    private sealed record Snapshot(long Generation, ISecretSet? Values);

}
