using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The operator's decision about a lambda connecting to other servers.")]
public sealed record ConnectivityDecision(
    [Summary("`true` allows it, asked for or not; `false` declines the request, or takes back what was allowed")]
    bool Allow,
    [Summary("What to tell the owner about it, up to 500 characters - they read it in their editor as it is written")]
    string? Note = null
);
