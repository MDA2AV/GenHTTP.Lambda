using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// One file of a lambda: part of its code - C# to compile, or any other file
/// kept with it - or one of its resources, which it reads and serves while it
/// runs.
/// </summary>
/// <remarks>
/// What a file holds is in one of three places, and is only turned into the
/// others when somebody asks for it: the text or the base64 it was sent as
/// (<see cref="Code"/>), the bytes it was read as from an archive or a push,
/// or - for a resource of a saved version - the lambda's resource store,
/// which keeps every resource once, by what it holds (<see cref="Stored"/>).
///
/// A version of a website ships its pages as resources, thousands of them.
/// Carried as strings, every save and every deployment of one took several
/// copies of them at twice their size, as text and as base64, in one string
/// the size of all of them together - a few hundred megabytes, all at once,
/// for a site of fifteen, in a process every lambda shares. So the bytes stay
/// bytes until the store has them, and a version names them rather than
/// carries them.
/// </remarks>
public sealed record LambdaFile
{
    private static readonly UTF8Encoding Text = new(false, false);

    private string? _code;

    private byte[]? _bytes;

    /// <param name="name">What it is called, which is also what diagnostics name</param>
    /// <param name="code">Its contents, base64 when <paramref name="encoding" /> says so</param>
    /// <param name="encoding">"base64" for a file that is not text, absent otherwise</param>
    [JsonConstructor]
    public LambdaFile(string name, string code, string? encoding = null)
    {
        Name = name;
        Encoding = encoding;
        _code = code;
    }

    private LambdaFile(string name, string? encoding, byte[]? bytes, StoredContent? stored)
    {
        Name = name;
        Encoding = encoding;
        _bytes = bytes;
        Stored = stored;
    }

    /// <summary>
    /// A resource as it was read, kept as the bytes it is: text where it is
    /// UTF-8 without a NUL, base64 otherwise - once somebody asks for it as either.
    /// </summary>
    /// <remarks>
    /// A byte order mark is not part of the text, as it never was when a
    /// resource was read into a string.
    /// </remarks>
    /// <param name="binary">True for a file that stays what it was before - not text - whatever its bytes look like now</param>
    public static LambdaFile FromBytes(string name, byte[] bytes, bool binary = false)
    {
        if (!binary && Utf8.IsValid(bytes) && !bytes.AsSpan().Contains((byte)0))
        {
            return new LambdaFile(name, null, bytes.AsSpan().StartsWith(Preamble) ? bytes[Preamble.Length..] : bytes, null);
        }

        return new LambdaFile(name, "base64", bytes, null);
    }

    /// <summary>
    /// A resource as the lambda's resource store keeps it.
    /// </summary>
    /// <param name="encoding">"base64" where it is not text, as it was when it was kept</param>
    public static LambdaFile FromStore(string name, StoredContent stored, string? encoding)
        => new(name, encoding, null, stored);

    private static ReadOnlySpan<byte> Preamble => [0xEF, 0xBB, 0xBF];

    /// <summary>What it is called, which is also what diagnostics name.</summary>
    public string Name { get; init; }

    /// <summary>
    /// What it holds, base64 when <see cref="Encoding" /> says so - made from
    /// its bytes, or read from the store, the first time it is asked for.
    /// </summary>
    /// <remarks>
    /// Given anew - an edit of the text, say - it is what the file holds from
    /// then on, and the bytes or the stored copy it was before are let go.
    /// </remarks>
    [JsonConverter(typeof(LongStringConverter))]
    public string Code
    {
        get => _code ??= Decode();
        init
        {
            _code = value;
            _bytes = null;
            Stored = null;
        }
    }

    /// <summary>"base64" for a file that is not text, absent otherwise.</summary>
    public string? Encoding { get; init; }

    /// <summary>
    /// Where the lambda's resource store keeps it - for a resource of a saved
    /// version or feature, and nothing for a file sent or read with what it holds.
    /// </summary>
    [JsonIgnore]
    public StoredContent? Stored { get; private init; }

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

    /// <summary>
    /// Whether what it holds is text sent with it - base64 that still has to
    /// be checked, say - rather than bytes it was read as or a stored copy.
    /// </summary>
    [JsonIgnore]
    public bool IsSent => _bytes == null && Stored == null;

    /// <summary>The bytes of a file, whatever it was sent, read or kept as.</summary>
    /// <remarks>A copy, made anew every time it is asked for - <see cref="Open"/> streams it.</remarks>
    [JsonIgnore]
    public byte[] Bytes
    {
        get
        {
            if (_bytes != null)
            {
                return _bytes;
            }

            if (Stored is { } stored)
            {
                return stored.ReadAll();
            }

            return Encoding == "base64" ? Convert.FromBase64String(Code) : Text.GetBytes(Code);
        }
    }

    /// <summary>How many bytes it holds - decoded rather than as sent - without making any of them.</summary>
    [JsonIgnore]
    public long Length
    {
        get
        {
            if (_bytes != null)
            {
                return _bytes.Length;
            }

            if (Stored is { } stored)
            {
                return stored.Length;
            }

            // a file that is not the base64 it claims to be is caught by
            // Validate; here it simply counts for nothing
            if (Encoding == "base64")
            {
                return Base64.IsValid(_code!, out var decoded) ? decoded : 0;
            }

            return Text.GetByteCount(_code!);
        }
    }

    /// <summary>The SHA-256 of what it holds, in lowercase hex - the name the resource store keeps it under.</summary>
    [JsonIgnore]
    public string Sha256 => Stored?.Sha256 ?? Convert.ToHexStringLower(SHA256.HashData(Bytes));

    /// <summary>Reads what the file holds, from wherever it is.</summary>
    public Stream Open() => Stored is { } stored ? stored.Open() : new MemoryStream(Bytes, false);

    private string Decode()
    {
        var bytes = _bytes ?? Stored?.ReadAll() ?? [];

        return Encoding == "base64" ? Convert.ToBase64String(bytes) : Text.GetString(bytes);
    }

    /// <summary>
    /// The same file: of the same name and encoding, holding the same.
    /// </summary>
    /// <remarks>
    /// Wherever either holds it, so a resource read from the store is the
    /// same as the one it was kept from.
    /// </remarks>
    public bool Equals(LambdaFile? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Name != other.Name || Encoding != other.Encoding)
        {
            return false;
        }

        if (Stored != null && other.Stored != null)
        {
            return Stored.Sha256 == other.Stored.Sha256;
        }

        if (IsSent && other.IsSent)
        {
            return _code == other._code;
        }

        return Length == other.Length && Sha256 == other.Sha256;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Encoding);

    /// <summary>
    /// Says which file it is - not what it holds, which a file of the store
    /// would have to be read for, and a large one would fill the log.
    /// </summary>
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Name = ").Append(Name);

        if (Encoding != null)
        {
            builder.Append(", Encoding = ").Append(Encoding);
        }

        builder.Append(", Length = ").Append(Length);

        return true;
    }

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

    /// <remarks>
    /// Written without escaping what only matters inside HTML: an envelope is
    /// stored and read back, never put into a page, and the default escapes
    /// every &lt;, &gt;, &amp; and ' of a file in six characters - a third more
    /// for a version that keeps a site's pages in its code.
    /// </remarks>
    private static readonly JsonSerializerOptions Format = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Marks the stored text as an envelope in today's layout, whose
    /// resources are named by what they hold and kept in the resource store.
    /// </summary>
    /// <remarks>
    /// No C# begins this way, so a blob that starts with it was written by this
    /// and one that does not is code from before there was more than one file
    /// - or an envelope of an earlier layout, which starts with its own marker.
    /// </remarks>
    private const string Marker = "{\"version\":3,\"files\":";

    /// <summary>
    /// Marks an envelope written in the second layout: today's names, with
    /// what every file holds in the envelope itself, resources included.
    /// </summary>
    private const string SecondMarker = "{\"version\":2,\"files\":";

    /// <summary>
    /// Marks an envelope written in the first layout: the files to serve at
    /// the root, and <c>.lambda/</c> for what was not the program.
    /// </summary>
    private const string FirstMarker = "{\"version\":1,\"files\":";

    #region Functionality

    /// <summary>
    /// Reads stored text as the files it holds, in today's layout - without
    /// what the resources hold, which can be named, counted and compared
    /// like this but not read.
    /// </summary>
    public static IReadOnlyList<LambdaFile> Parse(string? stored) => Parse(stored, null);

    /// <summary>
    /// Reads stored text as the files it holds, in today's layout, its
    /// resources read from the store of the lambda it belongs to.
    /// </summary>
    /// <remarks>
    /// Nothing written in an earlier layout is ever rewritten: it is read in
    /// today's, as it is read. An envelope of the second layout still carries
    /// what its resources hold, and keeps carrying it until it is saved again.
    /// </remarks>
    /// <param name="resources">The resource store of the lambda the text belongs to</param>
    public static IReadOnlyList<LambdaFile> Parse(string? stored, IResourceStore? resources)
    {
        var text = stored ?? string.Empty;

        var layout = LayoutOf(text);

        if (layout == 0)
        {
            return [new LambdaFile(EntryName, text)];
        }

        try
        {
            if (JsonSerializer.Deserialize<Envelope>(text, Format) is { Files.Count: > 0 } envelope)
            {
                return Files(envelope, layout, resources);
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
    /// Reads what a version was stored as straight from where it is stored,
    /// rather than as a string first.
    /// </summary>
    /// <remarks>
    /// An envelope of the second layout carries every resource of a version,
    /// a site's worth of them: read as one string, that string alone took
    /// twice what it held, beside the files read out of it.
    /// </remarks>
    /// <param name="stored">What was stored, which has to be seekable</param>
    /// <param name="resources">The resource store of the lambda it belongs to</param>
    public static IReadOnlyList<LambdaFile> Read(Stream stored, IResourceStore? resources)
    {
        var start = new byte[Marker.Length];

        var layout = LayoutOf(Encoding.ASCII.GetString(start, 0, stored.ReadAtLeast(start, start.Length, throwOnEndOfStream: false)));

        stored.Position = 0;

        if (layout != 0)
        {
            try
            {
                if (JsonSerializer.Deserialize<Envelope>(stored, Format) is { Files.Count: > 0 } envelope)
                {
                    return Files(envelope, layout, resources);
                }
            }
            catch (JsonException)
            {
                // somebody's code that only starts like an envelope, as above
            }

            stored.Position = 0;
        }

        using var reader = new StreamReader(stored, leaveOpen: true);

        return [new LambdaFile(EntryName, reader.ReadToEnd())];
    }

    /// <summary>
    /// The hashes of the resources stored text names: what the resource store
    /// has to keep for it.
    /// </summary>
    /// <remarks>
    /// Only today's layout names any. Text of an earlier one carries what its
    /// resources hold and is not read at all, however large it is.
    ///
    /// An envelope of today's layout that cannot be read throws rather than
    /// naming nothing: what it names would be taken from the store.
    /// </remarks>
    /// <exception cref="JsonException">Stored text that starts as an envelope of today's layout and is not one</exception>
    public static IReadOnlyList<string> References(Stream stored)
    {
        var start = new byte[Marker.Length];

        if (LayoutOf(Encoding.ASCII.GetString(start, 0, stored.ReadAtLeast(start, start.Length, throwOnEndOfStream: false))) != 3)
        {
            return [];
        }

        stored.Position = 0;

        var envelope = JsonSerializer.Deserialize<Envelope>(stored, Format) ?? throw new JsonException("The envelope is empty.");

        return [.. envelope.Files.Where(f => f.Stored != null).Select(f => f.Stored!)];
    }

    /// <summary>
    /// Writes the files as the single blob a version is stored as.
    /// </summary>
    /// <remarks>
    /// A lambda that is still one file is stored as that file and nothing else,
    /// so the common case stays readable on disk and anything reading an older
    /// version cannot tell the difference.
    ///
    /// A resource the store keeps is named by its hash; one it does not keep
    /// yet is written with what it holds, as it was before there was a store
    /// - which is what <see cref="Keep"/> is for, before a version is written.
    /// </remarks>
    public static string Serialize(IReadOnlyList<LambdaFile> files)
    {
        if (files.Count == 1 && files[0].Name == EntryName)
        {
            return files[0].Code;
        }

        var stored = files.Select(f => f.Stored is { } kept
                                      ? new StoredFile(f.Name, null, f.Encoding, kept.Sha256, kept.Length)
                                      : new StoredFile(f.Name, f.Code, f.Encoding))
                          .ToList();

        return JsonSerializer.Serialize(new Envelope(3, stored), Format);
    }

    /// <summary>
    /// Puts what the resources hold into the resource store of the lambda
    /// they are saved for, and gives back the files as it keeps them.
    /// </summary>
    /// <remarks>
    /// Before anything is written that names them, so that a version names
    /// only what is kept. A resource the store keeps already - read from a
    /// version of this lambda, or the same as one - is kept once, however
    /// many versions ship it.
    /// </remarks>
    /// <param name="resources">The resource store of the lambda the files are saved for</param>
    public static IReadOnlyList<LambdaFile> Keep(IReadOnlyList<LambdaFile> files, IResourceStore resources)
    {
        var kept = new List<LambdaFile>(files.Count);

        foreach (var file in files)
        {
            if (!file.IsResource)
            {
                kept.Add(file);
                continue;
            }

            if (file.Stored is { } stored && resources.Has(stored.Sha256))
            {
                kept.Add(ReferenceEquals(stored.Store, resources) ? file : LambdaFile.FromStore(file.Name, stored with { Store = resources }, file.Encoding));
                continue;
            }

            if (file.Stored is { Store: null })
            {
                throw new InvalidOperationException($"'{file.Name}' names a resource as {file.Stored.Sha256}, which the lambda it is saved for does not keep.");
            }

            kept.Add(LambdaFile.FromStore(file.Name, resources.Keep(file.Bytes), file.Encoding));
        }

        return kept;
    }

    /// <summary>
    /// Whether two stored blobs hold the same files, whichever layout either
    /// was written in.
    /// </summary>
    /// <remarks>
    /// Compared as they are first, which is the answer nearly always. Read
    /// otherwise - without the resource store, which is not needed to tell:
    /// a resource it keeps is compared by its hash, and one written with what
    /// it holds by the hash of that.
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

        var files = Parse(stored);

        var others = Parse(other);

        return files.Count == others.Count && files.Zip(others).All(pair => pair.First.Equals(pair.Second));
    }

    /// <summary>
    /// Which layout stored text starts in: 3 for today's, 2 and 1 for the
    /// earlier envelopes, 0 for code from before there were several files.
    /// </summary>
    private static int LayoutOf(string text)
    {
        if (text.StartsWith(Marker, StringComparison.Ordinal))
        {
            return 3;
        }

        if (text.StartsWith(SecondMarker, StringComparison.Ordinal))
        {
            return 2;
        }

        return text.StartsWith(FirstMarker, StringComparison.Ordinal) ? 1 : 0;
    }

    /// <summary>
    /// The files of an envelope, named as today's layout names them.
    /// </summary>
    private static IReadOnlyList<LambdaFile> Files(Envelope envelope, int layout, IResourceStore? resources)
    {
        var files = new List<LambdaFile>(envelope.Files.Count);

        foreach (var file in envelope.Files)
        {
            var name = layout == 1 ? Moved(file.Name) : file.Name;

            if (file.Stored == null)
            {
                files.Add(new LambdaFile(name, file.Code ?? string.Empty, file.Encoding));
            }
            else if (StoredContent.IsHash(file.Stored))
            {
                files.Add(LambdaFile.FromStore(name, new StoredContent(file.Stored, file.Length ?? 0, resources), file.Encoding));
            }
            else
            {
                // a hash becomes part of a path in the store, and what is not
                // one could name a file outside it
                throw new InvalidOperationException($"'{name}' is stored as '{file.Stored}', which is not the hash of a resource.");
            }
        }

        return files;
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
    public static long Size(LambdaFile file) => file.Length;

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

            // the same test Convert.FromBase64String makes, without the copy -
            // of what was sent as base64, since bytes read or kept are bytes
            if (file.Encoding == "base64" && file.IsSent && !Base64.IsValid(file.Code))
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

    private sealed record Envelope(int Version, IReadOnlyList<StoredFile> Files);

    /// <summary>
    /// A file as an envelope holds it: with what it holds, or - a resource of
    /// today's layout - with the hash and the length it is kept under.
    /// </summary>
    private sealed record StoredFile(string Name, [property: JsonConverter(typeof(LongStringConverter))] string? Code = null, string? Encoding = null,
                                     string? Stored = null, long? Length = null);

    #endregion

}
