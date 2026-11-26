namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// Why a run carries on from an earlier one.
/// </summary>
public enum AgentContinuationKind
{

    /// <summary>
    /// The earlier run ended with a question, and the message is the answer:
    /// it carries on with what it was asked then.
    /// </summary>
    Answer,

    /// <summary>
    /// The earlier run waited for somebody - a permission of the operator's,
    /// a key the owner had to enter - and that is there now: it carries on
    /// with what it was asked then, without the person repeating it.
    /// </summary>
    Unblocked,

    /// <summary>
    /// The earlier run stopped before it was done - its time was up, it
    /// failed, it was stopped - and is asked again: it carries on from what
    /// the platform says it did.
    /// </summary>
    Retry

}
