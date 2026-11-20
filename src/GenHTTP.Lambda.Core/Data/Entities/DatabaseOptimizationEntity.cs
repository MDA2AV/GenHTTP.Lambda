namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What keeping a database in shape last did: a lambda's, or the platform's
/// own records and analytics together.
/// </summary>
/// <remarks>
/// One row per database, replaced every time it is optimized, so what is
/// shown is the last time and nothing grows with the days. A row of a lambda
/// goes with the lambda.
/// </remarks>
public sealed class DatabaseOptimizationEntity
{

    /// <summary>
    /// The lambda whose database it was, or <see cref="Platform"/> for the
    /// platform's own stores.
    /// </summary>
    public long LambdaId { get; set; }

    /// <summary>
    /// When it was done.
    /// </summary>
    public DateTime Optimized { get; set; }

    /// <summary>
    /// How long it took, in milliseconds.
    /// </summary>
    public long Millis { get; set; }

    /// <summary>
    /// The room the database took before, in bytes.
    /// </summary>
    public long SizeBefore { get; set; }

    /// <summary>
    /// The room it took after.
    /// </summary>
    public long SizeAfter { get; set; }

    /// <summary>
    /// What was done, a line each, as the database calls it.
    /// </summary>
    public required string Steps { get; set; }

    /// <summary>
    /// What was left out and why, a line each - or nothing.
    /// </summary>
    public string? Skipped { get; set; }

    /// <summary>
    /// Why it failed, where it did.
    /// </summary>
    public string? Failure { get; set; }

    /// <summary>
    /// The row that is not a lambda's but the platform's own records and analytics.
    /// </summary>
    public const long Platform = 0;

}
