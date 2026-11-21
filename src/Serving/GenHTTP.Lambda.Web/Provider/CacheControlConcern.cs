using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Puts a Cache-Control header on whatever the wrapped handler answers.
///
/// GenHTTP has no such concern of its own - its cache policy sets Expires and
/// nothing else - and without the header a browser caches by its own
/// heuristics. For the index page that is the difference between working and
/// broken: a stale copy asks for the bundle hash it was built with, and once
/// that file is gone the application never starts.
/// </summary>
public sealed class CacheControlConcern : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private string Value { get; }

    #endregion

    #region Initialization

    public CacheControlConcern(IHandler content, string value)
    {
        Content = content;
        Value = value;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var response = await Content.HandleAsync(request);

        response?.Rebuild()
                 .Header("Cache-Control", Value);

        return response;
    }

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    #endregion

}
