using GenHTTP.Api.Content.IO;

using GenHTTP.Lambda.Integrations.Files;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Integrations.Local;

/// <summary>
/// Keeps the workspaces as directories in the data directory - what every
/// installation does unless it chooses another place for them.
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

    public IFileProvider Build(IntegrationContext context) => new LocalFileProvider(context.DataDirectory);

}

/// <summary>
/// A workspace as a directory: <c>workspaces/{id}</c> for a lambda,
/// <c>features/{id}/{feature}/workspace</c> for the copy a feature works on,
/// beside the rest of what the feature keeps.
/// </summary>
/// <remarks>
/// What is written goes through a file beside the one it replaces, which a
/// copy made meanwhile leaves out, and replaces it in one move - so a reader
/// sees the file before or after, and a write that fails leaves the one that
/// was there.
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

    #region Reading

    public IReadOnlyList<FileEntry> List(DataScope scope)
    {
        var root = Root(scope);

        if (!Directory.Exists(root))
        {
            return [];
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

        return entries;
    }

    public FileEntry? Find(DataScope scope, string path)
    {
        var resolved = Resolve(scope, path);

        if (File.Exists(resolved))
        {
            var info = new FileInfo(resolved);

            return new FileEntry(path, false, Length(info), Modified(info));
        }

        return Directory.Exists(resolved) ? new FileEntry(path, true, 0, DateTime.MinValue) : null;
    }

    public byte[]? Read(DataScope scope, string path)
    {
        var resolved = Resolve(scope, path);

        try
        {
            return File.Exists(resolved) ? File.ReadAllBytes(resolved) : null;
        }
        catch (FileNotFoundException)
        {
            // removed between looking and reading
            return null;
        }
    }

    public IResource? Resource(DataScope scope, string path)
    {
        var resolved = Resolve(scope, path);

        return File.Exists(resolved) ? GenHTTP.Modules.IO.Resource.FromFile(new FileInfo(resolved)).Build() : null;
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

    public void Write(DataScope scope, string path, ReadOnlySpan<byte> content)
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
                target.Write(content);
            }

            File.Move(staging, resolved, true);
        }
        catch
        {
            Undo(staging, created);
            throw;
        }
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

    public void CreateFolder(DataScope scope, string path) => Directory.CreateDirectory(Resolve(scope, path));

    public void Delete(DataScope scope, string path) => Remove(Resolve(scope, path));

    public void Clear(DataScope scope)
    {
        var root = Root(scope);

        if (!Directory.Exists(root))
        {
            return;
        }

        foreach (var entry in Directory.GetFileSystemEntries(root))
        {
            Remove(entry);
        }
    }

    #endregion

    #region Keeping them

    /// <remarks>
    /// Copied beside the copy it replaces and swapped in once complete, so a
    /// copy that fails halfway leaves the one there was.
    /// </remarks>
    public void Copy(DataScope source, DataScope target, CancellationToken cancellation = default)
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
    }

    public void Delete(DataScope scope) => RemoveDirectory(Folder(scope));

    public void DeleteAll(long lambdaId)
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
    }

    #endregion

    #region Describing it

    public IntegrationInfo Describe() => new(IntegrationKinds.Files, "Local",
                                             "A directory per workspace in the data directory.",
                                             [
                                                 new IntegrationDetail("Lambdas", Path.Combine(dataDirectory, "workspaces", "{lambda}")),
                                                 new IntegrationDetail("Drafts", Path.Combine(dataDirectory, "features", "{lambda}", "{draft}", "workspace"))
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
