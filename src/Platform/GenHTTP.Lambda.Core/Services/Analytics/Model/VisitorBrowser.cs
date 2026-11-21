namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// The browser of a person, or the app it was opened in, as the analytics
/// database keeps it.
/// </summary>
/// <remarks>
/// Stored as its number, as <see cref="VisitorDevice"/> is.
/// </remarks>
public enum VisitorBrowser
{

    /// <summary>A person in a browser none of the others is.</summary>
    Other = 0,

    Chrome = 1,

    Safari = 2,

    Firefox = 3,

    Edge = 4,

    Opera = 5,

    Brave = 6,

    SamsungInternet = 7,

    YandexBrowser = 8,

    Vivaldi = 9,

    UcBrowser = 10,

    DuckDuckGo = 11,

    Chromium = 12,

    Instagram = 13,

    Facebook = 14,

    TikTok = 15,

    WeChat = 16,

    Line = 17,

    Snapchat = 18,

    Pinterest = 19,

    LinkedIn = 20

}
