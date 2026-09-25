using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// Finds a lambda by the public key in the first segment of the path, the way
/// every lambda is reached below <c>/lambda/</c>.
/// </summary>
/// <remarks>
/// A key that does not exist is a page of the platform, not an error of a
/// lambda: browsers are sent to the single page application, which explains
/// the situation and offers to create one.
/// </remarks>
public sealed class KeyLocator(IMetaService meta, SpaResources spa) : ILambdaLocator
{

    public async ValueTask<ResolvedLambda?> LocateAsync(IRequest request)
    {
        var target = request.Header.Target;

        if (target.Current == null || !LambdaKeys.TryNormalize(target.Current.Value.Decode(), out var key, out _))
        {
            return null;
        }

        var lambda = await meta.ResolveAsync(key);

        if (lambda != null)
        {
            // the lambda routes on what comes after its key
            target.Advance();
        }

        return lambda;
    }

    public async ValueTask<IResponse> UnavailableAsync(IRequest request)
    {
        if (spa.PrefersMarkup(request))
        {
            return await spa.RenderAsync(request, ResponseStatus.NotFound);
        }

        var key = request.Header.Target.Current?.Decode();

        return request.Respond()
                      .Status(ResponseStatus.NotFound)
                      .Content($$"""{"error":"There is no lambda deployed at '{{key}}'."}""", ContentType.ApplicationJson)
                      .Build();
    }

}
