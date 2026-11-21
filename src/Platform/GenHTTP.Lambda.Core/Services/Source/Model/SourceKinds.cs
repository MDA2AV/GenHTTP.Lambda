using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// What a file of a packed project is to somebody reading it.
/// </summary>
public static class SourceKinds
{

    /// <summary>The lambda's code: Project.cs, and every other file of its own but its resources.</summary>
    public const string Code = "code";

    /// <summary>What it reads and serves while it runs: its front end, its migrations.</summary>
    public const string Resource = "resource";

    /// <summary>What was written about it, in docs/ of its code.</summary>
    public const string Docs = "docs";

    /// <summary>How it is tested, in tests/ of its code.</summary>
    public const string Tests = "tests";

    /// <summary>What stands in for the platform.</summary>
    public const string Platform = "platform";

    /// <summary>What makes it a project: the host, the project file, the Dockerfile, the license.</summary>
    public const string Project = "project";

    /// <summary>
    /// The kind of a file, by where the packer put it.
    /// </summary>
    /// <param name="path">Its path below the project's folder</param>
    public static string Of(string path)
    {
        if (path.StartsWith(ProjectPaths.Resources, StringComparison.Ordinal))
        {
            return Resource;
        }

        if (path.StartsWith("docs/", StringComparison.Ordinal))
        {
            return Docs;
        }

        if (path.StartsWith("tests/", StringComparison.Ordinal))
        {
            return Tests;
        }

        if (path.StartsWith(ProjectPaths.Platform, StringComparison.Ordinal))
        {
            return Platform;
        }

        return ProjectPaths.Classify(path).Kind == ProjectPathKind.Generated ? Project : Code;
    }

}
