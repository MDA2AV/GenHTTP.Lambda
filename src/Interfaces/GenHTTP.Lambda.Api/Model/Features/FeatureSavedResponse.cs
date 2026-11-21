using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A feature that was just saved.")]
public sealed record FeatureSavedResponse(
    [Summary("The feature afterwards")]
    FeatureResponse Feature,
    [Summary("What came of putting its preview online, where that was asked for (`?deploy=true`); absent otherwise")]
    FeaturePreviewResponse? Preview
);
