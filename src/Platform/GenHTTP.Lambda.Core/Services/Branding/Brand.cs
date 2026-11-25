using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Services.Hosting;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// How the installation presents itself on its pages: the name they show,
/// the picture beside it, and the colours and fonts they are drawn with.
/// </summary>
/// <remarks>
/// GenHTTP Lambda's own, drawn by the pages with the platform's mark, until
/// the operator gives it a name or a logo - where the host lets them (see
/// <c>LambdaOptions.Branding</c>). From then on it is theirs: their name in
/// plain words, and beside it their logo, or the first letter of the name
/// drawn on a plate where they gave none - which is also the icon of the
/// browser tab, so no page of a branded installation shows the platform's.
///
/// What the pages are drawn with is the operator's on its own
/// (<see cref="Look"/>): a colour alone makes nobody another installation, and
/// a name alone leaves the platform's blue.
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
    public static Brand Platform { get; } = new(null, null, BrandLook.Platform);

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

    /// <summary>
    /// The colours and fonts its pages are drawn with.
    /// </summary>
    public BrandLook Look { get; }

    #endregion

    #region Initialization

    private Brand(string? given, BrandPicture? logo, BrandLook look)
    {
        Given = given;
        Logo = logo;
        Look = look;

        if (Own)
        {
            Picture = logo ?? Monogram.Of(Name, look.AccentShades?.Light);
            PicturePath = $"{PlatformRoutes.ApiBase}/system/logo?v={VersionOf(Name, Picture)}";
        }
    }

    /// <summary>
    /// The installation as its operator gave it, or the platform's own where
    /// they gave nothing.
    /// </summary>
    public static Brand Of(string? given, BrandPicture? logo, BrandLook look) => given == null && logo == null && !look.Own ? Platform : new(given, logo, look);

    /// <summary>
    /// The same installation by another name.
    /// </summary>
    public Brand Named(string? given) => Of(given, Logo, Look);

    /// <summary>
    /// The same installation with another logo.
    /// </summary>
    public Brand With(BrandPicture? logo) => Of(Given, logo, Look);

    /// <summary>
    /// The same installation, drawn in other colours and fonts.
    /// </summary>
    public Brand With(BrandLook look) => Of(Given, Logo, look);

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
