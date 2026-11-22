namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// The models the build agent may run on.
/// </summary>
/// <remarks>
/// Which of them an installation offers, and to whom, is its operator's
/// (<see cref="AgentSettings"/>); which model of Anthropic's each runs as is
/// the runner's, which maps the names to identifiers of its own. Stored as
/// its number with what the agent wrote (see <c>VersionAuthor</c>), so a
/// member keeps its number for good; named by <see cref="AgentCatalog"/>.
/// </remarks>
public enum AgentModel
{
    Opus = 0,
    Sonnet = 1,
    Haiku = 2,
    Fable = 3
}
