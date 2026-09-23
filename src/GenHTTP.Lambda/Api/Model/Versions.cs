using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The source to store as the next version.
/// </summary>
/// <param name="Code">The snippet, for a lambda that is a single file</param>
/// <param name="Files">Every file, where it is more than one. Wins over Code.</param>
public sealed record CodeRequest(string? Code, IReadOnlyList<LambdaFile>? Files = null);

/// <summary>
/// One entry of the version history.
/// </summary>
public sealed record VersionResponse(int Version, DateTime Created);

/// <summary>
/// A version including the code it holds.
/// </summary>
/// <param name="Code">The snippet, so a caller that knows nothing of files still reads something</param>
/// <param name="Files">Every file the version is made of, the snippet first</param>
public sealed record VersionContentResponse(int Version, DateTime Created, string Code, IReadOnlyList<LambdaFile> Files);
