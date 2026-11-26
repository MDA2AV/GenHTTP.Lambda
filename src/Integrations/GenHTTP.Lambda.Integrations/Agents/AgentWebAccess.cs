namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// What of the web an agent reads, beyond the platform.
/// </summary>
/// <remarks>
/// Optional: an agent that reaches nothing but the platform is as valid as
/// one that searches the web, and the pages promise neither where it is not
/// so.
/// </remarks>
/// <param name="Reads">Whose pages it reads</param>
/// <param name="Searches">Whether it searches the web</param>
/// <param name="Policy">How it reads them, in a sentence for the operator - the methods, the addresses it refuses</param>
public sealed record AgentWebAccess(AgentWebScope Reads, bool Searches, string? Policy = null)
{

    /// <summary>
    /// An agent that reaches nothing beyond the platform.
    /// </summary>
    public static readonly AgentWebAccess None = new(AgentWebScope.None, false);

}
