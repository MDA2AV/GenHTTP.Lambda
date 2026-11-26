namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// Which way a file of a run travelled, stored as its number.
/// </summary>
public enum AgentFileDirection
{

    /// <summary>The person attached it to a message.</summary>
    Attached = 0,

    /// <summary>The agent handed it to the person.</summary>
    Returned = 1

}
