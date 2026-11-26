using GenHTTP.Lambda.Services.Background;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Looks for runs nobody follows - their server went away without letting
/// go - and follows them here.
/// </summary>
/// <remarks>
/// Every server of an installation runs it; the claim in the records decides
/// which one follows a run (<see cref="AgentFollower"/>). A run this server
/// follows already is left as it is.
/// </remarks>
public sealed class AgentFollowJob(AgentFollower follower) : IBackgroundJob
{

    public string Name => "agent followers";

    public TimeSpan Interval => TimeSpan.FromMinutes(1);

    public ValueTask ExecuteAsync(CancellationToken cancellation) => follower.ResumeAsync(cancellation);

}
