namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// What a visitor came on, as the analytics database keeps it.
/// </summary>
/// <remarks>
/// Stored as its number, so a member keeps its number for good and a new one
/// takes the next; what an answer calls it is <see cref="AnalyticsCodes"/>.
/// A number no member has is counted as other.
/// </remarks>
public enum VisitorDevice
{

    /// <summary>Neither a person nor a crawler: anything that names no browser.</summary>
    Other = 0,

    Desktop = 1,

    Mobile = 2,

    Tablet = 3,

    Bot = 4

}
