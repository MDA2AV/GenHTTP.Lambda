namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// A ticket issued to a job of the build agent before runs were records of
/// their own, by the SHA-256 of the ticket rather than the ticket itself,
/// with the model and effort the job runs on.
/// </summary>
/// <remarks>
/// A run's credential is kept with the run now (<see cref="AgentRunEntity.Ticket"/>).
/// These are still read for the day a ticket lasts, so a job under way while
/// an installation was upgraded marks what it saves, and are then removed.
/// </remarks>
public sealed class AgentTicketEntity
{

    public required string Hash { get; set; }

    public AgentModel Model { get; set; }

    public AgentEffort Effort { get; set; }

    public DateTime Expires { get; set; }

}
