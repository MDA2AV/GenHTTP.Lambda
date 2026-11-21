using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What a showcase entry may hold.")]
public sealed record ShowcaseLimitsResponse(
    [Summary("How many characters the title may have")]
    int Title,
    [Summary("How many characters the description may have")]
    int Description,
    [Summary("How large the picture may be, in bytes, as the operator set it")]
    int ImageBytes,
    [Summary("The media types a picture may have")]
    IReadOnlyList<string> ImageTypes,
    [Summary("How an entry should read")]
    string Tone
);
