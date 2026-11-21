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
