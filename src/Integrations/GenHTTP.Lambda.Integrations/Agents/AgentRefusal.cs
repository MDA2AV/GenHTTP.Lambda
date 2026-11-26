namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// Why an agent system did not take a run.
/// </summary>
public enum AgentRefusal
{

    /// <summary>It is too busy to take another now: its queue is full.</summary>
    Busy = 0,

    /// <summary>It cannot be reached, or is not set up to run anything.</summary>
    Unavailable = 1,

    /// <summary>It cannot run what was asked for, such as an option value it does not have.</summary>
    Invalid = 2

}
