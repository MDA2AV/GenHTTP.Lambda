using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Blocks a lambda, and the addresses it was changed from if any are named.")]
public sealed record BlockRequest(
    [Summary("Why, in the operator's words, up to 500 characters. Kept with the block and with the blocks of the addresses; never shown to the owner.")]
    string? Reason = null,
    [Summary("Addresses to refuse as well for two weeks, usually some of the lambda's `editorAddresses`: an IPv4 or IPv6 address - which blocks the /64 it belongs to - or such a /64")]
    IReadOnlyList<string>? Addresses = null
);
