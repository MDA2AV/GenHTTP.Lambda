namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// The orders the listing comes in.
/// </summary>
public enum LambdaOrder
{

    /// <summary>The newest first.</summary>
    Newest,

    /// <summary>The oldest first.</summary>
    Oldest,

    /// <summary>The one changed most recently first.</summary>
    Changed,

    /// <summary>The one visited most recently first, as the sweep last wrote it down; never visited last.</summary>
    Visited,

    /// <summary>By public key, alphabetically.</summary>
    Key

}
