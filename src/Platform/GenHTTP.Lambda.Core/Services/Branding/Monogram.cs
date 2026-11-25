using System.Globalization;
using System.Security;
using System.Text;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// The picture of an installation that was named but given no logo: the first
/// letter of its name, white on a square plate.
/// </summary>
/// <remarks>
/// So a branded installation never shows the platform's mark - not beside its
/// name, and not as the icon of a browser tab, which has to be a picture. The
/// plate is square like the platform's own mark, since nothing on the pages
/// has a rounded corner, and in the colour the pages draw what can be clicked
/// with: the platform's blue, or the operator's accent. Drawn in whatever face
/// the system has: a picture cannot load one.
/// </remarks>
public static class Monogram
{

    private static readonly BrandColor Plate = new(0x1a, 0x73, 0xe8);

    /// <summary>
    /// The plate with the first letter or digit of the name on it - its first
    /// character where it has neither.
    /// </summary>
    /// <param name="accent">The shades of the operator's accent, if they gave one - of the light theme, since a tab's icon is one picture in both</param>
    public static BrandPicture Of(string name, BrandShades? accent)
    {
        var letter = FirstOf(name).ToUpperInvariant();

        var plate = accent?.Fill ?? Plate;
        var ink = accent?.Ink ?? BrandColor.White;

        var svg = $"""
                   <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64"><rect width="64" height="64" fill="{plate.Hex}"/><text x="32" y="32" dy=".35em" text-anchor="middle" font-family="-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif" font-size="36" font-weight="600" fill="{ink.Hex}">{SecurityElement.Escape(letter)}</text></svg>
                   """;

        return new BrandPicture(Encoding.UTF8.GetBytes(svg), BrandChecks.Svg, DateTime.UnixEpoch);
    }

    /// <summary>
    /// The first letter or digit as it is read - a letter with its accent, an
    /// emoji with its modifiers - or the first of anything where there is none.
    /// </summary>
    private static string FirstOf(string name)
    {
        var elements = StringInfo.GetTextElementEnumerator(name);

        string? first = null;

        while (elements.MoveNext())
        {
            var element = elements.GetTextElement();

            first ??= element;

            if (char.IsLetterOrDigit(element, 0))
            {
                return element;
            }
        }

        return first ?? "?";
    }

}
