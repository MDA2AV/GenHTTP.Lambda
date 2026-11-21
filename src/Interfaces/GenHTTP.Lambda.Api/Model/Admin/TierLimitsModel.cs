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

[Summary("What a lambda of one tier may have, and what the tier is called.")]
public sealed record TierLimitsModel(
    [Summary("What a version may come to, its code and its resources together, in bytes")]
    long BuildBytes,
    [Summary("The room its data may take, its database and its workspace together, in bytes")]
    long DataBytes,
    [Summary("How many days of its analytics its owner is shown, of the year that is kept")]
    int AnalyticsDays,
    [Summary("Whether a lambda in it may connect to other servers while the operator decided nothing about it. A decision about the lambda - allowed or declined - comes first either way.")]
    bool Connectivity,
    [Summary("What the tier is called on the installation's pages, shown as written in every language, up to 24 characters; absent or empty for its own name, which the pages translate")]
    string? Label = null
);
