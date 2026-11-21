using System.Net;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// Hands a sentence somebody typed to the build agent, and reports back: a
/// build from the box on /build, or a change of a lambda from the editor.
/// </summary>
public interface IBuildService
{

    /// <summary>
    /// Whether this installation has an agent to build with.
    /// </summary>
    bool Available { get; }

    /// <summary>
    /// How many builds and changes one address is allowed in a day.
    /// </summary>
    int PerDay { get; }

    /// <summary>
    /// The models and efforts the pages may offer, with what they start on.
    /// </summary>
    AgentOffer Offer();

    /// <summary>
    /// What the operator decided about the agent: whether it builds and
    /// changes apps here, and on which models and efforts.
    /// </summary>
    AgentSettings GetSettings();

    /// <summary>
    /// Replaces what the operator decided about the agent.
    /// </summary>
    /// <returns>The settings as they were saved</returns>
    ValueTask<AgentSettings> SaveSettingsAsync(AgentSettings settings, CancellationToken cancellation = default);

    /// <summary>
    /// Whether the installation has a password for the models behind one.
    /// </summary>
    bool PasswordSet { get; }

    /// <summary>
    /// What the runner of the agent says of itself, or nothing where this
    /// installation has none.
    /// </summary>
    ValueTask<AgentRunner?> RunnerAsync(CancellationToken cancellation = default);

    /// <summary>
    /// Whether the box on /build is offered: there is an agent, and the operator has not switched it off.
    /// </summary>
    bool BuildsOffered();

    /// <summary>
    /// Whether the box in the Change section is offered: there is an agent, and the operator has not switched it off.
    /// </summary>
    bool ChangesOffered();

    /// <summary>
    /// Starts a build, if this caller has any left today.
    /// </summary>
    /// <param name="model">Which of the offered models to use; one behind the password needs it</param>
    /// <param name="effort">Which of the offered efforts to run at</param>
    /// <param name="password">The password, where a model behind it was asked for</param>
    ValueTask<BuildStarted> StartAsync(string? prompt, string? model, string? effort, string? password, IPAddress? caller);

    /// <summary>
    /// How a build is getting on, for anybody holding its id.
    /// </summary>
    ValueTask<BuildProgress> ProgressAsync(string id);

    /// <summary>
    /// What the Change section of a lambda shows: whether it can ask for a
    /// change, how many it has left, and the change under way or the last one.
    /// </summary>
    /// <param name="lambda">The lambda, by the id it is filed under</param>
    ValueTask<AgentState> StateAsync(long lambda, IPAddress? caller);

    /// <summary>
    /// Asks the agent to change a lambda - or to answer a question about it,
    /// or change one of its settings - if this caller has any left today and
    /// no other change of it is under way.
    /// </summary>
    /// <param name="lambda">The lambda, by the id the agent files the change under</param>
    /// <param name="privateKey">The editor key, which the agent needs to change anything</param>
    /// <param name="before">The version online now, to offer putting it back afterwards</param>
    /// <param name="deploy">Whether to merge the change and put it online once it works</param>
    /// <param name="language">The language of the control center, for the agent to fall back on</param>
    /// <param name="feature">The feature to go on with, by its key; a new one when left out</param>
    /// <param name="history">What was said before, as the page keeps it, for the agent to understand the message by</param>
    ValueTask<AgentState> ChangeAsync(long lambda, string privateKey, int? before, string? prompt, bool deploy,
                                      string? model, string? effort, string? password, string? language, string? feature,
                                      IReadOnlyList<ConversationTurn>? history, IPAddress? caller);

    /// <summary>
    /// Stops the change of a lambda that is under way. Whatever it saved so
    /// far stays saved.
    /// </summary>
    ValueTask<AgentState> StopAsync(long lambda, IPAddress? caller);

}
