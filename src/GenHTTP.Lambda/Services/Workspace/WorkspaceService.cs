using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// Reads and writes the private directory of a lambda on behalf of its owner.
/// </summary>
public sealed class WorkspaceService(IStorageService storage, IMetaService meta, LambdaOptions options, ILogger<WorkspaceService> logger) : IWorkspaceService
{

    #region Functionality

    public async ValueTask<WorkspaceListing> ListAsync(long lambdaId, CancellationToken cancellation = default)
    {
        var limits = await LimitsAsync(lambdaId, cancellation);

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

        return new WorkspaceListing(files, folders, used, limits.Quota, limits.MaxFiles, limits.MaxFileSize);
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
        var limits = await LimitsAsync(lambdaId, cancellation);

        var root = Root(lambdaId);

        var resolved = Resolve(root, path);

        var existing = File.Exists(resolved);

        var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories);

        if (!existing && files.Length >= limits.MaxFiles)
        {
            throw LambdaException.Invalid($"A workspace must not hold more than {limits.MaxFiles} files.");
        }

        // what the rest of the workspace takes already - the file being
        // replaced is not counted, since it will not be there beside this one
        var others = files.Where(f => f != resolved).Sum(Size);

        // never less than what is being replaced: a workspace is over its
        // quota once its lambda leaves the tier that filled it, and should
        // still be able to rewrite what it holds, only not to grow
        var room = Math.Max(limits.Quota - others, existing ? Size(resolved) : 0);

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
                await CopyAsync(content, target, limits, room, cancellation);
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

    public async ValueTask CreateFolderAsync(long lambdaId, string path, CancellationToken cancellation = default)
    {
        var limits = await LimitsAsync(lambdaId, cancellation);

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
         && Directory.GetDirectories(root, "*", SearchOption.AllDirectories).Length >= limits.MaxFiles)
        {
            throw LambdaException.Invalid($"A workspace must not hold more than {limits.MaxFiles} folders.");
        }

        Directory.CreateDirectory(resolved);

        logger.LogInformation("Workspace of lambda {LambdaId} gained folder '{Path}'", lambdaId, path);
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
    /// <param name="room">How large the file may grow before the workspace is past its quota</param>
    private static async ValueTask CopyAsync(Stream content, Stream target, WorkspaceLimits limits, long room, CancellationToken cancellation)
    {
        var buffer = new byte[81920];

        var total = 0L;

        int read;

        while ((read = await content.ReadAsync(buffer, cancellation)) > 0)
        {
            total += read;

            if (total > limits.MaxFileSize)
            {
                throw LambdaException.Invalid($"A workspace file must not exceed {limits.MaxFileSize} bytes.");
            }

            if (total > room)
            {
                throw LambdaException.Invalid($"A workspace must not hold more than {limits.Quota} bytes.");
            }

            await target.WriteAsync(buffer.AsMemory(0, read), cancellation);
        }
    }

    /// <summary>
    /// What the workspace of the lambda may hold, which its tier decides.
    /// </summary>
    /// <remarks>
    /// Asked every time rather than remembered, so a lambda that was just
    /// moved to another tier is held to the limits of the new one - the same
    /// ones its own code will be compiled with on its next request.
    /// </remarks>
    private async ValueTask<WorkspaceLimits> LimitsAsync(long lambdaId, CancellationToken cancellation)
        => options.WorkspaceOf(await meta.GetTierAsync(lambdaId, cancellation) ?? LambdaTier.Free);

    private static long Size(string path)
    {
        try
        {
            return new FileInfo(path).Length;
        }
        catch (IOException)
        {
            // removed since the directory was read
            return 0;
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
