using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What a tier lets a lambda have and how it keeps it, as this installation set it.")]
public sealed record TierResponse(
    [Summary("`Sandbox` for trying something, `Production` for an app people use, `System` for what the installation itself depends on, or `Demo` for one of its demos")]
    string Tier,
    [Summary("What this installation calls the tier on its pages, shown as written - `Premium` for production on genhttp.dev; absent where it goes by its own name")]
    string? Label,
    [Summary("What a version may come to, its code and its resources together, in bytes")]
    long VersionBytes,
    [Summary("The room its data may take, its database and its workspace together, in bytes")]
    long DataBytes,
    [Summary("How many days of its analytics its owner is shown, of the year that is kept")]
    int AnalyticsDays,
    [Summary("Whether a lambda in it may connect to other servers while the operator decided nothing about it")]
    bool Connectivity,
    [Summary("Whether a lambda in it may answer at a domain of its own")]
    bool OwnDomain,
    [Summary("Whether a lambda in it is taken offline once nobody visits or edits it, and deleted later; otherwise it is kept")]
    bool ExpiresWhenUnused,
    [Summary("Whether it is the installation's own: never blocked, never in the showcase, and not deleted with the editor key")]
    bool Installation,
    [Summary("The tier an app in it would be moved to for more, which only the operator does; absent where there is none")]
    string? Above,
    [Summary("What the installation calls that tier; absent where it goes by its own name, or there is none")]
    string? AboveLabel
);
