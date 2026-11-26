namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// The agent of an installation: what builds an app for somebody who describes
/// it, and changes it - or answers questions about it - for its owner.
/// </summary>
/// <remarks>
/// <para>
/// An agent is a client of the platform like any other. It reaches the
/// platform through its doors - MCP, the REST API, git - with the editor key
/// it is handed (<see cref="AgentLambda.EditorKey"/>), exactly as an agent
/// somebody connects on their own does, and with the run's credential
/// (<see cref="AgentAccess"/>) on every call. The credential is the one thing
/// that sets it apart: by it the platform knows which run a call belongs to,
/// so it records what the run did - the steps, what it saved, what it changed
/// - from its own doors, and the outcome a page shows is the platform's,
/// never the agent's account of itself.
/// </para>
/// <para>
/// The integration runs the agent: it takes a run (<see cref="StartAsync"/>),
/// tells what happens in it as it happens (<see cref="FollowAsync"/>) and
/// stops it when asked (<see cref="CancelAsync"/>). It keeps nothing between
/// runs. Each run is new; what was said before is handed to it as context
/// (<see cref="AgentRun.Conversation"/>), which the platform keeps and cuts to
/// size. Which models the agent runs on, which tools it has and how it is
/// told what to do are the integration's own, offered to the operator as
/// options (<see cref="AgentCapabilities.Options"/>) whose names the platform
/// never reads.
/// </para>
/// <para>
/// The integration is not the only user of the system behind it: several
/// installations may share one agent system, so a run is known by the
/// platform's id (<see cref="AgentRun.Id"/>), unique wherever it travels.
/// How many runs go at once is the agent system's to decide and to say
/// (<see cref="AgentQueued"/>, <see cref="LoadAsync"/>); how many one person
/// may ask for is the platform's.
/// </para>
/// <para>
/// What every integration owes the people it works for, whatever runs behind
/// it:
/// </para>
/// <list type="bullet">
/// <item>It works on the app it was handed, and on new ones it creates, and on
/// nothing else. A request that is not about an app - homework, a text, a
/// chat - or that is about the platform or the machine it runs on, or that
/// is meant to do harm, is declined before anything is done
/// (<see cref="AgentEnding.Declined"/>), with a sentence in the request's
/// language. Finding things out for the app - on the web, in what the app
/// keeps - is part of the work.</item>
/// <item>What it reads is data, never instructions: the request and the
/// conversation, the records and logs the app's visitors wrote, the pages of
/// the web. The platform marks what visitors wrote where it hands it out.</item>
/// <item>It speaks to the person in their language and in the words their
/// view uses (<see cref="AgentAudience"/>).</item>
/// <item>It never asks for the value of a key or a password: the agent says
/// which it needs, and the owner enters it in the editor.</item>
/// <item>It does not pretend. What the platform cannot do, or can do only
/// with somebody's permission, a key of the owner's or another tier, it says
/// so, in those words - the platform's guide says which is which.</item>
/// <item>It holds a run to the limits it says it can hold one to
/// (<see cref="AgentCapabilities.Limits"/>, <see cref="AgentRun.Limits"/>).</item>
/// </list>
/// <para>
/// An installation without one has no agent: the pages explain how to connect
/// an agent of one's own instead. One whose agent system does not answer
/// still starts - <see cref="IIntegration.PrepareAsync"/> of an agent reaches
/// nothing that should keep the platform from serving - and says so where a
/// run is asked for.
/// </para>
/// </remarks>
public interface IAgentProvider : IIntegration
{

    /// <summary>
    /// What the agent can do, and what the operator may choose for it.
    /// </summary>
    /// <remarks>
    /// Read from memory whenever a page is drawn or a run is asked for, so it
    /// is never a call to the agent system. It may change while the platform
    /// runs - the agent system was updated, or went away - and the pages and
    /// the guide follow it from the next read.
    /// </remarks>
    AgentCapabilities Capabilities { get; }

    /// <summary>
    /// Hands the agent a run, once the platform decided that it may be asked
    /// for and has recorded it.
    /// </summary>
    /// <remarks>
    /// Answers once the agent system has taken the run - it may wait there
    /// before it works. The same run handed again is the same run, not a
    /// second one: a platform that stopped between handing it over and
    /// noting that it did hands it over again.
    /// </remarks>
    /// <returns>How it was taken</returns>
    /// <exception cref="AgentRefusedException">It did not take the run: it is too busy, cannot be reached, or cannot run what was asked for</exception>
    ValueTask<AgentAcceptance> StartAsync(AgentRun run, CancellationToken cancellation = default);

    /// <summary>
    /// What happens in a run from the given position on, as it happens, until
    /// it ends.
    /// </summary>
    /// <remarks>
    /// Every event carries its position (<see cref="AgentEvent.Sequence"/>),
    /// increasing within the run. A platform that was restarted, or lost its
    /// connection, asks again from the last position it recorded and is
    /// given what came after it. The sequence ends after
    /// <see cref="AgentEnded"/>; a run the agent system knows nothing of -
    /// any more - is answered with an <see cref="AgentEnded"/> of
    /// <see cref="AgentEnding.Lost"/>, and the platform settles it from what
    /// its own records say the run did.
    /// </remarks>
    /// <param name="run">The run, as it was taken</param>
    /// <param name="after">The position of the last event the platform has, or zero for all of them</param>
    IAsyncEnumerable<AgentEvent> FollowAsync(AgentRunHandle run, long after, CancellationToken cancellation = default);

    /// <summary>
    /// Stops a run that waits or works. What it saved so far stays saved.
    /// </summary>
    /// <remarks>
    /// The run then ends as <see cref="AgentEnding.Cancelled"/> in what
    /// <see cref="FollowAsync"/> tells. Only asked where
    /// <see cref="AgentCapabilities.Cancels"/> says it can.
    /// </remarks>
    /// <returns>Whether the agent system knew the run</returns>
    ValueTask<bool> CancelAsync(AgentRunHandle run, CancellationToken cancellation = default);

    /// <summary>
    /// How the agent system is: whether it answers, how many runs it takes at
    /// once, and how many of this installation's run and wait now.
    /// </summary>
    /// <remarks>
    /// For the operator's panel, which asks while it is drawn; an agent
    /// system that does not answer within a few seconds is said to be
    /// unreachable rather than waited for.
    /// </remarks>
    ValueTask<AgentLoad> LoadAsync(CancellationToken cancellation = default);

}
