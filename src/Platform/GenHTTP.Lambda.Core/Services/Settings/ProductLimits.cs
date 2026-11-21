using System.Globalization;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Analytics;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// Every limit of the product.
/// </summary>
/// <param name="DefaultTier">The tier a new lambda starts in: the sandbox or production</param>
/// <param name="Sandbox">What a sandbox lambda may have, which a demo is held to as well</param>
/// <param name="Production">What a lambda in production may have; never less than a sandbox one</param>
/// <param name="System">What one of the installation's system lambdas may have</param>
/// <param name="Versions">How many versions of a lambda are kept, whatever its tier</param>
/// <param name="Features">How many features a lambda may have open at once, whatever its tier</param>
/// <param name="OfflineAfter">How long a lambda in a tier that is not kept while unused may go unused before it is taken offline</param>
/// <param name="RemovedAfter">How long such an unused lambda is kept before it is removed</param>
/// <param name="ShowcaseImageBytes">How large the picture of a showcase entry may be</param>
/// <param name="RequestsPerSecond">How many requests one client may send to the lambdas in a second</param>
/// <param name="BuildsPerDay">How many builds and changes one address may ask the build agent for in a day</param>
/// <remarks>
/// The number of versions and of features were the same in every tier, so
/// they hold for every lambda alike. The two durations are those of whichever
/// tier is not kept while unused - the sandbox. The last three are counted per
/// caller rather than per lambda, so they have no tier either.
/// </remarks>
public sealed record ProductLimits(
    LambdaTier DefaultTier,
    TierSettings Sandbox,
    TierSettings Production,
    TierSettings System,
    int Versions,
    int Features,
    TimeSpan OfflineAfter,
    TimeSpan RemovedAfter,
    int ShowcaseImageBytes,
    int RequestsPerSecond,
    int BuildsPerDay
);
