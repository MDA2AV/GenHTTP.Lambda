using System.Collections.Concurrent;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Secrets;

public sealed class SecretService(IDbContextFactory<LambdaDbContext> databases, IMetaService meta, IFeatureService features, IStorageService storage,
                                  SecretVault vault, ILogger<SecretService> logger) : ISecretService
{

    private readonly ConcurrentDictionary<(long LambdaId, int Version), IReadOnlyList<(string Name, bool Checked)>> _reads = [];

    #region Functionality

    public async ValueTask<SecretListing> ListAsync(string privateKey, string? feature = null, CancellationToken cancellation = default)
    {
        var (lambdaId, featureId) = await ResolveAsync(privateKey, feature, false, cancellation);

        var enabled = await EnabledAsync(lambdaId, cancellation);

        var (used, optional) = await UsedAsync(lambdaId, featureId, cancellation);

        var stored = await vault.ListAsync(lambdaId, featureId, cancellation);

        var names = stored.Select(s => s.Name).ToHashSet(StringComparer.Ordinal);

        return new SecretListing(enabled,
                                 [.. stored.Select(s => new SecretInfo(s.Name, s.Created, s.Changed, used.Contains(s.Name)))],
                                 [.. used.Order(StringComparer.Ordinal)],
                                 [.. used.Where(n => !names.Contains(n) && !optional.Contains(n)).Order(StringComparer.Ordinal)],
                                 [.. optional.Where(n => !names.Contains(n)).Order(StringComparer.Ordinal)],
                                 SecretVault.MaxSecrets);
    }

    public async ValueTask<SecretInfo> SetAsync(string privateKey, string name, string value, string? feature = null, CancellationToken cancellation = default)
    {
        var (lambdaId, featureId) = await ResolveAsync(privateKey, feature, true, cancellation);

        if (!await EnabledAsync(lambdaId, cancellation))
        {
            throw LambdaException.Conflict(DataKinds.SecretsOff);
        }

        var stored = await vault.StoreAsync(lambdaId, featureId, name?.Trim() ?? "", value, cancellation);

        // the name, never the value
        logger.LogInformation("Lambda {LambdaId} stored the secret {Name}{Where}", lambdaId, stored.Name, featureId != null ? $" in feature {featureId}" : "");

        var (used, _) = await UsedAsync(lambdaId, featureId, cancellation);

        return new SecretInfo(stored.Name, stored.Created, stored.Changed, used.Contains(stored.Name));
    }

    public async ValueTask DeleteAsync(string privateKey, string name, string? feature = null, CancellationToken cancellation = default)
    {
        var (lambdaId, featureId) = await ResolveAsync(privateKey, feature, true, cancellation);

        if (!await vault.RemoveAsync(lambdaId, featureId, name?.Trim() ?? "", cancellation))
        {
            throw LambdaException.NotFound($"There is no secret called '{name}'{(featureId != null ? " in this feature's copy" : "")}. Names are case sensitive.");
        }

        logger.LogInformation("Lambda {LambdaId} deleted the secret {Name}{Where}", lambdaId, name, featureId != null ? $" in feature {featureId}" : "");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// The lambda and, where one is named, the feature - checked for being
    /// changeable where something is about to change.
    /// </summary>
    private async ValueTask<(long LambdaId, long? FeatureId)> ResolveAsync(string privateKey, string? feature, bool editable, CancellationToken cancellation)
    {
        if (!string.IsNullOrWhiteSpace(feature))
        {
            var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, editable, cancellation);

            return (lambdaId, featureId);
        }

        var id = editable
            ? await meta.RequireEditableAsync(privateKey, cancellation)
            : await meta.GetIdAsync(privateKey, cancellation) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        return (id, null);
    }

    private async ValueTask<bool> EnabledAsync(long lambdaId, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await DataSwitches.IsEnabledAsync(database, lambdaId, DataKinds.Secrets, cancellation);
    }

    /// <summary>
    /// The names the code reads: the feature's own, or the lambda's - both
    /// what is online and what was saved last, which is what goes online next.
    /// </summary>
    /// <returns>Every name read, and those of them the code asks about with Exists first - it does without them</returns>
    private async ValueTask<(HashSet<string> Used, HashSet<string> Optional)> UsedAsync(long lambdaId, long? featureId, CancellationToken cancellation)
    {
        var sources = new List<IReadOnlyList<(string Name, bool Checked)>>();

        if (featureId is { } feature)
        {
            sources.Add(Reads(await storage.ReadFeatureAsync(lambdaId, feature, cancellation)));
        }
        else
        {
            await using var database = await databases.CreateDbContextAsync(cancellation);

            var active = await database.Lambdas.AsNoTracking().Where(l => l.Id == lambdaId).Select(l => l.ActiveVersion).FirstOrDefaultAsync(cancellation);

            var newest = await database.Deployments.AsNoTracking().Where(d => d.LambdaId == lambdaId).MaxAsync(d => (int?)d.Version, cancellation);

            foreach (var version in new[] { active, newest }.OfType<int>().Distinct())
            {
                sources.Add(await ReadsOfAsync(lambdaId, version, cancellation));
            }
        }

        var names = new HashSet<string>(StringComparer.Ordinal);

        var optional = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (name, isChecked) in sources.SelectMany(s => s))
        {
            names.Add(name);

            if (isChecked)
            {
                optional.Add(name);
            }
        }

        return (names, optional);
    }

    /// <summary>
    /// The secrets one version reads, remembered: a version never changes,
    /// and the dashboard asks every few seconds - reading a version with a
    /// hundred megabytes of assets each time to find two names would not do.
    /// </summary>
    private async ValueTask<IReadOnlyList<(string Name, bool Checked)>> ReadsOfAsync(long lambdaId, int version, CancellationToken cancellation)
    {
        if (_reads.TryGetValue((lambdaId, version), out var known))
        {
            return known;
        }

        var found = Reads(await storage.ReadAsync(lambdaId, version, cancellation));

        if (_reads.Count > 4096)
        {
            _reads.Clear();
        }

        return _reads[(lambdaId, version)] = found;
    }

    private static IReadOnlyList<(string Name, bool Checked)> Reads(string? source)
        => source == null ? [] : [.. LambdaSource.Parse(source).Where(f => f.IsCode).SelectMany(f => SecretVault.ReadBy(f.Code))];

    #endregion

}
