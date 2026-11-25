using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// What the operator may give the installation to present itself with: a name
/// that fits the bar of a page, a logo a browser draws, colours and fonts.
/// </summary>
/// <remarks>
/// Said once for every door - the panel, the API, the MCP tool - and with
/// fixed numbers, which the API and the tool may write down: they protect
/// the pages every visitor loads rather than being a promise to anybody.
/// </remarks>
public static class BrandChecks
{

    /// <summary>
    /// How long a name may be: room in the bar of a phone beside its links,
    /// and in the title of a tab.
    /// </summary>
    public const int MaxName = 40;

    /// <summary>
    /// How large a logo may be. Every page loads it, once, beside the name
    /// and as the icon of the tab - a mark rather than a photograph.
    /// </summary>
    public const int MaxLogoBytes = 512 * 1024;

    /// <summary>
    /// How many files a font may be made of: every weight it has, upright and
    /// slanted, or the parts it was cut into by script.
    /// </summary>
    public const int MaxFontFiles = 12;

    /// <summary>
    /// What an SVG is served as.
    /// </summary>
    public const string Svg = SvgImage.Type;

    /// <summary>
    /// Every kind of picture a logo may be.
    /// </summary>
    public static IReadOnlyList<string> Types { get; } = [.. ImageTypes.Raster, Svg];

    #region Functionality

    /// <summary>
    /// The name as it is kept, or nothing for the platform's own: trimmed,
    /// one line, and short enough for the bar.
    /// </summary>
    public static string? Name(string? name)
    {
        var trimmed = name?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > MaxName)
        {
            throw LambdaException.Invalid($"The name must not be longer than {MaxName} characters.");
        }

        // nothing that is no letter at all, or that turns the letters around
        // it the other way - a name is read in the bar and in every title
        if (trimmed.Any(c => char.IsControl(c) || c is >= '‪' and <= '‮' or >= '⁦' and <= '⁩'))
        {
            throw LambdaException.Invalid("The name has to be one line of text, without control characters.");
        }

        return trimmed;
    }

    /// <summary>
    /// What the logo is, read from its content: a PNG, JPEG, GIF or WebP image,
    /// or an SVG drawing.
    /// </summary>
    public static string Logo(byte[] content)
    {
        if (content.Length == 0)
        {
            throw LambdaException.Invalid("The logo is empty.");
        }

        if (content.Length > MaxLogoBytes)
        {
            throw LambdaException.Invalid($"The logo must not be larger than {MaxLogoBytes / 1024} KB.");
        }

        if (ImageTypes.Sniff(content) is { } raster)
        {
            return raster;
        }

        if (!SvgImage.Is(content, out var named))
        {
            throw LambdaException.Invalid("The logo has to be a PNG, JPEG, GIF, WebP or SVG image.");
        }

        return named ? Svg : throw LambdaException.Invalid(
            $"The logo is an SVG without xmlns=\"{SvgImage.Namespace}\" on its svg element, which a browser does not draw as a picture.");
    }

    /// <summary>
    /// The colour as it is kept, or nothing for the platform's own.
    /// </summary>
    /// <param name="what">Which of the two it is, for the refusal</param>
    public static BrandColor? Color(string? color, string what)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return null;
        }

        return BrandColor.TryParse(color, out var parsed)
            ? parsed
            : throw LambdaException.Invalid($"The {what} has to be a colour as #rrggbb, such as #1a73e8.");
    }

    #endregion

}
