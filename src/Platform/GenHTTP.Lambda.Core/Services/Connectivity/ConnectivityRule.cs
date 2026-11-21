using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Connectivity;

/// <summary>
/// Whether a lambda may connect to other servers: the operator allowed it,
/// or its tier does by default and the operator did not decline it.
/// </summary>
/// <remarks>
/// One rule for every place that asks - what a lambda is compiled with, what
/// the editor shows and what an agent is told - so they cannot come to
/// different answers. A grant of the lambda's, not a property of its tier:
/// the tier only says what holds while nothing was decided about the lambda
/// (<see cref="TierPolicy.Connectivity"/>), which each installation sets -
/// production by default, since a lambda there is one the operator knows; a
/// sandbox asks.
/// </remarks>
public static class ConnectivityRule
{

    /// <summary>
    /// How long what the owner says the lambda connects to may be.
    /// </summary>
    public const int MaxPurpose = 1000;

    /// <summary>
    /// How long what the operator says deciding it may be.
    /// </summary>
    public const int MaxNote = 500;

    /// <summary>
    /// Whether a lambda of this tier, with its request standing so, may connect.
    /// </summary>
    /// <remarks>
    /// What the operator decided about the lambda comes first, either way.
    /// A request that waits connects nothing until it is allowed - in a tier
    /// that connects by default, an owner only asks after a decline, which
    /// asking again does not undo - and moving a lambda to such a tier
    /// forgets a request it no longer needs (<c>LambdaHosting.ChangeTier</c>).
    /// </remarks>
    public static bool Allows(TierPolicy policy, ConnectivityStatus? status) => status switch
    {
        ConnectivityStatus.Allowed => true,
        ConnectivityStatus.Declined or ConnectivityStatus.Requested => false,
        _ => Includes(policy)
    };

    /// <summary>
    /// Whether the tier allows it while nothing was decided about the lambda.
    /// </summary>
    public static bool Includes(TierPolicy policy) => policy.Connectivity;

    /// <summary>
    /// What may be said about a lambda's connectivity, to whoever may read the lambda.
    /// </summary>
    public static ConnectivityInfo Describe(LambdaEntity lambda, TierPolicy policy)
        => new(Allows(policy, lambda.Connectivity), Includes(policy), lambda.Connectivity, lambda.ConnectivityChanged, lambda.ConnectivityPurpose, lambda.ConnectivityNote);

}
