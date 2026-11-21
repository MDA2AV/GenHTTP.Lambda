using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// One file of a version - or of a feature, or of what is about to become
/// either - as the platform hands it on: its name, how large it is, whether
/// it is text, and where its content is, which is read only by whoever needs
/// it.
/// </summary>
/// <remarks>
/// The doors send and receive a file as a <see cref="LambdaFile"/>, its
/// content a string. Inside the platform a version is passed on as these
/// instead, so content stays where it is until somebody reads it: what the
/// store keeps is read from the store - the C# to compile, the resources
/// copied out to be served, every file streamed into a zip - and what just
/// arrived is held as it came, the string of a request or the bytes of a
/// push, until the store keeps it.
///
/// A file of a version is text or binary as it was saved, whatever its
/// content: a file sent as base64 stays base64 though it holds text, which
/// is what the doors read it back as.
/// </remarks>
public sealed class VersionFile
{
    private static readonly UTF8Encoding Utf8 = new(false);

    private string? _hash;

    #region Get-/Setters

    /// <summary>
    /// Its path, as the lambda names it: <c>lambda.cs</c>, <c>resources/web/index.html</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// How many bytes it holds, which is what the allowance of a version counts.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Whether it is binary - sent and read back as base64 - rather than text.
    /// </summary>
    public bool Binary { get; }

    /// <summary>
    /// How long it is as text, in characters, where it is text.
    /// </summary>
    public long? Chars { get; }

    /// <summary>
    /// What the doors send its content as: <c>base64</c>, or nothing for text.
    /// </summary>
    public string? Encoding => Binary ? "base64" : null;

    /// <summary>
    /// How long it is as the doors send it, in characters: of its text, or of
    /// its base64.
    /// </summary>
    public long Length => Binary ? (Size + 2) / 3 * 4 : Chars ?? 0;

    /// <summary>Whether this is C# the lambda is compiled from.</summary>
    public bool IsCompiled => LambdaSource.IsCompiled(Name);

    /// <summary>Whether this belongs to the code rather than to the resources.</summary>
    public bool IsCode => LambdaSource.IsCode(Name);

    /// <summary>Whether this is a resource: read and served while the lambda runs.</summary>
    public bool IsResource => LambdaSource.IsResource(Name);

    /// <summary>
    /// The SHA-256 of its content, in lowercase hex - what the store keeps it
    /// under, and what tells whether two files hold the same.
    /// </summary>
    /// <remarks>
    /// Known for what the store keeps; worked out once for what just arrived,
    /// when it is asked for.
    /// </remarks>
    public string Hash => _hash ??= Content.Hash();

    internal FileContent Content { get; }

    #endregion

    #region Initialization

    private VersionFile(string name, long size, bool binary, long? chars, FileContent content, string? hash)
    {
        Name = name;
        Size = size;
        Binary = binary;
        Chars = chars;
        Content = content;

        _hash = hash;
    }

    /// <summary>
    /// A file as one of the doors sent it: text, or base64.
    /// </summary>
    /// <remarks>
    /// Refuses what <see cref="LambdaSource.Validate(IReadOnlyList{LambdaFile})"/>
    /// would refuse of its content, in the same words, for the paths that
    /// take a file in without validating all of them - a change applied to
    /// what is kept.
    /// </remarks>
    public static VersionFile FromSent(LambdaFile file)
    {
        if (file.Encoding is not (null or "" or "text" or "base64"))
        {
            throw LambdaException.Invalid($"'{file.Name}' asks for encoding '{file.Encoding}'. Only 'base64' is understood; leave it out for text.");
        }

        if (file.Encoding != "base64")
        {
            return FromText(file.Name, file.Code);
        }

        if (!Base64.IsValid(file.Code, out var decoded))
        {
            throw LambdaException.Invalid($"'{file.Name}' says it is base64 and is not.");
        }

        if (LambdaSource.IsCompiled(file.Name))
        {
            throw LambdaException.Invalid($"'{file.Name}' is C# to compile and has to be text (UTF-8), not base64.");
        }

        return new VersionFile(file.Name, decoded, true, null, new Base64Content(file.Code), null);
    }

    /// <summary>
    /// The files as one of the doors sent them.
    /// </summary>
    public static IReadOnlyList<VersionFile> FromSent(IEnumerable<LambdaFile> files) => [.. files.Select(FromSent)];

    /// <summary>
    /// A text file whose content is the given text.
    /// </summary>
    public static VersionFile FromText(string name, string text)
        => new(name, Utf8.GetByteCount(text), false, text.Length, new TextContent(text), null);

    /// <summary>
    /// A file whose content is the given bytes, as they are.
    /// </summary>
    /// <remarks>
    /// The bytes are held, not copied: those of a push are what the git
    /// module holds already.
    /// </remarks>
    /// <param name="binary">Whether it is binary; a text file has to be UTF-8</param>
    public static VersionFile FromBytes(string name, ReadOnlyMemory<byte> content, bool binary)
        => new(name, content.Length, binary, binary ? null : Utf8.GetCharCount(content.Span), new BytesContent(content), null);

    /// <summary>
    /// A file whose content the store of a lambda keeps, or is about to keep.
    /// </summary>
    internal static VersionFile Stored(string name, long size, bool binary, long? chars, StoredContent content)
        => new(name, size, binary, chars, content, content.Hash());

    /// <summary>
    /// The same content, under another name or the other way of sending it.
    /// </summary>
    /// <param name="binary">Whether it is binary; only content that is text can be made text</param>
    public VersionFile As(string name, bool binary)
    {
        if (!binary && Chars == null)
        {
            throw new InvalidOperationException($"'{name}' holds no text, so it can only be binary.");
        }

        return new VersionFile(name, Size, binary, Chars, Content, _hash);
    }

    #endregion

    #region Content

    /// <summary>
    /// Its text. Only for a text file: a binary one is read as bytes.
    /// </summary>
    public string ReadText()
    {
        if (Binary)
        {
            throw new InvalidOperationException($"'{Name}' is binary and has no text to read.");
        }

        Noted();

        return Content is TextContent text ? text.Text : Utf8.GetString(Content.ReadBytes());
    }

    /// <summary>
    /// Its bytes, as they are.
    /// </summary>
    public byte[] ReadBytes()
    {
        Noted();

        return Content.ReadBytes();
    }

    /// <summary>
    /// Its bytes, as a stream to read them from.
    /// </summary>
    public Stream OpenRead()
    {
        Noted();

        return Content.OpenRead();
    }

    /// <summary>
    /// Writes its bytes into a file, replacing what is there - by the file
    /// system where it is a file already.
    /// </summary>
    public void CopyTo(string path)
    {
        Noted();

        Content.CopyTo(path);
    }

    /// <summary>
    /// The file as the doors send it: its text, or its bytes in base64.
    /// </summary>
    public LambdaFile ToSent()
    {
        if (!Binary)
        {
            return new LambdaFile(Name, ReadText());
        }

        if (Content is Base64Content sent)
        {
            return new LambdaFile(Name, sent.Base64, "base64");
        }

        return new LambdaFile(Name, Convert.ToBase64String(ReadBytes()), "base64");
    }

    /// <summary>
    /// Counts that the content the store keeps was read.
    /// </summary>
    private void Noted()
    {
        if (Content is StoredContent { Reads: { } reads })
        {
            reads.Note(IsResource);
        }
    }

    public override string ToString() => Name;

    #endregion

}

/// <summary>
/// Where the content of a <see cref="VersionFile"/> is.
/// </summary>
internal abstract class FileContent
{

    public abstract Stream OpenRead();

    public virtual byte[] ReadBytes()
    {
        using var source = OpenRead();

        using var target = new MemoryStream();

        source.CopyTo(target);

        return target.ToArray();
    }

    public virtual void CopyTo(string path)
    {
        using var source = OpenRead();

        using var target = File.Create(path);

        source.CopyTo(target);
    }

    /// <summary>
    /// The SHA-256 of the content, in lowercase hex.
    /// </summary>
    public virtual string Hash() => Convert.ToHexStringLower(SHA256.HashData(ReadBytes()));

}

/// <summary>
/// Text, as a door sent it - or as a version kept in the layout of before held it.
/// </summary>
internal sealed class TextContent(string text) : FileContent
{
    private static readonly UTF8Encoding Utf8 = new(false);

    public string Text => text;

    public override Stream OpenRead() => new MemoryStream(ReadBytes(), false);

    public override byte[] ReadBytes() => Utf8.GetBytes(text);

}

/// <summary>
/// Bytes as a door sent them, in base64.
/// </summary>
internal sealed class Base64Content(string base64) : FileContent
{

    public string Base64 => base64;

    public override Stream OpenRead() => new MemoryStream(ReadBytes(), false);

    public override byte[] ReadBytes() => Convert.FromBase64String(base64);

}

/// <summary>
/// Bytes held where they are, such as those of a push.
/// </summary>
internal sealed class BytesContent(ReadOnlyMemory<byte> bytes) : FileContent
{

    public override Stream OpenRead() => new MemoryStream(bytes.ToArray(), false);

    public override byte[] ReadBytes() => bytes.ToArray();

    public override string Hash() => Convert.ToHexStringLower(SHA256.HashData(bytes.Span));

    public override void CopyTo(string path)
    {
        using var target = File.Create(path);

        target.Write(bytes.Span);
    }

    public ReadOnlySpan<byte> Span => bytes.Span;

}
