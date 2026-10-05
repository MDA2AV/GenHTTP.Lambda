using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// One file of a lambda: C# to compile, an asset to serve, part of what is
/// written about it - its documentation and its tests - or part of what its
/// assets are built from, its development space.
/// </summary>
/// <param name="Name">What it is called, which is also what diagnostics name</param>
/// <param name="Code">Its contents, base64 when <paramref name="Encoding" /> says so</param>
/// <param name="Encoding">"base64" for a file that is not text, absent otherwise</param>
public sealed record LambdaFile(string Name, [property: JsonConverter(typeof(LongStringConverter))] string Code, string? Encoding = null)
{

    /// <summary>Whether this is C# rather than something to serve.</summary>
    /// <remarks>
    /// Not serialised, along with Bytes and the other kinds. This record is
    /// the API's shape as well as the storage one, and all of these are
    /// worked out from what is already there - so sending them put a second,
    /// base64 copy of every file into every response that carried one,
    /// roughly doubling it, to say something the caller could see for itself
    /// from the name.
    /// </remarks>
    [JsonIgnore]
    public bool IsCode => LambdaSource.IsCode(Name);

    /// <summary>Whether this is documentation or a test rather than part of the program.</summary>
    [JsonIgnore]
    public bool IsContext => LambdaSource.IsContext(Name);

    /// <summary>Whether this is part of what the assets are built from rather than part of the program.</summary>
    [JsonIgnore]
    public bool IsDevelopment => LambdaSource.IsDevelopment(Name);

    /// <summary>Whether this is shipped to be served: neither code nor kept beside the program.</summary>
    [JsonIgnore]
    public bool IsAsset => LambdaSource.IsAsset(Name);

    /// <summary>The bytes of an asset, whatever it was sent as.</summary>
    [JsonIgnore]
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
///
/// Beside the program, a version keeps what is written about it - its
/// context - under <c>.lambda/</c>: its documentation in <c>docs/</c> and how
/// it is tested in <c>tests/</c>. Those files are the version's like any
/// other - saved, compared, rolled back, copied into a feature and merged with
/// it - because they describe that version of the program. They are never
/// compiled and never served, whatever they are called: a test script ending
/// in <c>.cs</c> is not code, and a page of documentation is not an asset.
/// A dot folder, because the root of a version is the root of what its assets
/// are served from, where a leading dot is the convention for "not served" -
/// and because no asset has ever been allowed a name starting with one, so no
/// version saved before this can have meant anything else by it.
///
/// The same folder holds the development space in <c>dev/</c>: the files the
/// assets, or the code, are built from with a build tool - whatever that
/// tool works from, which nothing here assumes anything about. Whoever
/// builds it - an agent, where it works - keeps those files and what it
/// built in the same version; the platform never builds anything. Kept like
/// the context and never compiled or served either, but a kind of its own:
/// files for a tool rather than pages, with the names such files have - dot
/// files included - and read rather than written by the editor.
/// </remarks>
public static class LambdaSource
{

    /// <summary>
    /// The file the snippet lives in, which is the one that returns a handler.
    /// </summary>
    public const string EntryName = "lambda.cs";

    #region Beside the program

    /// <summary>
    /// Where a version keeps what is not its program: what is written about
    /// it, and what its assets are built from.
    /// </summary>
    public const string LambdaFolder = ".lambda/";

    /// <summary>
    /// The documentation of a version: what it is and why, and how it is built and why.
    /// </summary>
    public const string DocsFolder = ".lambda/docs/";

    /// <summary>
    /// How a version is tested, and the scripts and data the tests use.
    /// </summary>
    public const string TestsFolder = ".lambda/tests/";

    /// <summary>
    /// What the app is, who it is for, why it exists and what people do with
    /// it - in the terms of the people who asked for it. The one page of the
    /// context that the owner of an app they had built reads as well.
    /// </summary>
    public const string ProductDoc = ".lambda/docs/product.md";

    /// <summary>
    /// The technical decisions behind the program, and why they were made.
    /// </summary>
    public const string DecisionsDoc = ".lambda/docs/decisions.md";

    /// <summary>
    /// How the app is tested automatically: what is checked, how, and how the
    /// scripts beside it are run.
    /// </summary>
    public const string TestingDoc = ".lambda/tests/README.md";

    /// <summary>
    /// The pages every version is meant to have, in the order they are read.
    /// </summary>
    public static readonly IReadOnlyList<string> ExpectedContext = [ProductDoc, DecisionsDoc, TestingDoc];

    /// <summary>
    /// The development space of a version: the files its assets, or its code,
    /// are built from with a build tool.
    /// </summary>
    public const string DevelopmentFolder = ".lambda/dev/";

    /// <summary>
    /// How the development space is built and where the build goes, in a few
    /// lines - the page the editor opens it on, and the one an agent is handed.
    /// </summary>
    public const string DevelopmentReadme = ".lambda/dev/README.md";

    #endregion

    /// <summary>Whether a name is C# to compile, rather than something to serve or kept beside the program.</summary>
    public static bool IsCode(string? name) => name?.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) == true && !IsBeside(name);

    /// <summary>Whether a name belongs to the context of a version: its documentation or its tests.</summary>
    public static bool IsContext(string? name) => IsBeside(name) && !IsDevelopment(name);

    /// <summary>Whether a name belongs to the development space of a version: what its assets are built from.</summary>
    public static bool IsDevelopment(string? name) => name?.StartsWith(DevelopmentFolder, StringComparison.Ordinal) == true;

    /// <summary>Whether a name is an asset: shipped with the program and served when the code asks.</summary>
    public static bool IsAsset(string? name) => name != null && !IsCode(name) && !IsBeside(name);

    /// <summary>
    /// Whether a name is kept beside the program rather than being part of it:
    /// the context or the development space, never compiled, never served and
    /// left out of what identifies a build.
    /// </summary>
    public static bool IsBeside(string? name) => name?.StartsWith(LambdaFolder, StringComparison.Ordinal) == true;

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
    /// <remarks>
    /// Measured without decoding anything: this is asked of every version on
    /// its way in, and a premium lambda may ship a hundred megabytes that
    /// would otherwise be decoded just to be counted and thrown away.
    /// </remarks>
    public static long AssetBytes(IReadOnlyList<LambdaFile> files)
    {
        long total = 0;

        foreach (var file in files)
        {
            if (file.IsAsset)
            {
                total += Bytes(file);
            }
        }

        return total;
    }

    /// <summary>
    /// How many bytes the documentation and the tests of a version come to.
    /// </summary>
    /// <remarks>
    /// Held to the same allowance as the assets. The allowance is there
    /// because every version carries its own copy of everything that is not
    /// code and is read whole to be saved - which is as true of a test's data
    /// as of a picture a page shows.
    /// </remarks>
    public static long ContextBytes(IReadOnlyList<LambdaFile> files)
    {
        long total = 0;

        foreach (var file in files)
        {
            if (file.IsContext)
            {
                total += Bytes(file);
            }
        }

        return total;
    }

    /// <summary>
    /// How many bytes the development space of a version comes to.
    /// </summary>
    /// <remarks>
    /// Held to the allowance of the assets as well, for the same reason: a
    /// copy in every version. What a project is - its sources and its lock
    /// file - is small beside what it installs, which is never kept.
    /// </remarks>
    public static long DevelopmentBytes(IReadOnlyList<LambdaFile> files)
    {
        long total = 0;

        foreach (var file in files)
        {
            if (file.IsDevelopment)
            {
                total += Bytes(file);
            }
        }

        return total;
    }

    private static long Bytes(LambdaFile file)
    {
        if (file.Encoding == "base64")
        {
            // a file that is not the base64 it claims to be is caught by
            // Validate; here it simply counts for nothing
            return Base64.IsValid(file.Code, out var decoded) ? decoded : 0;
        }

        return System.Text.Encoding.UTF8.GetByteCount(file.Code);
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

        if (files[0].Name != EntryName)
        {
            return $"The first file has to be '{EntryName}', which is the one that returns a handler.";
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            if (file.Name.StartsWith(".lambda", StringComparison.OrdinalIgnoreCase))
            {
                // said apart from code and assets, because somebody writing
                // here meant what is kept beside the program and should be
                // told where in it things go - a C# file included, which is a
                // test or a part of a project here
                if (IsDevelopment(file.Name))
                {
                    if (DevelopmentComplaint(file.Name) is { } complaint)
                    {
                        return complaint;
                    }
                }
                else if (!IsValidContextName(file.Name))
                {
                    return $"'{file.Name}' is not a usable name. {LambdaFolder} holds the documentation in {DocsFolder}, the tests in {TestsFolder} and the development space in {DevelopmentFolder}; below the first two, use letters, digits, dashes, underscores, dots and slashes, and no names starting with a dot.";
                }

                if (file.Encoding == "base64" && file.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    return $"'{file.Name}' is a page to be read and has to be text (UTF-8), not base64.";
                }
            }
            else if (file.IsCode)
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

            // the same test Convert.FromBase64String makes, without the copy
            if (file.Encoding == "base64" && !Base64.IsValid(file.Code))
            {
                return $"'{file.Name}' says it is base64 and is not.";
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
    /// Whether a name is one a file of the context may have.
    /// </summary>
    /// <remarks>
    /// Below <c>.lambda/docs/</c> or <c>.lambda/tests/</c> and nowhere else in
    /// it, so the folder keeps the shape every reader - the editor, an agent,
    /// the export - expects. Below that, the rules of an asset, because these
    /// end up as real files too - in a zip, in an exported project - with one
    /// difference: no extension is needed, since nothing infers a content type
    /// from one here and a test may well be a Makefile.
    /// </remarks>
    public static bool IsValidContextName(string? name)
    {
        if (name == null || name.Length > 160)
        {
            return false;
        }

        string rest;

        if (name.StartsWith(DocsFolder, StringComparison.Ordinal))
        {
            rest = name[DocsFolder.Length..];
        }
        else if (name.StartsWith(TestsFolder, StringComparison.Ordinal))
        {
            rest = name[TestsFolder.Length..];
        }
        else
        {
            return false;
        }

        if (rest.Length == 0 || rest.EndsWith('/'))
        {
            return false;
        }

        var segments = rest.Split('/');

        if (segments.Length > 6)
        {
            return false;
        }

        foreach (var segment in segments)
        {
            if (segment.Length is 0 or > 60 || segment.StartsWith('.'))
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

        return true;
    }

    /// <summary>
    /// Whether a name is one a file of the development space may have.
    /// </summary>
    public static bool IsValidDevelopmentName(string? name) => name != null && IsDevelopment(name) && DevelopmentComplaint(name) == null;

    /// <summary>
    /// What is wrong with a name in the development space, if anything.
    /// </summary>
    /// <remarks>
    /// Wider than the names of the context, because these are names a build
    /// tool's files have, which nobody here chose: dot files, and brackets,
    /// parentheses and plus signs, which some tools read meaning off. Still
    /// names that become real files anywhere - in a zip, a clone, an
    /// exported project on Windows - so no spaces and nothing a shell or a
    /// file system reads as something else.
    ///
    /// What a build installs, caches or writes for itself is not refused by
    /// name here, since every tool names it differently: the space says so
    /// itself, in its .gitignore files, which a clone and a zip put back
    /// follow (see <see cref="IgnoredPaths"/>). What is named in a save is
    /// kept, as git keeps a file that was added on purpose.
    /// </remarks>
    private static string? DevelopmentComplaint(string name)
    {
        var rest = name[DevelopmentFolder.Length..];

        var segments = rest.Split('/');

        if (segments.Contains(".git"))
        {
            return $"'{name}' is part of a git repository, which is no file of the development space.";
        }

        var usable = rest.Length > 0 && !rest.EndsWith('/') && name.Length <= 240 && segments.Length <= 16;

        foreach (var segment in segments)
        {
            if (!usable)
            {
                break;
            }

            usable = segment.Length is > 0 and <= 100 && segment is not ("." or "..") && segment.All(IsDevelopmentCharacter);
        }

        return usable
            ? null
            : $"'{name}' is not a usable name in the development space. Below {DevelopmentFolder}, use letters, digits and - _ . + @ ( ) [ ] {{ }} $ ~, no spaces, at most 16 folders deep and 240 characters in all.";
    }

    private static bool IsDevelopmentCharacter(char character)
        => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' or '+' or '@' or '(' or ')' or '[' or ']' or '{' or '}' or '$' or '~';

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
