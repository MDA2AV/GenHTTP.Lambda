using System.Text;

using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Guide;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The guide agents read first over MCP, and the operator's section of it.")]
public sealed record GuideResponse(
    [Summary("The guide as an agent reads it from `platform_guide` right now: Markdown, the operator's section last")]
    string Guide,
    [Summary("How large the guide is in UTF-8, in bytes - what an agent's client is handed with every call")]
    int Bytes,
    [Summary("The operator's section; absent while there is none")]
    GuideSectionResponse? Section,
    [Summary("What the operator's section may be")]
    GuideLimitsResponse Limits
)
{

    internal static GuideResponse Of(string guide, GuideSection? section)
        => new(guide, Encoding.UTF8.GetByteCount(guide), section is null ? null : GuideSectionResponse.Of(section), GuideLimitsResponse.Fixed);

}

[Summary("The section the operator wrote into the guide, for the agents that build on the installation.")]
public sealed record GuideSectionResponse(
    [Summary("Its title: the heading agents find it under")]
    string Title,
    [Summary("Its text, Markdown, as kept: trimmed, with line breaks as `\\n`")]
    string Text,
    [Summary("How large the text is in UTF-8, in bytes")]
    int Bytes
)
{

    internal static GuideSectionResponse Of(GuideSection section) => new(section.Title, section.Text, section.Bytes);

}

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

[Summary("The section the operator writes into the guide.")]
public sealed record GuideRequest(
    [Summary("The heading agents find it under: one line of at most 60 characters, and none of the platform's own sections")]
    string? Title,
    [Summary("What agents are to do and not to do here, as Markdown: at most 6 KB in UTF-8, with headings of the third level (`###`) and below")]
    string? Text
);
