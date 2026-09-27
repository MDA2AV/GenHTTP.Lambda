namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The domain of a lambda, as its owner configures it.
/// </summary>
/// <param name="Domain">The domain it is configured to answer at, if any</param>
/// <param name="Tier">The tier of the lambda, which decides whether it may have one</param>
/// <param name="Allowed">Whether the tier includes a domain of its own</param>
/// <param name="Served">Whether requests to the domain reach the lambda right now</param>
/// <param name="Dns">What the domain resolves to, as seen from the server; absent without a domain</param>
public sealed record DomainResponse(string? Domain, string Tier, bool Allowed, bool Served, DnsResponse? Dns);

/// <summary>
/// What a domain resolves to, looked up by the server as the request was answered.
/// </summary>
/// <remarks>
/// Only the addresses, and not whether they are the right ones: which
/// addresses this installation answers at is not something it can reliably
/// tell about itself from inside a container.
/// </remarks>
/// <param name="Addresses">Every address the name resolved to, IPv4 and IPv6 alike</param>
/// <param name="Problem">Why nothing could be resolved, where nothing could</param>
public sealed record DnsResponse(IReadOnlyList<string> Addresses, string? Problem);
