using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Execution;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Protection;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Finds a lambda by the domain the request was addressed to.
/// </summary>
/// <remarks>
/// The whole path belongs to the lambda here, so nothing is stepped past.
///
/// A domain whose lambda is offline is not a page of the platform: the
/// visitor asked for somebody's site, and the platform's own pages would load
/// their scripts and styles from that site and fall apart. They are told the
/// site is offline, in the same plain page a lambda's own errors are shown in.
/// </remarks>
public sealed class DomainLocator(IMetaService meta) : ILambdaLocator
{

    public ResolvedLambda? Locate(IRequest request)
    {
        if (request.GetHost() is not CustomDomain domain)
        {
            return null;
        }

        return meta.Resolve(domain.LambdaId);
    }

    public IResponse Unavailable(IRequest request)
        => LambdaErrorMapper.Render(request, request.Header.Headers.GetEntry("Accept"), ResponseStatus.ServiceUnavailable, "Offline",
                                        "This site is not online at the moment. Please try again later.", null);

}
