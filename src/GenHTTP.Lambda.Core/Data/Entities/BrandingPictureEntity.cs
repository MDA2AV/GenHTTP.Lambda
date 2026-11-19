namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// A picture the operator gave the installation to present itself with.
/// </summary>
/// <remarks>
/// One per purpose - the logo, for now - which is why the purpose is the key.
/// </remarks>
public sealed class BrandingPictureEntity
{

    /// <summary>
    /// What it is for: <c>logo</c>.
    /// </summary>
    public required string Purpose { get; set; }

    /// <summary>
    /// What the picture is, as read from its content rather than taken from
    /// whoever uploaded it.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// What the picture held while pictures were rows - empty since they are
    /// files of the files integration (<c>branding/{purpose}</c>), and left
    /// for the platform to move out as it starts.
    /// </summary>
    public byte[] Content { get; set; } = [];

    /// <summary>
    /// When it was uploaded, which names the version of its address.
    /// </summary>
    public DateTime Updated { get; set; }

}
