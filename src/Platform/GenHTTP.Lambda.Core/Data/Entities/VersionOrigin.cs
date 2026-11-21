namespace GenHTTP.Lambda.Data.Entities;

/// <summary>Where a version, a deployment or a feature came from.</summary>
/// <remarks>
/// Stored as its number, so a member keeps its number for good and a new one
/// takes the next; what the API, MCP and the pages call it is
/// <see cref="VersionOriginNames"/>. A row whose number no member has - a
/// later version's, or one written by hand - reads as not recorded.
/// </remarks>
public enum VersionOrigin
{

    /// <summary>Seeded from a template when the lambda was created.</summary>
    Template = 1,

    /// <summary>Through the REST API, which is also what the editor uses.</summary>
    Api = 2,

    /// <summary>Through the MCP endpoint, by an agent.</summary>
    Agent = 3,

    /// <summary>Pushed to the lambda's repository with git, a commit each.</summary>
    Git = 4,

    /// <summary>By the operator, through the administration panel.</summary>
    Admin = 5,

    /// <summary>By the installation itself, on a schedule.</summary>
    System = 6

}
