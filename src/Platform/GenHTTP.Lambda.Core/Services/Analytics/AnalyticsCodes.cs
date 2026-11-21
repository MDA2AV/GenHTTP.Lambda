namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// How the values of a dimension are kept: the open ones - pages,
/// referrers, countries, crawlers - as the text they are, the few the
/// classifier knows - devices, browsers, systems - as a small number each.
/// </summary>
/// <remarks>
/// A row per lambda, day and value repeated the same few words in every
/// table of every lambda; a number is what the database stores in a byte or
/// two. What is counted and handed out keeps the names - the classifier
/// works in them, and so does every answer - so they are spelled out here,
/// once, and never by <c>ToString()</c>: what is stored must not follow a
/// rename in code. A name nobody knows is kept as other, and so is a number
/// no member has when it is read.
/// </remarks>
public static class AnalyticsCodes
{

    /// <summary>
    /// The name of the values nobody knows, in every dimension kept as a number.
    /// </summary>
    public const string Other = "other";

    private static readonly (VisitorDevice Code, string Name)[] Devices =
    [
        (VisitorDevice.Desktop, "desktop"), (VisitorDevice.Mobile, "mobile"), (VisitorDevice.Tablet, "tablet"), (VisitorDevice.Bot, "bot")
    ];

    private static readonly (VisitorSystem Code, string Name)[] Systems =
    [
        (VisitorSystem.Windows, "Windows"), (VisitorSystem.MacOs, "macOS"), (VisitorSystem.Linux, "Linux"), (VisitorSystem.Android, "Android"),
        (VisitorSystem.Ios, "iOS"), (VisitorSystem.ChromeOs, "ChromeOS")
    ];

    private static readonly (VisitorBrowser Code, string Name)[] Browsers =
    [
        (VisitorBrowser.Chrome, "Chrome"), (VisitorBrowser.Safari, "Safari"), (VisitorBrowser.Firefox, "Firefox"), (VisitorBrowser.Edge, "Edge"),
        (VisitorBrowser.Opera, "Opera"), (VisitorBrowser.Brave, "Brave"), (VisitorBrowser.SamsungInternet, "Samsung Internet"),
        (VisitorBrowser.YandexBrowser, "Yandex Browser"), (VisitorBrowser.Vivaldi, "Vivaldi"), (VisitorBrowser.UcBrowser, "UC Browser"),
        (VisitorBrowser.DuckDuckGo, "DuckDuckGo"), (VisitorBrowser.Chromium, "Chromium"), (VisitorBrowser.Instagram, "Instagram"),
        (VisitorBrowser.Facebook, "Facebook"), (VisitorBrowser.TikTok, "TikTok"), (VisitorBrowser.WeChat, "WeChat"), (VisitorBrowser.Line, "LINE"),
        (VisitorBrowser.Snapchat, "Snapchat"), (VisitorBrowser.Pinterest, "Pinterest"), (VisitorBrowser.LinkedIn, "LinkedIn")
    ];

    /// <summary>
    /// Whether a dimension keeps its values as numbers.
    /// </summary>
    public static bool IsNumbered(AnalyticsDimension dimension)
        => dimension is AnalyticsDimension.Devices or AnalyticsDimension.Browsers or AnalyticsDimension.Systems;

    /// <summary>
    /// The number a value of a numbered dimension is kept as.
    /// </summary>
    public static short Code(AnalyticsDimension dimension, string name) => dimension switch
    {
        AnalyticsDimension.Devices => (short)Find(Devices, name, VisitorDevice.Other),
        AnalyticsDimension.Browsers => (short)Find(Browsers, name, VisitorBrowser.Other),
        AnalyticsDimension.Systems => (short)Find(Systems, name, VisitorSystem.Other),
        _ => throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "This dimension keeps its values as text.")
    };

    /// <summary>
    /// The name of a number a numbered dimension keeps.
    /// </summary>
    public static string Name(AnalyticsDimension dimension, int code) => dimension switch
    {
        AnalyticsDimension.Devices => Named(Devices, (VisitorDevice)code),
        AnalyticsDimension.Browsers => Named(Browsers, (VisitorBrowser)code),
        AnalyticsDimension.Systems => Named(Systems, (VisitorSystem)code),
        _ => throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "This dimension keeps its values as text.")
    };

    private static T Find<T>((T Code, string Name)[] known, string name, T other) where T : struct, Enum
    {
        foreach (var (code, spelled) in known)
        {
            if (string.Equals(spelled, name, StringComparison.Ordinal))
            {
                return code;
            }
        }

        return other;
    }

    private static string Named<T>((T Code, string Name)[] known, T code) where T : struct, Enum
    {
        foreach (var (candidate, name) in known)
        {
            if (EqualityComparer<T>.Default.Equals(candidate, code))
            {
                return name;
            }
        }

        return Other;
    }

}
