using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Services.Visits;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// Whether a visitor was a person or a program, and what they came with.
/// </summary>
/// <remarks>
/// In this order, each step only where the one before said nothing:
///
/// 1. A crawler that claims to be one of the operators that publish their
///    addresses is counted as theirs where it came from one of them, and as
///    an unconfirmed claim otherwise (<see cref="CrawlerRanges"/>).
/// 2. A crawler the list of crawlers knows is counted by its kind
///    (<see cref="CrawlerList"/>).
/// 3. Anything that does not name itself as a browser does - no agent, an
///    app's own client - is neither: "other", and no visitor.
/// 4. A person, with the browser, system and device the client hints say
///    where a Chromium browser sent them, and its agent's product tokens
///    otherwise.
///
/// Everything a visitor says about themselves can be made up, so this is what
/// they claim, coarsely, and never more: an owner learns that most of their
/// visitors come on phones, not who anybody is. Values that come from the
/// sender are only ever taken where they are one of a known few, so nobody can
/// write a word of their own into an owner's figures.
///
/// Worked out when visits are written down, never while a request is served,
/// and remembered per agent: a busy day is a few hundred different agents
/// sent a hundred thousand times.
/// </remarks>
public sealed class VisitorClassifier(CrawlerList crawlers, CrawlerRanges ranges)
{

    public const string Desktop = "desktop";

    public const string Mobile = "mobile";

    public const string Tablet = "tablet";

    public const string Bot = "bot";

    public const string Unknown = "other";

    /// <summary>
    /// How many agents are remembered. Past that, it starts over.
    /// </summary>
    private const int MostRemembered = 20_000;

    /// <summary>
    /// How much of an agent is read. The request line keeps 200 characters of
    /// it already; this holds for anything else that calls.
    /// </summary>
    private const int MostRead = 500;

    private readonly ConcurrentDictionary<(string Agent, ClientHints? Hints), Reading> _known = [];

    private int _generation = -1;

    #region Functionality

    /// <summary>
    /// What a visitor was, from what they sent.
    /// </summary>
    /// <param name="agent">Their User-Agent</param>
    /// <param name="hints">The client hints of a Chromium browser, where it sent them</param>
    /// <param name="client">Who asked, as the log names them - what a crawler's claim is checked against</param>
    public VisitorAgent Classify(string? agent, ClientHints? hints = null, string? client = null)
    {
        if (string.IsNullOrWhiteSpace(agent))
        {
            return new VisitorAgent(Unknown, null, null, null);
        }

        // a newer list of crawlers makes what was read with the one before old
        if (Interlocked.Exchange(ref _generation, crawlers.Generation) != crawlers.Generation || _known.Count >= MostRemembered)
        {
            _known.Clear();
        }

        // cut before it is remembered, so what is kept of an agent is bounded
        // however long it was sent
        var read = agent.Length <= MostRead ? agent : agent[..MostRead];

        var reading = _known.GetOrAdd((read, hints), key => Read(key.Agent, key.Hints));

        // the claim is remembered, its check is not: it is the address that
        // decides, and every visit has its own
        if (reading.Claimed is { } claimed)
        {
            return new VisitorAgent(Bot, null, null, ranges.Confirm(claimed, client));
        }

        return reading.Agent!;
    }

    private Reading Read(string agent, ClientHints? hints)
    {
        if (CrawlerRanges.Claimed(agent) is { } claimed)
        {
            return new Reading(null, claimed);
        }

        if (crawlers.KindOf(agent) is { } kind)
        {
            return new Reading(new VisitorAgent(Bot, null, null, kind), null);
        }

        // a browser names itself Mozilla, for reasons older than most of them
        if (!agent.StartsWith("Mozilla/", StringComparison.Ordinal) && !agent.StartsWith("Opera/", StringComparison.Ordinal))
        {
            return new Reading(new VisitorAgent(Unknown, null, null, null), null);
        }

        // the hints of Chromium, only where the agent is Chromium's too: what
        // another browser sends as hints is not what it is
        var chromium = Has(agent, "Chrome/") || Has(agent, "Chromium/");

        var trusted = chromium ? hints : null;

        var system = SystemOfHints(trusted?.Platform) ?? SystemOfAgent(agent);

        return new Reading(new VisitorAgent(DeviceOf(agent, trusted, system), BrandOf(trusted?.Brands) ?? BrowserOf(agent), system, null), null);
    }

    #endregion

    #region Browsers

    /// <summary>
    /// Apps that open pages in a browser of their own, which their agents
    /// name: a visitor from Instagram is worth telling from one in Safari.
    /// </summary>
    private static readonly (string Token, string Name)[] Apps =
    [
        ("Instagram ", "Instagram"), ("FBAN/", "Facebook"), ("FBAV/", "Facebook"), ("FB_IAB/", "Facebook"),
        ("musical_ly", "TikTok"), ("BytedanceWebview", "TikTok"), ("MicroMessenger/", "WeChat"), (" Line/", "LINE"),
        ("Snapchat", "Snapchat"), ("Pinterest/", "Pinterest"), ("LinkedInApp", "LinkedIn")
    ];

    /// <summary>
    /// Browsers by the token they add to the agent, the specific before the
    /// general: Edge, Opera and Samsung Internet name Chrome as well, and
    /// Chrome names Safari.
    /// </summary>
    private static readonly (string Token, string Name)[] Browsers =
    [
        ("Edg/", "Edge"), ("EdgA/", "Edge"), ("EdgiOS/", "Edge"),
        ("OPR/", "Opera"), ("OPT/", "Opera"), ("Opera", "Opera"),
        ("SamsungBrowser/", "Samsung Internet"),
        ("YaBrowser/", "Yandex Browser"),
        ("Vivaldi/", "Vivaldi"),
        ("UCBrowser/", "UC Browser"),
        ("DuckDuckGo/", "DuckDuckGo"),
        ("Firefox/", "Firefox"), ("FxiOS/", "Firefox"),
        ("CriOS/", "Chrome"), ("Chrome/", "Chrome"), ("Chromium/", "Chromium")
    ];

    private static string BrowserOf(string agent)
    {
        foreach (var (token, name) in Apps)
        {
            if (Has(agent, token))
            {
                return name;
            }
        }

        foreach (var (token, name) in Browsers)
        {
            if (Has(agent, token))
            {
                return name;
            }
        }

        // Safari says which version it is; a page shown inside an app on an
        // iPhone says Safari, but not that
        return Has(agent, "Safari/") && Has(agent, "Version/") ? "Safari" : Unknown;
    }

    /// <summary>
    /// The brands a Chromium browser names in its hints, by the names the
    /// agent would give them.
    /// </summary>
    private static readonly (string Brand, string Name)[] Brands =
    [
        ("Google Chrome", "Chrome"), ("Microsoft Edge", "Edge"), ("Opera", "Opera"), ("Opera GX", "Opera"), ("Brave", "Brave"),
        ("Samsung Internet", "Samsung Internet"), ("YaBrowser", "Yandex Browser"), ("Yandex", "Yandex Browser"), ("Vivaldi", "Vivaldi")
    ];

    /// <summary>
    /// The quoted brands of <c>Sec-CH-UA</c>: <c>"Google Chrome";v="141", "Not?A_Brand";v="8", "Chromium";v="141"</c>.
    /// </summary>
    private static readonly Regex Quoted = new("\"([^\"]{1,40})\"", RegexOptions.NonBacktracking | RegexOptions.CultureInvariant);

    /// <summary>
    /// The browser the hints name - not Chromium, which every one of them is,
    /// and not the made-up brand they add so nobody relies on the order
    /// (GREASE, "Not?A_Brand") - or nothing where it is none of the known.
    /// </summary>
    private static string? BrandOf(string? brands)
    {
        if (brands == null)
        {
            return null;
        }

        foreach (Match match in Quoted.Matches(brands))
        {
            var brand = match.Groups[1].Value;

            foreach (var (known, name) in Brands)
            {
                if (string.Equals(brand, known, StringComparison.OrdinalIgnoreCase))
                {
                    return name;
                }
            }
        }

        return null;
    }

    #endregion

    #region Systems and devices

    private static string? SystemOfHints(string? platform) => platform?.Trim('"', ' ') switch
    {
        null => null,
        "Windows" => "Windows",
        "macOS" => "macOS",
        "Android" => "Android",
        "iOS" => "iOS",
        "Chrome OS" or "Chromium OS" => "ChromeOS",
        "Linux" => "Linux",
        _ => null
    };

    private static string SystemOfAgent(string agent)
    {
        if (Has(agent, "Windows"))
        {
            return "Windows";
        }

        if (Has(agent, "iPhone") || Has(agent, "iPad") || Has(agent, "iPod"))
        {
            return "iOS";
        }

        // before Linux, which every Android agent names as well
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

        return Has(agent, "Linux") || Has(agent, "X11") ? "Linux" : Unknown;
    }

    /// <summary>
    /// A phone, a tablet or a computer. The hints say whether it is a phone;
    /// an Android that is not one is a tablet, as Android says in its agent by
    /// leaving out "Mobile". An iPad that asks for the pages of a computer
    /// says Macintosh and is counted as one - there is nothing else to tell
    /// it by.
    /// </summary>
    private static string DeviceOf(string agent, ClientHints? hints, string system)
    {
        switch (hints?.Mobile)
        {
            case "?1":
                return Mobile;
            case "?0":
                return system == "Android" ? Tablet : Desktop;
        }

        if (Has(agent, "iPad") || Has(agent, "Tablet") || (Has(agent, "Android") && !Has(agent, "Mobile")))
        {
            return Tablet;
        }

        // "Mobi" is what Mozilla recommends looking for; the rest are the
        // phones that do not say it
        return Has(agent, "Mobi") || Has(agent, "iPhone") || Has(agent, "iPod") ? Mobile : Desktop;
    }

    private static bool Has(string agent, string token) => agent.Contains(token, StringComparison.Ordinal);

    /// <summary>
    /// What an agent was read as, apart from the address: a claim to be one of
    /// the operators' crawlers is checked per visit.
    /// </summary>
    private sealed record Reading(VisitorAgent? Agent, CrawlerOperator? Claimed);

    #endregion

}

/// <summary>
/// What a visitor used.
/// </summary>
/// <param name="Device">desktop, mobile, tablet, bot or other</param>
/// <param name="Browser">The browser of a person, or nothing for anything else</param>
/// <param name="System">The operating system of a person, or nothing for anything else</param>
/// <param name="Crawler">
/// Whose crawler it is - an operator that was confirmed, or "unconfirmed" - or what kind it is, for a bot; nothing for
/// a person
/// </param>
public sealed record VisitorAgent(string Device, string? Browser, string? System, string? Crawler);
