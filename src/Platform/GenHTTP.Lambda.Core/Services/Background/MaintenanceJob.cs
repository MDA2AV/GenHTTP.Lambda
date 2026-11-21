using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Background;

/// <summary>
/// Retires what the sandbox no longer covers: deployments are taken down a day
/// after they went live, and lambdas nobody touched for a month are removed.
/// The previews of features follow the same rule as the lambdas they belong to.
/// What no version or feature holds any more is removed from the stores.
/// </summary>
public sealed class MaintenanceJob(IMetaService meta, IFeatureService features, IStorageService storage, LambdaOptions options) : IBackgroundJob
{

    public string Name => "maintenance";

    public TimeSpan Interval => options.MaintenanceInterval;

    public async ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        var now = DateTime.UtcNow;

        await meta.RunMaintenanceAsync(now, cancellation);

        await features.RunMaintenanceAsync(now, cancellation);

        await features.SweepAsync(cancellation);

        // last, so what the sweeps above let go of goes with it
        await storage.CollectAsync(cancellation);
    }

}
