using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The domain of a lambda, and whether it answers at it.")]
public sealed record DomainResponse(
    [Summary("The domain it is configured to answer at; absent without one")]
    string? Domain,
    [Summary("The tier of the lambda, which decides whether it may have one")]
    string Tier,
    [Summary("Whether the tier includes a domain of its own: only the premium tier does")]
    bool Allowed,
    [Summary("Whether requests to the domain reach the lambda right now - it has one, and its tier includes it. A domain kept from a premium tier is not served.")]
    bool Served,
    [Summary("What the domain resolves to, as the server sees it; absent without a domain")]
    DnsResponse? Dns
);

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
