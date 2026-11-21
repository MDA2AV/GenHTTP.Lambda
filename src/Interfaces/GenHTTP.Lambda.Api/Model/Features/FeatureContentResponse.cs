using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A feature with its files.")]
public sealed record FeatureContentResponse(
    [Summary("The feature")]
    FeatureResponse Feature,
    [Summary("Its files, `lambda.cs` first - all of them, or those of the folder asked for")]
    IReadOnlyList<LambdaFile> Files
);
