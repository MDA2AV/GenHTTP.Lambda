namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// Where a run has got to, stored as its number.
/// </summary>
/// <remarks>
/// How it ended is beside it (<see cref="AgentRunEntity.Ending"/>), as the
/// agent integration says it.
/// </remarks>
public enum AgentRunState
{

    /// <summary>Recorded, and being handed to the agent system.</summary>
    Starting = 0,

    /// <summary>Taken by the agent system, which works on others first.</summary>
    Queued = 1,

    /// <summary>Being worked on.</summary>
    Working = 2,

    /// <summary>Begun, and held up by something that passes by itself (<see cref="AgentRunEntity.WaitReason"/>).</summary>
    Waiting = 3,

    /// <summary>Over.</summary>
    Ended = 4

}
