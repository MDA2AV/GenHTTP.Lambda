using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// What the platform hands an integration as it is built.
/// </summary>
/// <param name="DataDirectory">The directory the installation keeps its data in, for whatever an integration keeps on the disk</param>
/// <param name="Loggers">Where the integration logs to, which is where the platform does</param>
public sealed record IntegrationContext(string DataDirectory, ILoggerFactory Loggers);
