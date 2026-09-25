namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// The service tier a lambda belongs to.
/// </summary>
/// <remarks>
/// Assigned by an administrator, never chosen by the owner. The name is what
/// is stored, so a tier can be added anywhere in the list without moving the
/// ones already written.
/// </remarks>
public enum LambdaTier
{

    /// <summary>
    /// Anybody's lambda: reachable below <c>/lambda/{key}/</c>, taken offline
    /// when unused and removed when abandoned.
    /// </summary>
    Free,

    /// <summary>
    /// May answer at a domain of its own, and is kept whether or not anybody
    /// has visited it lately.
    /// </summary>
    Premium

}
