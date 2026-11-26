namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// How the agent reaches the platform, and as which run.
/// </summary>
/// <remarks>
/// The agent uses the platform's doors as any client does. Every call it
/// makes - to MCP, to the REST API, as git - carries the credential in the
/// header named here: by it the platform knows which run the call belongs
/// to, records what it did and marks what it saved as this run's, on the
/// options it ran on. The credential grants nothing of its own; the editor
/// key does that. It is a secret all the same: never logged, never shown.
///
/// The addresses are the platform's public ones, or its paths where it has
/// none. An agent system configured with where each installation it serves
/// is may connect there instead, so that what an installation sends cannot
/// point it anywhere else.
/// </remarks>
/// <param name="Mcp">Where the platform's MCP endpoint answers</param>
/// <param name="Api">Where its REST API answers, below which <see cref="AgentAttachment.Path"/> and the OpenAPI description are</param>
/// <param name="CredentialHeader">The header every call carries the credential in</param>
/// <param name="Credential">The run's credential</param>
public sealed record AgentAccess(string Mcp, string Api, string CredentialHeader, string Credential)
{

    /// <summary>
    /// Leaves the credential out, should a record be printed.
    /// </summary>
    public override string ToString() => $"AgentAccess {{ Mcp = {Mcp}, Api = {Api}, CredentialHeader = {CredentialHeader} }}";

}
