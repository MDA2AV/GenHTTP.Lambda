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
    /// How many characters of C# a lambda in the given tier may have - never
    /// less for a premium lambda than for any other.
    /// </summary>
    int MaxCodeLengthOf(LambdaTier tier);

    /// <summary>
    /// How many bytes of assets a lambda in the given tier may ship.
    /// </summary>
    int MaxAssetBytesOf(LambdaTier tier);

    /// <summary>
    /// What a lambda in the given tier may keep in its workspace.
    /// </summary>
    /// <param name="enabled">Whether its owner left the workspace switched on</param>
    WorkspaceLimits WorkspaceOf(LambdaTier tier, bool enabled = true);

    /// <summary>
    /// How large the database of a lambda in the given tier may grow.
    /// </summary>
    long DatabaseOf(LambdaTier tier);

    /// <summary>
    /// Replaces the limits, every one of them.
    /// </summary>
    /// <returns>The limits that were saved, and those they replaced</returns>
    (ProductLimits Saved, ProductLimits Previous) Save(ProductLimits limits);

}
