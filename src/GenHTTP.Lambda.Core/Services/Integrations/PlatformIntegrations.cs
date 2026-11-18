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
/// <param name="DatabaseChosen">Whether the host chose the database rather than leaving the platform's own</param>
/// <param name="FilesChosen">Whether the host chose where the files are rather than leaving the platform's own</param>
public sealed record PlatformIntegrations(IDatabaseProvider Database, IFileProvider Files, ISecretProvider? Secrets, bool DatabaseChosen, bool FilesChosen)
    : IAsyncDisposable, IDisposable
{

    private IEnumerable<IIntegration> Built => Secrets is { } secrets ? [Database, Files, secrets] : [Database, Files];

    public void Dispose() => Dispose(Built);

    public async ValueTask DisposeAsync()
    {
        foreach (var integration in Built)
        {
            switch (integration)
            {
                case IAsyncDisposable asynchronous:
                    await asynchronous.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }

    internal static void Dispose(IEnumerable<IIntegration> integrations)
    {
        foreach (var integration in integrations)
        {
            (integration as IDisposable)?.Dispose();
        }
    }

}
