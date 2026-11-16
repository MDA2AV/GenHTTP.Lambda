using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Services.Hosting;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// How the installation presents itself on its pages: the name they show,
/// and the picture beside it.
/// </summary>
/// <remarks>
/// GenHTTP Lambda's own, drawn by the pages with the platform's mark, until
/// the operator gives it a name or a logo - where the host lets them (see
/// <c>LambdaOptions.Branding</c>). From then on it is theirs: their name in
/// plain words, and beside it their logo, or the first letter of the name
/// drawn on a plate where they gave none - which is also the icon of the
/// browser tab, so no page of a branded installation shows the platform's.
/// </remarks>
public sealed class Brand
{

    /// <summary>
    /// What the pages call an installation nobody named.
    /// </summary>
    public const string PlatformName = "GenHTTP Lambda";

    /// <summary>
    /// The platform's own: its name, and its mark drawn by the pages.
    /// </summary>
    public static Brand Platform { get; } = new(null, null);

    #region Get-/Setters

    /// <summary>
    /// What the operator named the installation, or nothing where it goes by
    /// the platform's name.
    /// </summary>
    public string? Given { get; }

    /// <summary>
    /// The name its pages show.
    /// </summary>
    public string Name => Given ?? PlatformName;

    /// <summary>
    /// The logo the operator gave it, if they did.
    /// </summary>
    public BrandPicture? Logo { get; }

    /// <summary>
    /// Whether the operator gave it a name or a logo - its pages then draw it
    /// as theirs rather than as GenHTTP Lambda.
    /// </summary>
    public bool Own => Given != null || Logo != null;

    /// <summary>
    /// What the pages show beside the name: the logo, or the letter drawn for
    /// the name - nothing where they draw the platform's mark themselves.
    /// </summary>
    public BrandPicture? Picture { get; }

    /// <summary>
    /// Where the pages load <see cref="Picture"/> from, named after what it
    /// is so it may be cached for good - nothing where there is none.
    /// </summary>
    public string? PicturePath { get; }

    #endregion

    #region Initialization

    private Brand(string? given, BrandPicture? logo)
    {
        Given = given;
        Logo = logo;

        if (Own)
        {
            Picture = logo ?? Monogram.Of(Name);
            PicturePath = $"{PlatformRoutes.ApiBase}/system/logo?v={VersionOf(Name, Picture)}";
        }
    }

    /// <summary>
    /// The installation as its operator gave it, or the platform's own where
    /// they gave nothing.
    /// </summary>
    public static Brand Of(string? given, BrandPicture? logo) => given == null && logo == null ? Platform : new(given, logo);

    /// <summary>
    /// A short name for what the picture and the name are, which changes
    /// whenever either does - so a page asks for the picture again once it
    /// is another, and never before.
    /// </summary>
    private static string VersionOf(string name, BrandPicture picture)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        hash.AppendData(Encoding.UTF8.GetBytes(name));
        hash.AppendData(Encoding.UTF8.GetBytes(picture.Type));
        hash.AppendData(picture.Content);

        return Convert.ToHexString(hash.GetHashAndReset())[..12].ToLowerInvariant();
    }

    #endregion

}

/// <summary>
/// A picture the installation presents itself with, as it is served.
/// </summary>
/// <param name="Type">What it is, as read from its content: <c>image/png</c>, <c>image/svg+xml</c>, …</param>
/// <param name="Updated">When the operator uploaded it, or when it was drawn</param>
public sealed record BrandPicture(byte[] Content, string Type, DateTime Updated)
{

    /// <summary>
    /// Whether every page a link is previewed with can show it - a social
    /// network draws no SVG.
    /// </summary>
    public bool Raster => Type != BrandChecks.Svg;

}
