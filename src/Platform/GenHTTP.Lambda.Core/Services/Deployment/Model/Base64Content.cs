namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// Bytes as a door sent them, in base64.
/// </summary>
internal sealed class Base64Content(string base64) : FileContent
{

    public string Base64 => base64;

    public override Stream OpenRead() => new MemoryStream(ReadBytes(), false);

    public override byte[] ReadBytes() => Convert.FromBase64String(base64);

}
