using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Background;

/// <summary>
/// Retires what the free tier no longer covers: deployments are taken down a day
/// after they went live, and lambdas nobody touched for a month are removed.
/// The previews of features follow the same rule as the lambdas they belong to.
/// </summary>
public sealed class MaintenanceJob(IMetaService meta, IFeatureService features, LambdaOptions options) : IBackgroundJob
{

    public string Name => "maintenance";

    public TimeSpan Interval => options.MaintenanceInterval;

    public async ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        var now = DateTime.UtcNow;

        await meta.RunMaintenanceAsync(now, cancellation);

        await features.RunMaintenanceAsync(now, cancellation);

        await features.SweepAsync(cancellation);
    }

}
