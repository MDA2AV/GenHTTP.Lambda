using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Integrations.Secrets;

namespace GenHTTP.Lambda.Services.Integrations;

/// <summary>
/// What the platform keeps its data in, as its host chose it.
/// </summary>
/// <remarks>
/// Built before the container - the database has to be migrated before the
/// services start - so the platform lets go of them itself.
/// </remarks>
/// <param name="Secrets">What the host chose for the secrets, or nothing where the platform keeps them itself</param>
public sealed record PlatformIntegrations(IDatabaseProvider Database, IFileProvider Files, ISecretProvider? Secrets) : IAsyncDisposable
{

    private IReadOnlyList<IIntegration> Built => Secrets is { } secrets ? [Database, Files, secrets] : [Database, Files];

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
