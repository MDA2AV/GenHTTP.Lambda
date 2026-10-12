namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// What a visitor used, as far as the name its browser or program gave
/// itself says.
/// </summary>
/// <remarks>
/// Read from the User-Agent header alone, which is written by the sender and
/// can say anything - so this is the coarse answer an owner wants (people on
/// phones, people on computers, search engines) and not a fingerprint. The
/// rules are ordered: a bot is a bot whatever browser it claims to be, Edge
/// and Opera claim to be Chrome, and Chrome claims to be Safari.
///
/// Worked out when visits are written down, never while a request is served.
/// </remarks>
public static class VisitorAgents
{

    public const string Desktop = "desktop";

    public const string Mobile = "mobile";

    public const string Tablet = "tablet";

    public const string Bot = "bot";

    public const string Unknown = "other";

    /// <summary>
    /// Who runs the crawlers worth naming, by what their agents contain.
    /// </summary>
    /// <remarks>
    /// Search engines, the AIs that read pages to answer with them, and the
    /// chats and networks that fetch a page to show a preview of a link -
    /// what an owner of a page meant to be found wants to know reached it.
    /// </remarks>
    private static readonly (string Mark, string Name)[] Crawlers =
    [
        ("googlebot", "Google"), ("google-inspectiontool", "Google"), ("googleother", "Google"), ("adsbot-google", "Google"),
        ("mediapartners-google", "Google"), ("storebot-google", "Google"), ("feedfetcher-google", "Google"), ("google-extended", "Google"),
        ("bingbot", "Bing"), ("bingpreview", "Bing"), ("msnbot", "Bing"), ("adidxbot", "Bing"),
        ("applebot", "Apple"),
        ("duckduckbot", "DuckDuckGo"), ("duckassistbot", "DuckDuckGo"),
        ("yandex", "Yandex"),
        ("baiduspider", "Baidu"),
        ("gptbot", "OpenAI"), ("chatgpt-user", "OpenAI"), ("oai-searchbot", "OpenAI"),
        ("claudebot", "Anthropic"), ("claude-user", "Anthropic"), ("claude-searchbot", "Anthropic"), ("anthropic-ai", "Anthropic"),
        ("perplexitybot", "Perplexity"), ("perplexity-user", "Perplexity"),
        ("facebookexternalhit", "Meta"), ("meta-externalagent", "Meta"), ("facebookbot", "Meta"),
        ("twitterbot", "X"),
        ("linkedinbot", "LinkedIn"),
        ("slackbot", "Slack"),
        ("discordbot", "Discord"),
        ("whatsapp", "WhatsApp"),
        ("telegrambot", "Telegram"),
        ("amazonbot", "Amazon"),
        ("bytespider", "ByteDance"),
        ("ccbot", "Common Crawl"),
        ("ahrefsbot", "Ahrefs"),
        ("semrushbot", "Semrush"),
        ("petalbot", "Petal")
    ];

    /// <summary>
    /// What else gives a program away that is not a browser somebody sits at.
    /// </summary>
    private static readonly string[] Programs =
    [
        "bot", "crawl", "spider", "slurp", "preview", "monitor", "uptime", "fetcher", "scanner", "headless", "lighthouse",
        "curl/", "wget/", "python", "go-http-client", "java/", "okhttp", "axios", "node-fetch", "undici", "httpclient",
        "libwww", "scrapy", "postman", "insomnia", "http_request", "phantomjs", "iframely", "embedly", "-ai"
    ];

    /// <summary>
    /// What kind of device asked, and - for a person - with which browser on
    /// which system, or - for a bot - whose it is.
    /// </summary>
    public static VisitorAgent Classify(string? agent)
    {
        if (string.IsNullOrWhiteSpace(agent))
        {
            return new VisitorAgent(Unknown, null, null, null);
        }

        foreach (var (mark, name) in Crawlers)
        {
            if (agent.Contains(mark, StringComparison.OrdinalIgnoreCase))
            {
                return new VisitorAgent(Bot, null, null, name);
            }
        }

        foreach (var mark in Programs)
        {
            if (agent.Contains(mark, StringComparison.OrdinalIgnoreCase))
            {
                return new VisitorAgent(Bot, null, null, Unknown);
            }
        }

        // a browser names itself Mozilla, for reasons older than most of them
        if (!agent.StartsWith("Mozilla/", StringComparison.Ordinal) && !agent.StartsWith("Opera", StringComparison.Ordinal))
        {
            return new VisitorAgent(Unknown, null, null, null);
        }

        return new VisitorAgent(DeviceOf(agent), BrowserOf(agent), SystemOf(agent), null);
    }

    private static string DeviceOf(string agent)
    {
        if (Has(agent, "iPad") || Has(agent, "Tablet") || (Has(agent, "Android") && !Has(agent, "Mobile")))
        {
            return Tablet;
        }

        return Has(agent, "Mobi") || Has(agent, "iPhone") || Has(agent, "iPod") || Has(agent, "Android") ? Mobile : Desktop;
    }

    private static string BrowserOf(string agent)
    {
        if (Has(agent, "Edg/") || Has(agent, "EdgA/") || Has(agent, "EdgiOS/"))
        {
            return "Edge";
        }

        if (Has(agent, "OPR/") || Has(agent, "Opera"))
        {
            return "Opera";
        }

        if (Has(agent, "SamsungBrowser/"))
        {
            return "Samsung Internet";
        }

        if (Has(agent, "Firefox/") || Has(agent, "FxiOS/"))
        {
            return "Firefox";
        }

        if (Has(agent, "Chrome/") || Has(agent, "CriOS/") || Has(agent, "Chromium/"))
        {
            return "Chrome";
        }

        return Has(agent, "Safari/") ? "Safari" : Unknown;
    }

    private static string SystemOf(string agent)
    {
        if (Has(agent, "Windows"))
        {
            return "Windows";
        }

        if (Has(agent, "iPhone") || Has(agent, "iPad") || Has(agent, "iPod"))
        {
            return "iOS";
        }

        if (Has(agent, "Android"))
        {
            return "Android";
        }

        if (Has(agent, "CrOS"))
        {
            return "ChromeOS";
        }

        if (Has(agent, "Macintosh") || Has(agent, "Mac OS X"))
        {
            return "macOS";
        }

        return Has(agent, "Linux") ? "Linux" : Unknown;
    }

    private static bool Has(string agent, string mark) => agent.Contains(mark, StringComparison.Ordinal);

}

/// <summary>
/// What a visitor used.
/// </summary>
/// <param name="Device">desktop, mobile, tablet, bot or other</param>
/// <param name="Browser">The browser of a person, or nothing for a bot</param>
/// <param name="System">The operating system of a person, or nothing for a bot</param>
/// <param name="Crawler">Whose bot it is, or nothing for a person</param>
public sealed record VisitorAgent(string Device, string? Browser, string? System, string? Crawler);
