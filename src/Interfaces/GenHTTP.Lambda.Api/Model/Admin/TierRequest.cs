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

[Summary("The tier to move a lambda to.")]
public sealed record TierRequest(
    [Summary("`Sandbox`, `Production` or `System`, in any case. `Free` and `Premium`, what the sandbox and production were called before, are read as them.")]
    string Tier
);
