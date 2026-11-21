namespace GenHTTP.Lambda.Data.Entities;

/// <summary>Where a version or a deployment came from.</summary>
public static class VersionOrigins
{

    /// <summary>Seeded from a template when the lambda was created.</summary>
    public const string Template = "template";

    /// <summary>Through the REST API, which is also what the editor uses.</summary>
    public const string Api = "api";

    /// <summary>Through the MCP endpoint, by an agent.</summary>
    public const string Agent = "agent";

    /// <summary>Pushed to the lambda's repository with git, a commit each.</summary>
    public const string Git = "git";

    /// <summary>By the operator, through the administration panel.</summary>
    public const string Admin = "admin";

    /// <summary>By the installation itself, on a schedule.</summary>
    public const string System = "system";

}
