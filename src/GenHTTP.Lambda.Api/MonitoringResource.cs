using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Connectivity;
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
                                               ITelemetryService telemetry, ILogBook book, LambdaOptions options, ILimitsService limits,
                                               IConnectivityService connectivity)
{
    private readonly LambdaSummaries _summaries = new(meta, workspace, secrets, data, telemetry, book, limits, connectivity);

    #region Functionality

    [ResourceMethod("lambdas/:privateKey/summary")]
    [Summary("Reads the whole of a lambda at a glance")]
    [Remarks("""
             Whether it is online and with which version, its traffic over the last hour and day, its last few problems, what
             its version and its data take, its limits, and what its documentation says. The figures about code and storage
             are about the version online, or the newest where none is.
             """)]
    [return: Summary("The lambda at a glance")]
    public async ValueTask<LambdaSummaryResponse> Summary([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => await _summaries.OfAsync(privateKey);

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
