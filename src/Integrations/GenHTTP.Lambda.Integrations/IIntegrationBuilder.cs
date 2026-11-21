using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// Configures an integration, which the platform builds once it knows where
/// its data is.
/// </summary>
/// <remarks>
/// A host makes the builder, with what only the host knows - a connection
/// string, an address - and hands it to the platform's builder. Building
/// configures and reaches nothing, as a GenHTTP handler's builder does: what
/// talks to the infrastructure - checking that it answers, preparing what it
/// needs - is <see cref="IIntegration.PrepareAsync"/>, which the platform
/// awaits as it starts, before anything is migrated.
/// </remarks>
public interface IIntegrationBuilder<out T> where T : IIntegration
{

    T Build(IntegrationContext context);

}
