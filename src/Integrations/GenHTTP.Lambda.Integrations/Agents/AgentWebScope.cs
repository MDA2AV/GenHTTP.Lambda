namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// Whose web pages an agent reads.
/// </summary>
public enum AgentWebScope
{

    /// <summary>None: it reaches nothing but the platform.</summary>
    None,

    /// <summary>Only those of the sites the person named in what they wrote.</summary>
    NamedSites,

    /// <summary>Those of any public site.</summary>
    Any

}
