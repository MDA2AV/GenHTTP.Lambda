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
    /// skipped. If everything sits in a single top level folder (as when a
    /// folder is zipped rather than its contents), that folder is removed.
    /// Text stays text, anything else is carried as base64.
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

            if (name.EndsWith('/') || IsIgnored(name))
            {
                continue;
            }

            entries.Add((name, entry));

            if (entries.Count > LambdaSource.MaxFiles + LambdaSource.MaxAssets)
            {
                throw LambdaException.Invalid($"The archive holds more files than a lambda may have ({LambdaSource.MaxFiles} C# files and {LambdaSource.MaxAssets} assets).");
            }
        }

        var prefix = CommonFolder(entries.Select(e => e.Name).ToList());

        var files = new List<LambdaFile>(entries.Count);

        long total = 0;

        foreach (var (name, entry) in entries)
        {
            var bytes = await ReadAsync(entry, maxBytes - total);

            total += bytes.Length;

            files.Add(ToFile(name[prefix.Length..], bytes));
        }

        return files.OrderBy(f => f.Name == LambdaSource.EntryName ? 0 : f.IsCode ? 1 : 2)
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

    private static bool IsIgnored(string name)
    {
        var segments = name.Split('/');

        return segments.Any(s => s.StartsWith('.') || s == "__MACOSX")
            || segments[^1] is "Thumbs.db" or "desktop.ini";
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
