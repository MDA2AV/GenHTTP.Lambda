using System.ComponentModel.DataAnnotations;
using System.Text;

using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.SharedFiles;

using GenHTTP.Modules.Mcp;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// What the operator's agent may change about the installation itself rather
/// than about a lambda on it: its name and its logo, its colours and its
/// fonts.
/// </summary>
/// <remarks>
/// On the operator's endpoint, and there only where the host lets the
/// operator brand the installation (see <see cref="OperatorMcpBuilder"/>).
/// The token is checked in front of it, as for every tool there; it used to
/// be an argument of this tool on the public endpoint, where every agent was
/// shown it and the token landed in the transcript.
///
/// The same changes the panel's Branding section makes and the API's
/// <c>/admin/branding</c>, through the same service, logged in the same words.
/// A font is named rather than sent: it is too large for an argument an agent
/// writes, so it is a shared file first.
/// </remarks>
public sealed class BrandingTools(IBrandingService branding, ISharedFiles shared, ILogger<BrandingTools> logger)
{

    [Tool("customize_installation", Title = "Name this installation and give it a logo, colours and fonts", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("The name and the logo the installation's pages show in place of GenHTTP Lambda's - in the bar of every page, the titles of the pages and their link previews, and the icon of the browser tab - and the colours and fonts the pages are drawn with. Nothing about a lambda changes. What is left out stays as it is; with nothing, it returns how the installation presents itself. An empty logo takes the logo away, which cannot be undone but by sending it again.")]
    public ValueTask<object> CustomizeInstallation([Summary("What the installation is called: one line of at most 40 characters. An empty string gives it back the name GenHTTP Lambda."), MaxLength(BrandChecks.MaxName)] string? name = null,
                                                   [Summary("The logo: an SVG's markup as it is, the name of a shared picture, or a PNG, JPEG, GIF, WebP or SVG image base64 encoded - at most 512 KB. A square mark reads best: it stands beside the name in the bar and is the icon of the browser tab, on a light and a dark background. An SVG needs xmlns=\"http://www.w3.org/2000/svg\" on its svg element. An empty string removes the logo.")] string? logo = null,
                                                   [Summary("What links, buttons and marks are drawn in, and the first light of the moving background, as #rrggbb. Any colour may be one: it is drawn lighter or darker where it is text, as far as reading it takes. An empty string gives back the platform's blue.")] string? accent = null,
                                                   [Summary("What is drawn in the platform's purple otherwise, and the second light of the moving background, as #rrggbb. An empty string gives back the platform's; the second light is then the colour beside the accent.")] string? secondAccent = null,
                                                   [Summary("The font of the pages, as the names of the shared files it is made of (list_shared_files): one variable font, or a file for each weight and slant. textFontLacks in the answer names the languages of the pages it has too few letters for, whose words would be drawn in two fonts: share the file that has them and add it. An empty list gives back the system's.")] List<string>? textFont = null,
                                                   [Summary("The font of the headings and of the installation's name in the bar, the same way. An empty list sets them in the text's.")] List<string>? headingFont = null,
                                                   [Summary("Only check it and work out what it would look like, changing nothing.")] bool preview = false)
        => Refusing.Async(() => ChangeAsync(name, logo, accent, secondAccent, textFont, headingFont, preview));

    private async ValueTask<object> ChangeAsync(string? name, string? logo, string? accent, string? second, List<string>? textFont, List<string>? headingFont, bool preview)
    {
        // all of it is checked before any of it is changed, so a refusal changes nothing
        var given = BrandChecks.Name(name);

        var picture = logo != null && logo.Trim().Length > 0 ? await PictureAsync(logo.Trim()) : null;

        if (picture != null)
        {
            BrandChecks.Logo(picture);
        }

        var brand = branding.Current;

        var look = await branding.ComposeAsync(accent == null ? brand.Look.Accent : BrandChecks.Color(accent, "accent"),
                                               second == null ? brand.Look.Second : BrandChecks.Color(second, "second accent"),
                                               textFont ?? [.. brand.Look.TextFont.Select(f => f.Name)],
                                               headingFont ?? [.. brand.Look.HeadingFont.Select(f => f.Name)]);

        if (preview)
        {
            logger.LogInformation("Previewed branding by operator");

            var drawn = (logo != null, picture) switch
            {
                (false, _) => brand.Logo,
                (_, null) => null,
                (_, { } sent) => new BrandPicture(sent, BrandChecks.Logo(sent), DateTime.UtcNow)
            };

            return Describe(Brand.Of(name == null ? brand.Given : given, drawn, look), false);
        }

        if (name != null)
        {
            (brand, var previous) = await branding.RenameAsync(name);

            logger.BrandingRenamed(previous, brand);
        }

        if (logo != null)
        {
            brand = await ChangeLogoAsync(brand, picture);
        }

        if (accent != null || second != null)
        {
            (brand, var previous) = await branding.ColorAsync(look.Accent, look.Second);

            logger.BrandingColored(previous, brand);
        }

        if (textFont != null || headingFont != null)
        {
            (brand, var previous) = await branding.FontAsync([.. look.TextFont.Select(f => f.Name)], [.. look.HeadingFont.Select(f => f.Name)]);

            logger.BrandingFonts(previous, brand);
        }

        if (name == null && logo == null && accent == null && second == null && textFont == null && headingFont == null)
        {
            logger.LogInformation("Read branding by operator");
        }

        return Describe(brand, true);
    }

    private async ValueTask<Brand> ChangeLogoAsync(Brand brand, byte[]? picture)
    {
        if (picture != null)
        {
            brand = await branding.ChangeLogoAsync(picture);

            logger.LogoUploaded(brand);

            return brand;
        }

        var had = brand.Logo != null;

        brand = await branding.RemoveLogoAsync();

        if (had)
        {
            logger.LogoRemoved();
        }

        return brand;
    }

    /// <summary>
    /// The picture from what the agent sent: the markup of an SVG as it is,
    /// the shared file of that name - base64 has no dot in it - or base64.
    /// </summary>
    private async ValueTask<byte[]> PictureAsync(string logo)
    {
        if (logo.StartsWith('<'))
        {
            return Encoding.UTF8.GetBytes(logo);
        }

        if (logo.Contains('.'))
        {
            return await shared.FindAsync(logo) is { Type.Kind: SharedFileKind.Picture } file && await shared.ReadAsync(file) is { } content
                ? content
                : throw LambdaException.Invalid("There is no shared picture of that name: list_shared_files says which there are.");
        }

        try
        {
            return Convert.FromBase64String(logo);
        }
        catch (FormatException)
        {
            throw LambdaException.Invalid("The logo has to be an SVG's markup, the name of a shared picture, or a picture base64 encoded.");
        }
    }

    private static object Describe(Brand brand, bool changed) => new
    {
        name = brand.Name,
        named = brand.Given != null,
        logo = brand.Logo is { } logo ? new { type = logo.Type, bytes = logo.Content.Length } : null,
        picture = changed ? brand.PicturePath : null,
        accent = brand.Look.Accent?.Hex,
        secondAccent = brand.Look.Second?.Hex,
        drawnAs = brand.Look.AccentShades is { } shades ? new { light = Describe(shades.Light, shades.Light.Text), dark = Describe(shades.Dark, shades.Dark.Tint) } : null,
        textFont = Describe(brand.Look.TextFont),
        textFontLacks = FontLetters.Lacking(brand.Look.TextFont),
        headingFont = Describe(brand.Look.HeadingFont),
        headingFontLacks = FontLetters.Lacking(brand.Look.HeadingFont),
        shown = !changed ? "Nothing was changed: this is how the pages would be."
              : brand.Own ? "The pages show this name, with the logo beside it - or the first letter of the name on a plate where there is no logo - from the next page anybody opens. Ask the operator to look at the front page in a light and a dark theme."
              : brand.Look.Own ? "The pages show GenHTTP Lambda, with its own mark, in these colours and fonts from the next page anybody opens. Ask the operator to look at the front page in a light and a dark theme."
              : "The pages show GenHTTP Lambda, with its own mark, as it is drawn."
    };

    private static object Describe(BrandShades shades, BrandColor text) => new { text = text.Hex, button = shades.Fill.Hex, writtenOnIt = shades.Ink.Hex };

    private static object Describe(IReadOnlyList<SharedFile> font)
        => font.Select(f => new { name = f.Name, family = f.Font!.Family, from = f.Font.From, to = f.Font.To, italic = f.Font.Italic });

}
