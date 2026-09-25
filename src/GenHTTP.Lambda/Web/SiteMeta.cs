using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// What a search engine and a link preview are told about the site: the title
/// and description of each public page, where it canonically lives, what may
/// be crawled and the sitemap listing the rest.
/// </summary>
/// <remarks>
/// Every route is answered with the same index page, and the client names the
/// page once its script runs. A crawler that runs no script, and every chat
/// that unfurls a link, never sees that - to them each page was the front
/// page. So the public pages are named here, in the markup, before it is sent.
///
/// The pages come from <c>pages.json</c>, which the frontend build puts next to
/// the index page and the client imports as well, so the two cannot disagree.
/// </remarks>
public sealed class SiteMeta
{
    private const string Site = "GenHTTP Lambda";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly Regex TitleTag = new("<title>.*?</title>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

    #region Get-/Setters

    private string PageFile { get; }

    /// <summary>
    /// The address pages are canonical under, without a trailing slash.
    /// </summary>
    private string? PublicUrl { get; }

    #endregion

    #region Initialization

    public SiteMeta(LambdaOptions options)
    {
        PageFile = Path.Combine(options.WebRoot, "pages.json");
        PublicUrl = options.PublicUrl;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// The public page at the given path, if it is one.
    /// </summary>
    public SitePage? Find(string path)
    {
        return ReadPages().GetValueOrDefault(Normalize(path));
    }

    /// <summary>
    /// The index page, named as the given page.
    /// </summary>
    public string Render(string markup, string path, SitePage page)
    {
        var title = WebUtility.HtmlEncode($"{page.Title} - {Site}");
        var description = WebUtility.HtmlEncode(page.Description);

        markup = TitleTag.Replace(markup, _ => $"<title>{title}</title>", 1);

        markup = SetMeta(markup, "name", "description", description);
        markup = SetMeta(markup, "property", "og:title", title);
        markup = SetMeta(markup, "property", "og:description", description);
        markup = SetMeta(markup, "name", "twitter:title", title);
        markup = SetMeta(markup, "name", "twitter:description", description);

        if (PublicUrl != null)
        {
            var address = WebUtility.HtmlEncode(PublicUrl + Normalize(path));

            markup = SetMeta(markup, "property", "og:url", address);
            markup = InHead(markup, $"<link rel=\"canonical\" href=\"{address}\" />");
        }

        return markup;
    }

    /// <summary>
    /// What crawlers may visit: the site, but not the API, the editor behind
    /// its keys or the views only an administrator can open.
    /// </summary>
    /// <remarks>
    /// The lambdas are not excluded. What somebody builds and shares is theirs
    /// to have found, and it lives under <c>/lambda</c> for as long as it does.
    /// </remarks>
    public string Robots()
    {
        var robots = new StringBuilder();

        robots.Append("User-agent: *\n")
              .Append("Disallow: /api/\n")
              .Append("Disallow: /mcp\n")
              .Append("Disallow: /start\n")
              .Append("Disallow: /editor/\n")
              .Append("Disallow: /admin\n")
              .Append("Disallow: /logs\n")
              .Append("Disallow: /stats\n");

        if (PublicUrl != null)
        {
            robots.Append($"\nSitemap: {PublicUrl}/sitemap.xml\n");
        }

        return robots.ToString();
    }

    /// <summary>
    /// Every public page, under the public address - or nothing, when there is
    /// no public address to list them under.
    /// </summary>
    public string? Sitemap()
    {
        if (PublicUrl == null)
        {
            return null;
        }

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var paths = ReadPages().Keys;

        var sitemap = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "urlset", paths.Select(path => new XElement(ns + "url", new XElement(ns + "loc", PublicUrl + path))))
        );

        return sitemap.Declaration + "\n" + sitemap;
    }

    private IReadOnlyDictionary<string, SitePage> ReadPages()
    {
        // read every time, because "npm run build" updates a running server;
        // it is a few hundred bytes, and only public pages ask for it
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, SitePage>>(File.ReadAllText(PageFile), Json) ?? [];
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            // no build, or a broken one: the pages are still served, unnamed
            return new Dictionary<string, SitePage>();
        }
    }

    /// <summary>
    /// A path the way the table spells it: "/docs/" is "/docs".
    /// </summary>
    private static string Normalize(string path)
        => path.Length > 1 ? path.TrimEnd('/') : path;

    /// <summary>
    /// Replaces the content of a meta tag, or adds the tag if the page has none.
    /// </summary>
    private static string SetMeta(string markup, string attribute, string key, string value)
    {
        var tag = $"<meta {attribute}=\"{key}\" content=\"{value}\" />";

        var existing = new Regex($"<meta\\s+{attribute}=\"{Regex.Escape(key)}\"[^>]*>", RegexOptions.IgnoreCase);

        return existing.IsMatch(markup) ? existing.Replace(markup, _ => tag, 1) : InHead(markup, tag);
    }

    private static string InHead(string markup, string element)
    {
        var end = markup.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);

        return end < 0 ? markup : markup.Insert(end, element + "\n");
    }

    #endregion

}

/// <summary>
/// A page a search engine should find, and what it should say about it.
/// </summary>
public sealed record SitePage(string Title, string Description);
