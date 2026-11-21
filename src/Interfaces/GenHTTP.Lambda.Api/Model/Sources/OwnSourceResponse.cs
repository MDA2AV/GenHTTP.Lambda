using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Whether a lambda's source is published, as its owner sees it, and what it can be published under.")]
public sealed record OwnSourceResponse(
    [Summary("What it is published under; absent when it never was")]
    SourceSettingsResponse? Source,
    [Summary("The licenses on offer, the default first")]
    IReadOnlyList<LicenseResponse> Licenses,
    [Summary("The license a source is published under unless another is asked for: `MIT`")]
    string Default,
    [Summary("How many characters the name in the license may have")]
    int MaxAuthor
);
