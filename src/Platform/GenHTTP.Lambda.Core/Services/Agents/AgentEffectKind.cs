namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// What a run changed, as the platform recorded it where it was changed.
/// </summary>
/// <remarks>
/// Kept in a run's events by name (<see cref="AgentEffect"/>), so a member
/// keeps its name for good.
/// </remarks>
public enum AgentEffectKind
{

    /// <summary>It created a lambda.</summary>
    Created,

    /// <summary>It saved a version.</summary>
    Saved,

    /// <summary>It put a version online.</summary>
    Deployed,

    /// <summary>It started a feature.</summary>
    Started,

    /// <summary>It saved into a feature.</summary>
    Drafted,

    /// <summary>It merged a feature into a version.</summary>
    Merged,

    /// <summary>It deleted a feature.</summary>
    Discarded,

    /// <summary>It switched a kind of data on.</summary>
    Enabled,

    /// <summary>It switched analytics on.</summary>
    Counted,

    /// <summary>It published the source, or changed its license.</summary>
    Published,

    /// <summary>It took the source down.</summary>
    Withdrawn,

    /// <summary>It asked for the lambda to connect to other servers.</summary>
    Requested,

    /// <summary>It withdrew such a request.</summary>
    Unrequested,

    /// <summary>It listed the lambda in the showcase, or changed its entry.</summary>
    Showcased,

    /// <summary>It took the lambda out of the showcase.</summary>
    Unshowcased,

    /// <summary>It changed how the lambda's editor opens.</summary>
    Viewed,

    /// <summary>It wrote a file into the lambda's workspace.</summary>
    Wrote,

    /// <summary>It removed a file from the lambda's workspace.</summary>
    Removed

}
