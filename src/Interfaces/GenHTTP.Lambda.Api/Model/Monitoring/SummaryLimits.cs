using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// Nothing counts files, only what they come to: a version, its code and its
/// resources together; and the data, its database and its workspace together.
/// </remarks>
[Summary("What a lambda of its tier may have on this installation, as the operator set it.")]
public sealed record SummaryLimits(
    [Summary("What a version may come to, its code and its resources together, in bytes")]
    long BuildBytes,
    [Summary("The room its data may take, its database and its workspace together, in bytes")]
    long DataBytes,
    [Summary("How many versions are kept; the oldest beyond that go with every save, but never the one online or one a feature is based on")]
    int Versions,
    [Summary("How long a lambda in a tier that is not kept while unused - the sandbox - may go without a visit or an edit before it is taken offline, in hours")]
    int DeploymentLifetimeHours,
    [Summary("How long such a lambda may go without a visit or an edit before it is deleted, in days")]
    int RetentionDays,
    [Summary("How many features it may have open at once")]
    int Features
);
