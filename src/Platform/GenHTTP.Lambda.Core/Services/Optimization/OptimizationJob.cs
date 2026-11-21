using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Background;

namespace GenHTTP.Lambda.Services.Optimization;

/// <summary>
/// Looks for databases due to be optimized, every quarter of an hour - which
/// finds each in its quiet hours, a stretch of three.
/// </summary>
public sealed class OptimizationJob(IOptimizationService optimization, LambdaOptions options) : IBackgroundJob
{

    public string Name => "Database optimization";

    public TimeSpan Interval => options.OptimizationInterval;

    public async ValueTask ExecuteAsync(CancellationToken cancellation) => await optimization.RunAsync(DateTime.UtcNow, cancellation);

}
