using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Optimization;

/// <summary>
/// What optimizing a database last did.
/// </summary>
[Summary("What keeping a database in shape last did.")]
public sealed record OptimizationRecord(
    [Summary("When it was done")]
    DateTime Optimized,
    [Summary("How long it took, in milliseconds")]
    long Millis,
    [Summary("The room the database took before, in bytes")]
    long Before,
    [Summary("The room it took after - less where room was given back")]
    long After,
    [Summary("What was done, as the database calls it - such as `PRAGMA optimize` or `VACUUM (ANALYZE)` - in the order it ran")]
    IReadOnlyList<string> Steps,
    [Summary("What was considered and left out, and why - such as a rebuild of a file too large for one")]
    IReadOnlyList<string> Skipped,
    [Summary("Why it failed; absent where it did not")]
    string? Failure
)
{

    internal static OptimizationRecord Of(DatabaseOptimizationEntity row)
        => new(row.Optimized, row.Millis, row.SizeBefore, row.SizeAfter, Lines(row.Steps), Lines(row.Skipped), row.Failure);

    internal static string? Join(IReadOnlyList<string> lines) => lines.Count == 0 ? null : string.Join('\n', lines);

    private static IReadOnlyList<string> Lines(string? text) => string.IsNullOrEmpty(text) ? [] : text.Split('\n');

}

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
