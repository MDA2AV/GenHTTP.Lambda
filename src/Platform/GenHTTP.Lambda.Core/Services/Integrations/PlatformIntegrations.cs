using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Integrations.Secrets;

namespace GenHTTP.Lambda.Services.Integrations;

/// <summary>
/// What the platform keeps its data in, and the agent it builds with, as its
/// host chose them.
/// </summary>
/// <remarks>
/// Built before the container - the database has to be migrated before the
/// services start - so the platform lets go of them itself.
/// </remarks>
/// <param name="Secrets">What the host chose for the secrets, or nothing where the platform keeps them itself</param>
/// <param name="Agent">The agent that builds and changes apps for the people who ask on the installation's pages, or nothing where there is none</param>
public sealed record PlatformIntegrations(IDatabaseProvider Database, IFileProvider Files, ISecretProvider? Secrets, IAgentProvider? Agent = null) : IAsyncDisposable
{

    private IReadOnlyList<IIntegration> Built => [Database, Files, .. Secrets is { } secrets ? [secrets] : Array.Empty<IIntegration>(),
                                                  .. Agent is { } agent ? [agent] : Array.Empty<IIntegration>()];

    public ValueTask DisposeAsync() => DisposeAsync(Built);

    /// <summary>
    /// Lets go of integrations in the order opposite to the one they were
    /// built in, as what came later may lean on what came before.
    /// </summary>
    internal static async ValueTask DisposeAsync(IReadOnlyList<IIntegration> integrations)
    {
        for (var i = integrations.Count - 1; i >= 0; i--)
        {
            await integrations[i].DisposeAsync();
        }
    }

}
