using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Connectivity;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A lambda whose connectivity is the operator's to decide.")]
public sealed record AdminConnectivityEntry(
    [Summary("Its public key")]
    string PublicKey,
    [Summary("`Sandbox`, `Production`, `System` or `Demo`")]
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
