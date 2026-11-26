using System.Net;

using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// The installation's agent, as the doors use it: a build from the box on
/// /build, a message about a lambda from its owner, and what came of each -
/// kept by the platform, whatever agent integration runs them.
/// </summary>
public interface IAgentService
{

    /// <summary>
    /// The header the calls of a run carry its credential in, by which what
    /// they do is known to be the run's.
    /// </summary>
    const string CredentialHeader = "X-Lambda-Agent";

    #region What there is

    /// <summary>
    /// Whether this installation has an agent.
    /// </summary>
    bool Available { get; }

    /// <summary>
    /// What the installation's agent can do, or nothing where it has none.
    /// </summary>
    AgentCapabilities? Capabilities { get; }

    /// <summary>
    /// How many builds and messages one address is allowed in a day.
    /// </summary>
    int PerDay { get; }

    /// <summary>
    /// What a page offers beside the box: the options as the operator offers
    /// them, the files a message may come with, and what the agent does.
    /// </summary>
    AgentOffer Offer();

    /// <summary>
    /// Whether the box on /build is offered: there is an agent, and the operator has not switched it off.
    /// </summary>
    bool BuildsOffered();

    /// <summary>
    /// Whether owners may write to the agent about their apps: there is an agent, and the operator has not switched it off.
    /// </summary>
    bool ChangesOffered();

    #endregion

    #region Who calls

    /// <summary>
    /// Who a call carrying the given credential is: a run of the agent, on
    /// its options - or nobody known.
    /// </summary>
    ValueTask<VersionAuthor?> AuthorOfAsync(string? credential, CancellationToken cancellation = default);

    #endregion

    #region The operator's

    /// <summary>
    /// What the operator decided about the agent.
    /// </summary>
    AgentSettings GetSettings();

    /// <summary>
    /// Changes what the operator decided, checked against what the agent offers.
    /// </summary>
    /// <returns>The settings, as they were saved</returns>
    ValueTask<AgentSettings> ChangeSettingsAsync(AgentSettingsChange change, CancellationToken cancellation = default);

    /// <summary>
    /// What a run on the given values would be held to, as the operator set it.
    /// </summary>
    AgentLimits LimitsOf(bool guarded);

    /// <summary>
    /// Whether the installation has a password for the values behind one.
    /// </summary>
    bool PasswordSet { get; }

    /// <summary>
    /// What the agent system says of itself, or nothing where there is none.
    /// </summary>
    ValueTask<AgentSystem?> SystemAsync(CancellationToken cancellation = default);

    /// <summary>
    /// How many runs ran on each value of each option, and for how long, over
    /// the given days - never what they were about.
    /// </summary>
    ValueTask<AgentUsageSummary> UsageAsync(int days, CancellationToken cancellation = default);

    /// <summary>
    /// The newest runs of the installation, for the operator.
    /// </summary>
    ValueTask<IReadOnlyList<RunListing>> NewestAsync(int take, CancellationToken cancellation = default);

    #endregion

    #region Files

    /// <summary>
    /// Keeps a file for a message to come, if the agent takes such files.
    /// </summary>
    ValueTask<RunFile> AttachAsync(string? name, string? type, ReadOnlyMemory<byte> content, CancellationToken cancellation = default);

    /// <summary>
    /// A file that travelled with a run, and what it holds - or nothing.
    /// </summary>
    ValueTask<(RunFile File, byte[] Content)?> ReadFileAsync(string? id, CancellationToken cancellation = default);

    /// <summary>
    /// A file that travelled with a run of a lambda, and what it holds - or
    /// nothing where it is no file of that lambda's runs.
    /// </summary>
    ValueTask<(RunFile File, byte[] Content)?> ReadFileAsync(long lambda, string? id, CancellationToken cancellation = default);

    /// <summary>
    /// The files attached to messages about a lambda, the newest first.
    /// </summary>
    ValueTask<IReadOnlyList<RunFile>> AttachedToAsync(long lambda, CancellationToken cancellation = default);

    #endregion

    #region Builds

    /// <summary>
    /// Starts a build, if this caller has any left today.
    /// </summary>
    ValueTask<BuildStarted> StartAsync(AgentAsk ask, IPAddress? caller);

    /// <summary>
    /// How a build is getting on, for anybody holding its id.
    /// </summary>
    ValueTask<BuildProgress> ProgressAsync(string id);

    /// <summary>
    /// A build as it is now, each time it changes, until it ended - and
    /// nothing every so often while nothing changes, which a listener answers
    /// to keep its connection.
    /// </summary>
    IAsyncEnumerable<BuildProgress?> FollowBuildAsync(string id, CancellationToken cancellation = default);

    #endregion

    #region Messages about a lambda

    /// <summary>
    /// What the Change section of a lambda shows: whether the agent can be
    /// asked, how many this caller has left, and the run under way or the last one.
    /// </summary>
    ValueTask<AgentState> StateAsync(long lambda, IPAddress? caller);

    /// <summary>
    /// The state of a lambda's runs each time it changes, while somebody
    /// listens - and nothing every so often while nothing changes.
    /// </summary>
    IAsyncEnumerable<AgentState?> FollowAsync(long lambda, IPAddress? caller, CancellationToken cancellation = default);

    /// <summary>
    /// Hands the agent a message about a lambda - a question, a setting, a
    /// change - if this caller has any left today and nothing else is under
    /// way for the lambda.
    /// </summary>
    /// <param name="lambda">The lambda, by the id its runs are filed under</param>
    /// <param name="privateKey">The editor key, which the agent needs to change anything</param>
    /// <param name="before">The version online now, which undoing the run puts back</param>
    ValueTask<AgentState> ChangeAsync(long lambda, string privateKey, int? before, AgentAsk ask, IPAddress? caller);

    /// <summary>
    /// Stops the run of a lambda that is under way. Whatever it saved so far
    /// stays saved.
    /// </summary>
    ValueTask<AgentState> StopAsync(long lambda, IPAddress? caller);

    /// <summary>
    /// Undoes what a run did to its lambda, where that can be undone.
    /// </summary>
    /// <param name="run">The run, or nothing for the newest one</param>
    ValueTask<UndoOutcome> UndoAsync(long lambda, string privateKey, string? run, CancellationToken cancellation = default);

    #endregion

}
