using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What this installation is like.")]
public sealed record PlatformResponse(
    [Summary("The terms of service, which creating a lambda accepts (`acceptedTerms`); absent where the installation has none")]
    string? Terms,
    [Summary("What a new lambda can start from, in the words of somebody who wants to build something")]
    IReadOnlyList<StarterResponse> Starters,
    [Summary("What a version of a new lambda may come to, its code and its resources together, in bytes - its tier's, `defaultTier`")]
    long BuildBytes,
    [Summary("How long a lambda in a tier that is not kept while unused - the sandbox - may go without a visit or an edit before it is taken offline, in hours")]
    int DeploymentLifetimeHours,
    [Summary("How long such a lambda may go without a visit or an edit before it is deleted, in days")]
    int RetentionDays,
    [Summary("The namespaces a lambda has imported without saying so")]
    IReadOnlyList<string> Imports,
    [Summary("The types, attributes and snippets the editor suggests while code is written")]
    IReadOnlyList<CompletionItem> Completions,
    [Summary("Whether the build agent is on offer, and how often")]
    BuildAvailability Build,
    [Summary("Where a lambda answers, with `{key}` where its public key goes: `https://{key}.genhttp.run/`, or `http://localhost:8080/lambda/{key}/` where lambdas answer below a path")]
    string LambdaUrl,
    [Summary("Who runs the installation, as far as its pages have to say so")]
    OperatorResponse Operator,
    [Summary("The tier a new lambda starts in, and what it lets it have")]
    TierResponse DefaultTier,
    [Summary("Every tier the operator assigns - `Sandbox`, `Production` and `System` - and what each lets a lambda have, as this installation set it")]
    IReadOnlyList<TierResponse> Tiers,
    [Summary("How the installation presents itself on its pages: its name, and the picture beside it")]
    BrandResponse Brand,
    [Summary("Who the installation is for, which its pages and its guide for agents speak to: `cloud` (a public platform), `home` (somebody, their household and friends) or `enterprise` (an organization and its people)")]
    string Profile
);
