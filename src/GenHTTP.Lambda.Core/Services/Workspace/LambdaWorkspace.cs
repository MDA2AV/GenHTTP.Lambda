using GenHTTP.Api.Content.IO;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Files;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// The workspace as a running lambda reads and writes it: the files
/// integration behind the rules a lambda is held to.
/// </summary>
/// <remarks>
/// <para>
/// What the class generated into a lambda once did with the directory it was
/// compiled with, done here instead: the lambda is handed functions to call
/// (<see cref="Bind"/>), so its code is the same whatever keeps its files,
/// and the rules are written once - an upload by the owner is held to the same
/// room in the same way.
/// </para>
/// <para>
/// Only the room is limited, counted in blocks as <see cref="WorkspaceLimits"/>
/// explains: what everything in the workspace takes, measured as it is
/// written, and what the database takes beside it, together within the room
/// of the lambda's data as its tier allows it now. A write that takes no more
/// room than what it replaces goes through even past the quota, which is where
/// a workspace is once its lambda leaves the tier that filled it: it can still
/// rewrite what it holds, only not grow. Reading never makes anything.
/// </para>
/// <para>
/// Every function is awaited first; the plain calls code written before makes
/// are the same functions through <see cref="Synchronous"/>, which waits for
/// nothing where the integration answers at once. What it throws is what code
/// reading files expects - a file that is not there, a name outside, a
/// workspace that is full - with what to do about it, which is what the log
/// then says.
/// </para>
/// </remarks>
/// <param name="room">The room of the lambda's data, as its tier allows it now</param>
/// <param name="beside">The room the rest of its data - its database - takes</param>
internal sealed class LambdaWorkspace(IFileProvider files, IWorkspaceVault vault, DataScope scope, Func<long> room, Func<CancellationToken, ValueTask<long>> beside)
{

    #region Functionality

    public WorkspaceBinding Bind() => new(
        name => Synchronous.Run(() => ExistsAsync(name, CancellationToken.None)),
        name => Synchronous.Run(() => ReadAsync(name, CancellationToken.None)),
        (name, content) => Synchronous.Run(() => WriteAsync(name, content, CancellationToken.None)),
        name => Synchronous.Run(() => DeleteAsync(name, CancellationToken.None)),
        () => Synchronous.Run(() => ListAsync(CancellationToken.None)),
        () => Synchronous.Run(() => FoldersAsync(CancellationToken.None)),
        name => Synchronous.Run(() => CreateFolderAsync(name, CancellationToken.None)),
        folder => Synchronous.Run(() => TreeAsync(folder, CancellationToken.None)),
        ExistsAsync,
        ReadAsync,
        WriteAsync,
        DeleteAsync,
        ListAsync,
        FoldersAsync,
        CreateFolderAsync
    );

    private async ValueTask<bool> ExistsAsync(string name, CancellationToken cancellation)
        => await files.FindAsync(scope, Resolve(name), cancellation) is { Folder: false };

    private async ValueTask<byte[]> ReadAsync(string name, CancellationToken cancellation)
        => await files.ReadAsync(scope, Resolve(name), cancellation) ?? throw new FileNotFoundException($"There is no file called '{name}' in the workspace.", name);

    private async ValueTask WriteAsync(string name, byte[] content, CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(content);

        var path = Resolve(name);

        await ReserveAsync(path, content.Length, cancellation);

        try
        {
            await files.WriteAsync(scope, path, content, cancellation);
        }
        finally
        {
            vault.Changed(scope.LambdaId, scope.FeatureId);
        }
    }

    /// <remarks>
    /// A file only, as it always was: a folder stays.
    /// </remarks>
    private async ValueTask DeleteAsync(string name, CancellationToken cancellation)
    {
        var path = Resolve(name);

        if (await files.FindAsync(scope, path, cancellation) is { Folder: false })
        {
            await files.DeleteAsync(scope, path, cancellation);

            vault.Changed(scope.LambdaId, scope.FeatureId);
        }
    }

    private async ValueTask<string[]> ListAsync(CancellationToken cancellation)
        => [.. (await files.ListAsync(scope, cancellation)).Where(e => !e.Folder).Select(e => e.Path)];

    private async ValueTask<string[]> FoldersAsync(CancellationToken cancellation)
        => [.. (await files.ListAsync(scope, cancellation)).Where(e => e.Folder).Select(e => e.Path)];

    private async ValueTask CreateFolderAsync(string name, CancellationToken cancellation)
    {
        var path = Resolve(name);

        if (await files.FindAsync(scope, path, cancellation) is { Folder: true })
        {
            return;
        }

        var entries = await files.ListAsync(scope, cancellation);

        if (WorkspacePaths.Measure(entries) + await beside(cancellation) + (WorkspacePaths.Missing(path, WorkspacePaths.Folders(entries)) + 1) * WorkspaceLimits.Block > room())
        {
            throw Full();
        }

        await files.CreateFolderAsync(scope, path, cancellation);

        vault.Changed(scope.LambdaId, scope.FeatureId);
    }

    /// <remarks>
    /// The folder is looked for first, so a name that is not there is said
    /// as the lambda starts rather than as nothing served.
    /// </remarks>
    private async ValueTask<IResourceTree> TreeAsync(string? folder, CancellationToken cancellation)
    {
        if (folder == null)
        {
            return files.Tree(scope, null);
        }

        var path = Resolve(folder);

        if (await files.FindAsync(scope, path, cancellation) is not { Folder: true })
        {
            var had = await FoldersAsync(cancellation);

            var known = had.Length > 0
                      ? "The folders this workspace holds are: " + string.Join(", ", had) + "."
                      : "This workspace holds no folders yet.";

            throw new InvalidOperationException($"There is no folder called '{folder}' in the workspace. {known}");
        }

        return files.Tree(scope, path);
    }

    #endregion

    #region Helpers

    private static string Resolve(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The name of a workspace file must not be empty.", nameof(name));
        }

        return WorkspacePaths.Resolve(name) ?? throw new UnauthorizedAccessException($"'{name}' is outside of the workspace of this lambda.");
    }

    /// <summary>
    /// Makes sure what is about to be written fits, the folders it goes into
    /// that are not there yet included.
    /// </summary>
    private async ValueTask ReserveAsync(string path, long size, CancellationToken cancellation)
    {
        var entries = await files.ListAsync(scope, cancellation);

        var added = WorkspaceLimits.Footprint(size) + WorkspacePaths.Missing(path, WorkspacePaths.Folders(entries)) * WorkspaceLimits.Block;

        // a file being replaced is not counted a second time
        var replaced = entries.FirstOrDefault(e => !e.Folder && e.Path == path) is { } existing ? WorkspaceLimits.Footprint(existing.Length) : 0;

        if (added > replaced && WorkspacePaths.Measure(entries) + await beside(cancellation) - replaced + added > room())
        {
            throw Full();
        }
    }

    private InvalidOperationException Full()
        => new($"The data of this lambda - its workspace and its database together - must not take more than {room()} bytes.");

    #endregion

}
