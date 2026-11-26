namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What an event of a run is, stored as its number.
/// </summary>
public enum AgentRunEventKind
{

    /// <summary>A call of the run's the platform saw at its doors: what it was about and how it went.</summary>
    Step = 0,

    /// <summary>A line the agent wrote for the person.</summary>
    Said = 1,

    /// <summary>Something the agent did beyond the platform, as the integration told it.</summary>
    Activity = 2,

    /// <summary>Something the run changed, as the platform recorded it where it was changed - what it is undone by.</summary>
    Effect = 3,

    /// <summary>A file the agent handed the person.</summary>
    Returned = 4,

    /// <summary>The owner undid the run: what was undone, and what was left.</summary>
    Undone = 5

}
