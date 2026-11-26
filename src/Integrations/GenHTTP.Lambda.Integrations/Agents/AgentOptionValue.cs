namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// One value of an option.
/// </summary>
/// <param name="Name">What it is called on the wire and in the settings, kept for good: a run records it, and what it wrote is marked with it</param>
/// <param name="Label">What the pages call it, in English - with a version where it has one</param>
/// <param name="Description">A line about it for the operator, where the label does not say enough</param>
/// <param name="Guarded">Whether it is offered only to whoever knows the installation's password until the operator decides otherwise - a dear model, say</param>
public sealed record AgentOptionValue(string Name, string Label, string? Description = null, bool Guarded = false);
