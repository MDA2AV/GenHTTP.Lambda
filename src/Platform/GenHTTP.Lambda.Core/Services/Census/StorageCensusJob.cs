using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Background;

namespace GenHTTP.Lambda.Services.Census;

/// <summary>
/// Measures what takes the room of the installation again, every hour, for
/// the operator's dashboards.
/// </summary>
/// <remarks>
/// Only where there is an operator to read it - an installation without an
/// admin token has no panel, and the walk would be for nobody. What runs out
/// first, the free space, is read whenever it is asked for; what took it
/// changes slowly enough for an hour.
/// </remarks>
public sealed class StorageCensusJob(ICensusService census, LambdaOptions options) : IBackgroundJob
{

    public string Name => "storage census";

    public TimeSpan Interval => TimeSpan.FromHours(1);

    public async ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        if (options.Administrable)
        {
            await census.MeasureAsync(cancellation);
        }
    }

}
