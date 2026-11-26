using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Where the platform writes down what a run of its agent did, as it does it:
/// the steps its calls took at the doors, and what they changed in the
/// services.
/// </summary>
/// <remarks>
/// <para>
/// The agent is a client like any other, so the platform sees what a run did
/// from its own side: every call carries the run's credential, the door it
/// comes through resolves it to an author (<see cref="VersionAuthor"/>), and
/// whatever the call changes is recorded here by the service that changes
/// it - whichever door it came through. A call with no run behind it costs
/// nothing here.
/// </para>
/// <para>
/// What a page says a run did - what it saved, put online, switched on,
/// what waits - and what undoing it does are read from what this holds.
/// </para>
/// </remarks>
public interface IAgentJournal
{

    /// <summary>
    /// Records something a run changed, where the author is a run.
    /// </summary>
    ValueTask RecordAsync(VersionAuthor? author, AgentEffect effect, CancellationToken cancellation = default);

    /// <summary>
    /// Records a step a run's call begins at a door, where the author is a
    /// run, and says what to complete it by.
    /// </summary>
    ValueTask<int?> BeginAsync(VersionAuthor? author, AgentStep step, CancellationToken cancellation = default);

    /// <summary>
    /// Completes a step once the call is done, with how it went.
    /// </summary>
    ValueTask CompleteAsync(VersionAuthor? author, int? number, AgentStep step, CancellationToken cancellation = default);

    /// <summary>
    /// Lets go of what is kept of a lambda's runs - its files - before the
    /// lambda is removed, which takes its runs with it.
    /// </summary>
    ValueTask ForgetAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// Marks the call at hand as one the door records as a step of its own -
    /// the MCP door does - for as long as it is not disposed of; what calls
    /// through other doors change is recorded as steps from their effects.
    /// </summary>
    IDisposable Step();

}
