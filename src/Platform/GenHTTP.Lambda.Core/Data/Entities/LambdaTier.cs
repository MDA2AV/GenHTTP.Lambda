namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What a lambda is for, which decides what it may have and how it is kept.
/// </summary>
/// <remarks>
/// Named by purpose rather than by price: who pays is the installation's
/// business, how much a lambda matters is the lambda's. What each tier means
/// is said once, in <c>TierPolicies</c>; nothing else compares a tier with one
/// of these values. Assigned by the operator, never chosen by the owner. The
/// name is what is stored, so a tier can be added anywhere in the list without
/// moving the ones already written.
/// </remarks>
public enum LambdaTier
{

    /// <summary>
    /// For trying something: taken offline when nobody uses it, and removed
    /// when abandoned. Where a new lambda starts, unless the installation says
    /// otherwise.
    /// </summary>
    Sandbox,

    /// <summary>
    /// An app people use: kept whether or not anybody visited it lately, with
    /// more room and a domain of its own.
    /// </summary>
    Production,

    /// <summary>
    /// Something the platform itself depends on - the collector of usage
    /// reports, say: kept, never blocked, never in the showcase, and not
    /// deleted by whoever holds its editor key.
    /// </summary>
    /// <remarks>
    /// The intended hook for running such a lambda apart from the others or
    /// ahead of them, which nothing does yet.
    /// </remarks>
    System,

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
