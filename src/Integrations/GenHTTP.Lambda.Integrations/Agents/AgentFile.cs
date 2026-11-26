namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// A file the agent hands the person.
/// </summary>
/// <param name="Name">What to call it, with its extension</param>
/// <param name="MediaType">What kind of file it is</param>
/// <param name="Content">What it holds</param>
/// <param name="Description">A line about it for the person, in their language</param>
public sealed record AgentFile(string Name, string MediaType, ReadOnlyMemory<byte> Content, string? Description = null);
