using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// Says of a stream of events that nothing in between may keep it back: no
/// cache, and no proxy buffering it - nginx holds a response back by default,
/// and the page would learn of a step a while after it happened.
/// </summary>
internal sealed class UnbufferedConcern(IHandler content) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var response = await content.HandleAsync(request);

        return response?.Rebuild()
                        .Header("Cache-Control", "no-cache")
                        .Header("X-Accel-Buffering", "no")
                        .Build();
    }

}
