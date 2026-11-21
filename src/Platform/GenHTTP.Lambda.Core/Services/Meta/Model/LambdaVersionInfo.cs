namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// A stored version of the code of a lambda.
/// </summary>
/// <param name="Specification">What the user wanted and why, where whoever saved it said</param>
/// <param name="Change">What it changed, in a line</param>
/// <param name="Origin">Where it came from: template, api or agent</param>
/// <param name="Writer">Who wrote it, where the platform knows it: build-agent</param>
/// <param name="Model">The model the build agent ran on, by the name it was offered by</param>
/// <param name="Effort">The effort it ran at</param>
public sealed record LambdaVersionInfo(int Version, DateTime Created, string? Specification = null, string? Change = null, string? Origin = null,
                                       string? Writer = null, string? Model = null, string? Effort = null);
