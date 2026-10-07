using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// One file of a lambda: part of its code - C# to compile, or any other file
/// kept with it - or one of its resources, which it reads and serves while it
/// runs.
/// </summary>
/// <param name="Name">What it is called, which is also what diagnostics name</param>
/// <param name="Code">Its contents, base64 when <paramref name="Encoding" /> says so</param>
/// <param name="Encoding">"base64" for a file that is not text, absent otherwise</param>
public sealed record LambdaFile(string Name, [property: JsonConverter(typeof(LongStringConverter))] string Code, string? Encoding = null)
{

    /// <summary>Whether this is C# the lambda is compiled from.</summary>
    /// <remarks>
    /// Not serialised, along with Bytes and the other kinds. This record is
    /// the API's shape as well as the storage one, and all of these are
    /// worked out from what is already there - so sending them put a second,
    /// base64 copy of every file into every response that carried one,
    /// roughly doubling it, to say something the caller could see for itself
    /// from the name.
    /// </remarks>
    [JsonIgnore]
    public bool IsCompiled => LambdaSource.IsCompiled(Name);

    /// <summary>Whether this belongs to the code rather than to the resources, compiled or not.</summary>
    [JsonIgnore]
    public bool IsCode => LambdaSource.IsCode(Name);

    /// <summary>Whether this is a resource: shipped to be read and served while the lambda runs.</summary>
    [JsonIgnore]
    public bool IsResource => LambdaSource.IsResource(Name);

    /// <summary>The bytes of a file, whatever it was sent as.</summary>
    [JsonIgnore]
    public byte[] Bytes => Encoding == "base64"
                         ? Convert.FromBase64String(Code)
                         : System.Text.Encoding.UTF8.GetBytes(Code);

}

/// <summary>
/// The source of a lambda: its code and its resources.
/// </summary>
/// <remarks>
/// A lambda is stored as one blob of text and always was, so the several files
/// are kept in that one blob as a small envelope rather than in a new column
/// and a new directory layout. Anything written before there was more than one
/// file is not an envelope, is read as a single entry file, and never has to
/// be migrated.
///
/// A version holds two things. Its code is every file that is not a
/// resource, in whatever folders it is in: the snippet in <c>lambda.cs</c>,
/// the other <c>.cs</c> files - all of them compiled, in whichever folder,
/// as a C# project compiles its own - and anything else the lambda is kept
/// with: what is written about it, what its front end is built from, a
/// tool's configuration, never compiled or served. Its resources are what is
/// below <c>resources/</c>: the front end, the migrations, pictures - written
/// into a folder of their own when the version goes online, where the code
/// reads and serves them.
///
/// The documentation and the tests are folders of the code like any other.
/// The editor reads <c>docs/</c> and <c>tests/</c> to show them, and the
/// agents are asked to keep them; the platform keeps them as it keeps every
/// file of the code.
///
/// The envelope says which layout it holds. The first one (version 1) kept
/// the files to serve at the root and what was not the program below
/// <c>.lambda/</c> - its documentation, its tests and its build folder. It is
/// read in the layout of today, as it is read: nothing written in it is ever
/// rewritten, so nothing can be half moved.
/// </remarks>
public static class LambdaSource
{

    /// <summary>
    /// The file the snippet lives in, which is the one that returns a handler.
    /// </summary>
    public const string EntryName = "lambda.cs";

    /// <summary>
    /// Where the resources of a version are: what it reads and serves while it runs.
    /// </summary>
    public const string ResourceFolder = "resources/";

    #region Conventions of the editor

    /// <summary>
    /// The documentation of a version: what it is and why, and how it is built and why.
    /// </summary>
    public const string DocsFolder = "docs/";

    /// <summary>
    /// How a version is tested, and the scripts and data the tests use.
    /// </summary>
    public const string TestsFolder = "tests/";

    /// <summary>
    /// What the app is, who it is for, why it exists and what people do with
    /// it - in the terms of the people who asked for it. The one page of the
    /// documentation that the owner of an app they had built reads as well.
    /// </summary>
    public const string ProductDoc = "docs/product.md";

    /// <summary>
    /// The technical decisions behind the program, and why they were made.
    /// </summary>
    public const string DecisionsDoc = "docs/decisions.md";

    /// <summary>
    /// How the app is tested automatically: what is checked, how, and how the
    /// scripts beside it are run.
    /// </summary>
    public const string TestingDoc = "tests/README.md";

    /// <summary>
    /// The pages every version is meant to have, in the order they are read.
    /// </summary>
    public static readonly IReadOnlyList<string> ExpectedPages = [ProductDoc, DecisionsDoc, TestingDoc];

    #endregion

    #region Kinds

    /// <summary>Whether a name is a resource, read and served while the lambda runs.</summary>
    public static bool IsResource(string? name) => name?.StartsWith(ResourceFolder, StringComparison.Ordinal) == true;

    /// <summary>Whether a name belongs to the code - everything that is not a resource.</summary>
    public static bool IsCode(string? name) => name != null && !IsResource(name);

    /// <summary>
    /// Whether a name is C# the lambda is compiled from: a <c>.cs</c> file of
    /// the code, in whichever folder.
    /// </summary>
    /// <remarks>
    /// As a C# project compiles every <c>.cs</c> file below it, so that its
    /// types can be laid out in folders. A resource is never compiled, whatever
    /// it is called: it is read and served.
    /// </remarks>
    public static bool IsCompiled(string? name)
        => IsCode(name) && name!.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Where a resource is below the folder the resources are written to.
    /// </summary>
    public static string WithinResources(string name) => IsResource(name) ? name[ResourceFolder.Length..] : name;

    #endregion

    private static readonly JsonSerializerOptions Format = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Marks the stored text as an envelope in today's layout.
    /// </summary>
    /// <remarks>
    /// No C# begins this way, so a blob that starts with it was written by this
    /// and one that does not is code from before there was more than one file
    /// - or an envelope of the first layout, which starts with its own marker.
    /// </remarks>
    private const string Marker = "{\"version\":2,\"files\":";

    /// <summary>
    /// Marks an envelope written in the first layout: the files to serve at
    /// the root, and <c>.lambda/</c> for what was not the program.
    /// </summary>
    private const string FirstMarker = "{\"version\":1,\"files\":";

    #region Functionality

    /// <summary>
    /// Reads stored text as the files it holds, in today's layout.
    /// </summary>
    public static IReadOnlyList<LambdaFile> Parse(string? stored)
    {
        var text = stored ?? string.Empty;

        var current = text.StartsWith(Marker, StringComparison.Ordinal);

        if (!current && !text.StartsWith(FirstMarker, StringComparison.Ordinal))
        {
            return [new LambdaFile(EntryName, text)];
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<Envelope>(text, Format);

            if (envelope?.Files is { Count: > 0 } files)
            {
                return current ? files : [.. files.Select(f => f with { Name = Moved(f.Name) })];
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

        return JsonSerializer.Serialize(new Envelope(2, files), Format);
    }

    /// <summary>
    /// Whether two stored blobs hold the same files, whichever layout either
    /// was written in.
    /// </summary>
    /// <remarks>
    /// Compared as they are first, which is the answer nearly always: only a
    /// blob of the first layout beside one written since is read to tell.
    /// </remarks>
    public static bool Same(string? stored, string? other)
    {
        if (string.Equals(stored, other, StringComparison.Ordinal))
        {
            return true;
        }

        if (stored == null || other == null)
        {
            return false;
        }

        return Serialize(Parse(stored)) == Serialize(Parse(other));
    }

    /// <summary>
    /// Where a file of the first layout is in today's: what was kept beside
    /// the program below <c>.lambda/</c> is at the top of the code, and what
    /// was served is a resource.
    /// </summary>
    /// <remarks>
    /// The C# was at the top then, and kept its name. A name of
    /// <c>.lambda/</c> was always one of its documentation, its tests or its
    /// build folder, which keep their folders: <c>docs/</c>, <c>tests/</c>
    /// and <c>build/</c>. Everything else was an asset.
    ///
    /// C# in <c>.lambda/</c> - a test, the sources of a tool - was never
    /// compiled, and every <c>.cs</c> file of the code is now. Moved as it
    /// is, it would be compiled into a program it was never part of, which
    /// it may not compile with, and a lambda online would no longer come up.
    /// So it keeps what it holds under a name that is not compiled:
    /// <c>.lambda/tests/Smoke.cs</c> is <c>tests/Smoke.cs.txt</c>.
    /// </remarks>
    internal static string Moved(string name)
    {
        if (name.StartsWith(".lambda/", StringComparison.Ordinal))
        {
            var moved = name[".lambda/".Length..];

            return moved.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ? moved + ".txt" : moved;
        }

        if (!name.Contains('/') && name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return name;
        }

        return ResourceFolder + name;
    }

    /// <summary>
    /// How many bytes a version comes to, its code and its resources together
    /// - decoded rather than as sent - which is what the allowance of its
    /// tier counts.
    /// </summary>
    /// <remarks>
    /// Measured without decoding anything: this is asked of every version on
    /// its way in, and a premium lambda may ship a hundred megabytes that
    /// would otherwise be decoded just to be counted and thrown away.
    /// </remarks>
    public static long Size(IEnumerable<LambdaFile> files)
    {
        long total = 0;

        foreach (var file in files)
        {
            total += Size(file);
        }

        return total;
    }

    /// <summary>
    /// How many bytes one file comes to, decoded.
    /// </summary>
    public static long Size(LambdaFile file)
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
            if (Complaint(file.Name) is { } complaint)
            {
                return complaint;
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

            if (file.IsCompiled && file.Encoding == "base64")
            {
                return $"'{file.Name}' is C# to compile and has to be text (UTF-8), not base64.";
            }

            if (!seen.Add(file.Name))
            {
                return $"There is more than one file called '{file.Name}' - names differ by more than their case, as they do on Windows and macOS.";
            }
        }

        // a file and a folder of the same name cannot both be on a disk
        foreach (var file in files)
        {
            for (var slash = file.Name.IndexOf('/'); slash > 0; slash = file.Name.IndexOf('/', slash + 1))
            {
                if (seen.Contains(file.Name[..slash]))
                {
                    return $"'{file.Name[..slash]}' is a file and a folder at once: '{file.Name}' is in it.";
                }
            }
        }

        return null;
    }

    /// <summary>
    /// What is wrong with the name of a file, if anything.
    /// </summary>
    public static string? Complaint(string name)
    {
        if (IsResource(name))
        {
            return IsValidResourceName(name)
                ? null
                : $"'{name}' is not a usable name for a resource. Below {ResourceFolder}, use letters, digits, dashes, underscores, dots and slashes, no names starting with a dot, an extension to serve it by, at most six folders deep and 120 characters.";
        }

        if (CodeComplaint(name) is { } complaint)
        {
            return complaint;
        }

        return !IsCompiled(name) || IsValidName(name[(name.LastIndexOf('/') + 1)..])
            ? null
            : $"'{name}' is not a usable name for a C# file. Name it with letters, digits, dashes, underscores and dots, starting with a letter and ending in '.cs', 40 characters at most - in any folder.";
    }

    /// <summary>
    /// Whether a name is one a resource may have.
    /// </summary>
    /// <remarks>
    /// A resource becomes a real file in a real directory, so this is about
    /// where it can end up rather than about how it reads. Segments are
    /// checked one at a time and '..' is not one of them, because the whole
    /// point of a relative path is that it can leave. No segment starts with
    /// a dot, which is the convention for "not served".
    /// </remarks>
    public static bool IsValidResourceName(string? name)
    {
        if (name == null || !IsResource(name))
        {
            return false;
        }

        var rest = name[ResourceFolder.Length..];

        if (string.IsNullOrWhiteSpace(rest) || rest.Length > 120 || rest.StartsWith('/') || rest.EndsWith('/'))
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
            if (segment.Length is 0 or > 60 || segment is "." or ".." || segment.StartsWith('.'))
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
        return Path.GetExtension(rest).Length > 1;
    }

    /// <summary>
    /// The names at the top of the code that the project of a lambda - its
    /// export, a clone - has for its own, in any case.
    /// </summary>
    /// <remarks>
    /// A lambda is exported and cloned as a .NET project, whose own files
    /// are at the top beside the lambda's. A file of the lambda called what
    /// one of them is called could not be in both. Compared in any case,
    /// since a clone on Windows or macOS cannot hold two names that differ
    /// only by it.
    /// </remarks>
    private static readonly HashSet<string> Platform = new(StringComparer.OrdinalIgnoreCase)
    {
        "Dockerfile", ".gitignore", ".dockerignore", "LICENSE", "AGENTS.md", "CLAUDE.md"
    };

    /// <summary>
    /// The folders at the top that are the project's or the platform's:
    /// what stands in for the platform, what dotnet builds into, what the
    /// app keeps while it runs in a clone - and the resources, in lowercase.
    /// </summary>
    private static readonly HashSet<string> PlatformFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Platform", "bin", "obj", "workspace", "database", "resources"
    };

    /// <summary>
    /// What is wrong with the name of a file of the code, if anything - and
    /// with the folders of a C# file, whose own name is held to more.
    /// </summary>
    /// <remarks>
    /// Wide, because these are names a tool's files have, which nobody here
    /// chose: dot files, and brackets, parentheses and plus signs, which some
    /// tools read meaning off. Still names that become real files anywhere -
    /// in a zip, a clone, an exported project on Windows - so no spaces, no
    /// name Windows reserves or ends with a dot, and nothing a shell or a file
    /// system reads as something else. A clone refuses a .git folder in any
    /// case, and by its short name on Windows, so this does too.
    ///
    /// What a build installs, caches or writes for itself is not refused by
    /// name here, since every tool names it differently: the folder says so
    /// itself, in its .gitignore files, which a clone and a zip put back
    /// follow (see <see cref="IgnoredPaths"/>). What is named in a save is
    /// kept, as git keeps a file that was added on purpose.
    /// </remarks>
    private static string? CodeComplaint(string name)
    {
        var segments = name.Split('/');

        if (segments[0].Equals(".lambda", StringComparison.OrdinalIgnoreCase))
        {
            return $"'{name}' is in .lambda/, which a lambda has no more: the documentation is in docs/ and the tests are in tests/, at the top of the code, "
                 + $"and what the lambda reads and serves - its front end, its migrations - is in {ResourceFolder}.";
        }

        if (segments.Any(s => s.Equals(".git", StringComparison.OrdinalIgnoreCase) || s.Equals("git~1", StringComparison.OrdinalIgnoreCase)))
        {
            return $"'{name}' is part of a git repository, which is no file of a lambda.";
        }

        if (segments.Length == 1 && (Platform.Contains(name) || name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)))
        {
            return $"'{name}' is what the project of a lambda - its export, a clone - has for its own at the top. Put it into a folder, or call it something else.";
        }

        if (segments.Length > 1 && PlatformFolders.Contains(segments[0]))
        {
            return segments[0].Equals("resources", StringComparison.OrdinalIgnoreCase)
                ? $"'{name}': the resources are in {ResourceFolder}, in lowercase."
                : $"'{name}' is in {segments[0]}/, which the project of a lambda - its export, a clone - has for its own. Use a folder of another name.";
        }

        // where a clone and an export had the files to serve, before they
        // were called resources: a file there now is most likely one of them,
        // brought from before, that would no longer be served
        if (segments.Length > 1 && segments[0].Equals("assets", StringComparison.OrdinalIgnoreCase))
        {
            return $"'{name}' is in assets/, where the files a lambda serves were kept before they were called resources: they are in {ResourceFolder} now. "
                 + "Move it there to have it served, or into a folder of another name to keep it with the code.";
        }

        var usable = !name.EndsWith('/') && name.Length <= 240 && segments.Length <= 16;

        foreach (var segment in segments)
        {
            if (!usable)
            {
                break;
            }

            usable = segment.Length is > 0 and <= 100 && segment is not ("." or "..") && !segment.EndsWith('.') && !IsReservedOnWindows(segment)
                  && segment.All(IsCodeCharacter);
        }

        return usable
            ? null
            : $"'{name}' is not a usable name. Use letters, digits and - _ . + @ ( ) [ ] {{ }} $ ~, no spaces, no name ending in a dot or one Windows reserves (CON, PRN, AUX, NUL, COM1, LPT1 and the like, with an extension too), at most 16 folders deep and 240 characters in all.";
    }

    /// <summary>
    /// Whether Windows refuses a file of this name, with an extension or without.
    /// </summary>
    private static bool IsReservedOnWindows(string segment)
    {
        var stem = segment.Split('.')[0].ToUpperInvariant();

        return stem is "CON" or "PRN" or "AUX" or "NUL" or "CONIN$" or "CONOUT$"
            || (stem.Length == 4 && stem[..3] is "COM" or "LPT" && stem[3] is >= '1' and <= '9');
    }

    private static bool IsCodeCharacter(char character)
        => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' or '+' or '@' or '(' or ')' or '[' or ']' or '{' or '}' or '$' or '~';

    /// <summary>
    /// Whether a name is one a C# file may have, without its folders.
    /// </summary>
    /// <remarks>
    /// Narrower than the rest of the code. The path reaches a <c>#line</c>
    /// directive and a diagnostic, and a quote in it would break the
    /// generated file; the folders are held to the rules of the code, which
    /// have none. Dots are there for the names .NET gives the parts of a
    /// class (<c>Store.Queries.cs</c>).
    /// </remarks>
    public static bool IsValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 40 || !name.EndsWith(".cs", StringComparison.Ordinal))
        {
            return false;
        }

        var stem = name[..^3];

        if (stem.Length == 0 || !char.IsAsciiLetter(stem[0]) || stem.EndsWith('.'))
        {
            return false;
        }

        foreach (var character in stem)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.'))
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
