using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// A stored version, with its files - whose content is read by whoever needs it.
/// </summary>
public sealed record LambdaVersionContent(int Version, DateTime Created, IReadOnlyList<VersionFile> Files, string? Specification = null, string? Change = null,
                                          string? Origin = null);
