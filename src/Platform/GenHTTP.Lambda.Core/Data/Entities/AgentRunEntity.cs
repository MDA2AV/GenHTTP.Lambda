using GenHTTP.Lambda.Integrations.Agents;

namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// One message to the installation's agent and how it went: what was asked,
/// on which options, where it got to and how it ended.
/// </summary>
/// <remarks>
/// Kept by the platform rather than by the agent system, so a page closed
/// and opened again - or a platform restarted - finds the run where it was,
/// and what a page says came of it is read from the platform's own records.
/// Kept for a while after it ended (<c>AgentRetention</c>), with its events
/// and files.
/// </remarks>
public sealed class AgentRunEntity
{

    /// <summary>The platform's id for it, a GUID in its hyphenated form.</summary>
    public required string Id { get; set; }

    public AgentRunKind Kind { get; set; }

    /// <summary>The lambda it works on - for a build, once it created one.</summary>
    public long? LambdaId { get; set; }

    public AgentRunState State { get; set; }

    /// <summary>How it ended, once it has.</summary>
    public AgentEnding? Ending { get; set; }

    /// <summary>What was asked, as it was asked - cut to what the agent takes.</summary>
    public required string Prompt { get; set; }

    /// <summary>The value of each of the integration's options it runs on, as a JSON object.</summary>
    public required string Options { get; set; }

    /// <summary>Whether a value behind the installation's password was chosen, which it was checked for.</summary>
    public bool Guarded { get; set; }

    /// <summary>The language of the page it was asked from.</summary>
    public string? Language { get; set; }

    /// <summary>The words of the view it was asked from.</summary>
    public EditorView View { get; set; }

    /// <summary>For a change: whether to put it online once it works.</summary>
    public bool Deploy { get; set; }

    /// <summary>For a change: the feature it was asked to go on with, by its key.</summary>
    public string? Feature { get; set; }

    /// <summary>The version online as it was asked for.</summary>
    public int? Before { get; set; }

    /// <summary>The run it carries on from, and why.</summary>
    public string? Continues { get; set; }

    public AgentContinuationKind? Continuation { get; set; }

    /// <summary>What the agent system knows it by, where that is not its id.</summary>
    public string? Reference { get; set; }

    /// <summary>Its place in the agent system's queue while it waits there.</summary>
    public int Position { get; set; }

    /// <summary>What holds it up while it waits after it began, and until when.</summary>
    public AgentWaitReason? WaitReason { get; set; }

    public DateTime? WaitUntil { get; set; }

    /// <summary>What it may take, in seconds and in steps; nothing for no limit.</summary>
    public int? TimeLimit { get; set; }

    public int? StepLimit { get; set; }

    /// <summary>What the agent wrote at the end, for the person: its note, its answer, its question, why it declined.</summary>
    public string? Message { get; set; }

    /// <summary>What went wrong, in the words of whatever it went wrong in.</summary>
    public string? Detail { get; set; }

    public long? InputTokens { get; set; }

    public long? OutputTokens { get; set; }

    public double? Cost { get; set; }

    public string? Currency { get; set; }

    /// <summary>The position of the last event of the agent system's taken over - what it is asked again from.</summary>
    public long Followed { get; set; }

    /// <summary>The SHA-256 of the run's credential, as lower case hex: what its calls are known by.</summary>
    public required string Ticket { get; set; }

    public DateTime Created { get; set; }

    /// <summary>When the agent first worked on it.</summary>
    public DateTime? Started { get; set; }

    public DateTime? Ended { get; set; }

}
