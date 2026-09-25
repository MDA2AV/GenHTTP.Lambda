using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// Checks the administration token before a request reaches the resource
/// behind it.
/// </summary>
/// <remarks>
/// In front of the resource rather than inside each of its methods, because
/// a method that takes a body cannot read a header: the framework releases the
/// headers of a request once its body has been bound, which happens before the
/// method runs. Here nothing has been read yet.
/// </remarks>
public sealed class AdminGateConcern(IHandler content, LambdaOptions options) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        AdminGate.Require(request, options);

        return content.HandleAsync(request);
    }

}

public sealed class AdminGateConcernBuilder(LambdaOptions options) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new AdminGateConcern(content, options);
}
