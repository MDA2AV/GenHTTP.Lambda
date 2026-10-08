using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Protection;
using GenHTTP.Lambda.Web;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Finds a lambda by the subdomain of the hosting domain the request was
/// addressed to, which is named after its public key - the way every lambda
/// is reached, unless it answers at a domain of its own.
/// </summary>
/// <remarks>
/// The whole path belongs to the lambda here, so nothing is stepped past.
///
/// A subdomain nothing is online at is not a page of the platform, which
/// never serves its pages at this domain: it is told so in a plain page of
/// its own, which leads to the site.
/// </remarks>
public sealed class SubdomainLocator(IMetaService meta, HostingPages pages) : ILambdaLocator
{

    public ResolvedLambda? Locate(IRequest request)
    {
        if (request.GetHost() is not HostedLambda hosted || !LambdaKeys.TryNormalize(hosted.Label, out var key, out _))
        {
            return null;
        }

        return meta.Resolve(key);
    }

    public IResponse Unavailable(IRequest request) => pages.Missing(request, request.GetHost()?.Name ?? string.Empty);

}
