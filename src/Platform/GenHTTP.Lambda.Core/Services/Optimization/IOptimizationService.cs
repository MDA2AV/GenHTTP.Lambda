namespace GenHTTP.Lambda.Services.Optimization;

/// <summary>
/// Keeps the databases in shape - the platform's own, and the lambdas' - each
/// in its quiet hours, unless the operator switched it off.
/// </summary>
/// <remarks>
/// What is decided here is the platform's: whether, which and when. How a
/// database is optimized is its integration's
/// (<c>IDatabaseProvider.OptimizeAsync</c>).
/// </remarks>
public interface IOptimizationService
{

    /// <summary>
    /// Optimizes what is due as of a moment and in its quiet hours, one
    /// database after the other, and writes down what each came to.
    /// </summary>
    ValueTask RunAsync(DateTime now, CancellationToken cancellation = default);

    /// <summary>
    /// Whether the databases are optimized, and how that went lately.
    /// </summary>
    ValueTask<OptimizationOverview> OverviewAsync(DateTime now, CancellationToken cancellation = default);

    /// <summary>
    /// What the last optimization of a lambda's database did, or nothing
    /// before the first.
    /// </summary>
    ValueTask<OptimizationRecord?> OfLambdaAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// Forgets what was done to the database of a lambda that is removed.
    /// </summary>
    ValueTask ForgetAsync(long lambdaId, CancellationToken cancellation = default);

}
