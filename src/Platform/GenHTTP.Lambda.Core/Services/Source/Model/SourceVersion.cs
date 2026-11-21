namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// A version as the history of a published source lists it: when, and what
/// it changed - never what was asked for in whose words, which stays with the
/// owner.
/// </summary>
/// <param name="Origin">Which door it came through: template, api, agent</param>
public sealed record SourceVersion(int Version, DateTime Created, string? Change, string? Origin);
