using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// What the platform hands an integration as it is built.
/// </summary>
/// <param name="DataDirectory">The directory the installation keeps its data in, for whatever an integration keeps on the disk</param>
/// <param name="Loggers">Where the integration logs to, which is where the platform does</param>
public sealed record IntegrationContext(string DataDirectory, ILoggerFactory Loggers);

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
