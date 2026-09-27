using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The source to store as the next version, and why it was written.
/// </summary>
/// <param name="Files">Every file of the lambda, <c>lambda.cs</c> first</param>
/// <param name="Specification">
/// What the user wants from this version and why - their requirements, in their
/// own words where possible. Optional, and cut past 4000 characters.
/// </param>
/// <param name="Change">
/// What this version changes, in a line. Optional, and cut past 500 characters.
/// </param>
public sealed record VersionRequest(IReadOnlyList<LambdaFile>? Files, string? Specification = null, string? Change = null);

/// <summary>
/// Changes to apply to the newest version, stored as the next one.
/// </summary>
/// <param name="Files">Files to add, or to replace where one of that name exists</param>
/// <param name="Remove">Names of files to remove</param>
/// <param name="Edits">Replacements within files, each of which has to match exactly once</param>
/// <param name="Specification">What the user wants from this change and why. Optional.</param>
/// <param name="Change">What this version changes, in a line. Optional.</param>
public sealed record VersionChangeRequest(IReadOnlyList<LambdaFile>? Files, IReadOnlyList<string>? Remove, IReadOnlyList<FileEdit>? Edits,
                                          string? Specification = null, string? Change = null);

/// <summary>
/// One entry of the version history.
/// </summary>
/// <param name="Specification">What the user wanted and why, where whoever saved it said</param>
/// <param name="Change">What it changed, where whoever saved it said</param>
/// <param name="Origin">Where it came from: template, api, agent or system</param>
public sealed record VersionResponse(int Version, DateTime Created, string? Specification, string? Change, string? Origin);

/// <summary>
/// A version that was just stored.
/// </summary>
/// <param name="Specification">What the user wanted and why, as it was kept</param>
/// <param name="Change">What it changes, as it was kept</param>
/// <param name="Origin">Where it came from</param>
/// <param name="Deployment">The outcome of deploying it, if that was asked for</param>
public sealed record SavedVersionResponse(int Version, DateTime Created, string? Specification, string? Change, string? Origin,
                                          DeploymentOutcomeResponse? Deployment);

/// <summary>
/// A version including the code it holds.
/// </summary>
/// <param name="Files">Every file the version is made of, <c>lambda.cs</c> first</param>
public sealed record VersionContentResponse(int Version, DateTime Created, string? Specification, string? Change, string? Origin,
                                            IReadOnlyList<LambdaFile> Files);
