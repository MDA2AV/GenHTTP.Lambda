using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What a lambda may have and do, as the operator set it.")]
public sealed record LimitsModel(
    [Summary("The tier a new lambda starts in: `Sandbox` or `Production`")]
    string DefaultTier,
    [Summary("What a sandbox lambda, or a demo, may have")]
    TierLimitsModel Sandbox,
    [Summary("What a lambda in production may have; never less than a sandbox one")]
    TierLimitsModel Production,
    [Summary("What a system lambda, one the installation itself depends on, may have")]
    TierLimitsModel System,
    [Summary("How many versions of a lambda are kept, whatever its tier; older ones are removed, never the one online")]
    int Versions,
    [Summary("How many features a lambda may have open at once, whatever its tier")]
    int Features,
    [Summary("How long a sandbox lambda may go without a visit or an edit before it is taken offline, in hours")]
    int OfflineAfterHours,
    [Summary("How long a sandbox lambda may go without a visit or an edit before it is deleted, in hours")]
    int RemovedAfterHours,
    [Summary("How large the picture of a showcase entry may be, in bytes")]
    int ShowcaseImageBytes,
    [Summary("How many requests one client may send to the lambdas in a second")]
    int RequestsPerSecond,
    [Summary("How many builds and changes one address may ask the build agent for in a day")]
    int BuildsPerDay
);
