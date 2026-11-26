namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// What an agent does beyond the platform, in a set every page has words for.
/// </summary>
public enum AgentActivityKind
{

    /// <summary>It reads a web page; the subject is the site.</summary>
    ReadsPage = 0,

    /// <summary>It searches the web.</summary>
    Searches = 1,

    /// <summary>Anything else; the subject names the tool, which a page shows as it is.</summary>
    Other = 2

}
