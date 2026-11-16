namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// What a picture somebody uploaded is, read from its first bytes.
/// </summary>
/// <remarks>
/// Never from what the uploader claims: the pictures are served from the
/// platform's own origin, and a type taken on trust is how a page of script
/// gets served as if it were one. Shared by every door a picture comes in at -
/// a showcase entry, the logo of the installation.
/// </remarks>
public static class ImageTypes
{

    /// <summary>
    /// The pictures a browser draws wherever an image goes, by what they are
    /// served as.
    /// </summary>
    public static readonly IReadOnlyList<string> Raster = ["image/png", "image/jpeg", "image/gif", "image/webp"];

    /// <summary>
    /// What a PNG, JPEG, GIF or WebP image is, or nothing for anything else.
    /// </summary>
    public static string? Sniff(ReadOnlySpan<byte> content)
    {
        if (content.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            return "image/png";
        }

        if (content.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]))
        {
            return "image/jpeg";
        }

        if (content.StartsWith("GIF87a"u8) || content.StartsWith("GIF89a"u8))
        {
            return "image/gif";
        }

        if (content.Length >= 12 && content.StartsWith("RIFF"u8) && content[8..12].SequenceEqual("WEBP"u8))
        {
            return "image/webp";
        }

        return null;
    }

}
