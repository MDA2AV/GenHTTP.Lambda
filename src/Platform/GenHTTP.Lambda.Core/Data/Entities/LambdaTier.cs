namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What a lambda is for, which decides what it may have and how it is kept.
/// </summary>
/// <remarks>
/// Named by purpose rather than by price: who pays is the installation's
/// business, how much a lambda matters is the lambda's. What each tier means
/// is said once, in <c>TierPolicies</c>; nothing else compares a tier with one
/// of these values. Assigned by the operator, never chosen by the owner.
/// Stored as its number, so a tier keeps its number for good and a new one
/// takes the next; what the API, MCP and the pages call it is
/// <see cref="LambdaTierNames"/>. A number no member has reads as the sandbox,
/// the tier that allows the least.
/// </remarks>
public enum LambdaTier
{

    /// <summary>
    /// For trying something: taken offline when nobody uses it, and removed
    /// when abandoned. Where a new lambda starts, unless the installation says
    /// otherwise.
    /// </summary>
    Sandbox = 0,

    /// <summary>
    /// An app people use: kept whether or not anybody visited it lately, with
    /// more room and a domain of its own.
    /// </summary>
    Production = 1,

    /// <summary>
    /// Something the platform itself depends on - the collector of usage
    /// reports, say: kept, never blocked, never in the showcase, and not
    /// deleted by whoever holds its editor key.
    /// </summary>
    /// <remarks>
    /// The intended hook for running such a lambda apart from the others or
    /// ahead of them, which nothing does yet.
    /// </remarks>
    System = 2,

    /// <summary>
    /// One of the installation's own demos: kept online, and read only for
    /// everybody holding its editor key - which is announced, so that anybody
    /// can read the code and the logs of something that already works.
    /// </summary>
    /// <remarks>
    /// Never assigned by hand. The demo seeder puts a lambda here and is the
    /// only thing that changes what it runs.
    /// </remarks>
    Demo = 3

}
