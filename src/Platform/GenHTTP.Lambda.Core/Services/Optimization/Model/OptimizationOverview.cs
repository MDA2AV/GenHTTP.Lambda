using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Optimization;

/// <summary>
/// Whether the platform keeps the databases in shape, and how that went lately.
/// </summary>
[Summary("Whether the platform keeps the databases in shape itself, and how that went lately.")]
public sealed record OptimizationOverview(
    [Summary("Whether it does: the operator's switch among the settings (`optimizeDatabases`)")]
    bool Enabled,
    [Summary("What the last optimization of the platform's own records and analytics did; absent before the first")]
    OptimizationRecord? Platform,
    [Summary("How many lambdas' databases were optimized in the last day")]
    int Lambdas,
    [Summary("Of those, how many failed")]
    int Failed,
    [Summary("What those gave back, in bytes")]
    long Freed,
    [Summary("When a lambda's database was last optimized; absent before the first")]
    DateTime? Last,
    [Summary("The hour of the day, in UTC, from which a database whose quiet hours are not known yet is optimized - 03:00 in the server's time zone - for three hours")]
    int FallbackFrom
);
