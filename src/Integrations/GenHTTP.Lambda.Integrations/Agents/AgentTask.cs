namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// What a run is for.
/// </summary>
public enum AgentTask
{

    /// <summary>
    /// Somebody describes an app that does not exist yet: the agent creates a
    /// lambda for it, writes it and puts it online. Whoever asked gets its
    /// editor key from the platform, which knows the lambda the run created.
    /// </summary>
    Build,

    /// <summary>
    /// The owner of an app writes to the agent about it: a question to answer
    /// from what the platform says about it, a setting to change, or a change
    /// of the app - made in a draft, which is tried before it goes online.
    /// </summary>
    Change

}
