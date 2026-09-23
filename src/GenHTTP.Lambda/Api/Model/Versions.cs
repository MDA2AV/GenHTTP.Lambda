using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The source to store as the next version.
/// </summary>
/// <param name="Files">Every file of the lambda, <c>lambda.cs</c> first</param>
public sealed record VersionRequest(IReadOnlyList<LambdaFile>? Files);

/// <summary>
/// One entry of the version history.
/// </summary>
public sealed record VersionResponse(int Version, DateTime Created);

/// <summary>
/// A version including the code it holds.
/// </summary>
/// <param name="Files">Every file the version is made of, <c>lambda.cs</c> first</param>
public sealed record VersionContentResponse(int Version, DateTime Created, IReadOnlyList<LambdaFile> Files);
