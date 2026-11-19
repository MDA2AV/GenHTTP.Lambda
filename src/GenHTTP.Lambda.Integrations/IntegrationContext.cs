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
/// string, an address - and hands it to the platform's builder; the platform
/// builds the integration before anything is migrated, and disposes of it
/// with itself where it can be.
///
/// Building may reach the infrastructure - check that it answers, prepare
/// what it needs - so it is awaited, as the platform starts.
/// </remarks>
public interface IIntegrationBuilder<T> where T : IIntegration
{

    ValueTask<T> BuildAsync(IntegrationContext context, CancellationToken cancellation = default);

}
