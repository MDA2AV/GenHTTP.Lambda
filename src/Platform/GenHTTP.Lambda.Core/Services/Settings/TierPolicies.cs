using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Settings;

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
            LambdaTier.Sandbox => Make(tier, sandbox, ownDomain: false, expires: true, installation: false, warm: false),

            // never less than a sandbox, however the two were configured:
            // the panel refuses that, a host's options do not
            LambdaTier.Production => Make(tier, limits.Production with
            {
                BuildBytes = Math.Max(limits.Production.BuildBytes, sandbox.BuildBytes),
                DataBytes = Math.Max(limits.Production.DataBytes, sandbox.DataBytes),
                AnalyticsDays = Math.Max(limits.Production.AnalyticsDays, sandbox.AnalyticsDays)
            }, ownDomain: true, expires: false, installation: false, warm: true),

            LambdaTier.System => Make(tier, limits.System, ownDomain: true, expires: false, installation: true, warm: true),

            // held to what a sandbox may have, and never connecting: a demo
            // teaches what anybody's lambda can do
            LambdaTier.Demo => Make(tier, sandbox with { Connectivity = false, Label = null }, ownDomain: false, expires: false, installation: true, warm: false,
                                    readOnly: true),

            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "This tier has no policy.")
        };
    }

    private static TierPolicy Make(LambdaTier tier, TierSettings settings, bool ownDomain, bool expires, bool installation, bool warm,
                                   bool readOnly = false)
        => new(tier, settings.Label, settings.BuildBytes, settings.DataBytes, settings.AnalyticsDays, settings.Connectivity,
               ownDomain, expires, installation, readOnly, warm);

}
