namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// How the listing is ordered.
/// </summary>
public enum SourceOrder
{

    /// <summary>
    /// The most starred first, and among the same the most recently changed.
    /// </summary>
    Stars,

    /// <summary>
    /// The one whose newest version is the newest first.
    /// </summary>
    Updated,

    /// <summary>
    /// The one published last first.
    /// </summary>
    Published

}
