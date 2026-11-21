using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Adds the same headers to every answer.
/// </summary>
public sealed class HeadersConcern(IHandler content, IReadOnlyList<(string Name, string Value)> headers) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var response = await content.HandleAsync(request);

        if (response != null)
        {
            var rebuilt = response.Rebuild();

            foreach (var (name, value) in headers)
            {
                rebuilt.Header(name, value);
            }
        }

        return response;
    }

}
