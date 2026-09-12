using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// Reads the public key from the path, looks up the lambda behind it and puts
/// it into the request, so the handler below only has to run it.
/// </summary>
/// <remarks>
/// Requests for a key that does not exist (or is not deployed) never reach the
/// execution layer - browsers are sent to the single page application, which
/// explains the situation and offers to create a lambda.
/// </remarks>
public sealed class LambdaResolutionConcern(IHandler content, IMetaService meta, SpaResources spa) : IConcern
{

    #region Get-/Setters

    public IHandler Content => content;

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Header.Target;

        if (target.Current == null)
        {
            return await Unavailable(request, null);
        }

        var key = target.Current.Value.Decode();

        ResolvedLambda? lambda = null;

        if (LambdaKeys.TryNormalize(key, out var normalized, out _))
        {
            lambda = await meta.ResolveAsync(normalized);
        }

        if (lambda == null)
        {
            return await Unavailable(request, key);
        }

        request.SetLambda(lambda);

        target.Advance();

        return await content.HandleAsync(request);
    }

    private async ValueTask<IResponse> Unavailable(IRequest request, string? key)
    {
        if (spa.PrefersMarkup(request))
        {
            return await spa.RenderAsync(request, ResponseStatus.NotFound);
        }

        return request.Respond()
                      .Status(ResponseStatus.NotFound)
                      .Content($$"""{"error":"There is no lambda deployed at '{{key}}'."}""", ContentType.ApplicationJson)
                      .Build();
    }

    #endregion

}

public sealed class LambdaResolutionConcernBuilder(IMetaService meta, SpaResources spa) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new LambdaResolutionConcern(content, meta, spa);
}
