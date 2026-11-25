using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// What a page of a bundle says about whose installation it is, before a
/// script of it runs: the name in its title and in the names its link
/// preview is given, the icon of its tab, the brand its scripts draw the bar
/// with, and the colours and fonts it is drawn in.
/// </summary>
/// <remarks>
/// Every bundle's index page names GenHTTP Lambda and links the platform's
/// icons, as it was built. On an installation its operator named, or gave a
/// logo, the server writes theirs into it instead - on every page it sends,
/// the editor's and the panel's as well as the site's - so neither a tab nor
/// the first moment of a page shows the platform's, and the scripts draw the
/// bar from <c>#lambda-brand</c> rather than asking for it. Its colours and
/// fonts are values for what the bundles' style sheets leave open
/// (<c>#lambda-look</c>, see <c>BrandStyle</c>), so a page is drawn in them
/// from its first moment as well - with the font it is set in named ahead of
/// everything else, to be there by then. Where the brand is the platform's
/// own, a page is sent as it was built, to the byte.
/// </remarks>
public static partial class PageBranding
{

    /// <summary>
    /// The element the brand is handed to the scripts in, as <c>brand.ts</c> in the kit reads it.
    /// </summary>
    public const string Element = "lambda-brand";

    /// <summary>
    /// The element the colours and the fonts are in, which the panel writes
    /// anew as the operator saves others.
    /// </summary>
    public const string LookElement = "lambda-look";

    private static readonly HtmlEncoder Html = HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All);

    [GeneratedRegex("(<title>)(.*?)(</title>)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex TitleTag();

    [GeneratedRegex("(<meta\\s+(?:name|property)=\"(?:og:title|og:site_name|og:image:alt|twitter:title|application-name|apple-mobile-web-app-title)\"\\s+content=\")([^\"]*)(\")", RegexOptions.IgnoreCase)]
    private static partial Regex NamingMeta();

    [GeneratedRegex("<link\\b[^>]*\\brel=\"[^\"]*icon[^\"]*\"[^>]*>\\s*", RegexOptions.IgnoreCase)]
    private static partial Regex IconLink();

    #region Functionality

    /// <summary>
    /// The page as the given brand presents it - as it was built, where the
    /// brand is the platform's own.
    /// </summary>
    public static string Apply(string markup, Brand brand)
    {
        if (brand.Own && brand.Picture is { } picture)
        {
            markup = Identify(markup, brand, picture);
        }

        return brand.Look.StyleSheet is { } sheet ? Draw(markup, brand.Look, sheet) : markup;
    }

    /// <summary>
    /// The page in the installation's colours and fonts: after the bundle's
    /// own style sheet, whose values these take the place of.
    /// </summary>
    private static string Draw(string markup, BrandLook look, string sheet)
    {
        var preloads = look.Preloaded.Select(f => $"<link rel=\"preload\" href=\"{Html.Encode(f.VersionedPath)}\" as=\"font\" type=\"{f.Type.Type}\" crossorigin />\n");

        return SiteMeta.InHead(markup, $"{string.Concat(preloads)}<style id=\"{LookElement}\">{sheet}</style>");
    }

    /// <summary>
    /// The page under the installation's name and picture.
    /// </summary>
    private static string Identify(string markup, Brand brand, BrandPicture picture)
    {
        var platform = Html.Encode(Brand.PlatformName);
        var name = Html.Encode(brand.Name);

        // the name wherever the page names the installation before a script
        // names the page: its title, and what a link preview calls the site -
        // only where the name is the whole of it or ends it, as the platform
        // names a page, so nothing a page says of its own is ever renamed
        string Rename(Match tag) => tag.Groups[1].Value + Renamed(tag.Groups[2].Value, platform, name) + tag.Groups[3].Value;

        markup = TitleTag().Replace(markup, Rename, 1);
        markup = NamingMeta().Replace(markup, Rename);

        // the picture as the only icon, where the platform's were: a touch
        // icon has to be a raster image, so an SVG is none
        var icons = $"<link rel=\"icon\" href=\"{Html.Encode(brand.PicturePath!)}\" type=\"{picture.Type}\" />";

        if (picture.Raster)
        {
            icons += $"\n<link rel=\"apple-touch-icon\" href=\"{Html.Encode(brand.PicturePath!)}\" />";
        }

        markup = IconLink().Replace(markup, string.Empty);

        // the script's text, where the default encoder escapes every angle
        // bracket, so nothing an operator named ends it early
        var said = JsonSerializer.Serialize(new { name = brand.Name, logo = brand.PicturePath });

        return SiteMeta.InHead(markup, $"{icons}\n<script id=\"{Element}\" type=\"application/json\">{said}</script>");
    }

    /// <summary>
    /// The installation's name in place of the platform's, where the
    /// platform's is all a text says or what ends it: <c>GenHTTP Lambda</c>,
    /// <c>Editor - GenHTTP Lambda</c>.
    /// </summary>
    private static string Renamed(string text, string platform, string name)
    {
        if (text == platform)
        {
            return name;
        }

        var suffix = " - " + platform;

        return text.EndsWith(suffix, StringComparison.Ordinal) ? text[..^platform.Length] + name : text;
    }

    /// <summary>
    /// The bundle's manifest, naming the installation and showing its picture
    /// as its icon - or nothing, where the brand is the platform's own and
    /// the manifest is served as it was built.
    /// </summary>
    /// <param name="built">The manifest the bundle was built with</param>
    public static string? Manifest(string built, Brand brand)
    {
        if (!brand.Own || brand.Picture is not { } picture)
        {
            return null;
        }

        JsonObject manifest;

        try
        {
            manifest = JsonNode.Parse(built) as JsonObject ?? [];
        }
        catch (JsonException)
        {
            manifest = [];
        }

        manifest["name"] = brand.Name;
        manifest["short_name"] = brand.Name;
        manifest["icons"] = new JsonArray(new JsonObject { ["src"] = brand.PicturePath, ["sizes"] = "any", ["type"] = picture.Type });

        return manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
    }

    #endregion

}
