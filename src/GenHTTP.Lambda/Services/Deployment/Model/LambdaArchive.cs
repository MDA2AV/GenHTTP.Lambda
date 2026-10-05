using System.IO.Compression;
using System.Text;

using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// The files of a version as a zip archive, one entry per file.
/// </summary>
/// <remarks>
/// Lets a caller fetch a version, change it locally and send it back in a
/// single request instead of shipping file by file. Unlike the export, the
/// archive holds the files exactly as they are named in the lambda, so it
/// can be uploaded again as it is.
/// </remarks>
public static class LambdaArchive
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private static readonly DateTimeOffset Timestamp = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Packs the given files into a zip archive.
    /// </summary>
    public static byte[] Pack(IReadOnlyList<LambdaFile> files)
    {
        using var buffer = new MemoryStream();

        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var entry = zip.CreateEntry(file.Name, CompressionLevel.Optimal);

                // fixed, so the same version always packs to the same bytes
                entry.LastWriteTime = Timestamp;

                using var target = entry.Open();

                target.Write(file.Bytes);
            }
        }

        return buffer.ToArray();
    }

    /// <summary>
    /// Reads the files from an uploaded zip archive.
    /// </summary>
    /// <remarks>
    /// Folders, hidden files and the metadata left by operating systems are
    /// skipped - except <c>.lambda/</c> at the root, which is the version's
    /// documentation, tests and development space rather than something an
    /// editor or a tool left behind. If everything sits in a single top level
    /// folder (as when a folder is zipped rather than its contents), that
    /// folder is removed. Text stays text, anything else is carried as base64.
    ///
    /// The development space is a project, whose dot files - its .gitignore,
    /// its .npmrc - are part of it, so they stay. It has usually been built
    /// in, too, so what its .gitignore files leave out is left out here as
    /// well - what the build installed, cached and made - the way git add
    /// leaves it out of a commit, whatever the toolchain calls it.
    /// </remarks>
    /// <param name="content">The archive</param>
    /// <param name="maxBytes">How many uncompressed bytes the archive may hold in total</param>
    public static async ValueTask<IReadOnlyList<LambdaFile>> UnpackAsync(Stream content, long maxBytes)
    {
        // reading needs a seekable stream, and copying the body ourselves
        // keeps what is buffered bounded
        using var body = new MemoryStream();

        await CopyAsync(content, body, maxBytes, "The archive is larger than a lambda may be.");

        body.Position = 0;

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
        var prefix = CommonFolder(entries.Select(e => e.Name).Where(n => !IsHidden(n)).ToList());

        var kept = new List<(string Name, ZipArchiveEntry Entry)>(entries.Count);

        foreach (var (full, entry) in entries)
        {
            var name = full.StartsWith(prefix, StringComparison.Ordinal) ? full[prefix.Length..] : full;

            if (!IsHidden(name))
            {
                kept.Add((name, entry));
            }
        }

        var files = new List<LambdaFile>(kept.Count);

        long total = 0;

        // what the development space says it does not keep, read before
        // anything it leaves out would be - the files above first, so one in
        // a folder they leave out is never read, as git never reads it
        var rules = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        var ignored = IgnoredPaths.None;

        foreach (var (name, entry) in kept.Where(k => LambdaSource.IsDevelopment(k.Name) && Path.GetFileName(k.Name) == ".gitignore")
                                          .OrderBy(k => k.Name.Count(c => c == '/')))
        {
            if (ignored.Ignores(name[LambdaSource.DevelopmentFolder.Length..]))
            {
                continue;
            }

            var bytes = await ReadAsync(entry, maxBytes - total);

            total += bytes.Length;

            rules[name] = bytes;

            ignored = IgnoredPaths.Of(rules.Select(r => (r.Key[LambdaSource.DevelopmentFolder.Length..], Encoding.UTF8.GetString(r.Value))));
        }

        foreach (var (name, entry) in kept)
        {
            if (LambdaSource.IsDevelopment(name) && ignored.Ignores(name[LambdaSource.DevelopmentFolder.Length..]))
            {
                continue;
            }

            if (!rules.TryGetValue(name, out var bytes))
            {
                bytes = await ReadAsync(entry, maxBytes - total);

                total += bytes.Length;
            }

            files.Add(ToFile(name, bytes));
        }

        return files.OrderBy(f => f.Name == LambdaSource.EntryName ? 0 : f.IsCode ? 1 : f.IsAsset ? 2 : f.IsContext ? 3 : 4)
                    .ThenBy(f => f.Name, StringComparer.Ordinal)
                    .ToList();
    }

    private static LambdaFile ToFile(string name, byte[] bytes)
    {
        if (TryText(bytes, out var text))
        {
            return new LambdaFile(name, text);
        }

        if (LambdaSource.IsCode(name))
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
    /// Whether a file is hidden - a repository, an editor's settings - rather
    /// than part of the lambda. What is kept beside the program at the root
    /// is not, and in the development space only a repository is.
    /// </summary>
    private static bool IsHidden(string name)
    {
        var segments = name.Split('/');

        if (LambdaSource.IsDevelopment(name))
        {
            return segments.Contains(".git");
        }

        var start = LambdaSource.IsBeside(name) ? 1 : 0;

        for (var i = start; i < segments.Length; i++)
        {
            if (segments[i].StartsWith('.'))
            {
                return true;
            }
        }

        return false;
    }

    private static string CommonFolder(List<string> names)
    {
        if (names.Count == 0 || names.Contains(LambdaSource.EntryName))
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

}
