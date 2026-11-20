using System.Security.Cryptography;
using System.Text;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// What a piece of content is, read off it once as it is kept: its SHA-256,
/// its size, its length where it is text, and the ids git gives it.
/// </summary>
/// <param name="Hash">The SHA-256 of the content, its name in the store</param>
/// <param name="Size">How many bytes it holds</param>
/// <param name="Chars">Its length in characters where it is UTF-8 without a NUL character, nothing where it is not text</param>
/// <param name="GitSha1">The id of a git blob of it, SHA-1, which git names objects by today</param>
/// <param name="GitSha256">The id of a git blob of it in git's SHA-256 object format</param>
internal sealed record ContentFacts(string Hash, long Size, long? Chars, string GitSha1, string GitSha256)
{
    private static readonly UTF8Encoding Strict = new(false, true);

    private const int Chunk = 81920;

    /// <summary>
    /// The facts of content held in memory.
    /// </summary>
    public static ContentFacts Of(ReadOnlySpan<byte> content)
    {
        var header = Header(content.Length);

        using var git1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        using var git256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        git1.AppendData(header);
        git1.AppendData(content);

        git256.AppendData(header);
        git256.AppendData(content);

        long? chars = null;

        if (!content.Contains((byte)0))
        {
            try
            {
                chars = Strict.GetCharCount(content);
            }
            catch (DecoderFallbackException)
            {
                // not UTF-8, so no text
            }
        }

        return new ContentFacts(Convert.ToHexStringLower(SHA256.HashData(content)), content.Length, chars,
                                Convert.ToHexStringLower(git1.GetHashAndReset()), Convert.ToHexStringLower(git256.GetHashAndReset()));
    }

    /// <summary>
    /// The facts of content in a file, read through once rather than held.
    /// </summary>
    public static ContentFacts Of(string file)
    {
        using var source = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, Chunk);

        var size = source.Length;

        var header = Header(size);

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var git1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        using var git256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        git1.AppendData(header);
        git256.AppendData(header);

        var decoder = Strict.GetDecoder();

        var text = true;

        long chars = 0;

        var buffer = new byte[Chunk];

        long total = 0;

        int read;

        while ((read = source.Read(buffer)) > 0)
        {
            var span = buffer.AsSpan(0, read);

            hash.AppendData(span);
            git1.AppendData(span);
            git256.AppendData(span);

            total += read;

            if (text)
            {
                text = Count(decoder, span, false, ref chars);
            }
        }

        if (text)
        {
            text = Count(decoder, [], true, ref chars);
        }

        if (total != size)
        {
            throw new IOException($"'{file}' changed while it was read.");
        }

        return new ContentFacts(Convert.ToHexStringLower(hash.GetHashAndReset()), size, text ? chars : null,
                                Convert.ToHexStringLower(git1.GetHashAndReset()), Convert.ToHexStringLower(git256.GetHashAndReset()));
    }

    /// <summary>
    /// Counts the characters of a piece of what may be text, and says whether it still is.
    /// </summary>
    private static bool Count(Decoder decoder, ReadOnlySpan<byte> span, bool flush, ref long chars)
    {
        if (span.Contains((byte)0))
        {
            return false;
        }

        try
        {
            chars += decoder.GetCharCount(span, flush);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    /// <summary>
    /// What git puts in front of the content of a blob before it hashes it.
    /// </summary>
    private static byte[] Header(long size) => Encoding.ASCII.GetBytes($"blob {size}\0");

}
