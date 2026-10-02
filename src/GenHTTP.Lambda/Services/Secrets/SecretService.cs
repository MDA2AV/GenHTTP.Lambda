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

    public SecretListing List(string privateKey, string? feature = null)
    {
        var (lambdaId, featureId) = Resolve(privateKey, feature, false);

        var enabled = Enabled(lambdaId);

        var (used, optional) = Used(lambdaId, featureId);

        var stored = vault.List(lambdaId, featureId);

        var names = stored.Select(s => s.Name).ToHashSet(StringComparer.Ordinal);

        return new SecretListing(enabled,
                                 [.. stored.Select(s => new SecretInfo(s.Name, s.Created, s.Changed, used.Contains(s.Name)))],
                                 [.. used.Order(StringComparer.Ordinal)],
                                 [.. used.Where(n => !names.Contains(n) && !optional.Contains(n)).Order(StringComparer.Ordinal)],
                                 [.. optional.Where(n => !names.Contains(n)).Order(StringComparer.Ordinal)],
                                 SecretVault.MaxSecrets);
    }

    public SecretInfo Set(string privateKey, string name, string value, string? feature = null)
    {
        var (lambdaId, featureId) = Resolve(privateKey, feature, true);

        if (!Enabled(lambdaId))
        {
            throw LambdaException.Conflict(DataKinds.SecretsOff);
        }

        var stored = vault.Store(lambdaId, featureId, name?.Trim() ?? "", value);

        // the name, never the value
        if (featureId != null)
        {
            logger.LogInformation("Set secret {Name} of feature #{FeatureId} of lambda #{LambdaId}", stored.Name, featureId, lambdaId);
        }
        else
        {
            logger.LogInformation("Set secret {Name} of lambda #{LambdaId}", stored.Name, lambdaId);
        }

        var (used, _) = Used(lambdaId, featureId);

        return new SecretInfo(stored.Name, stored.Created, stored.Changed, used.Contains(stored.Name));
    }

    public void Delete(string privateKey, string name, string? feature = null)
    {
        var (lambdaId, featureId) = Resolve(privateKey, feature, true);

        if (!vault.Remove(lambdaId, featureId, name?.Trim() ?? ""))
        {
            throw LambdaException.NotFound($"There is no secret called '{name}'{(featureId != null ? " in this feature's copy" : "")}. Names are case sensitive.");
        }

        if (featureId != null)
        {
            logger.LogInformation("Deleted secret {Name} of feature #{FeatureId} of lambda #{LambdaId}", name, featureId, lambdaId);
        }
        else
        {
            logger.LogInformation("Deleted secret {Name} of lambda #{LambdaId}", name, lambdaId);
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// The lambda and, where one is named, the feature - checked for being
    /// changeable where something is about to change.
    /// </summary>
    private (long LambdaId, long? FeatureId) Resolve(string privateKey, string? feature, bool editable)
    {
        if (!string.IsNullOrWhiteSpace(feature))
        {
            var (lambdaId, featureId) = features.Require(privateKey, feature, editable);

            return (lambdaId, featureId);
        }

        var id = editable
            ? meta.RequireEditable(privateKey)
            : meta.GetId(privateKey) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        return (id, null);
    }

    private bool Enabled(long lambdaId)
    {
        using var database = databases.CreateDbContext();

        return DataSwitches.IsEnabled(database, lambdaId, DataKinds.Secrets);
    }

    /// <summary>
    /// The names the code reads: the feature's own, or the lambda's - both
    /// what is online and what was saved last, which is what goes online next.
    /// </summary>
    /// <returns>Every name read, and those of them the code asks about with Exists first - it does without them</returns>
    private (HashSet<string> Used, HashSet<string> Optional) Used(long lambdaId, long? featureId)
    {
        var sources = new List<IReadOnlyList<(string Name, bool Checked)>>();

        if (featureId is { } feature)
        {
            sources.Add(Reads(storage.ReadFeature(lambdaId, feature)));
        }
        else
        {
            using var database = databases.CreateDbContext();

            var active = database.Lambdas.AsNoTracking().Where(l => l.Id == lambdaId).Select(l => l.ActiveVersion).FirstOrDefault();

            var newest = database.Deployments.AsNoTracking().Where(d => d.LambdaId == lambdaId).Max(d => (int?)d.Version);

            foreach (var version in new[] { active, newest }.OfType<int>().Distinct())
            {
                sources.Add(ReadsOf(lambdaId, version));
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
    private IReadOnlyList<(string Name, bool Checked)> ReadsOf(long lambdaId, int version)
    {
        if (_reads.TryGetValue((lambdaId, version), out var known))
        {
            return known;
        }

        var found = Reads(storage.Read(lambdaId, version));

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
