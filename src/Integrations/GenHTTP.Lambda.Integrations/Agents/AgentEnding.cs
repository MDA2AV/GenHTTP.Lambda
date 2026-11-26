namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// How a run ended.
/// </summary>
/// <remarks>
/// Stored by its number with the run, so a member keeps its number for good
/// and a new one takes the next.
/// </remarks>
public enum AgentEnding
{

    /// <summary>The agent finished: the message is its closing note, or its answer.</summary>
    Completed = 0,

    /// <summary>The agent needs to know something from the person to go on: the message is the question, their next message the answer.</summary>
    Asked = 1,

    /// <summary>The agent would not do what was asked, before it did anything: the message says why.</summary>
    Declined = 2,

    /// <summary>It was stopped.</summary>
    Cancelled = 3,

    /// <summary>Its time was up.</summary>
    TimedOut = 4,

    /// <summary>It took as many steps as it may.</summary>
    OutOfSteps = 5,

    /// <summary>The agent could not sign in to the service behind it: the installation's credentials, not anything about the request.</summary>
    SignInFailed = 6,

    /// <summary>It stopped with an error: the detail says which.</summary>
    Failed = 7,

    /// <summary>The agent system knows no such run - any more: it was restarted, or forgot it.</summary>
    Lost = 8

}
