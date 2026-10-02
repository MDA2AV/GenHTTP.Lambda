using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// The published sources, packed: a folder per lambda below
/// <c>{data}/sources</c>, and in it a project per version, as the zip a
/// visitor downloads.
/// </summary>
/// <remarks>
/// A cache and nothing more - the versions are what the source is, and
/// anything missing here is packed from them again. A version never changes,
/// but what it is packed with can: the license, who holds the copyright, the
/// key the project is named after, and the packer itself with every release of
/// this server. So a project is named by its version and a fingerprint of all
/// of that, and packing a version again throws the one packed before away.
///
/// Packing reads a whole version and compresses it, which for a premium lambda
/// is a hundred megabytes, so it happens once per version however many ask at
/// the same moment, and only two at a time for the whole server. What the
/// cache holds is kept to <see cref="LambdaOptions.SourceCacheBytes" /> by
/// letting go of the projects read least recently.
/// </remarks>
public sealed class SourceCache
{

    /// <summary>
    /// Locks shared out by lambda and version, so that one version is packed
    /// once while others are packed beside it - a fixed number of them,
    /// because a lock per version ever asked for would only ever grow.
    /// </summary>
    private readonly SemaphoreSlim[] _versions = [.. Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1))];

    /// <summary>
    /// How many versions are packed at the same time, for the whole server.
    /// </summary>
    private readonly SemaphoreSlim _packing = new(2, 2);

    private readonly object _trimming = new();

    #region Get-/Setters

    private LambdaOptions Options { get; }

    private ILogger<SourceCache> Logger { get; }

    #endregion

    #region Initialization

    public SourceCache(LambdaOptions options, ILogger<SourceCache> logger)
    {
        Options = options;
        Logger = logger;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// The packed project of a version, packed first where it is not there yet.
    /// </summary>
    /// <param name="fingerprint">Everything the project is packed with besides the version, hashed</param>
    /// <param name="pack">Writes the project into the stream it is given - reading the version only then, when it is needed</param>
    /// <returns>Where the zip is</returns>
    public async ValueTask<string> GetAsync(long lambdaId, int version, string fingerprint, Func<Stream, ValueTask> pack,
                                            CancellationToken cancellation = default)
    {
        var directory = Directory(lambdaId);

        var file = Path.Combine(directory, $"v{version}-{fingerprint}.zip");

        if (Touch(file))
        {
            return file;
        }

        var gate = _versions[(int)((uint)HashCode.Combine(lambdaId, version) % _versions.Length)];

        await gate.WaitAsync(cancellation);

        try
        {
            // packed by whoever held the lock before
            if (Touch(file))
            {
                return file;
            }

            await _packing.WaitAsync(cancellation);

            try
            {
                System.IO.Directory.CreateDirectory(directory);

                // a name no reader asks for, until it is whole
                var partial = Path.Combine(directory, $".{Guid.NewGuid():N}.partial");

                try
                {
                    // off the thread the request came in on: compressing a
                    // hundred megabytes is not something to do in passing
                    await Task.Run(async () =>
                    {
                        await using var stream = new FileStream(partial, FileMode.CreateNew, FileAccess.Write, FileShare.None);

                        await pack(stream);
                    }, cancellation);

                    File.Move(partial, file, true);
                }
                catch
                {
                    Delete(partial);
                    throw;
                }
            }
            finally
            {
                _packing.Release();
            }

            // the same version packed before, under another license or key
            foreach (var stale in System.IO.Directory.EnumerateFiles(directory, $"v{version}-*.zip"))
            {
                if (!string.Equals(stale, file, StringComparison.Ordinal))
                {
                    Delete(stale);
                }
            }

            Logger.LogInformation("Packed source of lambda #{LambdaId} version {Version}", lambdaId, version);

            Trim(file);

            return file;
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>
    /// Lets go of everything packed for a lambda, whose source is no longer
    /// published.
    /// </summary>
    public void Remove(long lambdaId)
    {
        try
        {
            var directory = Directory(lambdaId);

            if (System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.Delete(directory, true);
            }
        }
        catch (IOException)
        {
            // being read right now; the budget lets go of it in time
        }
    }

    #endregion

    #region Helpers

    private string Directory(long lambdaId) => Path.Combine(Options.SourceDirectory, lambdaId.ToString());

    /// <summary>
    /// Whether the project is there, noting that it was asked for - which is
    /// what the budget lets go of the least recent by.
    /// </summary>
    private static bool Touch(string file)
    {
        try
        {
            if (!File.Exists(file))
            {
                return false;
            }

            File.SetLastAccessTimeUtc(file, DateTime.UtcNow);

            return true;
        }
        catch (IOException)
        {
            // there, even if it could not be marked
            return File.Exists(file);
        }
    }

    /// <summary>
    /// Holds the cache to its budget, letting go of the projects read least
    /// recently - but never the one just packed, which is about to be read.
    /// </summary>
    /// <remarks>
    /// A project being read while it is deleted is read to its end: the file
    /// stays until the last handle to it is closed.
    /// </remarks>
    private void Trim(string keep)
    {
        lock (_trimming)
        {
            var root = new DirectoryInfo(Options.SourceDirectory);

            if (!root.Exists)
            {
                return;
            }

            var projects = root.EnumerateFiles("*.zip", SearchOption.AllDirectories).ToList();

            var total = projects.Sum(p => p.Length);

            foreach (var project in projects.OrderBy(p => p.LastAccessTimeUtc))
            {
                if (total <= Options.SourceCacheBytes)
                {
                    break;
                }

                if (string.Equals(project.FullName, Path.GetFullPath(keep), StringComparison.Ordinal))
                {
                    continue;
                }

                total -= project.Length;

                Delete(project.FullName);
            }
        }
    }

    private static void Delete(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (IOException)
        {
            // being read; let go of the next time around
        }
    }

    #endregion

}
