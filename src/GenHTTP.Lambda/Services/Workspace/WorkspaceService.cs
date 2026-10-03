using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// Reads and writes the private directory of a lambda on behalf of its owner -
/// or a feature's copy of it, which is held to the same quota.
/// </summary>
public sealed class WorkspaceService(IStorageService storage, IMetaService meta, ILimitsService tiers, ILogger<WorkspaceService> logger) : IWorkspaceService
{

    /// <summary>
    /// The largest file that travels as base64 in a JSON document.
    /// </summary>
    /// <remarks>
    /// That way a file is in memory several times over while the request runs
    /// - its bytes, the text they become, the characters of that text. The
    /// workspace does not limit how large one file may be, only the room they
    /// take together, so anything larger is sent as it is (see
    /// <see cref="Find"/>), which streams the bytes.
    /// </remarks>
    private const long EncodedLimit = 32 * 1024 * 1024;

    #region Functionality

    public WorkspaceListing List(long lambdaId, long? featureId = null)
    {
        var limits = Limits(lambdaId);

        // switched off, it holds nothing - and looking is no reason to make
        // the directory it would have
        if (!limits.Enabled)
        {
            return new WorkspaceListing([], [], 0, limits.Quota, false);
        }

        var root = Root(lambdaId, featureId);

        var files = new List<WorkspaceEntry>();

        var used = 0L;

        foreach (var path in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        {
            var info = new FileInfo(path);

            files.Add(new WorkspaceEntry(Relative(root, path), info.Length, info.LastWriteTimeUtc));

            used += WorkspaceLimits.Footprint(info.Length);
        }

        files.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));

        var folders = new List<string>();

        foreach (var path in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
        {
            folders.Add(Relative(root, path));

            used += WorkspaceLimits.Block;
        }

        folders.Sort(string.CompareOrdinal);

        return new WorkspaceListing(files, folders, used, limits.Quota);
    }

    public FileInfo? Find(long lambdaId, string path, long? featureId = null)
    {
        var resolved = Resolve(Root(lambdaId, featureId), path);

        return File.Exists(resolved) ? new FileInfo(resolved) : null;
    }

    public async ValueTask<WorkspaceContent?> ReadAsync(long lambdaId, string path, long? featureId = null, CancellationToken cancellation = default)
    {
        var root = Root(lambdaId, featureId);

        var resolved = Resolve(root, path);

        if (!File.Exists(resolved))
        {
            return null;
        }

        return new WorkspaceContent(Relative(root, resolved), await File.ReadAllBytesAsync(resolved, cancellation));
    }

    public async ValueTask<WorkspaceEncoded?> ReadEncodedAsync(long lambdaId, string path, long? featureId = null, CancellationToken cancellation = default)
    {
        var root = Root(lambdaId, featureId);

        var resolved = Resolve(root, path);

        if (!File.Exists(resolved))
        {
            return null;
        }

        var length = Size(resolved);

        if (length > EncodedLimit)
        {
            return new WorkspaceEncoded(Relative(root, resolved), length, null);
        }

        var content = await File.ReadAllBytesAsync(resolved, cancellation);

        return new WorkspaceEncoded(Relative(root, resolved), content.Length, Convert.ToBase64String(content));
    }

    public async ValueTask<WorkspaceEntry> WriteAsync(long lambdaId, string path, Stream content, long? expected = null, long? featureId = null,
                                                      CancellationToken cancellation = default)
    {
        var limits = RequireEnabled(lambdaId);

        var root = Root(lambdaId, featureId);

        var resolved = Resolve(root, path);

        var existing = File.Exists(resolved);

        var replaced = existing ? WorkspaceLimits.Footprint(Size(resolved)) : 0;

        // the folders this file goes into that are not there yet take room too
        var missing = Missing(root, resolved);

        // what the rest of the workspace takes already - the file being
        // replaced is not counted, since it will not be there beside this one
        var others = Used(root) - replaced + missing.Count * WorkspaceLimits.Block;

        // never less than what is being replaced: a workspace is over its
        // quota once its lambda leaves the tier that filled it, and should
        // still be able to rewrite what it holds, only not to grow
        var room = Math.Max(limits.Quota - others, replaced);

        // written through a temporary file so a rejected upload cannot leave a
        // half written one behind in place of what was there - nor the folders
        // it would have gone into
        var staging = resolved + ".uploading";

        try
        {
            foreach (var folder in missing)
            {
                Directory.CreateDirectory(folder);
            }

            long received;

            await using (var target = File.Create(staging))
            {
                received = await CopyAsync(content, target, limits, room, cancellation);
            }

            // a connection the engine dropped looks like a body that ended -
            // kept, it would be a truncated file passing for the whole one
            if (expected is { } length && received != length)
            {
                throw LambdaException.Invalid($"The upload ended after {received:N0} of {length:N0} bytes and was not kept.");
            }

            File.Move(staging, resolved, true);
        }
        catch (Exception)
        {
            Delete(staging);

            foreach (var folder in missing.AsEnumerable().Reverse())
            {
                RemoveIfEmpty(folder);
            }

            throw;
        }

        if (featureId != null)
        {
            logger.LogInformation("Wrote workspace file {Path} of feature #{FeatureId} of lambda #{LambdaId}", path, featureId, lambdaId);
        }
        else
        {
            logger.LogInformation("Wrote workspace file {Path} of lambda #{LambdaId}", path, lambdaId);
        }

        var info = new FileInfo(resolved);

        return new WorkspaceEntry(Relative(root, resolved), info.Length, info.LastWriteTimeUtc);
    }

    public async ValueTask<WorkspaceEntry> WriteEncodedAsync(long lambdaId, string path, string? content, long? featureId = null,
                                                             CancellationToken cancellation = default)
    {
        byte[] bytes;

        try
        {
            bytes = Convert.FromBase64String(content ?? string.Empty);
        }
        catch (FormatException)
        {
            throw LambdaException.Invalid("The content of a file has to be base64 encoded.");
        }

        using var stream = new MemoryStream(bytes);

        return await WriteAsync(lambdaId, path, stream, featureId: featureId, cancellation: cancellation);
    }

    public void CreateFolder(long lambdaId, string path, long? featureId = null)
    {
        var limits = RequireEnabled(lambdaId);

        var root = Root(lambdaId, featureId);

        var resolved = Resolve(root, path);

        if (File.Exists(resolved))
        {
            throw LambdaException.Invalid("There is already a file with that name.");
        }

        /*
         * A folder takes a block of the quota even though it holds nothing. It
         * is a thing on the disk, and making a thousand of them is the same
         * nuisance as making a thousand empty files, which counting in blocks
         * exists to stop.
         */
        if (!Directory.Exists(resolved)
         && Used(root) + (Missing(root, resolved).Count + 1) * WorkspaceLimits.Block > limits.Quota)
        {
            throw LambdaException.Invalid($"A workspace must not hold more than {limits.Quota} bytes.");
        }

        Directory.CreateDirectory(resolved);

        if (featureId != null)
        {
            logger.LogInformation("Created workspace folder {Path} of feature #{FeatureId} of lambda #{LambdaId}", path, featureId, lambdaId);
        }
        else
        {
            logger.LogInformation("Created workspace folder {Path} of lambda #{LambdaId}", path, lambdaId);
        }
    }

    public void Delete(long lambdaId, string path, long? featureId = null)
    {
        var resolved = Resolve(Root(lambdaId, featureId), path);

        Delete(resolved);
    }

    public void Clear(long lambdaId, long? featureId = null)
    {
        var root = Root(lambdaId, featureId);

        foreach (var entry in Directory.GetFileSystemEntries(root))
        {
            Delete(entry);
        }

        if (featureId != null)
        {
            logger.LogInformation("Cleared workspace of feature #{FeatureId} of lambda #{LambdaId}", featureId, lambdaId);
        }
        else
        {
            logger.LogInformation("Cleared workspace of lambda #{LambdaId}", lambdaId);
        }
    }

    /// <summary>
    /// Copies the upload over, refusing it the moment it grows past what a
    /// lambda would be allowed to write itself.
    /// </summary>
    /// <param name="room">How large the file may grow before the workspace is past its quota</param>
    /// <returns>How many bytes arrived</returns>
    private static async ValueTask<long> CopyAsync(Stream content, Stream target, WorkspaceLimits limits, long room, CancellationToken cancellation)
    {
        var buffer = new byte[81920];

        var total = 0L;

        int read;

        while ((read = await content.ReadAsync(buffer, cancellation)) > 0)
        {
            total += read;

            if (WorkspaceLimits.Footprint(total) > room)
            {
                throw LambdaException.Invalid($"A workspace must not hold more than {limits.Quota} bytes.");
            }

            await target.WriteAsync(buffer.AsMemory(0, read), cancellation);
        }

        // an empty file still takes a block
        if (WorkspaceLimits.Footprint(total) > room)
        {
            throw LambdaException.Invalid($"A workspace must not hold more than {limits.Quota} bytes.");
        }

        return total;
    }

    /// <summary>
    /// The room everything in the workspace takes, counted as the quota is.
    /// </summary>
    private static long Used(string root)
    {
        var used = 0L;

        foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        {
            used += WorkspaceLimits.Footprint(Size(file));
        }

        return used + Directory.GetDirectories(root, "*", SearchOption.AllDirectories).LongLength * WorkspaceLimits.Block;
    }

    /// <summary>
    /// The folders between the root and a path that are not there yet,
    /// outermost first.
    /// </summary>
    private static List<string> Missing(string root, string path)
    {
        var missing = new List<string>();

        var directory = Path.GetDirectoryName(path);

        while (directory != null && directory.Length >= root.Length && !Directory.Exists(directory))
        {
            missing.Insert(0, directory);

            directory = Path.GetDirectoryName(directory);
        }

        return missing;
    }

    private static void RemoveIfEmpty(string folder)
    {
        try
        {
            if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
            {
                Directory.Delete(folder);
            }
        }
        catch (IOException)
        {
            // something arrived in it meanwhile, which may keep it
        }
    }

    /// <summary>
    /// What the workspace of the lambda may hold, which its tier decides - and
    /// whether it has one, which its owner does.
    /// </summary>
    /// <remarks>
    /// Asked every time rather than remembered, so a lambda that was just
    /// moved to another tier is held to the limits of the new one - the same
    /// ones its own code will be compiled with on its next request.
    /// </remarks>
    private WorkspaceLimits Limits(long lambdaId)
        => meta.GetWorkspaceLimits(lambdaId) ?? tiers.WorkspaceOf(LambdaTier.Free);

    /// <summary>
    /// The limits of a workspace that is switched on, or a refusal saying how
    /// to switch it on.
    /// </summary>
    /// <remarks>
    /// Refused here as the lambda itself refuses it, so a file uploaded by
    /// hand cannot land where the lambda has been told there is nothing.
    /// </remarks>
    private WorkspaceLimits RequireEnabled(long lambdaId)
    {
        var limits = Limits(lambdaId);

        return limits.Enabled ? limits : throw LambdaException.Conflict(DataKinds.WorkspaceOff);
    }

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

    private string Root(long lambdaId, long? featureId)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(storage.GetWorkspace(lambdaId, featureId))) + Path.DirectorySeparatorChar;

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

        var resolved = Path.GetFullPath(Path.Combine(root, path));

        if (!resolved.StartsWith(root, StringComparison.Ordinal))
        {
            throw LambdaException.Invalid($"'{path}' is outside of the workspace of this lambda.");
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
