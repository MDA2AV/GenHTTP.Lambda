using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What this installation is like.")]
public sealed record PlatformResponse(
    [Summary("The terms of service, which creating a lambda accepts (`acceptedTerms`)")]
    string Terms,
    [Summary("What a new lambda can start from, in the words of somebody who wants to build something")]
    IReadOnlyList<StarterResponse> Starters,
    [Summary("What a version of a free lambda may come to, its code and its resources together, in bytes")]
    long BuildBytes,
    [Summary("How long a free lambda may go without a visit or an edit before it is taken offline, in hours")]
    int DeploymentLifetimeHours,
    [Summary("How long a free lambda may go without a visit or an edit before it is deleted, in days")]
    int RetentionDays,
    [Summary("The namespaces a lambda has imported without saying so")]
    IReadOnlyList<string> Imports,
    [Summary("The types, attributes and snippets the editor suggests while code is written")]
    IReadOnlyList<CompletionItem> Completions,
    [Summary("Whether the build agent is on offer, and how often")]
    BuildAvailability Build,
    [Summary("Where a lambda answers, with `{key}` where its public key goes: `https://{key}.genhttp.run/`")]
    string LambdaUrl
);

[Summary("Something a new lambda can be started from.")]
public sealed record StarterResponse(
    [Summary("What to pass as `template` when creating the lambda")]
    string Id,
    [Summary("What somebody would want to build, in their words")]
    string Title,
    [Summary("What they would get")]
    string Description,
    [Summary("Where the demo it copies answers, to look at first; absent for the empty lambda")]
    string? Demo
);

[Summary("Which pages the site links to.")]
public sealed record FeaturesResponse(
    [Summary("Whether the header links to the enterprise page, which can be reached either way")]
    bool Enterprise
);
