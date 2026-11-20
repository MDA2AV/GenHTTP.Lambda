using System.IO.Compression;
using System.Text;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// How the files of a lambda are laid out in an archive.
/// </summary>
public enum ArchiveLayout
{

    /// <summary>Named as the lambda names them: lambda.cs, its code, resources/.</summary>
    Lambda,

    /// <summary>
    /// Where a clone of the lambda has them: Project.cs for lambda.cs, the
    /// other .cs files named the .NET way, and the rest as the lambda names it.
    /// </summary>
    Project

}

/// <summary>
/// The files of a version as a zip archive, one entry per file.
/// </summary>
/// <remarks>
/// Lets a caller fetch a version, change it locally and send it back in a
/// single request instead of shipping file by file. Unlike the export, the
/// archive holds the files exactly as they are named in the lambda, so it
/// can be uploaded again as it is.
///
/// Or, asked for, where a clone of the lambda has them - which is the same
/// but for the snippet, which a clone has in the class it is the body of, and
/// the names of the other C# files. Whoever builds what they build into the
/// lambda with a .NET tool, without git, gets and sends back the clone's
/// layout, with the lambda's files only: the platform's - the host, the
/// project file, what stands in for the platform - are the clone's to have,
/// and left out going both ways.
///
/// Neither way holds an archive in memory: one is packed from where its
/// files are kept into a file beside the stores, and one sent is written to
/// such a file as it arrives and unpacked from there, each of its files into
/// a file of its own, which the store of the lambda moves in as it keeps it.
/// </remarks>
public static class LambdaArchive
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private static readonly DateTimeOffset Timestamp = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    #region Packing

    /// <summary>
    /// Packs the given files into a zip archive in memory - for an archive
    /// known to be small.
    /// </summary>
    public static byte[] Pack(IReadOnlyList<VersionFile> files, ArchiveLayout layout = ArchiveLayout.Lambda)
    {
        using var buffer = new MemoryStream();

        Pack(files, layout, buffer);

        return buffer.ToArray();
    }

    /// <summary>
    /// Packs the given files into a file beside the stores, away from the
    /// reactor, and opens it to be sent - removed once it is closed.
    /// </summary>
    /// <param name="staging">Where the archive is written to</param>
    public static async ValueTask<Stream> PackAsync(IReadOnlyList<VersionFile> files, ArchiveLayout layout, string staging)
    {
        Directory.CreateDirectory(staging);

        var target = Path.Combine(staging, $"{Guid.NewGuid():n}.zip");

        try
        {
            await Offload.Run(() =>
            {
                using var stream = File.Create(target);

                Pack(files, layout, stream);
            });

            return new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 81920, FileOptions.DeleteOnClose);
        }
        catch
        {
            File.Delete(target);
            throw;
        }
    }

    /// <summary>
    /// Packs the given files into a zip archive written to the stream, one
    /// file after another as they are read.
    /// </summary>
    public static void Pack(IReadOnlyList<VersionFile> files, ArchiveLayout layout, Stream target)
    {
        using var zip = new ZipArchive(target, ZipArchiveMode.Create, true);

        foreach (var file in files)
        {
            var snippet = layout == ArchiveLayout.Project && file.Name == LambdaSource.EntryName;

            var name = layout == ArchiveLayout.Lambda ? file.Name : snippet ? ProjectPaths.Snippet : ProjectPaths.Of(file.Name);

            var entry = zip.CreateEntry(name, CompressionLevel.Optimal);

            // fixed, so the same version always packs to the same bytes
            entry.LastWriteTime = Timestamp;

            using var content = entry.Open();

            if (snippet)
            {
                // a clone has the snippet in the class it is the body of
                content.Write(Encoding.UTF8.GetBytes(ProjectSnippet.ForRepository(file.ReadText())));
            }
            else
            {
                using var source = file.OpenRead();

                source.CopyTo(content);
            }
        }
    }

    #endregion

    #region Unpacking

    /// <summary>
    /// Reads the files from an uploaded zip archive.
    /// </summary>
    /// <remarks>
    /// What a commit of the files would hold is what is read. Folders, a git
    /// repository and the metadata left by operating systems are skipped, and
    /// so are hidden files among the resources, which nothing serves. The
    /// code may hold the files of any tool, whose dot files - a .gitignore
    /// among them - are part of it, so they stay. If everything sits in a
    /// single top level folder (as when a folder is zipped rather than its
    /// contents), that folder is removed. Text stays text, anything else is
    /// carried as base64.
    ///
    /// A folder of the code has often been built in, so what its .gitignore
    /// files leave out is left out here as well - what a build installed,
    /// cached and wrote for itself - the way git add leaves it out of a
    /// commit, whatever the tool calls it; so is what the platform's own
    /// .gitignore of a clone leaves out at the top. A file the lambda already
    /// has is kept all the same, as git keeps a file it tracks.
    /// </remarks>
    /// <param name="content">The archive</param>
    /// <param name="maxBytes">How many uncompressed bytes the archive may hold in total</param>
    /// <param name="basis">The files the archive was made from: the newest version's, or the feature's</param>
    /// <param name="staging">Where the archive and its files wait until the lambda keeps them</param>
    public static async ValueTask<StagedFiles> UnpackAsync(Stream content, long maxBytes, IReadOnlyList<VersionFile> basis, string staging)
    {
        var staged = StagedFiles.In(staging);

        try
        {
            await using var body = await BufferAsync(content, maxBytes, staged.Folder);

            staged.Files = await Offload.Run(() => FilesOfAsync(body, maxBytes, basis, staged.Folder));

            return staged;
        }
        catch
        {
            staged.Dispose();
            throw;
        }
    }

    private static async Task<IReadOnlyList<VersionFile>> FilesOfAsync(Stream body, long maxBytes, IReadOnlyList<VersionFile> basis, string folder)
    {
        var read = await ReadAsync(body, maxBytes, Lambda, basis.Select(f => f.Name), folder);

        return Ordered(read.Select(r => ToFile(r.Path, r.File)).ToList(), []);
    }

    /// <summary>
    /// Reads the files from an uploaded zip archive laid out as a clone of
    /// the lambda, back into the lambda's.
    /// </summary>
    /// <remarks>
    /// What a commit of the clone would hold is what is read, as for the
    /// lambda's own layout - what dotnet run wrote into bin/ and obj/ left
    /// out with the rest of what the .gitignore files say. The platform's own
    /// files are skipped rather than read, being the clone's and no part of
    /// the lambda.
    ///
    /// The files are named as the lambda they came from names them - a
    /// Store.cs that was store.cs stays store.cs - and Project.cs that is
    /// what that lambda's snippet becomes is that snippet, to the byte. A
    /// file the lambda already has is kept whatever a .gitignore says, as
    /// git keeps a file it tracks.
    /// </remarks>
    /// <param name="basis">The files the archive was made from: the newest version's, or the feature's</param>
    /// <param name="staging">Where the archive and its files wait until the lambda keeps them</param>
    public static async ValueTask<StagedFiles> UnpackProjectAsync(Stream content, long maxBytes, IReadOnlyList<VersionFile> basis, string staging)
    {
        var staged = StagedFiles.In(staging);

        try
        {
            await using var body = await BufferAsync(content, maxBytes, staged.Folder);

            // a snippet taken out of its class is parsed, so all of it is work
            staged.Files = await Offload.Run(() => ProjectFilesOfAsync(body, maxBytes, basis, staged.Folder));

            return staged;
        }
        catch
        {
            staged.Dispose();
            throw;
        }
    }

    private static async Task<IReadOnlyList<VersionFile>> ProjectFilesOfAsync(Stream body, long maxBytes, IReadOnlyList<VersionFile> basis, string folder)
    {
        var names = basis.ToDictionary(f => ProjectPaths.Of(f.Name), f => f.Name, StringComparer.Ordinal);

        var read = await ReadAsync(body, maxBytes, Project, names.Keys, folder);

        var files = new List<VersionFile>(read.Count);

        string? project = null;

        foreach (var (path, file) in read)
        {
            var kind = ProjectPaths.Classify(path);

            if (kind.Kind == ProjectPathKind.Generated)
            {
                continue;
            }

            if (kind.Name == LambdaSource.EntryName)
            {
                project = TryText(File.ReadAllBytes(file), out var text) ? text : throw LambdaException.Invalid($"{ProjectPaths.Snippet} is not UTF-8 text.");
                continue;
            }

            files.Add(ToFile(names.GetValueOrDefault(path, kind.Name!), file));
        }

        if (project == null)
        {
            throw LambdaException.Invalid($"The archive has no {ProjectPaths.Snippet}, which holds the code of the lambda.");
        }

        files.Insert(0, VersionFile.FromText(LambdaSource.EntryName, Snippet(project, basis)));

        return Ordered(files, basis);
    }

    /// <summary>
    /// The snippet a Project.cs holds: the one of the files it was made from,
    /// where it is what that one becomes, and taken out of the class otherwise.
    /// </summary>
    private static string Snippet(string project, IReadOnlyList<VersionFile> basis)
    {
        if (basis.FirstOrDefault(f => f.Name == LambdaSource.EntryName) is { Binary: false } before)
        {
            var snippet = before.ReadText();

            if (ProjectSnippet.ForRepository(snippet).ReplaceLineEndings("\n") == project.ReplaceLineEndings("\n"))
            {
                return snippet;
            }
        }

        var unwrapped = ProjectSnippet.Unwrap(project);

        return unwrapped.Snippet ?? throw LambdaException.Invalid(unwrapped.Complaint!);
    }

    /// <summary>
    /// The snippet first, then the files in the order the lambda they came
    /// from had them, then the new ones by kind and name.
    /// </summary>
    private static List<VersionFile> Ordered(List<VersionFile> files, IReadOnlyList<VersionFile> basis)
    {
        var known = basis.Select((f, i) => (f.Name, i)).ToDictionary(x => x.Name, x => x.i, StringComparer.Ordinal);

        return files.OrderBy(f => f.Name == LambdaSource.EntryName ? 0 : 1)
                    .ThenBy(f => known.GetValueOrDefault(f.Name, int.MaxValue))
                    .ThenBy(f => f.IsCompiled ? 0 : f.IsCode ? 1 : 2)
                    .ThenBy(f => f.Name, StringComparer.Ordinal)
                    .ToList();
    }

    #endregion

    #region Layouts

    /// <summary>
    /// What tells the two layouts apart where an archive is read.
    /// </summary>
    /// <param name="Root">The file at the root of the layout, which says that no folder wraps it</param>
    private sealed record Layout(string Root);

    private static readonly Layout Lambda = new(LambdaSource.EntryName);

    private static readonly Layout Project = new(ProjectPaths.Snippet);

    #endregion

    #region Reading

    /// <summary>
    /// The body of the request, which reading needs to be able to seek in -
    /// written to a file beside the stores rather than held.
    /// </summary>
    /// <remarks>
    /// Read where the request is, and only then is the rest - unpacking, and
    /// holding every path against the .gitignore files - taken away from the
    /// reactor in one hop.
    /// </remarks>
    private static async ValueTask<Stream> BufferAsync(Stream content, long maxBytes, string folder)
    {
        var body = new FileStream(Path.Combine(folder, "archive.zip"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 81920,
                                  FileOptions.DeleteOnClose | FileOptions.Asynchronous);

        try
        {
            // what a build installed is the likeliest reason, and leaving it out
            // here would mean taking it in first
            await CopyAsync(content, body, maxBytes, "The archive is larger than a lambda may be. It is counted as it is sent, before any .gitignore leaves something out, so zip what a commit would hold - never what a build installed, node_modules for example.");

            body.Position = 0;

            return body;
        }
        catch
        {
            await body.DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// The files of an archive that are kept in the given layout, by their
    /// paths in it, each written into a file of its own in the folder.
    /// </summary>
    /// <param name="tracked">Paths kept whatever a .gitignore says, as they are in the lambda already</param>
    private static async ValueTask<List<(string Path, string File)>> ReadAsync(Stream body, long maxBytes, Layout layout, IEnumerable<string> tracked, string folder)
    {
        ZipArchive zip;

        try
        {
            zip = await ZipArchive.CreateAsync(body, ZipArchiveMode.Read, true, null);
        }
        catch (InvalidDataException)
        {
            throw LambdaException.Invalid("The body is not a zip archive.");
        }

        await using var _ = zip;

        var entries = new List<(string Name, ZipArchiveEntry Entry)>();

        foreach (var entry in zip.Entries)
        {
            var name = entry.FullName.Replace('\\', '/');

            if (name.EndsWith('/') || IsLitter(name))
            {
                continue;
            }

            entries.Add((name, entry));
        }

        // the wrapping folder is found among what is plainly the lambda's,
        // so a hidden file beside it does not hide that it is there
        var prefix = CommonFolder(entries.Select(e => e.Name).Where(n => !IsHidden(n)).ToList(), layout.Root);

        var kept = new List<(string Name, ZipArchiveEntry Entry)>(entries.Count);

        foreach (var (full, entry) in entries)
        {
            var name = full.StartsWith(prefix, StringComparison.Ordinal) ? full[prefix.Length..] : full;

            if (!IsHidden(name))
            {
                kept.Add((name, entry));
            }
        }

        var result = new List<(string Path, string File)>(kept.Count);

        long total = 0;

        // what the folders of the code say they do not keep, read before
        // anything they leave out would be - the files above first, so one in
        // a folder they leave out is never read, as git never reads it. The
        // one at the top is the platform's, in a clone: what it says is what
        // applies there, in either layout
        var rules = new Dictionary<string, string>(StringComparer.Ordinal);

        var ignored = IgnoredPaths.None.With(".gitignore", ProjectPacker.RepositoryIgnored);

        bool Ignores(string name) => ignored.Ignores(name);

        var known = tracked.ToHashSet(StringComparer.Ordinal);

        foreach (var (name, entry) in kept.Where(k => k.Name.Contains('/') && !LambdaSource.IsResource(k.Name) && Path.GetFileName(k.Name) == ".gitignore")
                                          .OrderBy(k => k.Name.Count(c => c == '/')))
        {
            if (Ignores(name))
            {
                continue;
            }

            var file = await StageAsync(entry, maxBytes - total, folder);

            total += new FileInfo(file).Length;

            rules[name] = file;

            ignored = ignored.With(name, Encoding.UTF8.GetString(File.ReadAllBytes(file)));
        }

        foreach (var (name, entry) in kept)
        {
            if (!known.Contains(name) && Ignores(name))
            {
                continue;
            }

            if (!rules.TryGetValue(name, out var file))
            {
                file = await StageAsync(entry, maxBytes - total, folder);

                total += new FileInfo(file).Length;
            }

            result.Add((name, file));
        }

        return result;
    }

    /// <summary>
    /// A file of the lambda from a file of the archive: text where it is
    /// UTF-8, without the byte order marks it begins with, and binary
    /// otherwise - which C# may not be.
    /// </summary>
    private static VersionFile ToFile(string name, string file)
    {
        var facts = ContentFacts.Of(file);

        if (facts.Chars == null)
        {
            if (LambdaSource.IsCompiled(name))
            {
                throw LambdaException.Invalid($"'{name}' is not UTF-8 text.");
            }

            return Staged(name, file, facts, true);
        }

        var marks = Marks(file);

        if (marks > 0)
        {
            // the text without them, which is what it is read as
            var trimmed = file + ".text";

            using (var source = File.OpenRead(file))
            using (var target = File.Create(trimmed))
            {
                source.Position = marks;
                source.CopyTo(target);
            }

            File.Delete(file);

            file = trimmed;

            facts = ContentFacts.Of(file);
        }

        return Staged(name, file, facts, false);
    }

    private static VersionFile Staged(string name, string file, ContentFacts facts, bool binary)
        => VersionFile.Stored(name, facts.Size, binary, facts.Chars, new StoredContent(file, facts.Hash, null, true, facts));

    /// <summary>
    /// How many bytes of byte order marks a file of text begins with - any
    /// number of them, as the characters they stand for were trimmed off it.
    /// </summary>
    private static int Marks(string file)
    {
        using var source = File.OpenRead(file);

        var mark = new byte[3];

        var marks = 0;

        while (source.ReadAtLeast(mark, 3, throwOnEndOfStream: false) == 3 && mark is [0xEF, 0xBB, 0xBF])
        {
            marks += 3;
        }

        return marks;
    }

    private static bool TryText(byte[] bytes, out string text)
    {
        try
        {
            text = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            text = string.Empty;
            return false;
        }

        if (text.Contains('\0'))
        {
            return false;
        }

        text = text.TrimStart('\uFEFF');
        return true;
    }

    /// <summary>
    /// Writes a file of the archive into a file of its own in the folder.
    /// </summary>
    private static async ValueTask<string> StageAsync(ZipArchiveEntry entry, long remaining, string folder)
    {
        var file = Path.Combine(folder, Guid.NewGuid().ToString("n"));

        // the sizes in the archive are whatever the sender claims, so the
        // limit is enforced on what is actually read
        await using var source = await entry.OpenAsync();

        await using var target = new FileStream(file, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);

        await CopyAsync(source, target, remaining, "The archive is larger than a lambda may be once unpacked.");

        return file;
    }

    private static async ValueTask CopyAsync(Stream source, Stream target, long limit, string complaint)
    {
        var buffer = new byte[81920];

        long total = 0;

        int read;

        while ((read = await source.ReadAsync(buffer)) > 0)
        {
            total += read;

            if (total > limit)
            {
                throw LambdaException.Invalid(complaint);
            }

            await target.WriteAsync(buffer.AsMemory(0, read));
        }
    }

    /// <summary>
    /// What an operating system leaves in an archive, wherever it is.
    /// </summary>
    private static bool IsLitter(string name)
    {
        var segments = name.Split('/');

        return segments.Contains("__MACOSX") || segments[^1] is "Thumbs.db" or "desktop.ini" or ".DS_Store";
    }

    /// <summary>
    /// Whether a file is hidden rather than part of the lambda: a repository,
    /// wherever it is, and a dot file among the resources, which nothing
    /// serves. The dot files of the code are a tool's, and kept.
    /// </summary>
    private static bool IsHidden(string name)
    {
        var segments = name.Split('/');

        if (segments.Contains(".git"))
        {
            return true;
        }

        return LambdaSource.IsResource(name) && segments.Any(s => s.StartsWith('.'));
    }

    private static string CommonFolder(List<string> names, string root)
    {
        if (names.Count == 0 || names.Contains(root))
        {
            return string.Empty;
        }

        var slash = names[0].IndexOf('/');

        if (slash < 0)
        {
            return string.Empty;
        }

        var prefix = names[0][..(slash + 1)];

        return names.All(n => n.StartsWith(prefix, StringComparison.Ordinal)) ? prefix : string.Empty;
    }

    #endregion

}

/// <summary>
/// The files of an archive that was sent, each waiting in a file of its own
/// beside the stores until the lambda keeps it - which moves it in.
/// </summary>
/// <remarks>
/// Disposed of once the request is answered: what was kept was moved, and
/// what is left - an archive that was refused, a file the lambda had - goes.
/// </remarks>
public sealed class StagedFiles : IDisposable
{

    private StagedFiles(string folder)
    {
        Folder = folder;
    }

    /// <summary>
    /// The folder they wait in, one per archive.
    /// </summary>
    public string Folder { get; }

    /// <summary>
    /// The files, in the order of a lambda.
    /// </summary>
    public IReadOnlyList<VersionFile> Files { get; internal set; } = [];

    internal static StagedFiles In(string staging)
    {
        var folder = Path.Combine(staging, Guid.NewGuid().ToString("n"));

        Directory.CreateDirectory(folder);

        return new StagedFiles(folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Folder, true);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // still open somewhere: the next start empties the staging
        }
    }

}
