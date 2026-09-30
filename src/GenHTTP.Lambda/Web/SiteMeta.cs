using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Source;

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

    /// <summary>
    /// The entry of <c>pages.json</c> that names the page of every published
    /// source, with <c>{name}</c> where the name of the lambda goes.
    /// </summary>
    /// <remarks>
    /// A template rather than a page: its address has a placeholder in it, so
    /// it is never looked up, listed or rendered as one - only its words are
    /// used, which is how a page the build cannot know is named in every
    /// language the build does know.
    /// </remarks>
    public const string SourceTemplate = "/source/:key";

    /// <summary>
    /// What the page of a published source is called where the build named it
    /// nothing.
    /// </summary>
    private static readonly SiteText SourceFallback = new("{name} - Source code",
        "The source code of {name}, an app built with GenHTTP Lambda: read it, download it and run it anywhere.");

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

        if (!Pages().TryGetValue(bare, out var entry) || !entry.Text.TryGetValue(language, out var text))
        {
            return null;
        }

        // the languages it is written in, which are all it may name as its translations
        var languages = SiteLanguages.All.Where(entry.Text.ContainsKey).ToList();

        // the picture drawn in its language, where there is one, and the page's own otherwise
        return new SitePage(language, bare, text.Title, text.Description, languages, text.Image ?? entry.Image);
    }

    /// <summary>
    /// Whether the path is a public page asked for without a language.
    /// </summary>
    public bool IsPage(string path) => Pages().ContainsKey(Normalize(path));

    /// <summary>
    /// Whether the build named any public pages at all.
    /// </summary>
    public bool HasPages => Pages().Count > 0;

    /// <summary>
    /// The page of a published source, in a language: named after the lambda
    /// in the words the build gave the page, and described by what the lambda
    /// says it is.
    /// </summary>
    /// <param name="path">Its path without a language: /source/{key}, and whatever follows</param>
    /// <param name="name">What the lambda is called</param>
    /// <param name="about">What it says it is, if it says</param>
    /// <param name="image">Its picture on the showcase, if it has one</param>
    public SitePage Source(string language, string path, string name, string? about, string? image)
    {
        var entry = ReadPages().GetValueOrDefault(SourceTemplate);

        var text = entry?.Text.GetValueOrDefault(language) ?? entry?.Text.GetValueOrDefault(SiteLanguages.Default) ?? SourceFallback;

        var languages = entry != null ? SiteLanguages.All.Where(entry.Text.ContainsKey).ToList() : [.. SiteLanguages.All];

        if (!languages.Contains(language))
        {
            languages.Add(language);
        }

        var description = string.IsNullOrWhiteSpace(about) ? text.Description.Replace("{name}", name, StringComparison.Ordinal) : about;

        return new SitePage(language, path, text.Title.Replace("{name}", name, StringComparison.Ordinal), description, languages,
                            image ?? text.Image ?? entry?.Image);
    }

    /// <summary>
    /// The page of a published source, named, with what a search engine is
    /// told about the code beside it in schema.org terms.
    /// </summary>
    public string RenderSource(string markup, SitePage page, SourceSchema schema)
    {
        markup = Render(markup, page);

        return PublicUrl == null ? markup : InHead(markup, StructuredData.RenderSource(schema, PublicUrl, SiteLanguages.In(page.Language, page.Path), page.Language));
    }

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
        var picture = page.Image ?? DefaultImage;

        var image = Encode((PublicUrl ?? string.Empty) + picture);

        markup = SetMeta(markup, "property", "og:image", image);
        markup = SetMeta(markup, "property", "og:image:type", ImageTypeOf(page, picture));
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
              .Append("Disallow: /features/\n")
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
    /// <param name="sources">The published sources, each a page in every language the page of one is written in</param>
    public string? Sitemap(IReadOnlyList<SourceAddress>? sources = null)
    {
        if (PublicUrl == null)
        {
            return null;
        }

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XNamespace xhtml = "http://www.w3.org/1999/xhtml";

        var pages = Pages();

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

        // the page of each published source, when its newest version was saved
        // being when it last changed
        var template = ReadPages().GetValueOrDefault(SourceTemplate);

        var written = template != null ? SiteLanguages.All.Where(template.Text.ContainsKey).ToList() : [.. SiteLanguages.All];

        var published = (sources ?? []).SelectMany(source => written.Select(language =>
        {
            var path = $"/source/{source.PublicKey}";

            return new XElement(ns + "url",
                new XElement(ns + "loc", PublicUrl + SiteLanguages.In(language, path)),
                source.Updated is { } updated ? new XElement(ns + "lastmod", updated.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)) : null,
                written.SelectMany(other => SiteLanguages.HreflangsOf(other).Select(hreflang => Alternate(hreflang, SiteLanguages.In(other, path)))),
                Alternate("x-default", path));
        }));

        var sitemap = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "urlset", new XAttribute(XNamespace.Xmlns + "xhtml", xhtml.NamespaceName), urls, published)
        );

        return sitemap.Declaration + "\n" + sitemap;
    }

    /// <summary>
    /// The site as a language model reads it (<c>llms.txt</c>): what it is,
    /// and a link to each public page and to what an agent works with here.
    /// </summary>
    /// <remarks>
    /// In English, the language models read best. Without a public address
    /// the links are relative, which is less than a model wants but still
    /// points somewhere.
    /// </remarks>
    public string LlmsText()
    {
        var root = PublicUrl ?? string.Empty;

        var pages = Pages();

        var summary = pages.TryGetValue("/", out var front) && front.Text.TryGetValue("en", out var home)
                          ? home.Description
                          : "Describe an app to an AI agent and get it online with a link to share.";

        var text = new StringBuilder();

        text.Append($"# {Site}\n\n")
            .Append($"> {summary}\n\n")
            .Append("An app is a snippet of C# whose GenHTTP handler is served at a public HTTPS address. ")
            .Append("An agent creates, changes and deploys it through the MCP server or the REST API below.\n\n")
            .Append("## Pages\n\n");

        foreach (var (path, entry) in pages)
        {
            if (entry.Text.TryGetValue("en", out var page))
            {
                text.Append($"- [{page.Title}]({root}{SiteLanguages.In("en", path)}): {page.Description}\n");
            }
        }

        text.Append("\n## For agents\n\n")
            .Append($"- [MCP server]({root}/mcp): Streamable HTTP; start with the platform_guide tool\n")
            .Append($"- [REST API]({root}/api/v1/openapi.json): OpenAPI description of the same functionality\n");

        return text.ToString();
    }

    /// <summary>
    /// What an agent can use here, for agents and registries that look for it
    /// (<c>/.well-known/ai-catalog.json</c>, Agentic Resource Discovery) - or
    /// nothing, when there is no public address to name it under.
    /// </summary>
    public string? AiCatalog()
    {
        if (PublicUrl == null)
        {
            return null;
        }

        var host = new Uri(PublicUrl).Host;

        var catalog = new
        {
            specVersion = "1.0",
            host = new { displayName = Site, documentationUrl = $"{PublicUrl}/en/docs" },
            entries = new object[]
            {
                new
                {
                    identifier = $"urn:air:{host}:server:lambda",
                    displayName = Site,
                    type = "application/mcp-server-card+json",
                    description = "Write, deploy and host small C# web services and sites at a public address.",
                    representativeQueries = new[]
                    {
                        "build a poll and give me a link to share it",
                        "host a small web app an AI agent wrote",
                        "deploy a C# web service to a public HTTPS address"
                    },
                    data = new
                    {
                        name = "dev.genhttp/lambda",
                        title = Site,
                        description = "Write, deploy and host small C# web services and sites at a public address.",
                        websiteUrl = PublicUrl,
                        remotes = new[] { new { type = "streamable-http", url = $"{PublicUrl}/mcp" } }
                    }
                },
                new
                {
                    identifier = $"urn:air:{host}:api:lambda",
                    displayName = $"{Site} REST API",
                    type = "application/vnd.oai.openapi+json",
                    description = "The same functionality as the MCP server over HTTP; versions can be downloaded and uploaded as a zip, and a feature being worked on downloaded and put back as one.",
                    url = $"{PublicUrl}/api/v1/openapi.json"
                }
            }
        };

        return JsonSerializer.Serialize(catalog, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// The pages of the build, without the templates for the pages it cannot
    /// know - whose addresses have a placeholder in them.
    /// </summary>
    private IReadOnlyDictionary<string, SiteEntry> Pages()
        => ReadPages().Where(p => !p.Key.Contains(':')).ToDictionary(p => p.Key, p => p.Value);

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
    /// What a picture is, by its name: the previews in English are PNG, the
    /// ones in the other languages JPEG, which is a fifth of the size.
    /// </summary>
    private static string ImageType(string path)
        => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
               ? "image/jpeg"
               : "image/png";

    /// <summary>
    /// The type of a picture that is not one of the site's own, as it was
    /// sniffed when it was uploaded - or what its name says.
    /// </summary>
    private static string ImageTypeOf(SitePage page, string picture) => page.ImageType ?? ImageType(picture);

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
/// <param name="ImageType">What the picture is, where its name does not say - an owner's picture on the showcase</param>
public sealed record SitePage(string Language, string Path, string Title, string Description, IReadOnlyList<string> Languages, string? Image = null,
                              string? ImageType = null);

/// <summary>
/// A page as <c>pages.json</c> has it: its picture, and its words by language.
/// </summary>
public sealed record SiteEntry(string? Image, Dictionary<string, SiteText> Text);

/// <summary>
/// The name and description of a page in one language, and the picture of
/// its link preview in that language, if it has one drawn in it.
/// </summary>
public sealed record SiteText(string Title, string Description, string? Image = null);
