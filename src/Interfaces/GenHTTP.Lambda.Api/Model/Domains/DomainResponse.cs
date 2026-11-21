using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The domain of a lambda, and whether it answers at it.")]
public sealed record DomainResponse(
    [Summary("The domain it is configured to answer at; absent without one")]
    string? Domain,
    [Summary("The tier of the lambda, which decides whether it may have one")]
    string Tier,
    [Summary("Whether the tier includes a domain of its own: production and system lambdas do, a sandbox does not")]
    bool Allowed,
    [Summary("Whether requests to the domain reach the lambda right now - it has one, and its tier includes it. A domain kept from a tier that had one is not served.")]
    bool Served,
    [Summary("What the domain resolves to, as the server sees it; absent without a domain")]
    DnsResponse? Dns,
    [Summary("Where the domain has to point to reach this installation; absent without a domain")]
    DomainTargetResponse? Target
);
