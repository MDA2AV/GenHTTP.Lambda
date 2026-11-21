using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Hosting;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// Sends <c>/admin/mcp</c> to the operator's endpoint and everything else
/// below <c>/admin</c> to the panel, as it came.
/// </summary>
/// <param name="content">The panel</param>
/// <param name="endpoint">The operator's endpoint, or nothing on an installation without an operator</param>
internal sealed class OperatorRoute(IHandler content, IHandler? endpoint) : IConcern
{
    private static readonly PathSegment Mcp = new(PlatformRoutes.Mcp);

    #region Get-/Setters

    public IHandler Content => content;

    #endregion

    #region Functionality

    public async ValueTask PrepareAsync(IServer server)
    {
        await content.PrepareAsync(server);

        if (endpoint != null)
        {
            await endpoint.PrepareAsync(server);
        }
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Header.Target;

        if (target.Current is not { } segment || !segment.Equals(Mcp))
        {
            return content.HandleAsync(request);
        }

        if (endpoint == null)
        {
            return new((IResponse?)null);
        }

        target.Advance();

        return endpoint.HandleAsync(request);
    }

    #endregion

}
