namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// One place a run sends something to.
/// </summary>
/// <param name="Name">Whose it is, such as a company or a service</param>
/// <param name="Address">Where it is, such as a host name - or how it is chosen, such as "the sites a request names"</param>
/// <param name="What">What goes there, in a sentence</param>
public sealed record AgentDestination(string Name, string Address, string What);
