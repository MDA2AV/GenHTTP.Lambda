using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Where the files of a lambda are in the .NET project it is exported as and
/// read as with git, and which file of the lambda a path of the project is.
/// </summary>
/// <remarks>
/// The snippet is <c>Project.cs</c>, the other code files keep their names the
/// way .NET writes them (<c>store.cs</c> is <c>Store.cs</c>), the assets go
/// into <c>assets/</c>, and the documentation and the tests leave
/// <c>.lambda/</c> for <c>docs/</c> and <c>tests/</c>, where a .NET project
/// keeps them. A code file that would be called what the project calls its
/// own - <c>program.cs</c> - is put beside it under a name of its own.
///
/// What else the project holds is the platform's: the program that hosts the
/// lambda, its project file, what stands in for the platform in
/// <c>Platform/</c>, and the files around them. Every one of those is a
/// <see cref="Generated"/> path; anything that is neither is no part of a
/// lambda's project at all.
/// </remarks>
public static class ProjectPaths
{

    /// <summary>
    /// The file the snippet becomes.
    /// </summary>
    public const string Snippet = "Project.cs";

    /// <summary>
    /// The program that hosts what <see cref="Snippet"/> returns.
    /// </summary>
    public const string Program = "Program.cs";

    /// <summary>
    /// Where the files the lambda ships go.
    /// </summary>
    public const string Assets = "assets/";

    /// <summary>
    /// What stands in for the platform.
    /// </summary>
    public const string Platform = "Platform/";

    /// <summary>
    /// The files at the root of a project that are the platform's, besides
    /// <see cref="Program"/> and the project file.
    /// </summary>
    private static readonly HashSet<string> Around = new(StringComparer.Ordinal)
    {
        "Dockerfile", ".gitignore", ".dockerignore", "LICENSE", "AGENTS.md", "CLAUDE.md"
    };

    /// <summary>
    /// The names a code file of the lambda cannot have in the project, since
    /// the project's own are called that.
    /// </summary>
    private static readonly HashSet<string> Taken = new(StringComparer.OrdinalIgnoreCase) { Snippet, Program };

    #region Functionality

    /// <summary>
    /// Where a file of the lambda is in the project.
    /// </summary>
    public static string Of(string name)
    {
        if (name == LambdaSource.EntryName)
        {
            return Snippet;
        }

        if (LambdaSource.IsContext(name))
        {
            return name[LambdaSource.ContextFolder.Length..];
        }

        if (LambdaSource.IsCode(name))
        {
            var capitalized = Capitalize(name);

            // beside the project's own, under a name the lambda could not give it
            return Taken.Contains(capitalized) ? $"{capitalized[..^3]}.Lambda.cs" : capitalized;
        }

        return Assets + name;
    }

    /// <summary>
    /// What a path of the project is to the lambda.
    /// </summary>
    public static ProjectPath Classify(string path)
    {
        if (path == Snippet)
        {
            return new ProjectPath(ProjectPathKind.Lambda, LambdaSource.EntryName);
        }

        if (path.StartsWith(Assets, StringComparison.Ordinal))
        {
            return new ProjectPath(ProjectPathKind.Lambda, path[Assets.Length..]);
        }

        if (path.StartsWith("docs/", StringComparison.Ordinal) || path.StartsWith("tests/", StringComparison.Ordinal))
        {
            return new ProjectPath(ProjectPathKind.Lambda, LambdaSource.ContextFolder + path);
        }

        if (path == Program || path.StartsWith(Platform, StringComparison.Ordinal) || Around.Contains(path) || IsProjectFile(path))
        {
            return new ProjectPath(ProjectPathKind.Generated, null);
        }

        if (!path.Contains('/') && path.EndsWith(".cs", StringComparison.Ordinal))
        {
            return new ProjectPath(ProjectPathKind.Lambda, path);
        }

        return new ProjectPath(ProjectPathKind.Foreign, null);
    }

    /// <summary>
    /// The name a code file of the lambda gets in the project: "store.cs"
    /// becomes "Store.cs".
    /// </summary>
    public static string Capitalize(string name)
    {
        var slash = name.LastIndexOf('/') + 1;

        return slash < name.Length ? name[..slash] + char.ToUpperInvariant(name[slash]) + name[(slash + 1)..] : name;
    }

    private static bool IsProjectFile(string path) => !path.Contains('/') && path.EndsWith(".csproj", StringComparison.Ordinal);

    #endregion

}

/// <summary>
/// What a path of a project is to the lambda it was made from.
/// </summary>
/// <param name="Name">The file of the lambda it is, where it is one - as the path says, which may differ from the name the lambda gives it in case</param>
public sealed record ProjectPath(ProjectPathKind Kind, string? Name);

public enum ProjectPathKind
{

    /// <summary>A file of the lambda: its code, an asset, its documentation or a test.</summary>
    Lambda,

    /// <summary>What the platform puts around a lambda to make it a project.</summary>
    Generated,

    /// <summary>Neither: nothing a lambda's project holds.</summary>
    Foreign

}
