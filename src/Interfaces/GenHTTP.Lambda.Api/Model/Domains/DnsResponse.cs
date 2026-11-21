using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// Only the addresses, and not whether they are the right ones: which
/// addresses this installation answers at is not something it can reliably
/// tell about itself from inside a container.
/// </remarks>
[Summary("What a domain resolves to, looked up by the server as the request was answered.")]
[Remarks("Only the addresses - not whether they are this installation's, which is for whoever knows where it runs to compare.")]
public sealed record DnsResponse(
    [Summary("Every address the domain resolved to, IPv4 and IPv6 alike")]
    IReadOnlyList<string> Addresses,
    [Summary("Why nothing could be resolved, where nothing could: it does not resolve yet, or the name servers could not be asked")]
    string? Problem
);
