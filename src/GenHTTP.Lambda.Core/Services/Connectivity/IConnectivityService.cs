namespace GenHTTP.Lambda.Services.Connectivity;

/// <summary>
/// Whether a lambda may connect to other servers - asked for by its owner,
/// decided by the operator.
/// </summary>
/// <remarks>
/// The asymmetry of the platform once more: an owner, or their agent, says
/// what the lambda connects to and why, and the operator - who answers for
/// what the server does to others - decides. A premium lambda may without
/// asking (see <see cref="ConnectivityRule"/>).
///
/// Nothing here deploys or compiles anything. What a lambda is compiled with
/// includes whether it may connect, so the next request after a decision
/// builds it again under the new one - and code that connects no longer
/// compiles where it may not.
/// </remarks>
public interface IConnectivityService
{

    /// <summary>
    /// Where the connectivity of the lambda behind an editor key stands.
    /// </summary>
    ConnectivityInfo Get(string privateKey);

    /// <summary>
    /// Asks the operator to let the lambda connect to other servers.
    /// </summary>
    /// <remarks>
    /// Asked again while it waits, the purpose is replaced and it keeps its
    /// place; asked after a decline, it waits again. Nothing is asked for a
    /// lambda that may already connect - its tier includes it, or it was
    /// allowed. Refused for a demo and a blocked lambda, like every change.
    /// </remarks>
    /// <param name="purpose">What it connects to and why, in the owner's words - required</param>
    ConnectivityInfo Request(string privateKey, string? purpose);

    /// <summary>
    /// Takes the request back while it waits, or gives up what was allowed -
    /// code that connects then no longer compiles. Nothing happens when
    /// nothing was asked or it was declined.
    /// </summary>
    ConnectivityInfo Withdraw(string privateKey);

    /// <summary>
    /// The operator's decision: allow the lambda to connect, asked for or not,
    /// or decline the request - which takes back what was allowed.
    /// </summary>
    /// <remarks>
    /// Declining what is neither asked for nor allowed is refused: there is
    /// nothing to decline. Refused for a demo, which never connects, and for a
    /// blocked lambda, of which nothing changes but its block.
    /// </remarks>
    /// <param name="note">What the operator says about it, which the owner reads</param>
    ConnectivityInfo Decide(string privateKey, bool allow, string? note);

    /// <summary>
    /// The lambdas whose request waits for the operator, the longest waiting
    /// first, and then the ones allowed, the most recently allowed first.
    /// </summary>
    IReadOnlyList<ConnectivityListing> List();

    /// <summary>
    /// Whether the code of the version online, or of the newest, connects to
    /// other servers - as far as what is written tells, read off each version
    /// once.
    /// </summary>
    /// <param name="live">The version online, if one is</param>
    /// <param name="latest">The newest version, if there is one</param>
    bool Connects(long lambdaId, int? live, int? latest);

}
