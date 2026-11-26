namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// An immutable version of the code of a lambda. Saving or deploying creates a
/// new record, existing ones are never updated.
/// </summary>
public sealed class DeploymentEntity
{

    public long Id { get; set; }

    public long LambdaId { get; set; }

    /// <summary>
    /// The version number, starting at one and counting up per lambda.
    /// </summary>
    public int Version { get; set; }

    public DateTime Created { get; set; }

    /// <summary>
    /// What the user wanted from this version and why - their requirements,
    /// usually as an agent restated them when it wrote this version.
    /// </summary>
    public string? Specification { get; set; }

    /// <summary>
    /// What this version changed, in a line.
    /// </summary>
    public string? Change { get; set; }

    /// <summary>
    /// Where it came from.
    /// </summary>
    public VersionOrigin? Origin { get; set; }

    /// <summary>
    /// Who wrote it, where the platform knows it: the build agent, on a job it
    /// issued a ticket for.
    /// </summary>
    public VersionWriter? Writer { get; set; }

    /// <summary>
    /// The model the build agent ran on, by the name it was offered by.
    /// </summary>
    public AgentModel? AgentModel { get; set; }

    /// <summary>
    /// The effort the build agent ran at.
    /// </summary>
    public AgentEffort? AgentEffort { get; set; }

    /// <summary>
    /// The run of the installation's agent that wrote it, where one did - kept
    /// as long as the run is, and only as an id: the run may be gone.
    /// </summary>
    public string? AgentRun { get; set; }

    /// <summary>
    /// The value of each option of the agent integration the run ran on, as a
    /// JSON object - kept for good, unlike the run. Empty for what was written
    /// before, whose model and effort are in the columns above.
    /// </summary>
    public string? AgentOptions { get; set; }

    public LambdaEntity? Lambda { get; set; }

}
