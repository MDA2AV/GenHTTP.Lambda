using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The tier to move a lambda to.")]
public sealed record TierRequest(
    [Summary("`Sandbox`, `Production` or `System`, in any case. `Free` and `Premium`, what the sandbox and production were called before, are read as them.")]
    string Tier
);
