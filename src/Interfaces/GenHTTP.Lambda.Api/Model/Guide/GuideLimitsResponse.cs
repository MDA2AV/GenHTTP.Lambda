using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Guide;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What the operator's section of the guide may be.")]
public sealed record GuideLimitsResponse(
    [Summary("How long the title may be, in characters")]
    int Title,
    [Summary("How large the text may be in UTF-8, in bytes")]
    int TextBytes
)
{

    internal static GuideLimitsResponse Fixed { get; } = new(GuideChecks.MaxTitle, GuideChecks.MaxTextBytes);

}
