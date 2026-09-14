using System.Text.Json;

using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// One file of a lambda: either C# to compile, or an asset to serve.
/// </summary>
/// <param name="Name">What it is called, which is also what diagnostics name</param>
/// <param name="Code">Its contents, base64 when <paramref name="Encoding" /> says so</param>
/// <param name="Encoding">"base64" for a file that is not text, absent otherwise</param>
public sealed record LambdaFile(string Name, string Code, string? Encoding = null)
{

    /// <summary>Whether this is C# rather than something to serve.</summary>
    public bool IsCode => LambdaSource.IsCode(Name);

    /// <summary>The bytes of an asset, whatever it was sent as.</summary>
    public byte[] Bytes => Encoding == "base64"
                         ? Convert.FromBase64String(Code)
                         : System.Text.Encoding.UTF8.GetBytes(Code);

}

/// <summary>
/// The source of a lambda: an entry file, and whatever else it was split into.
/// </summary>
/// <remarks>
/// A lambda is stored as one blob of text and always was, so the several files
/// are kept in that one blob as a small envelope rather than in a new column
/// and a new directory layout. Anything written before this existed is not an
/// envelope, is read as a single entry file, and never has to be migrated.
///
/// The first file is the snippet - the statements that return a handler. The
/// rest are ordinary C#: types, and nothing that has to run.
/// </remarks>
public static class LambdaSource
{

    /// <summary>
    /// The file the snippet lives in, which is the one that returns a handler.
    /// </summary>
    public const string EntryName = "lambda.cs";

    /// <summary>
    /// How many C# files one lambda may be split into. Assets are not counted:
    /// a frontend is many small files and none of them is a reason to run out.
    /// </summary>
    public const int MaxFiles = 12;

    /// <summary>
    /// How many assets one lambda may ship.
    /// </summary>
    public const int MaxAssets = 60;

    /// <summary>Whether a name is C# rather than something to serve.</summary>
    public static bool IsCode(string? name) => name?.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) == true;

    private static readonly JsonSerializerOptions Format = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Marks the stored text as an envelope rather than a snippet.
    /// </summary>
    /// <remarks>
    /// No C# begins this way, so a blob that starts with it was written by this
    /// and one that does not is code from before there was more than one file.
    /// </remarks>
    private const string Marker = "{\"version\":1,\"files\":";

    #region Functionality

    /// <summary>
    /// Reads stored text as the files it holds.
    /// </summary>
    public static IReadOnlyList<LambdaFile> Parse(string? stored)
    {
        var text = stored ?? string.Empty;

        if (!text.StartsWith(Marker, StringComparison.Ordinal))
        {
            return [new LambdaFile(EntryName, text)];
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<Envelope>(text, Format);

            if (envelope?.Files is { Count: > 0 } files)
            {
                return files;
            }
        }
        catch (JsonException)
        {
            // text that starts like an envelope but is not one is still
            // somebody's code, and losing it would be worse than compiling it
        }

        return [new LambdaFile(EntryName, text)];
    }

    /// <summary>
    /// Writes the files as the single blob a version is stored as.
    /// </summary>
    /// <remarks>
    /// A lambda that is still one file is stored as that file and nothing else,
    /// so the common case stays readable on disk and anything reading an older
    /// version cannot tell the difference.
    /// </remarks>
    public static string Serialize(IReadOnlyList<LambdaFile> files)
    {
        if (files.Count == 1 && files[0].Name == EntryName)
        {
            return files[0].Code;
        }

        return JsonSerializer.Serialize(new Envelope(1, files), Format);
    }

    /// <summary>
    /// The combined length of the code, which is what the size limit counts.
    /// </summary>
    /// <remarks>
    /// Assets are left out on purpose. They are not compiled, and a page of
    /// markup charged against the budget for the program that serves it makes
    /// the budget wrong for both of them.
    /// </remarks>
    public static int Length(IReadOnlyList<LambdaFile> files)
    {
        var total = 0;

        foreach (var file in files)
        {
            if (file.IsCode)
            {
                total += file.Code.Length;
            }
        }

        return total;
    }

    /// <summary>
    /// How many bytes of assets are shipped, decoded rather than as sent.
    /// </summary>
    public static long AssetBytes(IReadOnlyList<LambdaFile> files)
    {
        long total = 0;

        foreach (var file in files)
        {
            if (!file.IsCode)
            {
                try
                {
                    total += file.Bytes.Length;
                }
                catch (FormatException)
                {
                    // a file that is not the base64 it claims to be is caught
                    // by Validate; here it simply counts for nothing
                }
            }
        }

        return total;
    }

    /// <summary>
    /// Checks the shape of what was submitted, and says what is wrong with it.
    /// </summary>
    /// <returns>The complaint, or null where there is none</returns>
    public static string? Validate(IReadOnlyList<LambdaFile>? files)
    {
        if (files is not { Count: > 0 })
        {
            return "A lambda needs at least one file.";
        }

        if (files.Count(f => f.IsCode) > MaxFiles)
        {
            return $"A lambda may be split into at most {MaxFiles} C# files. Assets do not count towards that.";
        }

        if (files.Count(f => !f.IsCode) > MaxAssets)
        {
            return $"A lambda may ship at most {MaxAssets} assets.";
        }

        if (files[0].Name != EntryName)
        {
            return $"The first file has to be '{EntryName}', which is the one that returns a handler.";
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            if (file.IsCode)
            {
                if (!IsValidName(file.Name))
                {
                    return $"'{file.Name}' is not a usable name for a C# file. Use letters, digits, dashes and underscores, ending in '.cs'.";
                }
            }
            else if (!IsValidAssetName(file.Name))
            {
                return $"'{file.Name}' is not a usable name for an asset. Use letters, digits, dashes, underscores, dots and slashes, and no leading or doubled slashes.";
            }

            if (file.Encoding is not (null or "" or "text" or "base64"))
            {
                return $"'{file.Name}' asks for encoding '{file.Encoding}'. Only 'base64' is understood; leave it out for text.";
            }

            if (file.Encoding == "base64")
            {
                try
                {
                    _ = Convert.FromBase64String(file.Code);
                }
                catch (FormatException)
                {
                    return $"'{file.Name}' says it is base64 and is not.";
                }
            }

            if (!seen.Add(file.Name))
            {
                return $"There is more than one file called '{file.Name}'.";
            }
        }

        return null;
    }

    /// <summary>
    /// Whether a name is one an asset may have.
    /// </summary>
    /// <remarks>
    /// An asset becomes a real file in a real directory, so this is about
    /// where it can end up rather than about how it reads. Segments are
    /// checked one at a time and '..' is not one of them, because the whole
    /// point of a relative path is that it can leave.
    /// </remarks>
    public static bool IsValidAssetName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 120 || name.StartsWith('/') || name.EndsWith('/'))
        {
            return false;
        }

        var segments = name.Split('/');

        if (segments.Length > 6)
        {
            return false;
        }

        foreach (var segment in segments)
        {
            if (segment.Length is 0 or > 60 || segment is "." or "..")
            {
                return false;
            }

            if (segment.StartsWith('.'))
            {
                return false;
            }

            foreach (var character in segment)
            {
                if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.'))
                {
                    return false;
                }
            }
        }

        // something to infer a content type from; a file with no extension
        // would be served as a download and surprise whoever shipped it
        return Path.GetExtension(name).Length > 1;
    }

    /// <summary>
    /// Whether a name is one a C# file may have.
    /// </summary>
    /// <remarks>
    /// Deliberately narrow. The name reaches a <c>#line</c> directive and a
    /// diagnostic, and a name with a quote or a path separator in it would
    /// either break the generated file or point somewhere it should not.
    /// </remarks>
    public static bool IsValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 40 || !name.EndsWith(".cs", StringComparison.Ordinal))
        {
            return false;
        }

        var stem = name[..^3];

        if (stem.Length == 0 || !char.IsAsciiLetter(stem[0]))
        {
            return false;
        }

        foreach (var character in stem)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_'))
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Types

    private sealed record Envelope(int Version, IReadOnlyList<LambdaFile> Files);

    #endregion

}
