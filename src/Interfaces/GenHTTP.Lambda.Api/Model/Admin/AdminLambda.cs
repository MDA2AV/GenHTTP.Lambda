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
/// This carries the editor key, so it only ever answers a request that has
/// presented the administration token.
/// </remarks>
[Summary("A lambda as the operator sees it: what is kept about it, and what it has been doing since the server came up.")]
public sealed record AdminLambda(
    [Summary("Its public key")]
    string PublicKey,
    [Summary("Its editor key")]
    string PrivateKey,
    [Summary("`Sandbox`, `Production`, `System` or `Demo`")]
    string Tier,
    [Summary("What the installation calls its tier on its pages; absent where it goes by its own name")]
    string? TierLabel,
    [Summary("When it was created")]
    DateTime Created,
    [Summary("When something about it last changed")]
    DateTime Modified,
    [Summary("The version online; absent while none is")]
    int? ActiveVersion,
    [Summary("Its newest version")]
    int? LatestVersion,
    [Summary("How many versions are kept")]
    int Versions,
    [Summary("When it is taken offline unless it is used before then")]
    DateTime? DeployedUntil,
    [Summary("When it is deleted unless it is used before then")]
    DateTime? KeptUntil,
    [Summary("Requests since the server came up")]
    long Requests,
    [Summary("Of those, answered with a server error")]
    long Failed,
    [Summary("When the last request arrived, since the server came up")]
    DateTime? LastSeen,
    [Summary("The domain of its own, if it has one")]
    string? Domain,
    [Summary("Whether it answers at that domain")]
    bool DomainServed,
    [Summary("Since when it is blocked; absent while it is not")]
    DateTime? Blocked = null,
    [Summary("Whether it may connect to other servers, and where its request for that stands")]
    ConnectivityResponse? Connectivity = null,
    [Summary("Which of the platform's features it uses")]
    AdminLambdaUses? Uses = null,
    [Summary("When it was last visited, as the platform writes it down - at most hourly, so it may be behind `lastSeen`; absent if it never was")]
    DateTime? Visited = null
);
