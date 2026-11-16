using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// A lambda at a glance, as its owner's overview shows it: whether it is
/// online, how it is used, whether it is failing, what it keeps and how much
/// of its allowances that takes, and what its documentation says.
/// </summary>
/// <remarks>
/// Put together in one place for the two routes that answer with it - the
/// owner's summary, and the operator's page of the lambda - so the operator
/// reads what the owner reads. Made by each resource from the services it
/// was handed, since the container registers nothing of the API.
/// </remarks>
internal sealed class LambdaSummaries(IMetaService meta, IWorkspaceService workspace, ISecretService secrets, IDataService data,
                                      ITelemetryService telemetry, ILogBook book, ILimitsService limits, IConnectivityService connectivity)
{

    #region Functionality

    public LambdaSummaryResponse Of(string privateKey)
    {
        var lambda = meta.Require(privateKey);

        var id = meta.RequireId(privateKey);

        var versions = meta.GetVersions(privateKey);

        var activations = meta.GetActivations(privateKey);

        var now = DateTime.UtcNow;

        var current = activations.FirstOrDefault(a => a.Ended == null);

        var live = versions.FirstOrDefault(v => v.Version == lambda.ActiveVersion);

        var latest = versions.Count > 0 ? versions[0] : null;

        var traffic = telemetry.DescribeLambda(id);

        var policy = lambda.Policy;

        var product = limits.Get();

        /*
         * A request answered with a client error is written as a warning, and
         * a browser asking for a favicon that is not there is not a problem
         * anybody needs to be shown first. What is: anything at error, and
         * anything the server said about the lambda itself - a handler that
         * threw is a warning with the trace attached.
         */
        var (said, _, _) = book.Read(0, null, LogLevel.Warning, 500, lambdaId: id, feature: FeatureLines.None);

        var problems = said.Where(l => l.Level is "error" or "critical" || l.Source != "Requests")
                           .TakeLast(5)
                           .Reverse()
                           .ToList();

        var measured = live?.Version ?? latest?.Version;

        // read off the version once, since it never changes
        var known = meta.GetFacts(id, measured);

        return new LambdaSummaryResponse(
            LambdaDescription.Of(lambda),
            live == null ? null : VersionResource.Describe(live),
            latest == null ? null : VersionResource.Describe(latest),
            versions.Count,
            current == null
                ? null
                : new ActivationResponse(current.Version, current.Started, current.Origin, null, null,
                                         (long)(now - current.Started).TotalSeconds),
            Summarize(traffic),
            [.. problems.Select(MonitoringResource.Describe)],
            Measure(privateKey, id, measured, known),
            new SummaryLimits(
                policy.MaxVersionBytes,
                policy.MaxDataBytes,
                product.Versions,
                (int)product.OfflineAfter.TotalHours,
                (int)product.RemovedAfter.TotalDays,
                product.Features
            ),
            known.Documentation,
            connectivity.Connects(id, live?.Version, latest?.Version)
        );
    }

    #endregion

    #region Helpers

    private static TrafficSummary Summarize(LambdaTraffic traffic)
    {
        var hourly = new List<int>(24);

        // four quarters an hour, the ring ending on the one that is running
        for (var i = 0; i + 4 <= traffic.Quarters.Count; i += 4)
        {
            hourly.Add(traffic.Quarters.Skip(i).Take(4).Sum(q => q.Requests));
        }

        var day = traffic.Quarters;

        var answered = day.Sum(q => q.Requests);

        var millis = day.Sum(q => q.AverageMillis * q.Requests);

        return new TrafficSummary(
            traffic.Minutes.Sum(m => m.Requests),
            traffic.Minutes.Sum(m => m.Failed),
            answered,
            day.Sum(q => q.Failed),
            day.Sum(q => q.Rejected),
            answered > 0 ? Math.Round(millis / answered, 2) : 0,
            day.Sum(q => q.Upgrades),
            hourly,
            traffic.Totals?.LastSeen,
            traffic.Since
        );
    }

    /// <summary>
    /// What the lambda keeps: the files of a version, and its data beside it.
    /// </summary>
    private StorageSummary Measure(string privateKey, long id, int? version, VersionFacts known)
    {
        var listing = workspace.List(id);

        var kept = secrets.List(privateKey);

        var database = data.Get(privateKey, DataKinds.DatabaseId);

        return new StorageSummary(
            version,
            known.CodeFiles,
            known.CodeBytes,
            known.ResourceFiles,
            known.ResourceBytes,
            listing.Files.Count,
            listing.UsedBytes,
            known.ServesResources,
            known.ServesWorkspace,
            listing.Enabled,
            known.UsesWorkspace,
            kept.Enabled,
            kept.Secrets.Count,
            [.. kept.Missing],
            database.Enabled,
            database.Items,
            database.UsedBytes,
            known.UsesDatabase
        );
    }

    #endregion

}
