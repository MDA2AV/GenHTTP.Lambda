using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Services.Census;
using GenHTTP.Lambda.Services.Integrations;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Optimization;
using GenHTTP.Lambda.Services.QuietHours;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// Everything the operator's dashboards draw that is not about the machine:
/// the lambdas counted, what waits for the operator, and the room left. The
/// panel asks for it as it opens a section, which is how the count beside
/// Connectivity stays current without a timer.
/// </remarks>
[Summary("The installation at a glance, for whoever runs it: its lambdas counted, what waits for a decision, and the room left.")]
public sealed record AdminOverviewResponse(
    [Summary("The lambdas counted, the demos apart")]
    AdminCensusResponse Lambdas,
    [Summary("What waits for the operator")]
    AdminAttentionResponse Attention,
    [Summary("The volume of the data directory: how large it is and how much is free, read as this was asked for; absent where the system does not say")]
    AdminDiskResponse? Disk,
    [Summary("The newest lambdas, the newest first")]
    IReadOnlyList<AdminLambda> Newest,
    [Summary("The lambdas that answered with a server error in the last hour, the most first, with what they did in that hour")]
    IReadOnlyList<LambdaActivity> Failing
);
