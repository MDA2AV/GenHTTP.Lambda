using System.IO.Compression;
using System.Text;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Meta;

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
/// </remarks>
public static class LambdaArchive
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private static readonly DateTimeOffset Timestamp = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    #region Packing

    /// <summary>
    /// Packs the given files into a zip archive.
    /// </summary>
    public static byte[] Pack(IReadOnlyList<LambdaFile> files, ArchiveLayout layout = ArchiveLayout.Lambda)
    {
        using var buffer = new MemoryStream();

        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var (name, content) = layout == ArchiveLayout.Lambda ? (file.Name, file.Bytes) : InProject(file);

                var entry = zip.CreateEntry(name, CompressionLevel.Optimal);

                // fixed, so the same version always packs to the same bytes
                entry.LastWriteTime = Timestamp;

                using var target = entry.Open();

                target.Write(content);
            }
        }

        return buffer.ToArray();
    }

    /// <summary>
    /// A file of the lambda where a clone has it: the snippet in the class
    /// it is the body of, the rest where <see cref="ProjectPaths"/> puts it.
    /// </summary>
    private static (string Path, byte[] Content) InProject(LambdaFile file)
        => file.Name == LambdaSource.EntryName
            ? (ProjectPaths.Snippet, Encoding.UTF8.GetBytes(ProjectSnippet.ForRepository(file.Code)))
            : (ProjectPaths.Of(file.Name), file.Bytes);

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
    public static async ValueTask<IReadOnlyList<LambdaFile>> UnpackAsync(Stream content, long maxBytes, IReadOnlyList<LambdaFile> basis)
    {
        using var body = await BufferAsync(content, maxBytes);

        return await Offload.Run(() => UnpackAsync(body, maxBytes, basis));
    }

    private static async Task<IReadOnlyList<LambdaFile>> UnpackAsync(MemoryStream body, long maxBytes, IReadOnlyList<LambdaFile> basis)
    {
        var read = await ReadAsync(body, maxBytes, Lambda, basis.Select(f => f.Name));

        return Ordered(read.Select(r => ToFile(r.Path, r.Bytes)).ToList(), []);
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
    public static async ValueTask<IReadOnlyList<LambdaFile>> UnpackProjectAsync(Stream content, long maxBytes, IReadOnlyList<LambdaFile> basis)
    {
        using var body = await BufferAsync(content, maxBytes);

        // a snippet taken out of its class is parsed, so all of it is work
        return await Offload.Run(() => UnpackProjectAsync(body, maxBytes, basis));
    }

    private static async Task<IReadOnlyList<LambdaFile>> UnpackProjectAsync(MemoryStream body, long maxBytes, IReadOnlyList<LambdaFile> basis)
    {
        var names = basis.ToDictionary(f => ProjectPaths.Of(f.Name), f => f.Name, StringComparer.Ordinal);

        var read = await ReadAsync(body, maxBytes, Project, names.Keys);

        var files = new List<LambdaFile>(read.Count);

        string? project = null;

        foreach (var (path, bytes) in read)
        {
            var kind = ProjectPaths.Classify(path);

            if (kind.Kind == ProjectPathKind.Generated)
            {
                continue;
            }

            if (kind.Name == LambdaSource.EntryName)
            {
                project = TryText(bytes, out var text) ? text : throw LambdaException.Invalid($"{ProjectPaths.Snippet} is not UTF-8 text.");
                continue;
            }

            files.Add(ToFile(names.GetValueOrDefault(path, kind.Name!), bytes));
        }

        if (project == null)
        {
            throw LambdaException.Invalid($"The archive has no {ProjectPaths.Snippet}, which holds the code of the lambda.");
        }

        files.Insert(0, new LambdaFile(LambdaSource.EntryName, Snippet(project, basis)));

        return Ordered(files, basis);
    }

    /// <summary>
    /// The snippet a Project.cs holds: the one of the files it was made from,
    /// where it is what that one becomes, and taken out of the class otherwise.
    /// </summary>
    private static string Snippet(string project, IReadOnlyList<LambdaFile> basis)
    {
        if (basis.FirstOrDefault(f => f.Name == LambdaSource.EntryName) is { } before
            && ProjectSnippet.ForRepository(before.Code).ReplaceLineEndings("\n") == project.ReplaceLineEndings("\n"))
        {
            return before.Code;
        }

        var unwrapped = ProjectSnippet.Unwrap(project);

        return unwrapped.Snippet ?? throw LambdaException.Invalid(unwrapped.Complaint!);
    }

    /// <summary>
    /// The snippet first, then the files in the order the lambda they came
    /// from had them, then the new ones by kind and name.
    /// </summary>
    private static List<LambdaFile> Ordered(List<LambdaFile> files, IReadOnlyList<LambdaFile> basis)
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
    /// The body of the request, which reading needs to be able to seek in.
    /// </summary>
    /// <remarks>
    /// Read where the request is, and only then is the rest - unpacking, and
    /// holding every path against the .gitignore files - taken away from the
    /// reactor in one hop.
    /// </remarks>
    private static async ValueTask<MemoryStream> BufferAsync(Stream content, long maxBytes)
    {
        // copying the body ourselves keeps what is buffered bounded
        var body = new MemoryStream();

        // what a build installed is the likeliest reason, and leaving it out
        // here would mean taking it in first
        await CopyAsync(content, body, maxBytes, "The archive is larger than a lambda may be. It is counted as it is sent, before any .gitignore leaves something out, so zip what a commit would hold - never what a build installed, node_modules for example.");

        body.Position = 0;

        return body;
    }

    /// <summary>
    /// The files of an archive that are kept in the given layout, by their
    /// paths in it.
    /// </summary>
    /// <param name="tracked">Paths kept whatever a .gitignore says, as they are in the lambda already</param>
    private static async ValueTask<List<(string Path, byte[] Bytes)>> ReadAsync(MemoryStream body, long maxBytes, Layout layout, IEnumerable<string> tracked)
    {
        ZipArchive zip;

        try
        {
            zip = await ZipArchive.CreateAsync(body, ZipArchiveMode.Read, false, null);
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

        var result = new List<(string Path, byte[] Bytes)>(kept.Count);

        long total = 0;

        // what the folders of the code say they do not keep, read before
        // anything they leave out would be - the files above first, so one in
        // a folder they leave out is never read, as git never reads it. The
        // one at the top is the platform's, in a clone: what it says is what
        // applies there, in either layout
        var rules = new Dictionary<string, byte[]>(StringComparer.Ordinal);

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

            var bytes = await ReadAsync(entry, maxBytes - total);

            total += bytes.Length;

            rules[name] = bytes;

            ignored = ignored.With(name, Encoding.UTF8.GetString(bytes));
        }

        foreach (var (name, entry) in kept)
        {
            if (!known.Contains(name) && Ignores(name))
            {
                continue;
            }

            if (!rules.TryGetValue(name, out var bytes))
            {
                bytes = await ReadAsync(entry, maxBytes - total);

                total += bytes.Length;
            }

            result.Add((name, bytes));
        }

        return result;
    }

    private static LambdaFile ToFile(string name, byte[] bytes)
    {
        if (TryText(bytes, out var text))
        {
            return new LambdaFile(name, text);
        }

        if (LambdaSource.IsCompiled(name))
        {
            throw LambdaException.Invalid($"'{name}' is not UTF-8 text.");
        }

        return new LambdaFile(name, Convert.ToBase64String(bytes), "base64");
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

        text = text.TrimStart('﻿');
        return true;
    }

    private static async ValueTask<byte[]> ReadAsync(ZipArchiveEntry entry, long remaining)
    {
        // the sizes in the archive are whatever the sender claims, so the
        // limit is enforced on what is actually read
        await using var source = await entry.OpenAsync();

        using var target = new MemoryStream();

        await CopyAsync(source, target, remaining, "The archive is larger than a lambda may be once unpacked.");

        return target.ToArray();
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
