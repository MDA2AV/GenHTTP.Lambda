using GenHTTP.Lambda.Services.Background;

namespace GenHTTP.Lambda.Services.Blocking;

/// <summary>
/// Lets go of the address blocks that are older than two weeks.
/// </summary>
/// <remarks>
/// An address changes hands - a home connection is given another one, a
/// phone network hands it to the next customer - so a block that held for
/// good would sooner or later refuse somebody who did nothing. Hourly, so a
/// block is let go of within an hour of its time.
/// </remarks>
public sealed class AddressBlockJob(IAddressBlocks blocks) : IBackgroundJob
{

    public string Name => "address blocks";

    public TimeSpan Interval => TimeSpan.FromHours(1);

    public ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        blocks.Expire(DateTime.UtcNow);

        return ValueTask.CompletedTask;
    }

}
