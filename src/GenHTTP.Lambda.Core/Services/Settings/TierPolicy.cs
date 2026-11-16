using GenHTTP.Lambda.Configuration;
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
public sealed record TierPolicy(LambdaTier Tier, string? Label, long MaxVersionBytes, long MaxDataBytes, int AnalyticsDays,
                                bool Connectivity, bool OwnDomain, bool ExpiresWhenUnused, bool Installation, bool ReadOnly)
{

    /// <summary>
    /// What the tier is called where a person reads it in English: its label, or its name.
    /// </summary>
    public string Name => Label ?? Tier.ToString();

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

/// <summary>
/// What each tier means: the one place where a tier is compared with a value.
/// </summary>
/// <remarks>
/// Named by purpose. A sandbox is for trying and goes when nobody uses it;
/// production is for an app people use and is kept, with a domain of its own;
/// a system lambda is something the platform depends on, kept and never
/// blocked; a demo is the installation's own and read only. How much each may
/// have and whether it connects by default is the installation's to set
/// (<see cref="TierSettings"/>); what is fixed here is the purpose, which a
/// setting would contradict - a sandbox that is kept for ever is a lambda in
/// production.
///
/// System is also where running a lambda apart from the others, or ahead of
/// them, would hang - which nothing does yet: they share one process.
/// </remarks>
public static class TierPolicies
{

    /// <summary>
    /// Every tier, from the least to the most that it allows.
    /// </summary>
    public static IReadOnlyList<LambdaTier> All { get; } = Enum.GetValues<LambdaTier>();

    /// <summary>
    /// What a lambda in the given tier may have and how it is treated, with
    /// the limits as they stand.
    /// </summary>
    public static TierPolicy Of(LambdaTier tier, ProductLimits limits)
    {
        var sandbox = limits.Sandbox;

        return tier switch
        {
            LambdaTier.Sandbox => Make(tier, sandbox, ownDomain: false, expires: true, installation: false),

            // never less than a sandbox, however the two were configured:
            // the panel refuses that, a host's options do not
            LambdaTier.Production => Make(tier, limits.Production with
            {
                BuildBytes = Math.Max(limits.Production.BuildBytes, sandbox.BuildBytes),
                DataBytes = Math.Max(limits.Production.DataBytes, sandbox.DataBytes),
                AnalyticsDays = Math.Max(limits.Production.AnalyticsDays, sandbox.AnalyticsDays)
            }, ownDomain: true, expires: false, installation: false),

            LambdaTier.System => Make(tier, limits.System, ownDomain: true, expires: false, installation: true),

            // held to what a sandbox may have, and never connecting: a demo
            // teaches what anybody's lambda can do
            LambdaTier.Demo => Make(tier, sandbox with { Connectivity = false, Label = null }, ownDomain: false, expires: false, installation: true, readOnly: true),

            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "This tier has no policy.")
        };
    }

    private static TierPolicy Make(LambdaTier tier, TierSettings settings, bool ownDomain, bool expires, bool installation, bool readOnly = false)
        => new(tier, settings.Label, settings.BuildBytes, settings.DataBytes, settings.AnalyticsDays, settings.Connectivity,
               ownDomain, expires, installation, readOnly);

}
