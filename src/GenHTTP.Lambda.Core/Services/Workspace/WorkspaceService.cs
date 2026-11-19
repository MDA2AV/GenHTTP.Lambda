using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// Reads and writes the files of a lambda on behalf of its owner - or a
/// feature's copy of them, which is held to the same quota.
/// </summary>
/// <remarks>
/// The quota is the room of the lambda's data, which its database shares:
/// what the database takes is not there for the workspace. Names and room are
/// worked out as they are for what the lambda writes itself (see
/// <see cref="WorkspacePaths"/>); where the files are kept is the files
/// integration's.
/// </remarks>
public sealed class WorkspaceService(IFileProvider files, IMetaService meta, ILimitsService tiers, IWorkspaceVault vault, IDatabaseVault databases,
                                    ILogger<WorkspaceService> logger) : IWorkspaceService
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
        // anything it would have
        if (!limits.Enabled)
        {
            return new WorkspaceListing([], [], 0, Room(limits, lambdaId, featureId), false);
        }

        var entries = files.List(new DataScope(lambdaId, featureId));

        var listed = entries.Where(e => !e.Folder)
                            .Select(e => new WorkspaceEntry(e.Path, e.Length, e.Modified))
                            .OrderBy(e => e.Path, StringComparer.Ordinal)
                            .ToList();

        var folders = entries.Where(e => e.Folder)
                             .Select(e => e.Path)
                             .Order(StringComparer.Ordinal)
                             .ToList();

        return new WorkspaceListing(listed, folders, WorkspacePaths.Measure(entries), Room(limits, lambdaId, featureId));
    }

    public WorkspaceFile? Find(long lambdaId, string path, long? featureId = null)
    {
        var resolved = Resolve(path);

        return files.Resource(new DataScope(lambdaId, featureId), resolved) is { } content ? new WorkspaceFile(NameOf(resolved), content) : null;
    }

    public ValueTask<WorkspaceContent?> ReadAsync(long lambdaId, string path, long? featureId = null, CancellationToken cancellation = default)
    {
        var resolved = Resolve(path);

        var content = files.Read(new DataScope(lambdaId, featureId), resolved);

        return ValueTask.FromResult(content == null ? null : new WorkspaceContent(resolved, content));
    }

    public ValueTask<WorkspaceEncoded?> ReadEncodedAsync(long lambdaId, string path, long? featureId = null, CancellationToken cancellation = default)
    {
        var scope = new DataScope(lambdaId, featureId);

        var resolved = Resolve(path);

        if (files.Find(scope, resolved) is not { Folder: false } found)
        {
            return ValueTask.FromResult<WorkspaceEncoded?>(null);
        }

        if (found.Length > EncodedLimit)
        {
            return ValueTask.FromResult<WorkspaceEncoded?>(new WorkspaceEncoded(resolved, found.Length, null));
        }

        var content = files.Read(scope, resolved);

        return ValueTask.FromResult(content == null ? null : new WorkspaceEncoded(resolved, content.Length, Convert.ToBase64String(content)));
    }

    public async ValueTask<WorkspaceEntry> WriteAsync(long lambdaId, string path, Stream content, long? expected = null, long? featureId = null,
                                                      CancellationToken cancellation = default)
    {
        var limits = RequireEnabled(lambdaId);

        var scope = new DataScope(lambdaId, featureId);

        var resolved = Resolve(path);

        var entries = files.List(scope);

        if (entries.Any(e => e.Folder && e.Path == resolved))
        {
            throw LambdaException.Invalid("There is already a folder with that name.");
        }

        var replaced = entries.FirstOrDefault(e => !e.Folder && e.Path == resolved) is { } existing ? WorkspaceLimits.Footprint(existing.Length) : 0;

        // what the rest of the workspace takes already - the file being
        // replaced is not counted, since it will not be there beside this
        // one - and the folders this file goes into that are not there yet
        var others = WorkspacePaths.Measure(entries) - replaced + WorkspacePaths.Missing(resolved, WorkspacePaths.Folders(entries)) * WorkspaceLimits.Block;

        // never less than what is being replaced: a workspace is over its
        // quota once its lambda leaves the tier that filled it, and should
        // still be able to rewrite what it holds, only not to grow
        var room = Math.Max(Room(limits, lambdaId, featureId) - others, replaced);

        // refused the moment it grows past what a lambda would be allowed to
        // write itself; the integration keeps what was there, and none of
        // the folders it would have gone into, when the content fails
        var guarded = new GuardedUpload(content, room, expected, () => Full(limits, lambdaId));

        try
        {
            await files.WriteAsync(scope, resolved, guarded, cancellation);
        }
        finally
        {
            vault.Changed(lambdaId, featureId);
        }

        if (featureId != null)
        {
            logger.LogInformation("Wrote workspace file {Path} of feature #{FeatureId} of lambda #{LambdaId}", path, featureId, lambdaId);
        }
        else
        {
            logger.LogInformation("Wrote workspace file {Path} of lambda #{LambdaId}", path, lambdaId);
        }

        var written = files.Find(scope, resolved);

        return new WorkspaceEntry(resolved, written?.Length ?? guarded.Received, written?.Modified ?? DateTime.UtcNow);
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

        var scope = new DataScope(lambdaId, featureId);

        var resolved = Resolve(path);

        var entries = files.List(scope);

        if (entries.Any(e => !e.Folder && e.Path == resolved))
        {
            throw LambdaException.Invalid("There is already a file with that name.");
        }

        /*
         * A folder takes a block of the quota even though it holds nothing. It
         * is a thing of its own, and making a thousand of them is the same
         * nuisance as making a thousand empty files, which counting in blocks
         * exists to stop.
         */
        if (!entries.Any(e => e.Folder && e.Path == resolved)
         && WorkspacePaths.Measure(entries) + (WorkspacePaths.Missing(resolved, WorkspacePaths.Folders(entries)) + 1) * WorkspaceLimits.Block > Room(limits, lambdaId, featureId))
        {
            throw Full(limits, lambdaId);
        }

        files.CreateFolder(scope, resolved);

        vault.Changed(lambdaId, featureId);

        if (featureId != null)
        {
            logger.LogInformation("Created workspace folder {Path} of feature #{FeatureId} of lambda #{LambdaId}", path, featureId, lambdaId);
        }
        else
        {
            logger.LogInformation("Created workspace folder {Path} of lambda #{LambdaId}", path, lambdaId);
        }
    }

    /// <remarks>
    /// A folder with everything in it: the editor asks before it gets here,
    /// and a folder that refuses to go while it holds something is a folder
    /// somebody has to empty by hand one file at a time.
    /// </remarks>
    public void Delete(long lambdaId, string path, long? featureId = null)
    {
        files.Delete(new DataScope(lambdaId, featureId), Resolve(path));

        vault.Changed(lambdaId, featureId);
    }

    public void Clear(long lambdaId, long? featureId = null)
    {
        files.Clear(new DataScope(lambdaId, featureId));

        vault.Changed(lambdaId, featureId);

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
    /// The room the workspace may take: the room of the lambda's data, less
    /// what its database takes of it.
    /// </summary>
    private long Room(WorkspaceLimits limits, long lambdaId, long? featureId) => Math.Max(0, limits.Quota - databases.SizeOf(lambdaId, featureId));

    /// <summary>
    /// What a write past the room of the lambda's data is refused with: the
    /// limit, its tier, and the tier that allows more, as the installation
    /// calls them.
    /// </summary>
    private LambdaException Full(WorkspaceLimits limits, long lambdaId)
    {
        var tier = meta.GetTier(lambdaId);

        var named = tier is { } known ? $" in the tier '{tiers.PolicyOf(known).Name}'" : string.Empty;

        var beyond = tier is { } current && tiers.Above(current) is { } above && above.MaxDataBytes > limits.Quota
            ? $" In the tier '{above.Name}' it may take {above.MaxDataBytes} bytes; whoever runs this installation decides the tier."
            : string.Empty;

        return LambdaException.Invalid($"The data of a lambda{named} - its workspace and its database together - must not take more than {limits.Quota} bytes.{beyond}");
    }

    /// <summary>
    /// What the workspace of the lambda may hold, which its tier decides - and
    /// whether it has one, which its owner does.
    /// </summary>
    /// <remarks>
    /// Asked every time rather than remembered, so a lambda that was just
    /// moved to another tier is held to the limits of the new one - the same
    /// ones its own code reads as it writes. One that is gone gets the least
    /// room of any tier.
    /// </remarks>
    private WorkspaceLimits Limits(long lambdaId)
        => meta.GetWorkspaceLimits(lambdaId) ?? tiers.WorkspaceOf(LambdaTier.Sandbox);

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

    /// <summary>
    /// Turns a requested name into a path inside the workspace, or refuses it.
    /// </summary>
    /// <remarks>
    /// The same check made for what the lambda writes itself, for the same
    /// reason: the name arrives from outside, and "../../etc/passwd" is a name.
    /// </remarks>
    private static string Resolve(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw LambdaException.Invalid("The name of a workspace file must not be empty.");
        }

        return WorkspacePaths.Resolve(path) ?? throw LambdaException.Invalid($"'{path}' is outside of the workspace of this lambda.");
    }

    private static string NameOf(string path) => path[(path.LastIndexOf('/') + 1)..];

    #endregion

}
