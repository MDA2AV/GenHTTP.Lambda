namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// What visits are counted by, besides the day: a table each in the analytics
/// database.
/// </summary>
public enum AnalyticsDimension
{
    /// <summary>The path that was answered, relative to the lambda</summary>
    Pages,

    /// <summary>The host of the page a visitor came from, where it was another site</summary>
    Referrers,

    /// <summary>The country the visitor's address was allocated to, or empty where it is not known</summary>
    Countries,

    /// <summary>desktop, mobile, tablet, bot or other</summary>
    Devices,

    /// <summary>The browser of a person, or the app it was opened in - never of a bot</summary>
    Browsers,

    /// <summary>The operating system of a person, never of a bot</summary>
    Systems,

    /// <summary>
    /// A bot: whose it is where its operator confirms it by its address, "unconfirmed" where it only claimed to be one
    /// of those, and the kind of crawler the list says otherwise
    /// </summary>
    Crawlers
}
