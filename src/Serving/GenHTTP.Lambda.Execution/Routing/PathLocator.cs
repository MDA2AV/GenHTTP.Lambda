using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Protection;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Execution;

/// <summary>
/// Finds the lambda named by the first segment of the path, the way every
/// lambda is reached below <c>/lambda/</c> where the lambdas answer by path.
/// </summary>
/// <remarks>
/// The lambda routes on what comes after its key, so it is stepped past - a
/// lambda answers at the root of its address either way, and relative links
/// work the same below a path as at a host of its own.
///
/// The key is read as it is spelled (<see cref="LambdaKeys.TryRead"/>), never
/// by the rules for claiming one, so a rule made later does not take a lambda
/// from before it offline.
/// </remarks>
public sealed class PathLocator(IMetaService meta, HostingPages pages) : ILambdaLocator
{

    public async ValueTask<ResolvedLambda?> LocateAsync(IRequest request)
    {
        var target = request.Header.Target;

        if (target.Current is not { } segment || !LambdaKeys.TryRead(segment.Decode(), out var key))
        {
            return null;
        }

        var lambda = await meta.ResolveAsync(key);

        if (lambda != null)
        {
            target.Advance();
        }

        return lambda;
    }

    public IResponse Unavailable(IRequest request)
    {
        var key = request.Header.Target.Current?.Decode() ?? string.Empty;

        return pages.Missing(request, $"/{Services.Hosting.PlatformRoutes.Lambdas}/{key}/");
    }

}
