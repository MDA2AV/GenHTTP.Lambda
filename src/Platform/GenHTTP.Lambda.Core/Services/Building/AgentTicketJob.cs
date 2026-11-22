using GenHTTP.Lambda.Services.Background;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// Removes the tickets of the build agent's jobs once they expired.
/// </summary>
/// <remarks>
/// An expired ticket is refused whether it is still kept or not; this only
/// keeps the table as small as the jobs of the last day.
/// </remarks>
public sealed class AgentTicketJob(AgentTickets tickets) : IBackgroundJob
{

    public string Name => "agent tickets";

    public TimeSpan Interval => TimeSpan.FromHours(1);

    public async ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        await tickets.ExpireAsync(DateTime.UtcNow, cancellation);
    }

}
