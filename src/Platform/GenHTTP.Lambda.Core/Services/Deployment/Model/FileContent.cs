using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Deployment.Model;

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
