namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// A version of the code of a lambda.
/// </summary>
/// <remarks>
/// The newest version of a lambda is the one being worked on and may be saved
/// over, which counts up its <see cref="Revision" />; every version before it
/// is history and never changes again.
/// </remarks>
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
    /// How many times it has been saved, the save that created it included.
    /// </summary>
    public int Revision { get; set; } = 1;

    /// <summary>
    /// When it was last saved over, or null while it is as it was created.
    /// </summary>
    public DateTime? Modified { get; set; }

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
    /// Where it came from, one of <see cref="VersionOrigins" />.
    /// </summary>
    public string? Origin { get; set; }

    public LambdaEntity? Lambda { get; set; }

}

/// <summary>Where a version or a deployment came from.</summary>
public static class VersionOrigins
{

    /// <summary>Seeded from a template when the lambda was created.</summary>
    public const string Template = "template";

    /// <summary>Through the REST API, which is also what the editor uses.</summary>
    public const string Api = "api";

    /// <summary>Through the MCP endpoint, by an agent.</summary>
    public const string Agent = "agent";

    /// <summary>By the operator, through the administration panel.</summary>
    public const string Admin = "admin";

    /// <summary>By the installation itself, on a schedule.</summary>
    public const string System = "system";

}
