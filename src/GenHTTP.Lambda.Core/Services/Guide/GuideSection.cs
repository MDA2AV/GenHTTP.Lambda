using System.Text;

namespace GenHTTP.Lambda.Services.Guide;

/// <summary>
/// What the operator of an installation wrote into the guide for the agents
/// that build there: its rules, under a heading of their own.
/// </summary>
/// <param name="Title">The heading agents find it under</param>
/// <param name="Text">Markdown, with headings of the third level and below</param>
public sealed record GuideSection(string Title, string Text)
{

    /// <summary>
    /// How large the text is, as an agent's client is handed it.
    /// </summary>
    public int Bytes => Encoding.UTF8.GetByteCount(Text);

}
