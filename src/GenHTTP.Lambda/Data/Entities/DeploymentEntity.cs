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
    /// What was asked for, in the words of whoever asked - usually the request
    /// an agent was answering when it wrote this version.
    /// </summary>
    public string? Prompt { get; set; }

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
