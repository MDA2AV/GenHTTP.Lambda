using GenHTTP.Lambda.Integrations.Secrets;

namespace GenHTTP.Lambda.Services.Integrations;

/// <summary>
/// Tells which integrations the installation keeps its data in.
/// </summary>
/// <remarks>
/// Asked of the integrations each time rather than kept: what one says about
/// itself is cheap, and may say how it is doing.
/// </remarks>
public sealed class IntegrationService(PlatformIntegrations integrations, ISecretProvider secrets) : IIntegrationService
{

    public IReadOnlyList<IntegrationState> List() =>
    [
        new(integrations.Database.Describe(), !integrations.DatabaseChosen),
        new(integrations.Files.Describe(), !integrations.FilesChosen),
        new(secrets.Describe(), integrations.Secrets == null)
    ];

}
