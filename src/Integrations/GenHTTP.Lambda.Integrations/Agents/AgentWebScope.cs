namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// Whose web pages an agent reads.
/// </summary>
public enum AgentWebScope
{

    /// <summary>None: it reaches nothing but the platform.</summary>
    None = 0,

    /// <summary>Only those of the sites the person named in what they wrote.</summary>
    NamedSites = 1,

    /// <summary>Those of any public site.</summary>
    Any = 2

}
