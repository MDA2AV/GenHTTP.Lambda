using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Hands the running server to the deployment service, which prepares every
/// handler it compiles against it.
/// </summary>
/// <remarks>
/// For that alone. Whatever answers a request has the server already, as
/// <c>IRequest.Server</c>, and takes it from there. A handler is compiled by
/// the services, which are handed no request, and sometimes there is none to
/// hand: the seeder compiles the demos once the server is up, and a merge or
/// a deployment puts a version online from the service that was asked. So the
/// server is captured here while the chain is prepared, before anything can
/// be compiled.
/// </remarks>
public sealed class ServerRegistry
{

    /// <summary>
    /// The server instance, available as soon as the handler chain has been prepared.
    /// </summary>
    public IServer? Instance { get; private set; }

    public IServer Require() => Instance ?? throw new InvalidOperationException("The server is not running yet.");

    internal void Register(IServer server) => Instance = server;

    /// <summary>
    /// A concern that captures the server instance while the chain is prepared.
    /// </summary>
    public IConcernBuilder Capture() => new RegistrationConcernBuilder(this);

    private sealed class RegistrationConcernBuilder(ServerRegistry registry) : IConcernBuilder
    {
        public IConcern Build(IHandler content) => new RegistrationConcern(content, registry);
    }

    private sealed class RegistrationConcern(IHandler content, ServerRegistry registry) : IConcern
    {
        public IHandler Content => content;

        public ValueTask PrepareAsync(IServer server)
        {
            registry.Register(server);

            return content.PrepareAsync(server);
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request) => content.HandleAsync(request);
    }

}
