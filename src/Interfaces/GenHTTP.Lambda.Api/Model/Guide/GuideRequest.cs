using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The section the operator writes into the guide.")]
public sealed record GuideRequest(
    [Summary("The heading agents find it under: one line of at most 60 characters, and none of the platform's own sections")]
    string? Title,
    [Summary("What agents are to do and not to do here, as Markdown: at most 6 KB in UTF-8, with headings of the third level (`###`) and below")]
    string? Text
);
