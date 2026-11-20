namespace GenHTTP.Lambda.Services.QuietHours;

/// <summary>
/// When the installation, and each lambda, is usually quiet: what work that
/// can wait is best done in.
/// </summary>
/// <remarks>
/// An indication worked out from how many requests each hour of the day saw
/// lately - light to keep, and not precise (see <c>QuietProfile</c>). It
/// decides nothing itself; whoever plans work asks it.
/// </remarks>
public interface IQuietHoursService
{

    /// <summary>
    /// The typical day of the installation as a whole, from every request the
    /// server answered.
    /// </summary>
    ValueTask<TypicalDay> OfInstallationAsync(DateTime now, CancellationToken cancellation = default);

    /// <summary>
    /// The typical day of a lambda, from the requests it answered - not its
    /// features' previews.
    /// </summary>
    ValueTask<TypicalDay> OfLambdaAsync(long lambdaId, DateTime now, CancellationToken cancellation = default);

    /// <summary>
    /// The typical days of the lambdas asked for that were ever visited, for
    /// whoever decides about many at once.
    /// </summary>
    ValueTask<IReadOnlyDictionary<long, TypicalDay>> OfLambdasAsync(IReadOnlyCollection<long> lambdaIds, DateTime now, CancellationToken cancellation = default);

    /// <summary>
    /// Writes down what was counted since the last time, as of a moment.
    /// </summary>
    ValueTask FlushAsync(DateTime now, CancellationToken cancellation = default);

    /// <summary>
    /// Forgets what was counted of a lambda that is removed.
    /// </summary>
    ValueTask ForgetAsync(long lambdaId, CancellationToken cancellation = default);

}
