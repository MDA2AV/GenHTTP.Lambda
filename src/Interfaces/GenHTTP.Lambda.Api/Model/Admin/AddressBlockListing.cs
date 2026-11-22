using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Every address the server refuses, and the address of whoever asks.")]
public sealed record AddressBlockListing(
    [Summary("The blocks, newest first")]
    IReadOnlyList<AddressBlockResponse> Blocks,
    [Summary("The address this request came from as a block would cover it - so the operator can tell whether one of these is theirs; absent where it is on one of the installation's own networks")]
    string? You
);
