using GenHTTP.Api.Content.IO;

using GenHTTP.Lambda.Integrations.Files;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Integrations.Local;

/// <summary>
/// Keeps the files as directories in the data directory - the workspaces and
/// the platform's pictures - what every installation does unless it chooses
/// another place for them.
/// </summary>
/// <example>
/// <code>
/// LambdaPlatform.Create().Files(LocalFiles.Create())
/// </code>
/// </example>
public static class LocalFiles
{

    public static LocalFilesBuilder Create() => new();

}

/// <summary>
/// Configures the local files of an installation.
/// </summary>
/// <remarks>
/// Nothing to configure: where they are follows from the data directory.
/// </remarks>
public sealed class LocalFilesBuilder : IIntegrationBuilder<IFileProvider>
{

    public ValueTask<IFileProvider> BuildAsync(IntegrationContext context, CancellationToken cancellation = default)
        => ValueTask.FromResult<IFileProvider>(new LocalFileProvider(context.DataDirectory));

}

/// <summary>
/// A workspace as a directory: <c>workspaces/{id}</c> for a lambda,
/// <c>features/{id}/{feature}/workspace</c> for the copy a feature works on,
/// beside the rest of what the feature keeps. A store of the platform is the
/// directory of its name: <c>branding/logo</c>, <c>showcase/{id}</c>.
/// </summary>
/// <remarks>
/// <para>
/// What is written goes through a file beside the one it replaces, which a
/// copy made meanwhile leaves out, and replaces it in one move - so a reader
/// sees the file before or after, and a write that fails leaves the one that
/// was there.
/// </para>
/// <para>
/// Synchronous inside, whatever the contract says: a file of this machine is
/// read and written on the thread that asks - on the Ioxide engine the
/// reactor of the request, which it never leaves for that. Every task it
/// returns has completed already, but for an upload, which is awaited as it
/// arrives.
/// </para>
/// </remarks>
internal sealed class LocalFileProvider(string dataDirectory) : IFileProvider
{

    /// <summary>
    /// What a file being written is called until it is complete.
    /// </summary>
    /// <remarks>
    /// The name an upload had before lambdas wrote this way too, so a copy
    /// made by an earlier version of the platform leaves out the same files.
    /// </remarks>
    private const string Staging = ".uploading";

    #region The platform's own

    public ValueTask<byte[]?> ReadAsync(FileStore store, string name, CancellationToken cancellation = default)
    {
        var file = Stored(store, name);

        try
        {
            return ValueTask.FromResult(File.Exists(file) ? File.ReadAllBytes(file) : null);
        }
        catch (FileNotFoundException)
        {
            return ValueTask.FromResult<byte[]?>(null);
        }
    }

    public ValueTask WriteAsync(FileStore store, string name, ReadOnlyMemory<byte> content, CancellationToken cancellation = default)
    {
        var file = Stored(store, name);

        Directory.CreateDirectory(Path.GetDirectoryName(file)!);

        var staging = file + Staging;

        try
        {
            using (var target = File.Create(staging))
            {
                target.Write(content.Span);
            }

            File.Move(staging, file, true);
        }
        catch
        {
            Remove(staging);
            throw;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(FileStore store, string name, CancellationToken cancellation = default)
    {
        Remove(Stored(store, name));

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Where a file of a store is - one level below the store, the names
    /// being the platform's own.
    /// </summary>
    private string Stored(FileStore store, string name)
    {
        if (!Plain(name))
        {
            throw new ArgumentException($"'{name}' is no file of a store: a name of letters, digits, dashes and dots.");
        }

        return Path.Combine(StoreDirectory(store), name);
    }

    /// <summary>
    /// The directory of a store, spelled out rather than made of its name:
    /// the pictures of every installation are where it says.
    /// </summary>
    private string StoreDirectory(FileStore store) => Path.Combine(dataDirectory, store switch
    {
        FileStore.Branding => "branding",
        FileStore.Showcase => "showcase",
        _ => throw new ArgumentOutOfRangeException(nameof(store), store, "The platform keeps no such store.")
    });

    private static bool Plain(string name)
        => name.Length > 0 && name[0] != '.' && name.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');

    #endregion

    #region Reading

    public ValueTask<IReadOnlyList<FileEntry>> ListAsync(DataScope scope, CancellationToken cancellation = default)
    {
        var root = Root(scope);

        if (!Directory.Exists(root))
        {
            return ValueTask.FromResult<IReadOnlyList<FileEntry>>([]);
        }

        var entries = new List<FileEntry>();

        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var info = new FileInfo(path);

            entries.Add(new FileEntry(Relative(root, path), false, Length(info), Modified(info)));
        }

        foreach (var path in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
        {
            entries.Add(new FileEntry(Relative(root, path), true, 0, DateTime.MinValue));
        }

        return ValueTask.FromResult<IReadOnlyList<FileEntry>>(entries);
    }

    public ValueTask<FileEntry?> FindAsync(DataScope scope, string path, CancellationToken cancellation = default)
    {
        var resolved = Resolve(scope, path);

        if (File.Exists(resolved))
        {
            var info = new FileInfo(resolved);

            return ValueTask.FromResult<FileEntry?>(new FileEntry(path, false, Length(info), Modified(info)));
        }

        return ValueTask.FromResult(Directory.Exists(resolved) ? new FileEntry(path, true, 0, DateTime.MinValue) : null);
    }

    public ValueTask<byte[]?> ReadAsync(DataScope scope, string path, CancellationToken cancellation = default)
    {
        var resolved = Resolve(scope, path);

        try
        {
            return ValueTask.FromResult(File.Exists(resolved) ? File.ReadAllBytes(resolved) : null);
        }
        catch (FileNotFoundException)
        {
            // removed between looking and reading
            return ValueTask.FromResult<byte[]?>(null);
        }
    }

    public ValueTask<IResource?> ResourceAsync(DataScope scope, string path, CancellationToken cancellation = default)
    {
        var resolved = Resolve(scope, path);

        return ValueTask.FromResult(File.Exists(resolved) ? GenHTTP.Modules.IO.Resource.FromFile(new FileInfo(resolved)).Build() : null);
    }

    /// <remarks>
    /// The directory itself, read as it is served: what is written to it
    /// later is served from then on.
    /// </remarks>
    public IResourceTree Tree(DataScope scope, string? folder)
    {
        var root = Root(scope);

        Directory.CreateDirectory(root);

        return ResourceTree.FromDirectory(folder == null ? root : Resolve(scope, folder)).Build();
    }

    #endregion

    #region Writing

    public ValueTask WriteAsync(DataScope scope, string path, ReadOnlyMemory<byte> content, CancellationToken cancellation = default)
    {
        var resolved = Resolve(scope, path);

        // the workspace itself is there from the first write on, and stays
        Directory.CreateDirectory(Folder(scope));

        var created = MakeFolders(Root(scope), resolved);

        var staging = resolved + Staging;

        try
        {
            using (var target = File.Create(staging))
            {
                target.Write(content.Span);
            }

            File.Move(staging, resolved, true);
        }
        catch
        {
            Undo(staging, created);
            throw;
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask WriteAsync(DataScope scope, string path, Stream content, CancellationToken cancellation = default)
    {
        var resolved = Resolve(scope, path);

        // the workspace itself is there from the first write on, and stays
        Directory.CreateDirectory(Folder(scope));

        var created = MakeFolders(Root(scope), resolved);

        var staging = resolved + Staging;

        try
        {
            await using (var target = File.Create(staging))
            {
                await content.CopyToAsync(target, cancellation);
            }

            File.Move(staging, resolved, true);
        }
        catch
        {
            Undo(staging, created);
            throw;
        }
    }

    public ValueTask CreateFolderAsync(DataScope scope, string path, CancellationToken cancellation = default)
    {
        Directory.CreateDirectory(Resolve(scope, path));

        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(DataScope scope, string path, CancellationToken cancellation = default)
    {
        Remove(Resolve(scope, path));

        return ValueTask.CompletedTask;
    }

    public ValueTask ClearAsync(DataScope scope, CancellationToken cancellation = default)
    {
        var root = Root(scope);

        if (Directory.Exists(root))
        {
            foreach (var entry in Directory.GetFileSystemEntries(root))
            {
                Remove(entry);
            }
        }

        return ValueTask.CompletedTask;
    }

    #endregion

    #region Keeping them

    /// <remarks>
    /// Copied beside the copy it replaces and swapped in once complete, so a
    /// copy that fails halfway leaves the one there was. On the thread it is
    /// asked on, which the platform chose to be away from a request's.
    /// </remarks>
    public ValueTask CopyAsync(DataScope source, DataScope target, CancellationToken cancellation = default)
    {
        var into = Folder(target);

        var staging = $"{into}.copying";

        var from = Folder(source);

        RemoveDirectory(staging);

        Directory.CreateDirectory(staging);

        try
        {
            if (Directory.Exists(from))
            {
                Copy(new DirectoryInfo(from), staging, cancellation);
            }
        }
        catch
        {
            RemoveDirectory(staging);
            throw;
        }

        RemoveDirectory(into);

        Directory.CreateDirectory(Path.GetDirectoryName(into)!);

        Directory.Move(staging, into);

        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(DataScope scope, CancellationToken cancellation = default)
    {
        RemoveDirectory(Folder(scope));

        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAllAsync(long lambdaId, CancellationToken cancellation = default)
    {
        RemoveDirectory(Folder(new DataScope(lambdaId)));

        var features = Path.Combine(dataDirectory, "features", lambdaId.ToString());

        if (Directory.Exists(features))
        {
            foreach (var feature in Directory.EnumerateDirectories(features))
            {
                RemoveDirectory(Path.Combine(feature, "workspace"));
            }
        }

        return ValueTask.CompletedTask;
    }

    #endregion

    #region Describing it

    public IntegrationInfo Describe() => new(IntegrationKind.Files, "Local",
                                             [
                                                 new IntegrationDetail("Lambdas", Path.Combine(dataDirectory, "workspaces", "{lambda}")),
                                                 new IntegrationDetail("Drafts", Path.Combine(dataDirectory, "features", "{lambda}", "{draft}", "workspace")),
                                                 new IntegrationDetail("Logo", StoreDirectory(FileStore.Branding)),
                                                 new IntegrationDetail("Showcase", StoreDirectory(FileStore.Showcase))
                                             ]);

    public string? Guide(GuideContext context) => null;

    #endregion

    #region Helpers

    /// <summary>
    /// The directory of a workspace.
    /// </summary>
    private string Folder(DataScope scope)
    {
        var directory = scope.FeatureId is { } feature
            ? Path.Combine(dataDirectory, "features", scope.LambdaId.ToString(), feature.ToString(), "workspace")
            : Path.Combine(dataDirectory, "workspaces", scope.LambdaId.ToString());

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
    }

    /// <summary>
    /// The directory of a workspace as the paths in it begin.
    /// </summary>
    private string Root(DataScope scope) => Folder(scope) + Path.DirectorySeparatorChar;

    /// <summary>
    /// A path of the workspace on the disk.
    /// </summary>
    /// <remarks>
    /// The platform hands over paths it checked; this checks once more that
    /// what it comes to is inside, since a mistake there would be a file
    /// anywhere the server may write.
    /// </remarks>
    private string Resolve(DataScope scope, string path)
    {
        var root = Root(scope);

        var resolved = Path.GetFullPath(Path.Combine(root, path));

        if (!resolved.StartsWith(root, StringComparison.Ordinal) && resolved + Path.DirectorySeparatorChar != root)
        {
            throw new UnauthorizedAccessException($"'{path}' is outside of the workspace.");
        }

        return resolved;
    }

    private static string Relative(string root, string path) => path[root.Length..].Replace('\\', '/');

    /// <summary>
    /// Makes the folders a file goes into that are not there yet.
    /// </summary>
    /// <returns>Those that were made, outermost first</returns>
    private static List<string> MakeFolders(string root, string file)
    {
        var missing = new List<string>();

        var directory = Path.GetDirectoryName(file);

        while (directory != null && directory.Length >= root.Length && !Directory.Exists(directory))
        {
            missing.Insert(0, directory);

            directory = Path.GetDirectoryName(directory);
        }

        foreach (var folder in missing)
        {
            Directory.CreateDirectory(folder);
        }

        return missing;
    }

    /// <summary>
    /// Takes back what a write that failed left: the file it was writing, and
    /// the folders it made for it.
    /// </summary>
    private static void Undo(string staging, List<string> created)
    {
        Remove(staging);

        foreach (var folder in created.AsEnumerable().Reverse())
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
    }

    /// <summary>
    /// Copies a folder with everything in it, empty folders included - an
    /// empty folder is a real thing in a workspace, which code may rely on.
    /// </summary>
    private static void Copy(DirectoryInfo source, string target, CancellationToken cancellation)
    {
        foreach (var file in source.EnumerateFiles())
        {
            cancellation.ThrowIfCancellationRequested();

            // a file still being written is not part of what is there yet
            if (file.Name.EndsWith(Staging, StringComparison.Ordinal))
            {
                continue;
            }

            file.CopyTo(Path.Combine(target, file.Name), true);
        }

        foreach (var folder in source.EnumerateDirectories())
        {
            var inner = Path.Combine(target, folder.Name);

            Directory.CreateDirectory(inner);

            Copy(folder, inner, cancellation);
        }
    }

    /// <summary>
    /// Removes a file, or a folder with everything in it.
    /// </summary>
    /// <remarks>
    /// A file that cannot be removed is not worth failing a request over.
    /// </remarks>
    private static void Remove(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch (Exception)
        {
            // left for the next attempt
        }
    }

    private static void RemoveDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
        catch (IOException)
        {
            // content that is still in use cannot be removed on every
            // platform - the next start cleans it up
        }
    }

    private static long Length(FileInfo file)
    {
        try
        {
            return file.Length;
        }
        catch (IOException)
        {
            // removed since the directory was read
            return 0;
        }
    }

    private static DateTime Modified(FileInfo file)
    {
        try
        {
            return file.LastWriteTimeUtc;
        }
        catch (IOException)
        {
            return DateTime.MinValue;
        }
    }

    #endregion

}
