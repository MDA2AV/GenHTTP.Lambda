using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Whether a request arrived at a lambda's own domain rather than at the
/// platform, decided once and remembered on the request.
/// </summary>
/// <remarks>
/// Several layers want to know - the outermost one that names the caller on
/// every log line, the router that picks the handler, the counters of the
/// lambda - and they have to agree. So the first to ask decides against the
/// registry and every later one reads that answer back, however the registry
/// has changed in between.
///
/// Read from the Host header, which is read at the very start of a request:
/// once a body has been consumed, the headers of a request may no longer be
/// there to read.
/// </remarks>
public static class DomainRequest
{
    private const string Key = "__LAMBDA_DOMAIN";

    /// <summary>
    /// The custom domain the request was addressed to, or nothing when it is
    /// addressed to the platform.
    /// </summary>
    public static CustomDomain? ResolveDomain(this IRequest request, DomainRegistry registry)
    {
        if (request.Properties.TryGet<Decision>(Key, out var decided))
        {
            return decided.Domain;
        }

        CustomDomain? domain = null;

        if (DomainNames.Normalize(request.Header.Headers.GetEntry("Host")) is { } host && registry.TryFind(host, out var lambdaId))
        {
            domain = new CustomDomain(host, lambdaId);
        }

        request.Properties[Key] = new Decision(domain);

        return domain;
    }

    /// <summary>
    /// The custom domain an earlier layer found the request addressed to.
    /// </summary>
    /// <remarks>
    /// Nothing both for a request to the platform and for one nobody has
    /// decided about yet - which inside the handlers of a lambda cannot
    /// happen, since the router decides before it hands one over.
    /// </remarks>
    public static CustomDomain? GetDomain(this IRequest request)
        => request.Properties.TryGet<Decision>(Key, out var decided) ? decided.Domain : null;

    /// <summary>
    /// Boxes the answer, so that "not a custom domain" is remembered as well
    /// and not asked again.
    /// </summary>
    private sealed record Decision(CustomDomain? Domain);

}

/// <summary>
/// A domain the request was addressed to, and the lambda it belonged to when
/// it arrived.
/// </summary>
/// <param name="Name">The domain, normalized - what is shown and what the log records</param>
public sealed record CustomDomain(string Name, long LambdaId);
