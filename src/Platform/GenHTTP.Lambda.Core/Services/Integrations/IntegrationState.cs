using GenHTTP.Lambda.Integrations;

namespace GenHTTP.Lambda.Services.Integrations;

/// <param name="Info">What the integration says about itself</param>
/// <param name="Assembly">The assembly it is in, which names it: what a host references to use it</param>
public sealed record IntegrationState(IntegrationInfo Info, string Assembly)
{

    public static IntegrationState Of(IIntegration integration)
        => new(integration.Describe(), integration.GetType().Assembly.GetName().Name ?? integration.GetType().FullName ?? integration.GetType().Name);

}
