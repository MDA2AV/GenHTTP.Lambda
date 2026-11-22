using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("An address a lambda was changed from with its editor key.")]
public sealed record EditorAddressResponse(
    [Summary("The address, as the connection came from it")]
    string Address,
    [Summary("What a block of it covers: the address, or the /64 of an IPv6 one")]
    string Range,
    [Summary("When the lambda was first changed from it")]
    DateTime FirstSeen,
    [Summary("When it was last changed from it, to within an hour")]
    DateTime LastSeen,
    [Summary("Whether a block covers it now")]
    bool Blocked
);
