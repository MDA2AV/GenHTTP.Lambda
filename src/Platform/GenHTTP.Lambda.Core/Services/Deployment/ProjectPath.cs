namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// What a path of a project is to the lambda it was made from.
/// </summary>
/// <param name="Name">The file of the lambda it is, where it is one - as the path says, which may differ from the name the lambda gives it in case</param>
public sealed record ProjectPath(ProjectPathKind Kind, string? Name);
