namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// A file whoever runs the installation shares across it.
/// </summary>
/// <remarks>
/// What it is, never what it holds: its content is a file of the files
/// integration (<c>shared/{hash}</c>), so a name that is given another content
/// names another file rather than a file that is being written.
/// </remarks>
public sealed class SharedFileEntity
{

    public required string Name { get; set; }

    public int Bytes { get; set; }

    /// <summary>
    /// The SHA-256 of the content, as a hex string.
    /// </summary>
    public required string Hash { get; set; }

    public DateTime Updated { get; set; }

    /// <summary>
    /// What a font calls itself, and nothing for any other file - as is the
    /// rest of what one says of itself.
    /// </summary>
    public string? FontFamily { get; set; }

    public int? FontFrom { get; set; }

    public int? FontTo { get; set; }

    public bool? FontItalic { get; set; }

    /// <summary>
    /// The letters a font has, of those the pages are written with.
    /// </summary>
    public string? FontLetters { get; set; }

}
