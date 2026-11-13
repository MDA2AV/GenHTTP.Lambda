using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Egress;

/// <summary>
/// Whether a lambda may connect to other servers: its tier includes it, or
/// the operator allowed it.
/// </summary>
/// <remarks>
/// One rule for every place that asks - what a lambda is compiled with, what
/// the editor shows and what an agent is told - so they cannot come to
/// different answers. Decided by the owner: premium includes it, since a
/// premium lambda is one the operator knows; a free one asks.
/// </remarks>
public static class EgressRule
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
    public static bool Allows(LambdaTier tier, EgressStatus? status) => Includes(tier) || status == EgressStatus.Allowed;

    /// <summary>
    /// Whether the tier includes it, whatever was asked.
    /// </summary>
    public static bool Includes(LambdaTier tier) => tier == LambdaTier.Premium;

    /// <summary>
    /// What may be said about a lambda's egress, to whoever may read the lambda.
    /// </summary>
    public static EgressInfo Describe(LambdaEntity lambda)
        => new(Allows(lambda.Tier, lambda.Egress), Includes(lambda.Tier), lambda.Egress, lambda.EgressChanged, lambda.EgressPurpose, lambda.EgressNote);

}
