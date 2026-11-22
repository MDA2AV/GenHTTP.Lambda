using System.Security.Cryptography;

namespace GenHTTP.Lambda.Services.Deployment.Model;

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
