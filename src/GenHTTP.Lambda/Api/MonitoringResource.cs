using System.Text.RegularExpressions;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;

using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// How one lambda is doing: its traffic, what it has said, and the whole of
/// it at a glance.
/// </summary>
/// <remarks>
/// The owner's slice of what the operator sees across the installation, and
/// behind the same thing everything else about a lambda is: its editor key.
/// Everything here is filtered by the identity the key resolves to rather
/// than by the public key, because a public key can be given up and claimed
/// again, and what was said under it before belongs to whoever said it.
/// </remarks>
public sealed partial class MonitoringResource(IMetaService meta, IWorkspaceService workspace, LambdaTelemetry telemetry,
                                               LogBook book, LambdaOptions options)
{

    #region Functionality

    /// <summary>
    /// Whether the lambda is online, how it is being used, whether it is
    /// failing, and how much of its allowance it spends.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/summary")]
    public async ValueTask<LambdaSummaryResponse> Summary(string privateKey)
    {
        var lambda = await meta.RequireAsync(privateKey);

        var id = await meta.RequireIdAsync(privateKey);

        var versions = await meta.GetVersionsAsync(privateKey);

        var activations = await meta.GetActivationsAsync(privateKey);

        var now = DateTime.UtcNow;

        var current = activations.FirstOrDefault(a => a.Ended == null);

        var live = versions.FirstOrDefault(v => v.Version == lambda.ActiveVersion);

        var latest = versions.Count > 0 ? versions[0] : null;

        var traffic = telemetry.Describe(id);

        /*
         * A request answered with a client error is written as a warning, and
         * a browser asking for a favicon that is not there is not a problem
         * anybody needs to be shown first. What is: anything at error, and
         * anything the server said about the lambda itself - a handler that
         * threw is a warning with the trace attached.
         */
        var (said, _, _) = book.Read(0, null, LogLevel.Warning, 500, lambdaId: id);

        var problems = said.Where(l => l.Level is "error" or "critical" || l.Source != "Requests")
                           .TakeLast(5)
                           .Reverse()
                           .ToList();

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
            await MeasureAsync(privateKey, id, live?.Version ?? latest?.Version),
            new SummaryLimits(
                options.MaxCodeLength,
                LambdaSource.MaxFiles,
                options.MaxAssetBytes,
                LambdaSource.MaxAssets,
                WorkspaceLimits.Quota,
                WorkspaceLimits.MaxFiles,
                WorkspaceLimits.MaxFileSize,
                options.MaxVersions,
                (int)options.DeploymentLifetime.TotalHours,
                (int)options.Retention.TotalDays
            )
        );
    }

    /// <summary>
    /// The traffic of the lambda: the last hour by the minute, the last day by
    /// the quarter hour, how requests were answered and what was asked for.
    /// </summary>
    /// <remarks>
    /// Counted in memory since the server last started, which the answer
    /// says, so a restart starts every figure here again from nothing.
    /// </remarks>
    [ResourceMethod("lambdas/:privateKey/traffic")]
    public async ValueTask<LambdaTraffic> Traffic(string privateKey)
        => telemetry.Describe(await meta.RequireIdAsync(privateKey));

    /// <summary>
    /// What the lambda and the server about it have said, oldest first.
    /// </summary>
    /// <param name="since">
    /// The last sequence already seen. Omitted, only the tail comes back.
    /// </param>
    /// <param name="level">The lowest level worth returning: debug, info, warn or error</param>
    /// <param name="limit">At most this many lines, up to 2000</param>
    /// <remarks>
    /// Its requests, what it printed, and what the server said while serving
    /// it - a handler that threw, a request that timed out. Held in a ring
    /// shared with everything else on the installation, so a busy neighbour
    /// shortens how far back this reaches; it is for watching, not keeping.
    /// </remarks>
    [ResourceMethod("lambdas/:privateKey/logs")]
    public async ValueTask<OwnerLogResponse> Logs(string privateKey, long? since, string? level, int? limit)
    {
        var id = await meta.RequireIdAsync(privateKey);

        var wanted = Math.Clamp(limit ?? (since.HasValue ? 1000 : 500), 1, 2000);

        var (lines, cursor, missed) = book.Read(since ?? 0, null, Minimum(level), wanted, lambdaId: id);

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
    /// What the lambda keeps: the files of a version and the workspace beside it.
    /// </summary>
    private async ValueTask<StorageSummary> MeasureAsync(string privateKey, long id, int? version)
    {
        IReadOnlyList<LambdaFile> files = [];

        if (version is { } wanted)
        {
            try
            {
                files = LambdaSource.Parse((await meta.GetVersionAsync(privateKey, wanted)).Code);
            }
            catch (LambdaException)
            {
                // a version whose code has gone missing is reported as empty
                // rather than making the whole summary unavailable
            }
        }

        var code = files.Where(f => f.IsCode).ToList();

        var listing = await workspace.ListAsync(id);

        return new StorageSummary(
            version,
            code.Count,
            LambdaSource.Length(files),
            files.Count - code.Count,
            LambdaSource.AssetBytes(files),
            listing.Files.Count,
            listing.UsedBytes,
            code.Any(f => ServingAssets().IsMatch(f.Code)),
            code.Any(f => ServingWorkspace().IsMatch(f.Code))
        );
    }

    /// <summary>
    /// The calls that turn a directory into something served. Read from the
    /// code, so it says what the code asks for rather than what a request
    /// would find - which is the right thing to warn about.
    /// </summary>
    [GeneratedRegex(@"\bAssets\s*\.\s*(App|Files|Tree)\s*\(")]
    private static partial Regex ServingAssets();

    [GeneratedRegex(@"\bWorkspace\s*\.\s*(App|Files|Tree)\s*\(")]
    private static partial Regex ServingWorkspace();

    private static LogLevel Minimum(string? level) => level?.Trim().ToLowerInvariant() switch
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
