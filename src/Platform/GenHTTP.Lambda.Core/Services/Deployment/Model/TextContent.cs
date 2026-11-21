using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Deployment.Model;

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
