namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// The models the build agent ran on before what a run is chosen on became
/// the options of the agent integration.
/// </summary>
/// <remarks>
/// Kept to read what was stored then: the model of what the build agent
/// wrote (the <c>agent_model</c> columns of the versions and drafts), and of
/// the tickets of the jobs under way while an installation was upgraded. A
/// run now records the value of its integration's <c>model</c> option, as
/// text (see <see cref="AgentModelNames"/>). A member keeps its number for
/// good.
/// </remarks>
public enum AgentModel
{
    Opus = 0,
    Sonnet = 1,
    Haiku = 2,
    Fable = 3
}
