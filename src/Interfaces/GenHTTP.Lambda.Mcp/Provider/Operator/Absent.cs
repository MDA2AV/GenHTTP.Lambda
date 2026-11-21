using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// The operator's endpoint on an installation that has no operator: nothing
/// is found there.
/// </summary>
/// <remarks>
/// Answering nothing rather than a response of its own, so the request is
/// not found the way every other path nobody serves is - and it is not handed
/// on to what is mounted beside it, the panel's pages, which would answer
/// with a page.
/// </remarks>
internal sealed class Absent : IHandler
{

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request) => new((IResponse?)null);

}
