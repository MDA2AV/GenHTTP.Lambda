using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// Everything that depends on the tier of a lambda: what it may have, how it
/// is kept, and who decides about it.
/// </summary>
/// <remarks>
/// Every check that depends on a tier reads this rather than comparing the
/// tier with a value, so what a tier means is said once, in
/// <see cref="TierPolicies"/>, and an installation that sets its tiers
/// otherwise is followed everywhere alike.
/// </remarks>
/// <param name="Tier">The tier it is the policy of</param>
/// <param name="Label">What the installation calls the tier on its pages, or nothing for its own name</param>
/// <param name="MaxVersionBytes">How large a version may be: its code and its resources together</param>
/// <param name="MaxDataBytes">The room its data may take: its database and its workspace together</param>
/// <param name="AnalyticsDays">How many days of its analytics its owner is shown</param>
/// <param name="Connectivity">Whether it may connect to other servers unless the operator decided otherwise about it</param>
/// <param name="OwnDomain">Whether it may answer at a domain of its own</param>
/// <param name="ExpiresWhenUnused">Whether it is taken offline, and later removed, once nobody visits or edits it</param>
/// <param name="Installation">
/// Whether it is the installation's own rather than an app of the people who use it: never blocked, never in the
/// showcase, never deleted by whoever holds its editor key, and never offered as the tier that allows more
/// </param>
/// <param name="ReadOnly">Whether only the installation changes it, whoever holds its editor key - a demo</param>
/// <param name="WarmedUp">
/// Whether what it has online is compiled as the server starts, so that its first visitor after a restart does not
/// wait for it - rather than on its first request
/// </param>
public sealed record TierPolicy(LambdaTier Tier, string? Label, long MaxVersionBytes, long MaxDataBytes, int AnalyticsDays,
                                bool Connectivity, bool OwnDomain, bool ExpiresWhenUnused, bool Installation, bool ReadOnly,
                                bool WarmedUp)
{

    /// <summary>
    /// What the tier is called where a person reads it in English: its label, or its name.
    /// </summary>
    public string Name => Label ?? LambdaTierNames.Name(Tier);

    /// <summary>
    /// Whether the operator may block a lambda in this tier.
    /// </summary>
    public bool Blockable => !Installation;

    /// <summary>
    /// Whether a lambda in this tier may be listed in the showcase.
    /// </summary>
    public bool Showcased => !Installation;

    /// <summary>
    /// Whether whoever holds the editor key may delete a lambda in this tier.
    /// </summary>
    public bool DeletedByOwner => !Installation;

    /// <summary>
    /// Whether the operator may move a lambda into this tier or out of it.
    /// </summary>
    public bool Assignable => !ReadOnly;

}
