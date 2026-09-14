using System.Text.Json;

using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// One file of a lambda.
/// </summary>
/// <param name="Name">What it is called, which is also what diagnostics name</param>
/// <param name="Code">Its contents</param>
public sealed record LambdaFile(string Name, string Code);

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
    /// How many files one lambda may be split into.
    /// </summary>
    public const int MaxFiles = 12;

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
    /// The combined length of every file, which is what the size limit counts.
    /// </summary>
    public static int Length(IReadOnlyList<LambdaFile> files)
    {
        var total = 0;

        foreach (var file in files)
        {
            total += file.Code.Length;
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

        if (files.Count > MaxFiles)
        {
            return $"A lambda may be split into at most {MaxFiles} files.";
        }

        if (files[0].Name != EntryName)
        {
            return $"The first file has to be '{EntryName}', which is the one that returns a handler.";
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            if (!IsValidName(file.Name))
            {
                return $"'{file.Name}' is not a usable file name. Use letters, digits, dashes and underscores, ending in '.cs'.";
            }

            if (!seen.Add(file.Name))
            {
                return $"There is more than one file called '{file.Name}'.";
            }
        }

        return null;
    }

    /// <summary>
    /// Whether a name is one a file may have.
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
