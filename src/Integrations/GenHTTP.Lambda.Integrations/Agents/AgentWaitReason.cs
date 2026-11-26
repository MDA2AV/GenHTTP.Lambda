namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// What holds a run up while it waits.
/// </summary>
public enum AgentWaitReason
{

    /// <summary>The service behind the agent is overloaded, and is asked again shortly.</summary>
    Overloaded = 0,

    /// <summary>A limit of the account the agent runs on was reached, and is asked again once it allows.</summary>
    RateLimited = 1,

    /// <summary>The agent system makes room for the run, such as starting what it runs in.</summary>
    Preparing = 2,

    /// <summary>Anything else, which the detail says.</summary>
    Other = 3

}
