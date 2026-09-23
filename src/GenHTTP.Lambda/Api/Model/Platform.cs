using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// What the editor needs to know about the platform it talks to.
/// </summary>
public sealed record PlatformResponse(
    string Terms,
    IReadOnlyList<TemplateGroupResponse> Templates,
    int MaxCodeLength,
    int DeploymentLifetimeHours,
    int RetentionDays,
    IReadOnlyList<string> Imports,
    IReadOnlyList<CompletionItem> Completions,
    BuildAvailability Build
);

/// <summary>
/// One example a new lambda can be started from, with the code it would be
/// seeded with so the assistant can show it before anything is created.
/// </summary>
public sealed record TemplateResponse(string Id, string Name, string Description, string Code, bool Hidden);

public sealed record TemplateGroupResponse(string Id, string Name, string Description, IReadOnlyList<TemplateResponse> Templates);
