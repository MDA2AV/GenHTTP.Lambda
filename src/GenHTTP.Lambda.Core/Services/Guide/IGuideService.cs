namespace GenHTTP.Lambda.Services.Guide;

/// <summary>
/// The guide agents read first over MCP (<c>platform_guide</c>), and the
/// section its operator writes into it for the agents that build there.
/// </summary>
/// <remarks>
/// Both doors take it from here - MCP hands it to agents, the operator's
/// routes show it in the panel - so neither reaches into the other. The
/// section is only there where the installation has an operator: without an
/// admin token nobody could see or remove it.
/// </remarks>
public interface IGuideService
{

    /// <summary>
    /// The operator's section, if there is one.
    /// </summary>
    GuideSection? Section { get; }

    /// <summary>
    /// The guide as an agent reads it now: Markdown, the operator's section
    /// last.
    /// </summary>
    /// <param name="origin">The address the caller reached the platform by, where the installation names no public one</param>
    string Write(string origin);

    /// <summary>
    /// Writes the operator's section, or replaces it.
    /// </summary>
    /// <returns>The section as it is kept</returns>
    GuideSection Change(string? title, string? text);

    /// <summary>
    /// Takes the operator's section out of the guide.
    /// </summary>
    /// <returns>The section that was there, or nothing where there was none</returns>
    GuideSection? Remove();

}
