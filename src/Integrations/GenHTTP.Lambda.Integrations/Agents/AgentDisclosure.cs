namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// What an agent is, who runs it, and where it sends what it is given - what
/// an operator states in the privacy statement of the installation.
/// </summary>
/// <remarks>
/// Shown in the operator's panel beside the agent's settings, in English. It
/// names services and addresses, never a credential.
/// </remarks>
/// <param name="Agent">What the agent is, such as the program it runs</param>
/// <param name="Vendor">Who provides the model or the service behind it, where somebody does</param>
/// <param name="Destinations">Everywhere a run sends something of what it is given or reads; none for an agent that keeps everything on the installation's own machines</param>
public sealed record AgentDisclosure(string Agent, string? Vendor, IReadOnlyList<AgentDestination> Destinations);
