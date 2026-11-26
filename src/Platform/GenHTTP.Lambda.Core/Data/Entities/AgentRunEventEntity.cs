namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// Something that happened in a run, in the order it happened: a step the
/// platform saw the run's calls take, a line the agent said, something it
/// did beyond the platform, something it changed, a file it handed back.
/// </summary>
/// <remarks>
/// What it holds depends on its kind and is kept as a JSON object, the shape
/// the API hands out for a step, or the effect the platform recorded. A step
/// is recorded as it begins and completed in place once it is done.
/// </remarks>
public sealed class AgentRunEventEntity
{

    public required string RunId { get; set; }

    /// <summary>Its place in the run, from one.</summary>
    public int Number { get; set; }

    public DateTime At { get; set; }

    public AgentRunEventKind Kind { get; set; }

    /// <summary>What tells it apart, where it is completed later: the id of an activity of the agent's.</summary>
    public string? Key { get; set; }

    public required string Data { get; set; }

}
