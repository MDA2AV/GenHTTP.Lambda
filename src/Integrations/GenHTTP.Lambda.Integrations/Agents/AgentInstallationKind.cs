namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// Whom the installation a run works on is for, which changes what is said
/// and built - never what is allowed.
/// </summary>
public enum AgentInstallationKind
{

    /// <summary>A public platform anybody uses.</summary>
    Public,

    /// <summary>Somebody's own installation, for them and the people around them.</summary>
    Home,

    /// <summary>An organization's, whose colleagues have tools built: no "Made with" line, nothing of the platform's own links.</summary>
    Enterprise

}
