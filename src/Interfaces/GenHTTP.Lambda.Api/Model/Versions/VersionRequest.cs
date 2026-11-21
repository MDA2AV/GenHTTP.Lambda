using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Every file of a lambda, to be saved as its next version, and why it was written.")]
public sealed record VersionRequest(
    [Summary("Every file of the lambda, `lambda.cs` first")]
    IReadOnlyList<LambdaFile>? Files,
    [Summary("What the user wants from this version and why - their requirements, in their own words where possible. Optional, and cut past 4000 characters.")]
    string? Specification = null,
    [Summary("What this version changes, in one line for the owner. Optional, and cut past 500 characters.")]
    string? Change = null
);
