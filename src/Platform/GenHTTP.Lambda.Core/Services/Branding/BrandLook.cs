using GenHTTP.Lambda.Services.SharedFiles;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// What the pages of the installation are drawn with: its colours and its
/// fonts.
/// </summary>
/// <remarks>
/// Apart from what the installation is called and the picture beside its name
/// (see <see cref="Brand"/>): one that only has a colour of its own is still
/// GenHTTP Lambda, under its name and its mark, in another colour - and one
/// that is named goes by its name in the platform's blue until it is given
/// one.
/// </remarks>
public sealed class BrandLook
{

    /// <summary>
    /// The platform's own colours, in whatever face the system has.
    /// </summary>
    public static BrandLook Platform { get; } = new(null, null, [], []);

    #region Get-/Setters

    /// <summary>
    /// What links, buttons and marks are drawn in, or nothing for the
    /// platform's blue.
    /// </summary>
    public BrandColor? Accent { get; }

    /// <summary>
    /// What is drawn in the platform's purple otherwise, and the second light
    /// of the moving background - or nothing for the platform's.
    /// </summary>
    public BrandColor? Second { get; }

    /// <summary>
    /// The shades of the accent, where there is one.
    /// </summary>
    public BrandPalette? AccentShades { get; }

    /// <summary>
    /// The shades of the second accent, where there is one.
    /// </summary>
    public BrandPalette? SecondShades { get; }

    /// <summary>
    /// The second light of the moving background, where it is not the
    /// platform's: the second accent, or the colour beside the first.
    /// </summary>
    public BrandPalette? SecondLight { get; }

    /// <summary>
    /// The files of the font the pages are set in; none for the system's.
    /// </summary>
    public IReadOnlyList<SharedFile> TextFont { get; }

    /// <summary>
    /// The files of the font their headings are set in; none for the text's.
    /// </summary>
    public IReadOnlyList<SharedFile> HeadingFont { get; }

    /// <summary>
    /// Whether the operator chose any of it.
    /// </summary>
    public bool Own => Accent != null || Second != null || TextFont.Count > 0 || HeadingFont.Count > 0;

    /// <summary>
    /// What the pages are sent with to be drawn this way, or nothing where
    /// they are drawn as they were built.
    /// </summary>
    public string? StyleSheet { get; }

    /// <summary>
    /// The files a page is certain to need, which it is told of before its
    /// style sheets are read: the one its text is set in, and the one its
    /// headings are.
    /// </summary>
    public IReadOnlyList<SharedFile> Preloaded { get; }

    #endregion

    #region Initialization

    private BrandLook(BrandColor? accent, BrandColor? second, IReadOnlyList<SharedFile> textFont, IReadOnlyList<SharedFile> headingFont)
    {
        Accent = accent;
        Second = second;

        TextFont = textFont;
        HeadingFont = headingFont;

        if (accent is { } first)
        {
            AccentShades = BrandPalette.Of(first);
        }

        if (second is { } other)
        {
            SecondShades = BrandPalette.Of(other);
        }

        SecondLight = SecondShades ?? (accent is { } alone ? BrandPalette.Of(BrandPalette.Beside(alone)) : null);

        StyleSheet = Own ? BrandStyle.Of(this) : null;

        Preloaded = [.. new[] { Closest(textFont, 400), Closest(headingFont, 700) }.OfType<SharedFile>().DistinctBy(f => f.Name)];
    }

    /// <summary>
    /// The look of an installation, or the platform's where nothing was chosen.
    /// </summary>
    public static BrandLook Of(BrandColor? accent, BrandColor? second, IReadOnlyList<SharedFile> textFont, IReadOnlyList<SharedFile> headingFont)
        => accent == null && second == null && textFont.Count == 0 && headingFont.Count == 0 ? Platform : new(accent, second, textFont, headingFont);

    #endregion

    #region Functionality

    /// <summary>
    /// The fonts for a page of the installation that is not one of a bundle's:
    /// the faces, and what its text and its headings are set in - nothing
    /// where there are none.
    /// </summary>
    /// <param name="origin">The address of the platform, without a slash at its end, where the page may answer at another</param>
    /// <param name="system">What the page is set in otherwise</param>
    public string FontsFor(string origin, string system) => BrandStyle.Plain(this, origin, system);

    /// <summary>
    /// The file an upright word of the given weight is drawn from, as far as
    /// their weights say.
    /// </summary>
    private static SharedFile? Closest(IReadOnlyList<SharedFile> files, int weight)
        => files.Where(f => f.Font is { Italic: false })
                .MinBy(f => weight < f.Font!.From ? f.Font.From - weight : Math.Max(0, weight - f.Font.To));

    #endregion

}
