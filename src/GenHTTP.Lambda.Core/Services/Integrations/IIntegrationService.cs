using GenHTTP.Lambda.Integrations;

namespace GenHTTP.Lambda.Services.Integrations;

/// <summary>
/// Which integrations the installation keeps its data in, as its operator and
/// its usage report are told.
/// </summary>
public interface IIntegrationService
{

    /// <summary>
    /// Every integration in use - the database, the files, the secrets, in
    /// that order.
    /// </summary>
    IReadOnlyList<IntegrationState> List();

}

/// <param name="Info">What the integration says about itself</param>
/// <param name="Assembly">The assembly it is in, which names it: what a host references to use it</param>
public sealed record IntegrationState(IntegrationInfo Info, string Assembly)
{

    public static IntegrationState Of(IIntegration integration)
        => new(integration.Describe(), integration.GetType().Assembly.GetName().Name ?? integration.GetType().FullName ?? integration.GetType().Name);

}
