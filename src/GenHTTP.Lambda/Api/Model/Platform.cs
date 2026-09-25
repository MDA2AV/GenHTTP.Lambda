using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// What the editor needs to know about the platform it talks to.
/// </summary>
public sealed record PlatformResponse(
    string Terms,
    IReadOnlyList<StarterResponse> Starters,
    int MaxCodeLength,
    int DeploymentLifetimeHours,
    int RetentionDays,
    IReadOnlyList<string> Imports,
    IReadOnlyList<CompletionItem> Completions,
    BuildAvailability Build
);

/// <summary>
/// Something a new lambda can be started from.
/// </summary>
/// <param name="Id">What to pass as the template when creating the lambda</param>
/// <param name="Title">What somebody would want to build, in their words</param>
/// <param name="Description">What they would get</param>
/// <param name="Demo">Where the demo it copies is running, to look at first; null for the empty lambda</param>
public sealed record StarterResponse(string Id, string Title, string Description, string? Demo);
