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
    Premium,

    /// <summary>
    /// One of the installation's own demos: kept online, and read only for
    /// everybody holding its editor key - which is announced, so that anybody
    /// can read the code and the logs of something that already works.
    /// </summary>
    /// <remarks>
    /// Never assigned by hand. The demo seeder puts a lambda here and is the
    /// only thing that changes what it runs.
    /// </remarks>
    Demo

}
