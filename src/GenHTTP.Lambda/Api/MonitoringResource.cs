using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Services.Settings;

using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// The owner's slice of what the operator sees across the installation, and
/// behind the same thing everything else about a lambda is: its editor key.
/// Everything here is filtered by the identity the key resolves to rather
/// than by the public key, because a public key can be given up and claimed
/// again, and what was said under it before belongs to whoever said it.
/// </remarks>
[Tag(ApiDocumentation.Monitoring)]
[Summary("How a lambda is doing: the whole of it at a glance, its traffic, and its log.")]
[Remarks("""
         `GET …/summary` answers the question *is this all right?* in one call: whether it is online, how it is used,
         whether it is failing, what it keeps, and how much of its allowances that takes - what an agent asks after deploying
         to see whether it works.

         Traffic and logs are kept in memory: a restart of the server starts the counting again, and the log is a ring shared
         with every other lambda, so a busy neighbour shortens how far back it reaches. They are for watching, not keeping;
         what is counted of visitors over time is under *Analytics*. Neither ever holds a visitor's address.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class MonitoringResource(IMetaService meta, IWorkspaceService workspace, ISecretService secrets, IDataService data,
                                               ITelemetryService telemetry, ILogBook book, LambdaOptions options, ILimitsService limits)
{

    #region Functionality

    [ResourceMethod("lambdas/:privateKey/summary")]
    [Summary("Reads the whole of a lambda at a glance")]
    [Remarks("""
             Whether it is online and with which version, its traffic over the last hour and day, its last few problems, what
             its version and its data take, its limits, and what its documentation says. The figures about code and storage
             are about the version online, or the newest where none is.
             """)]
    [return: Summary("The lambda at a glance")]
    public LambdaSummaryResponse Summary([Summary(ApiDocumentation.PrivateKey)] string privateKey)
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

        var tier = Enum.Parse<LambdaTier>(lambda.Tier);

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
            [.. problems.Select(Describe)],
            Measure(privateKey, id, measured, known),
            new SummaryLimits(
                limits.BuildOf(tier),
                limits.DataOf(tier),
                limits.Of(tier).Versions,
                (int)product.OfflineAfter.TotalHours,
                (int)product.RemovedAfter.TotalDays,
                limits.Of(tier).Features
            ),
            known.Documentation
        );
    }

    [ResourceMethod("lambdas/:privateKey/traffic")]
    [Summary("Reads the traffic of a lambda")]
    [Remarks("""
             The last hour by the minute, the last day by the quarter hour, how requests were answered, what was asked for and
             at which of its addresses. Counted in memory since the server last started, which `since` says.
             """)]
    [return: Summary("Its traffic since the server came up")]
    public LambdaTraffic Traffic([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => telemetry.DescribeLambda(meta.RequireId(privateKey));

    [ResourceMethod("lambdas/:privateKey/logs")]
    [Summary("Reads the log of a lambda")]
    [Remarks("""
             Its requests, what its code printed, and what the server said while serving it - a handler that threw, a request
             that timed out. Oldest first. To follow it, pass the `cursor` of each answer as `since` of the next; `missed` says
             how many lines went by unread in between. Requests are logged as warnings when they were answered with a client
             error and as errors with a server error. The previews of its features have logs of their own.
             """)]
    [return: Summary("The lines, and where to carry on from")]
    public OwnerLogResponse Logs([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                                 [Summary("The `cursor` of the previous answer: only what came after it. Left out, the last 500 lines.")] long? since,
                                 [Summary("The lowest level to answer with: `debug`, `info` (the default), `warn` or `error`")] string? level,
                                 [Summary("At most this many lines, up to 2000; by default 500, or 1000 with `since`")] int? limit)
    {
        var id = meta.RequireId(privateKey);

        var wanted = Math.Clamp(limit ?? (since.HasValue ? 1000 : 500), 1, 2000);

        var (lines, cursor, missed) = book.Read(since ?? 0, null, Minimum(level), wanted, lambdaId: id, feature: FeatureLines.None);

        return new OwnerLogResponse([.. lines.Select(Describe)], cursor, missed, options.CaptureLambdaOutput);
    }

    #endregion

    #region Helpers

    internal static OwnerLogEntry Describe(LogLine line)
        => new(line.Seq, line.At, line.Level, line.Source, line.Text, line.Detail, line.Country, line.Agent, line.Repeats, line.Domain);

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

    internal static LogLevel Minimum(string? level) => level?.Trim().ToLowerInvariant() switch
    {
        "trace" => LogLevel.Trace,
        "debug" => LogLevel.Debug,
        "warn" or "warning" => LogLevel.Warning,
        "error" => LogLevel.Error,
        "critical" => LogLevel.Critical,
        _ => LogLevel.Information
    };

    #endregion

}
