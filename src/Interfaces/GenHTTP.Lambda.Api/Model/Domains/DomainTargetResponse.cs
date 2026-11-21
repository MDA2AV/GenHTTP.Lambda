using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// Made from the installation's public address rather than written into the
/// editor: a domain has to point where that address points, whatever is in
/// front of the server - a reverse proxy, a load balancer.
/// </remarks>
[Summary("Where a domain has to point to reach this installation.")]
[Remarks("The name of the installation's public address, for a CNAME, and what that name resolves to as the server sees it, for A and AAAA records.")]
public sealed record DomainTargetResponse(
    [Summary("The name a CNAME points at - the host of the installation's public address; absent where that is an IP address")]
    string? Name,
    [Summary("The addresses that name resolves to, IPv4 and IPv6 alike; empty where it could not be resolved, or resolves to this machine only")]
    IReadOnlyList<string> Addresses
);
