using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Hands the running server instance to the services that need it - mainly the
/// deployment service, which has to prepare the handlers it compiles.
/// </summary>
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
