using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Workspace;

namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// What a lambda may have and do in its tier, and what holds for every
/// lambda alike, as the operator set it in the administration panel.
/// </summary>
public interface ILimitsService
{

    /// <summary>
    /// The limits as they stand.
    /// </summary>
    ProductLimits Get();

    /// <summary>
    /// What a lambda in the given tier may have and how it is treated.
    /// </summary>
    TierPolicy PolicyOf(LambdaTier tier);

    /// <summary>
    /// The tiers whose policy says so - for a query that asks the database
    /// about the lambdas of those tiers.
    /// </summary>
    IReadOnlyList<LambdaTier> TiersWhere(Func<TierPolicy, bool> predicate);

    /// <summary>
    /// The tier an app in the given one would be moved to for more - what a
    /// refusal names, and what the editor offers - or nothing where there is
    /// none: in production, or in one of the installation's own tiers.
    /// </summary>
    /// <remarks>
    /// Never one of the installation's own tiers: an owner is not told that a
    /// system lambda or a demo would have more room.
    /// </remarks>
    TierPolicy? Above(LambdaTier tier);

    /// <summary>
    /// What a lambda in the given tier may keep in its workspace: the room of
    /// its data, which its database takes some of.
    /// </summary>
    /// <param name="enabled">Whether its owner left the workspace switched on</param>
    WorkspaceLimits WorkspaceOf(LambdaTier tier, bool enabled = true);

    /// <summary>
    /// Replaces the limits, every one of them.
    /// </summary>
    /// <returns>The limits that were saved, and those they replaced</returns>
    (ProductLimits Saved, ProductLimits Previous) Save(ProductLimits limits);

}
