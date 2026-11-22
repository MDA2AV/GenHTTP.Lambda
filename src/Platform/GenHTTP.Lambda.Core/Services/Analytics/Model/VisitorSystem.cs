namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// The operating system of a person, as the analytics database keeps it.
/// </summary>
/// <remarks>
/// Stored as its number, as <see cref="VisitorDevice"/> is.
/// </remarks>
public enum VisitorSystem
{

    /// <summary>A person on a system none of the others is.</summary>
    Other = 0,

    Windows = 1,

    MacOs = 2,

    Linux = 3,

    Android = 4,

    Ios = 5,

    ChromeOs = 6

}
