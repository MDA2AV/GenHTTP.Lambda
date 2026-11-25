using System.Globalization;
using System.Text;

using GenHTTP.Lambda.Services.SharedFiles;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// The style sheet a page is sent with to be drawn in the installation's
/// colours and fonts.
/// </summary>
/// <remarks>
/// Nothing but values for what the kit's own style sheet leaves open
/// (<c>index.css</c> there says what each is for and what the platform has):
/// every page of every component is drawn from the same few, so none of them
/// has to know whose they are.
///
/// Written from what was read and worked out - numbers, and names of shared
/// files, which are letters, digits, dashes, underscores and dots - and never
/// from what somebody typed.
/// </remarks>
internal static class BrandStyle
{

    /// <summary>
    /// What the fonts are called in the pages - never what they call
    /// themselves, which may be the name of one a visitor has as well.
    /// </summary>
    private const string TextFamily = "Lambda Brand Text";

    private const string HeadingFamily = "Lambda Brand Headings";

    #region Functionality

    /// <summary>
    /// The style sheet of a page that is drawn with the kit's.
    /// </summary>
    public static string Of(BrandLook look)
    {
        var sheet = new StringBuilder();

        Faces(sheet, TextFamily, look.TextFont, string.Empty);
        Faces(sheet, HeadingFamily, look.HeadingFont, string.Empty);

        sheet.Append(":root{");

        Shades(sheet, look, palette => palette.Light);

        if (look.HeadingFont.Count > 0)
        {
            // over the text's font, which draws what the headings' has no letter for
            sheet.Append($"--font-heading:\"{HeadingFamily}\",\"{TextFamily}\",var(--font-system);--heading-synthesis:none;");
        }

        sheet.Append("}html.dark{");

        Shades(sheet, look, palette => palette.Dark);

        return sheet.Append('}').ToString();
    }

    /// <summary>
    /// The fonts for a page that is not drawn with the kit's style sheet, and
    /// may answer at another address: the faces, and what its text and its
    /// headings are set in - nothing where there are none.
    /// </summary>
    /// <param name="origin">The address of the platform, without a slash at its end</param>
    /// <param name="system">What the page is set in otherwise</param>
    public static string Plain(BrandLook look, string origin, string system)
    {
        var sheet = new StringBuilder();

        if (look.TextFont.Count > 0)
        {
            Faces(sheet, TextFamily, look.TextFont, origin);

            sheet.Append($"body{{font-family:\"{TextFamily}\",{system}}}");
        }

        if (look.HeadingFont.Count > 0)
        {
            Faces(sheet, HeadingFamily, look.HeadingFont, origin);

            sheet.Append($"h1{{font-family:\"{HeadingFamily}\",\"{TextFamily}\",{system};font-synthesis-weight:none}}");
        }

        return sheet.ToString();
    }

    private static void Faces(StringBuilder sheet, string family, IReadOnlyList<SharedFile> files, string origin)
    {
        foreach (var file in files)
        {
            if (file.Font is not { } face)
            {
                continue;
            }

            var weight = face.Variable ? string.Create(CultureInfo.InvariantCulture, $"{face.From} {face.To}") : face.From.ToString(CultureInfo.InvariantCulture);

            // the words are there to be read before their font is: shown in
            // the system's meanwhile, and swapped once it has arrived
            sheet.Append($"@font-face{{font-family:\"{family}\";src:url(\"{origin}{file.VersionedPath}\");font-weight:{weight};font-style:{(face.Italic ? "italic" : "normal")};font-display:swap}}");
        }
    }

    private static void Shades(StringBuilder sheet, BrandLook look, Func<BrandPalette, BrandShades> theme)
    {
        if (look.AccentShades is { } accent)
        {
            var shades = theme(accent);

            Set(sheet, "accent-400", shades.Tint);
            Set(sheet, "accent-500", shades.Line);
            Set(sheet, "accent-600", shades.Text);
            Set(sheet, "accent-700", shades.Deep);
            Set(sheet, "accent-fill", shades.Fill);
            Set(sheet, "accent-hover", shades.Hover);
            Set(sheet, "accent-ink", shades.Ink);
            Set(sheet, "accent-send", shades.Fill);
            Set(sheet, "accent-send-hover", shades.Hover);
            Set(sheet, "accent-send-ink", shades.Ink);
            Set(sheet, "accent-chart", shades.Line);
            Set(sheet, "aurora-a", shades.Light);
        }

        if (look.SecondShades is { } second)
        {
            var shades = theme(second);

            Set(sheet, "second-400", shades.Tint);
            Set(sheet, "second-500", shades.Line);
            Set(sheet, "second-700", shades.Deep);
            Set(sheet, "second-ink", BrandPalette.InkOn(shades.Line));
        }

        if (look.SecondLight is { } light)
        {
            Set(sheet, "aurora-b", theme(light).Light);
        }
    }

    private static void Set(StringBuilder sheet, string name, BrandColor color) => sheet.Append($"--{name}:{color.Channels};");

    #endregion

}
