using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Whether a request arrived at a lambda's host rather than at the platform -
/// a domain of its own, or its subdomain of the hosting domain - decided once
/// and remembered on the request.
/// </summary>
/// <remarks>
/// Several layers want to know - the redirect in front of everything, the
/// outermost one that names the caller on every log line, the router that
/// picks the handler, the counters of the lambda - and they have to agree. So
/// the first to ask decides against the registry and every later one reads
/// that answer back, however the registry has changed in between.
///
/// Read from the Host header, which is read at the very start of a request:
/// once a body has been consumed, the headers of a request may no longer be
/// there to read.
/// </remarks>
public static class DomainRequest
{
    private const string Key = "__LAMBDA_DOMAIN";

    /// <summary>
    /// The host of a lambda the request was addressed to, or nothing when it
    /// is addressed to the platform.
    /// </summary>
    public static LambdaHost? ResolveHost(this IRequest request, IDomainRegistry registry)
    {
        if (request.Properties.TryGet<Decision>(Key, out var decided))
        {
            return decided.Host;
        }

        var host = DomainNames.Normalize(request.Header.Headers.GetEntry("Host")) is { } name ? registry.Find(name) : null;

        request.Properties[Key] = new Decision(host);

        return host;
    }

    /// <summary>
    /// The host of a lambda an earlier layer found the request addressed to.
    /// </summary>
    /// <remarks>
    /// Nothing both for a request to the platform and for one nobody has
    /// decided about yet - which inside the handlers of a lambda cannot
    /// happen, since the router decides before it hands one over.
    /// </remarks>
    public static LambdaHost? GetHost(this IRequest request)
        => request.Properties.TryGet<Decision>(Key, out var decided) ? decided.Host : null;

    /// <summary>
    /// Boxes the answer, so that "not a lambda's host" is remembered as well
    /// and not asked again.
    /// </summary>
    private sealed record Decision(LambdaHost? Host);

}

/// <summary>
/// A host that belongs to the lambdas rather than to the platform.
/// </summary>
/// <param name="Name">The host, normalized - what is shown and what the log records</param>
public abstract record LambdaHost(string Name);

/// <summary>
/// A domain of its own the request was addressed to, and the lambda it
/// belonged to when it arrived.
/// </summary>
public sealed record CustomDomain(string Name, long LambdaId) : LambdaHost(Name);

/// <summary>
/// A subdomain of the hosting domain, which is the address of the lambda
/// whose public key it is named after - if there is one.
/// </summary>
/// <param name="Label">What is in front of the hosting domain, a key only where a key could be what it says</param>
public sealed record HostedLambda(string Name, string Label) : LambdaHost(Name);

/// <summary>
/// The hosting domain itself, which is nobody's lambda.
/// </summary>
public sealed record HostingHome(string Name) : LambdaHost(Name);
