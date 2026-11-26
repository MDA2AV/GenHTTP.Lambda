using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Agents;
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
                                    IAgentJournal journal, ILogger<WorkspaceService> logger) : IWorkspaceService
{

    /// <summary>
    /// The largest file that travels as base64 in a JSON document.
    /// </summary>
    /// <remarks>
    /// That way a file is in memory several times over while the request runs
    /// - its bytes, the text they become, the characters of that text. The
    /// workspace does not limit how large one file may be, only the room they
    /// take together, so anything larger is sent as it is (see
    /// <see cref="FindAsync"/>), which streams the bytes.
    /// </remarks>
    private const long EncodedLimit = 32 * 1024 * 1024;

    #region Functionality

    public async ValueTask<WorkspaceListing> ListAsync(long lambdaId, long? featureId = null, CancellationToken cancellation = default)
    {
        var limits = await LimitsAsync(lambdaId, cancellation);

        // switched off, it holds nothing - and looking is no reason to make
        // anything it would have
        if (!limits.Enabled)
        {
            return new WorkspaceListing([], [], 0, await RoomAsync(limits, lambdaId, featureId, cancellation), false);
        }

        var entries = await files.ListAsync(new DataScope(lambdaId, featureId), cancellation);

        var listed = entries.Where(e => !e.Folder)
                            .Select(e => new WorkspaceEntry(e.Path, e.Length, e.Modified))
                            .OrderBy(e => e.Path, StringComparer.Ordinal)
                            .ToList();

        var folders = entries.Where(e => e.Folder)
                             .Select(e => e.Path)
                             .Order(StringComparer.Ordinal)
                             .ToList();

        return new WorkspaceListing(listed, folders, WorkspacePaths.Measure(entries), await RoomAsync(limits, lambdaId, featureId, cancellation));
    }

    public async ValueTask<WorkspaceFile?> FindAsync(long lambdaId, string path, long? featureId = null, CancellationToken cancellation = default)
    {
        var resolved = Resolve(path);

        return await files.ResourceAsync(new DataScope(lambdaId, featureId), resolved, cancellation) is { } content ? new WorkspaceFile(NameOf(resolved), content) : null;
    }

    public async ValueTask<WorkspaceContent?> ReadAsync(long lambdaId, string path, long? featureId = null, CancellationToken cancellation = default)
    {
        var resolved = Resolve(path);

        var content = await files.ReadAsync(new DataScope(lambdaId, featureId), resolved, cancellation);

        return content == null ? null : new WorkspaceContent(resolved, content);
    }

    public async ValueTask<WorkspaceEncoded?> ReadEncodedAsync(long lambdaId, string path, long? featureId = null, CancellationToken cancellation = default)
    {
        var scope = new DataScope(lambdaId, featureId);

        var resolved = Resolve(path);

        if (await files.FindAsync(scope, resolved, cancellation) is not { Folder: false } found)
        {
            return null;
        }

        if (found.Length > EncodedLimit)
        {
            return new WorkspaceEncoded(resolved, found.Length, null);
        }

        var content = await files.ReadAsync(scope, resolved, cancellation);

        return content == null ? null : new WorkspaceEncoded(resolved, content.Length, Convert.ToBase64String(content));
    }

    public async ValueTask<WorkspaceEntry> WriteAsync(long lambdaId, string path, Stream content, long? expected = null, long? featureId = null,
                                                      VersionAuthor? author = null, CancellationToken cancellation = default)
    {
        var written = await WrittenAsync(lambdaId, path, content, expected, featureId, cancellation);

        // what a run put into the app's own data, which undoing it leaves - a
        // feature's copy goes with the feature
        if (featureId == null)
        {
            await journal.RecordAsync(author, new AgentEffect(AgentEffectKind.Wrote, lambdaId, Name: path), CancellationToken.None);
        }

        return written;
    }

    private async ValueTask<WorkspaceEntry> WrittenAsync(long lambdaId, string path, Stream content, long? expected, long? featureId, CancellationToken cancellation)
    {
        var limits = await RequireEnabledAsync(lambdaId, cancellation);

        var scope = new DataScope(lambdaId, featureId);

        var resolved = Resolve(path);

        var entries = await files.ListAsync(scope, cancellation);

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
        var room = Math.Max(await RoomAsync(limits, lambdaId, featureId, cancellation) - others, replaced);

        // refused the moment it grows past what a lambda would be allowed to
        // write itself; the integration keeps what was there, and none of
        // the folders it would have gone into, when the content fails
        // what it is refused with, read before the content streams in
        var full = await FullAsync(limits, lambdaId, cancellation);

        var guarded = new GuardedUpload(content, room, expected, () => full);

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

        var written = await files.FindAsync(scope, resolved, cancellation);

        return new WorkspaceEntry(resolved, written?.Length ?? guarded.Received, written?.Modified ?? DateTime.UtcNow);
    }

    public async ValueTask<WorkspaceEntry> WriteEncodedAsync(long lambdaId, string path, string? content, long? featureId = null,
                                                             VersionAuthor? author = null, CancellationToken cancellation = default)
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

        return await WriteAsync(lambdaId, path, stream, featureId: featureId, author: author, cancellation: cancellation);
    }

    public async ValueTask CreateFolderAsync(long lambdaId, string path, long? featureId = null, CancellationToken cancellation = default)
    {
        var limits = await RequireEnabledAsync(lambdaId, cancellation);

        var scope = new DataScope(lambdaId, featureId);

        var resolved = Resolve(path);

        var entries = await files.ListAsync(scope, cancellation);

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
         && WorkspacePaths.Measure(entries) + (WorkspacePaths.Missing(resolved, WorkspacePaths.Folders(entries)) + 1) * WorkspaceLimits.Block
          > await RoomAsync(limits, lambdaId, featureId, cancellation))
        {
            throw await FullAsync(limits, lambdaId, cancellation);
        }

        await files.CreateFolderAsync(scope, resolved, cancellation);

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
    public async ValueTask DeleteAsync(long lambdaId, string path, long? featureId = null, VersionAuthor? author = null, CancellationToken cancellation = default)
    {
        await files.DeleteAsync(new DataScope(lambdaId, featureId), Resolve(path), cancellation);

        vault.Changed(lambdaId, featureId);

        if (featureId == null)
        {
            await journal.RecordAsync(author, new AgentEffect(AgentEffectKind.Removed, lambdaId, Name: path), CancellationToken.None);
        }
    }

    public async ValueTask ClearAsync(long lambdaId, long? featureId = null, CancellationToken cancellation = default)
    {
        await files.ClearAsync(new DataScope(lambdaId, featureId), cancellation);

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
    private async ValueTask<long> RoomAsync(WorkspaceLimits limits, long lambdaId, long? featureId, CancellationToken cancellation)
        => Math.Max(0, limits.Quota - await databases.SizeOfAsync(lambdaId, featureId, cancellation));

    /// <summary>
    /// What a write past the room of the lambda's data is refused with: the
    /// limit, its tier, and the tier that allows more, as the installation
    /// calls them.
    /// </summary>
    private async ValueTask<LambdaException> FullAsync(WorkspaceLimits limits, long lambdaId, CancellationToken cancellation)
    {
        var tier = await meta.GetTierAsync(lambdaId, cancellation);

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
    private async ValueTask<WorkspaceLimits> LimitsAsync(long lambdaId, CancellationToken cancellation)
        => await meta.GetWorkspaceLimitsAsync(lambdaId, cancellation) ?? tiers.WorkspaceOf(LambdaTier.Sandbox);

    /// <summary>
    /// The limits of a workspace that is switched on, or a refusal saying how
    /// to switch it on.
    /// </summary>
    /// <remarks>
    /// Refused here as the lambda itself refuses it, so a file uploaded by
    /// hand cannot land where the lambda has been told there is nothing.
    /// </remarks>
    private async ValueTask<WorkspaceLimits> RequireEnabledAsync(long lambdaId, CancellationToken cancellation)
    {
        var limits = await LimitsAsync(lambdaId, cancellation);

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
