using System.IO.Compression;
using System.Text;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// The files of an archive that was sent, each waiting in a file of its own
/// beside the stores until the lambda keeps it - which moves it in.
/// </summary>
/// <remarks>
/// Disposed of once the request is answered: what was kept was moved, and
/// what is left - an archive that was refused, a file the lambda had - goes.
/// </remarks>
public sealed class StagedFiles : IDisposable
{

    private StagedFiles(string folder)
    {
        Folder = folder;
    }

    /// <summary>
    /// The folder they wait in, one per archive.
    /// </summary>
    public string Folder { get; }

    /// <summary>
    /// The files, in the order of a lambda.
    /// </summary>
    public IReadOnlyList<VersionFile> Files { get; internal set; } = [];

    internal static StagedFiles In(string staging)
    {
        var folder = Path.Combine(staging, Guid.NewGuid().ToString("n"));

        Directory.CreateDirectory(folder);

        return new StagedFiles(folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Folder, true);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // still open somewhere: the next start empties the staging
        }
    }

}
