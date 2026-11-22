namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// The models the build agent may run on.
/// </summary>
/// <remarks>
/// Which of them an installation offers, and to whom, is its operator's
/// (<see cref="AgentSettings"/>); which model of Anthropic's each runs as is
/// the runner's, which maps the names to identifiers of its own.
/// </remarks>
public enum AgentModel
{
    Opus,
    Sonnet,
    Haiku,
    Fable
}
