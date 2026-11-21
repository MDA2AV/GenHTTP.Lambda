using GenHTTP.Lambda.Services.Building;

namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// A ticket issued to a job of the build agent, by the SHA-256 of the ticket
/// rather than the ticket itself, with the model and effort the job runs on.
/// </summary>
public sealed class AgentTicketEntity
{

    public required string Hash { get; set; }

    public AgentModel Model { get; set; }

    public AgentEffort Effort { get; set; }

    public DateTime Expires { get; set; }

}
