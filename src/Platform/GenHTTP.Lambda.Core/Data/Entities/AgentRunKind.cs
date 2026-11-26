namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What a run is for, stored as its number; named by <see cref="AgentRunKindNames"/>.
/// </summary>
public enum AgentRunKind
{

    /// <summary>A new app, asked for on /build.</summary>
    Build = 0,

    /// <summary>A message about an app that exists, from its owner.</summary>
    Change = 1

}
