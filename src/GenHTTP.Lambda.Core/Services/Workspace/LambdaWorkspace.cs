using GenHTTP.Api.Content.IO;

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
/// What it throws is what code reading files expects - a file that is not
/// there, a name outside, a workspace that is full - with what to do about
/// it, which is what the log then says.
/// </para>
/// </remarks>
/// <param name="room">The room of the lambda's data, as its tier allows it now</param>
/// <param name="beside">The room the rest of its data - its database - takes</param>
internal sealed class LambdaWorkspace(IFileProvider files, IWorkspaceVault vault, DataScope scope, Func<long> room, Func<long> beside)
{

    #region Functionality

    public WorkspaceBinding Bind() => new(Exists, Read, Write, Delete, List, Folders, CreateFolder, Tree);

    private bool Exists(string name) => files.Find(scope, Resolve(name)) is { Folder: false };

    private byte[] Read(string name)
        => files.Read(scope, Resolve(name)) ?? throw new FileNotFoundException($"There is no file called '{name}' in the workspace.", name);

    private void Write(string name, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var path = Resolve(name);

        Reserve(path, content.Length);

        try
        {
            files.Write(scope, path, content);
        }
        finally
        {
            vault.Changed(scope.LambdaId, scope.FeatureId);
        }
    }

    /// <remarks>
    /// A file only, as it always was: a folder stays.
    /// </remarks>
    private void Delete(string name)
    {
        var path = Resolve(name);

        if (files.Find(scope, path) is { Folder: false })
        {
            files.Delete(scope, path);

            vault.Changed(scope.LambdaId, scope.FeatureId);
        }
    }

    private string[] List() => [.. files.List(scope).Where(e => !e.Folder).Select(e => e.Path)];

    private string[] Folders() => [.. files.List(scope).Where(e => e.Folder).Select(e => e.Path)];

    private void CreateFolder(string name)
    {
        var path = Resolve(name);

        if (files.Find(scope, path) is { Folder: true })
        {
            return;
        }

        var entries = files.List(scope);

        if (WorkspacePaths.Measure(entries) + beside() + (WorkspacePaths.Missing(path, WorkspacePaths.Folders(entries)) + 1) * WorkspaceLimits.Block > room())
        {
            throw Full();
        }

        files.CreateFolder(scope, path);

        vault.Changed(scope.LambdaId, scope.FeatureId);
    }

    private IResourceTree Tree(string? folder)
    {
        if (folder == null)
        {
            return files.Tree(scope, null);
        }

        var path = Resolve(folder);

        if (files.Find(scope, path) is not { Folder: true })
        {
            var had = Folders();

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
    private void Reserve(string path, long size)
    {
        var entries = files.List(scope);

        var added = WorkspaceLimits.Footprint(size) + WorkspacePaths.Missing(path, WorkspacePaths.Folders(entries)) * WorkspaceLimits.Block;

        // a file being replaced is not counted a second time
        var replaced = entries.FirstOrDefault(e => !e.Folder && e.Path == path) is { } existing ? WorkspaceLimits.Footprint(existing.Length) : 0;

        if (added > replaced && WorkspacePaths.Measure(entries) + beside() - replaced + added > room())
        {
            throw Full();
        }
    }

    private InvalidOperationException Full()
        => new($"The data of this lambda - its workspace and its database together - must not take more than {room()} bytes.");

    #endregion

}
