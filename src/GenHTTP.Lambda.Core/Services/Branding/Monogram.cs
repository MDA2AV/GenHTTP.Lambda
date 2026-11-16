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
/// has a rounded corner, and in the blue the pages draw what can be clicked
/// with. Drawn in whatever face the system has: a picture cannot load one.
/// </remarks>
public static class Monogram
{

    private const string Plate = "#1a73e8";

    /// <summary>
    /// The plate with the first letter or digit of the name on it - its first
    /// character where it has neither.
    /// </summary>
    public static BrandPicture Of(string name)
    {
        var letter = FirstOf(name).ToUpperInvariant();

        var svg = $"""
                   <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64"><rect width="64" height="64" fill="{Plate}"/><text x="32" y="32" dy=".35em" text-anchor="middle" font-family="-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif" font-size="36" font-weight="600" fill="#ffffff">{SecurityElement.Escape(letter)}</text></svg>
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
