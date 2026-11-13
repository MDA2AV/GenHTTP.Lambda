using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Connectivity;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Whether a lambda's code may connect to other servers, and where its owner's request for that stands.")]
public sealed record ConnectivityResponse(
    [Summary("Whether its code may connect to other servers now - HttpClient, sockets, a WebSocket client, mail, DNS: its tier includes it, or the operator allowed it. Where it may not, code that does is refused as it compiles, with `LAMBDA0003`.")]
    bool Allowed,
    [Summary("Whether its tier includes it, whatever was asked: the premium tier does")]
    bool Included,
    [Summary("Where the request stands: `Requested` while it waits for the operator, `Allowed`, or `Declined` - asked for and turned down, or allowed and taken back. Absent while nobody asked.")]
    string? Status,
    [Summary("Since when it stands so")]
    DateTime? Changed,
    [Summary("What the owner said the lambda connects to, and why")]
    string? Purpose,
    [Summary("What the operator said deciding it, which the owner reads")]
    string? Note
)
{

    public static ConnectivityResponse Of(ConnectivityInfo? connectivity)
    {
        var known = connectivity ?? ConnectivityInfo.None;

        return new(known.Allowed, known.Included, known.Status?.ToString(), known.Changed, known.Purpose, known.Note);
    }

}

[Summary("Asks for a lambda to connect to other servers.")]
public sealed record ConnectivityRequest(
    [Summary("What it connects to and why, for the operator who decides: the addresses or services, and what the lambda does with them - `api.open-meteo.com, for the forecast on the front page`. Required, up to 1000 characters.")]
    string? Purpose
);

[Summary("The operator's decision about a lambda connecting to other servers.")]
public sealed record ConnectivityDecision(
    [Summary("`true` allows it, asked for or not; `false` declines the request, or takes back what was allowed")]
    bool Allow,
    [Summary("What to tell the owner about it, up to 500 characters - they read it in their editor as it is written")]
    string? Note = null
);

[Summary("The lambdas that asked to connect to other servers, and those that may.")]
public sealed record AdminConnectivityListing(
    [Summary("Those whose request waits for a decision, the longest waiting first")]
    IReadOnlyList<AdminConnectivityEntry> Requested,
    [Summary("Those the operator allowed, the most recently allowed first. Premium lambdas may connect without being listed here.")]
    IReadOnlyList<AdminConnectivityEntry> Allowed
);

[Summary("A lambda whose connectivity is the operator's to decide.")]
public sealed record AdminConnectivityEntry(
    [Summary("Its public key")]
    string PublicKey,
    [Summary("`Free`, `Premium` or `Demo`")]
    string Tier,
    [Summary("The version online; absent while none is")]
    int? ActiveVersion,
    [Summary("Since when it is blocked; absent while it is not")]
    DateTime? Blocked,
    [Summary("Where its connectivity stands, and what its owner said it is for")]
    ConnectivityResponse Connectivity,
    [Summary("Whether the code of its version online, or of its newest, connects to other servers - as far as what is written tells")]
    bool Connects
);
