using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// Reads and writes the private directory of a lambda on behalf of its owner.
/// </summary>
public sealed class WorkspaceService(IStorageService storage, ILogger<WorkspaceService> logger) : IWorkspaceService
{

    #region Functionality

    public ValueTask<WorkspaceListing> ListAsync(long lambdaId, CancellationToken cancellation = default)
    {
        var root = Root(lambdaId);

        var files = new List<WorkspaceEntry>();

        var used = 0L;

        foreach (var path in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        {
            var info = new FileInfo(path);

            files.Add(new WorkspaceEntry(Relative(root, path), info.Length, info.LastWriteTimeUtc));

            used += info.Length;
        }

        files.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));

        var folders = new List<string>();

        foreach (var path in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
        {
            folders.Add(Relative(root, path));
        }

        folders.Sort(string.CompareOrdinal);

        return ValueTask.FromResult(new WorkspaceListing(files, folders, used, WorkspaceLimits.Quota,
                                                         WorkspaceLimits.MaxFiles, WorkspaceLimits.MaxFileSize));
    }

    public async ValueTask<WorkspaceContent?> ReadAsync(long lambdaId, string path, CancellationToken cancellation = default)
    {
        var root = Root(lambdaId);

        var resolved = Resolve(root, path);

        if (!File.Exists(resolved))
        {
            return null;
        }

        return new WorkspaceContent(Relative(root, resolved), await File.ReadAllBytesAsync(resolved, cancellation));
    }

    public async ValueTask<WorkspaceEntry> WriteAsync(long lambdaId, string path, Stream content, CancellationToken cancellation = default)
    {
        var root = Root(lambdaId);

        var resolved = Resolve(root, path);

        var existing = File.Exists(resolved);

        if (!existing && Directory.GetFiles(root, "*", SearchOption.AllDirectories).Length >= WorkspaceLimits.MaxFiles)
        {
            throw LambdaException.Invalid($"A workspace must not hold more than {WorkspaceLimits.MaxFiles} files.");
        }

        var directory = Path.GetDirectoryName(resolved);

        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        // written through a temporary file so a rejected upload cannot leave a
        // half written one behind in place of what was there
        var staging = resolved + ".uploading";

        try
        {
            await using (var target = File.Create(staging))
            {
                await CopyAsync(content, target, cancellation);
            }

            File.Move(staging, resolved, true);
        }
        catch (Exception)
        {
            Delete(staging);
            throw;
        }

        logger.LogInformation("Workspace of lambda {LambdaId} received '{Path}'", lambdaId, path);

        var info = new FileInfo(resolved);

        return new WorkspaceEntry(Relative(root, resolved), info.Length, info.LastWriteTimeUtc);
    }

    public ValueTask CreateFolderAsync(long lambdaId, string path, CancellationToken cancellation = default)
    {
        var root = Root(lambdaId);

        var resolved = Resolve(root, path);

        if (File.Exists(resolved))
        {
            throw LambdaException.Invalid("There is already a file with that name.");
        }

        /*
         * Counted against the file limit even though it holds none. A folder
         * is a thing on the disk and making a thousand of them is the same
         * nuisance as making a thousand empty files, which the limit exists
         * to stop.
         */
        if (!Directory.Exists(resolved)
         && Directory.GetDirectories(root, "*", SearchOption.AllDirectories).Length >= WorkspaceLimits.MaxFiles)
        {
            throw LambdaException.Invalid($"A workspace must not hold more than {WorkspaceLimits.MaxFiles} folders.");
        }

        Directory.CreateDirectory(resolved);

        logger.LogInformation("Workspace of lambda {LambdaId} gained folder '{Path}'", lambdaId, path);

        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(long lambdaId, string path, CancellationToken cancellation = default)
    {
        var resolved = Resolve(Root(lambdaId), path);

        Delete(resolved);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Copies the upload over, refusing it the moment it grows past what a
    /// lambda would be allowed to write itself.
    /// </summary>
    private static async ValueTask CopyAsync(Stream content, Stream target, CancellationToken cancellation)
    {
        var buffer = new byte[81920];

        var total = 0L;

        int read;

        while ((read = await content.ReadAsync(buffer, cancellation)) > 0)
        {
            total += read;

            if (total > WorkspaceLimits.MaxFileSize)
            {
                throw LambdaException.Invalid($"A workspace file must not exceed {WorkspaceLimits.MaxFileSize} bytes.");
            }

            await target.WriteAsync(buffer.AsMemory(0, read), cancellation);
        }
    }

    private string Root(long lambdaId)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(storage.GetWorkspace(lambdaId))) + Path.DirectorySeparatorChar;

    /// <summary>
    /// Turns a requested name into a path inside the workspace, or refuses it.
    /// </summary>
    /// <remarks>
    /// The same check the generated workspace class makes, for the same reason:
    /// the name arrives from outside, and "../../etc/passwd" is a name.
    /// </remarks>
    private static string Resolve(string root, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw LambdaException.Invalid("The name of a workspace file must not be empty.");
        }

        // query values arrive exactly as they were sent, so "%2F" is still four
        // characters here rather than a separator. Decoding before the check
        // below is what makes that check mean anything: an encoded "../" that
        // stayed encoded would be a legal file name rather than a refusal.
        var decoded = Uri.UnescapeDataString(path);

        var resolved = Path.GetFullPath(Path.Combine(root, decoded));

        if (!resolved.StartsWith(root, StringComparison.Ordinal))
        {
            throw LambdaException.Invalid($"'{decoded}' is outside of the workspace of this lambda.");
        }

        return resolved;
    }

    private static string Relative(string root, string path)
        => path[root.Length..].Replace('\\', '/');

    private static void Delete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                // with everything in it: the editor asks before it gets here,
                // and a folder that refuses to go while it holds something is
                // a folder somebody has to empty by hand one file at a time
                Directory.Delete(path, true);
            }
        }
        catch (Exception)
        {
            // a file that cannot be removed is not worth failing the request over
        }
    }

    #endregion

}
