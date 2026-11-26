using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// What a run changed, as the platform recorded it where it was changed.
/// </summary>
/// <remarks>
/// Kept in a run's events by name (<see cref="AgentEffect"/>), so a member
/// keeps its name for good.
/// </remarks>
// kept in the records by these names, which stay as they are when a member is
// renamed - as a number of a column does
public enum AgentEffectKind
{

    /// <summary>It created a lambda.</summary>
    [JsonStringEnumMemberName("Created")]
    Created = 0,

    /// <summary>It saved a version.</summary>
    [JsonStringEnumMemberName("Saved")]
    Saved = 1,

    /// <summary>It put a version online.</summary>
    [JsonStringEnumMemberName("Deployed")]
    Deployed = 2,

    /// <summary>It started a feature.</summary>
    [JsonStringEnumMemberName("Started")]
    Started = 3,

    /// <summary>It saved into a feature.</summary>
    [JsonStringEnumMemberName("Drafted")]
    Drafted = 4,

    /// <summary>It merged a feature into a version.</summary>
    [JsonStringEnumMemberName("Merged")]
    Merged = 5,

    /// <summary>It deleted a feature.</summary>
    [JsonStringEnumMemberName("Discarded")]
    Discarded = 6,

    /// <summary>It switched a kind of data on.</summary>
    [JsonStringEnumMemberName("Enabled")]
    Enabled = 7,

    /// <summary>It switched analytics on.</summary>
    [JsonStringEnumMemberName("Counted")]
    Counted = 8,

    /// <summary>It published the source, or changed its license.</summary>
    [JsonStringEnumMemberName("Published")]
    Published = 9,

    /// <summary>It asked for the lambda to connect to other servers.</summary>
    [JsonStringEnumMemberName("Requested")]
    Requested = 10,

    /// <summary>It listed the lambda in the showcase, or changed its entry.</summary>
    [JsonStringEnumMemberName("Showcased")]
    Showcased = 11,

    /// <summary>It changed how the lambda's editor opens.</summary>
    [JsonStringEnumMemberName("Viewed")]
    Viewed = 12,

    /// <summary>It wrote a file into the lambda's workspace.</summary>
    [JsonStringEnumMemberName("Wrote")]
    Wrote = 13,

    /// <summary>It removed a file from the lambda's workspace.</summary>
    [JsonStringEnumMemberName("Removed")]
    Removed = 14

}
