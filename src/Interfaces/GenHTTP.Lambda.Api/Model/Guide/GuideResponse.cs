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
