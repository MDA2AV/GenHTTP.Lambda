using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Background;

/// <summary>
/// Retires what the free tier no longer covers: deployments are taken down a day
/// after they went live, and lambdas nobody touched for a month are removed.
/// </summary>
public sealed class MaintenanceJob(IMetaService meta, LambdaOptions options) : IBackgroundJob
{

    public string Name => "maintenance";

    public TimeSpan Interval => options.MaintenanceInterval;

    public async ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        await meta.RunMaintenanceAsync(DateTime.UtcNow, cancellation);
    }

}
