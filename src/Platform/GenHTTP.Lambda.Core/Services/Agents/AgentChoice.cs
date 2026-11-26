using GenHTTP.Lambda.Integrations.Agents;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// What a run runs on, as the caller may have it.
/// </summary>
/// <param name="Options">The value of every option of the integration's, by option name</param>
/// <param name="Guarded">Whether a value behind the password was chosen - and the password checked</param>
/// <param name="Limits">What the run may take</param>
public sealed record AgentChoice(IReadOnlyDictionary<string, string> Options, bool Guarded, AgentLimits Limits);
