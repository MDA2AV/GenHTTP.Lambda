using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// What a search engine and a link preview are told about the site: the title
/// and description of each public page in each language, where it canonically
/// lives and where it lives in the other languages, what may be crawled and the
/// sitemap listing the rest.
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

    /// <summary>
    /// The picture shown for a page that has none of its own.
    /// </summary>
    private const string DefaultImage = "/social/default.png";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly Regex TitleTag = new("<title>.*?</title>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex HtmlTag = new("<html\\b[^>]*>", RegexOptions.IgnoreCase);

    /// <summary>
    /// Escapes what markup needs escaped and leaves every language's letters
    /// as they are - a title reads "veröffentlichen", not "ver&amp;#246;ffentlichen".
    /// </summary>
    private static readonly HtmlEncoder Html = HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All);

    private static string Encode(string value) => Html.Encode(value);

    private static readonly Regex LangAttribute = new("\\slang=\"[^\"]*\"", RegexOptions.IgnoreCase);

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
    /// The public page at the given path, in the language the path names - or
    /// nothing, for a path that has no language or is no page.
    /// </summary>
    public SitePage? Find(string path)
    {
        var normalized = Normalize(path);

        if (SiteLanguages.Of(normalized) is not { } language)
        {
            return null;
        }

        var bare = SiteLanguages.Without(normalized);

        if (!ReadPages().TryGetValue(bare, out var entry) || !entry.Text.TryGetValue(language, out var text))
        {
            return null;
        }

        // the languages it is written in, which are all it may name as its translations
        var languages = SiteLanguages.All.Where(entry.Text.ContainsKey).ToList();

        return new SitePage(language, bare, text.Title, text.Description, languages, entry.Image);
    }

    /// <summary>
    /// Whether the path is a public page asked for without a language.
    /// </summary>
    public bool IsPage(string path) => ReadPages().ContainsKey(Normalize(path));

    /// <summary>
    /// The index page, named as the given page and in its language.
    /// </summary>
    public string Render(string markup, SitePage page)
    {
        var title = Encode($"{page.Title} - {Site}");
        var description = Encode(page.Description);

        markup = HtmlTag.Replace(markup, tag => Lang(tag.Value, SiteLanguages.TagOf(page.Language)), 1);

        markup = TitleTag.Replace(markup, _ => $"<title>{title}</title>", 1);

        markup = SetMeta(markup, "name", "description", description);
        markup = SetMeta(markup, "property", "og:title", title);
        markup = SetMeta(markup, "property", "og:description", description);
        markup = SetMeta(markup, "name", "twitter:title", title);
        markup = SetMeta(markup, "name", "twitter:description", description);

        // which language the preview is in, and which others there are
        markup = SetMeta(markup, "property", "og:locale", SiteLanguages.Locales[page.Language]);

        markup = InHead(markup, string.Join("\n", page.Languages.Where(l => l != page.Language)
                                                        .Select(l => $"<meta property=\"og:locale:alternate\" content=\"{SiteLanguages.Locales[l]}\" />")));

        // a preview needs the full address of the picture, which only the
        // public address can give - without it, the path is still better than
        // the front page's picture on every page
        var image = Encode((PublicUrl ?? string.Empty) + (page.Image ?? DefaultImage));

        markup = SetMeta(markup, "property", "og:image", image);
        markup = SetMeta(markup, "property", "og:image:alt", title);
        markup = SetMeta(markup, "name", "twitter:image", image);

        if (PublicUrl != null)
        {
            var address = Encode(PublicUrl + SiteLanguages.In(page.Language, page.Path));

            markup = SetMeta(markup, "property", "og:url", address);
            markup = InHead(markup, $"<link rel=\"canonical\" href=\"{address}\" />");
            markup = InHead(markup, Alternates(page));
        }

        if (page.Path == "/")
        {
            markup = InHead(markup, StructuredData.Render(Site, page.Description, page.Language));
        }

        return markup;
    }

    /// <summary>
    /// The page in every language it is written in, and without one for a
    /// visitor matching none of them - where they are sent on to the language
    /// they prefer.
    /// </summary>
    private string Alternates(SitePage page)
    {
        var links = page.Languages.SelectMany(language => SiteLanguages.HreflangsOf(language).Select(hreflang => (hreflang, SiteLanguages.In(language, page.Path))))
                                  .Append(("x-default", page.Path))
                                  .Select(link => $"<link rel=\"alternate\" hreflang=\"{link.Item1}\" href=\"{Encode(PublicUrl + link.Item2)}\" />");

        return string.Join("\n", links);
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
    /// Every public page in every language, under the public address, each
    /// with the addresses of its translations - or nothing, when there is no
    /// public address to list them under.
    /// </summary>
    public string? Sitemap()
    {
        if (PublicUrl == null)
        {
            return null;
        }

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XNamespace xhtml = "http://www.w3.org/1999/xhtml";

        var pages = ReadPages();

        XElement Alternate(string hreflang, string path)
            => new(xhtml + "link",
                   new XAttribute("rel", "alternate"),
                   new XAttribute("hreflang", hreflang),
                   new XAttribute("href", PublicUrl + path));

        var urls = pages.SelectMany(page => SiteLanguages.All.Where(page.Value.Text.ContainsKey)
                                                         .Select(language => new XElement(ns + "url",
                                                             new XElement(ns + "loc", PublicUrl + SiteLanguages.In(language, page.Key)),
                                                             SiteLanguages.All.Where(page.Value.Text.ContainsKey)
                                                                         .SelectMany(other => SiteLanguages.HreflangsOf(other)
                                                                                                           .Select(hreflang => Alternate(hreflang, SiteLanguages.In(other, page.Key)))),
                                                             Alternate("x-default", page.Key))));

        var sitemap = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "urlset", new XAttribute(XNamespace.Xmlns + "xhtml", xhtml.NamespaceName), urls)
        );

        return sitemap.Declaration + "\n" + sitemap;
    }

    private IReadOnlyDictionary<string, SiteEntry> ReadPages()
    {
        // read every time, because "npm run build" updates a running server;
        // it is a few kilobytes, and only public pages ask for it
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, SiteEntry>>(File.ReadAllText(PageFile), Json) ?? [];
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            // no build, or a broken one: the pages are still served, unnamed
            return new Dictionary<string, SiteEntry>();
        }
    }

    /// <summary>
    /// A path the way the table spells it: "/docs/" is "/docs".
    /// </summary>
    public static string Normalize(string path)
        => path.Length > 1 ? path.TrimEnd('/') : path;

    /// <summary>
    /// The opening tag of the document, in the given language.
    /// </summary>
    private static string Lang(string tag, string language)
    {
        var attribute = $" lang=\"{language}\"";

        return LangAttribute.IsMatch(tag) ? LangAttribute.Replace(tag, attribute, 1) : tag.Insert("<html".Length, attribute);
    }

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
/// A page a search engine should find, in one of its languages, and what it
/// should say about it there - with the path of the picture a link preview
/// shows, if it has one of its own.
/// </summary>
/// <param name="Language">The language it is shown in</param>
/// <param name="Path">Its path without a language, as <c>pages.json</c> spells it</param>
/// <param name="Languages">Every language the page is written in, its own included</param>
public sealed record SitePage(string Language, string Path, string Title, string Description, IReadOnlyList<string> Languages, string? Image = null);

/// <summary>
/// A page as <c>pages.json</c> has it: its picture, and its words by language.
/// </summary>
public sealed record SiteEntry(string? Image, Dictionary<string, SiteText> Text);

/// <summary>
/// The name and description of a page in one language.
/// </summary>
public sealed record SiteText(string Title, string Description);
