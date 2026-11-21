using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Guide;

namespace GenHTTP.Lambda.Api.Model;

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
