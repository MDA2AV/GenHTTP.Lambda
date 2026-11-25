using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Puts a concern that needs nothing but services into a chain, taken from
/// the container of the platform the chain is built for.
/// </summary>
/// <remarks>
/// GenHTTP's own integration (<see cref="Dependent"/>) takes it from the scope
/// the host opens around a request, and a host opens one: for the container
/// added to it last. A platform may not be the only one of its server, so its
/// chains are bound to it as they are built, and ask nobody but it.
/// </remarks>
public static class ServiceConcern
{

    /// <summary>
    /// The concern of the given type.
    /// </summary>
    public static IConcernBuilder Of<T>(IServiceProvider services) where T : class, IDependentConcern => new Builder<T>(services);

    private sealed class Builder<T>(IServiceProvider services) : IConcernBuilder where T : class, IDependentConcern
    {
        public IConcern Build(IHandler content) => new Concern(content, services.GetRequiredService<T>());
    }

    private sealed class Concern(IHandler content, IDependentConcern concern) : IConcern
    {
        public IHandler Content => content;

        public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => concern.HandleAsync(content, request);
    }

}
