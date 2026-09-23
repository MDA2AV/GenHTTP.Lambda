using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The source to store as the next version.
/// </summary>
/// <param name="Files">Every file of the lambda, <c>lambda.cs</c> first</param>
public sealed record VersionRequest(IReadOnlyList<LambdaFile>? Files);

/// <summary>
/// Changes to apply to the newest version, stored as the next one.
/// </summary>
/// <param name="Files">Files to add, or to replace where one of that name exists</param>
/// <param name="Remove">Names of files to remove</param>
/// <param name="Edits">Replacements within files, each of which has to match exactly once</param>
public sealed record VersionChangeRequest(IReadOnlyList<LambdaFile>? Files, IReadOnlyList<string>? Remove, IReadOnlyList<FileEdit>? Edits);

/// <summary>
/// One entry of the version history.
/// </summary>
public sealed record VersionResponse(int Version, DateTime Created);

/// <summary>
/// A version that was just stored.
/// </summary>
/// <param name="Deployment">The outcome of deploying it, if that was asked for</param>
public sealed record SavedVersionResponse(int Version, DateTime Created, DeploymentOutcomeResponse? Deployment);

/// <summary>
/// A version including the code it holds.
/// </summary>
/// <param name="Files">Every file the version is made of, <c>lambda.cs</c> first</param>
public sealed record VersionContentResponse(int Version, DateTime Created, IReadOnlyList<LambdaFile> Files);
