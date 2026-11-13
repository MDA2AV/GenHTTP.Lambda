using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Workspace;

namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// What a lambda may have and do in its tier, as the operator set it in the
/// administration panel.
/// </summary>
public interface ILimitsService
{

    /// <summary>
    /// The limits as they stand.
    /// </summary>
    ProductLimits Get();

    /// <summary>
    /// What a lambda in the given tier may have. A demo is held to what a free
    /// lambda may have.
    /// </summary>
    TierLimits Of(LambdaTier tier);

    /// <summary>
    /// How large a version of a lambda in the given tier may be, its code and
    /// its resources together - never less for a premium lambda than for any
    /// other.
    /// </summary>
    long BuildOf(LambdaTier tier);

    /// <summary>
    /// How much room the data of a lambda in the given tier may take, its
    /// database and its workspace together.
    /// </summary>
    long DataOf(LambdaTier tier);

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
