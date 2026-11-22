using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// What the operator narrows the listing of the lambdas to, and in which order it comes.
/// </summary>
/// <remarks>
/// Only what the platform's database holds can narrow it: the requests and
/// errors of a lambda are counted in memory, page by page, and a filter on
/// them could not be paged.
/// </remarks>
/// <param name="Search">Only lambdas whose public key or domain contains this</param>
/// <param name="Tier">Only lambdas of this tier</param>
/// <param name="State">Only lambdas in this state</param>
/// <param name="Uses">Only lambdas that use every one of these</param>
/// <param name="View">Only lambdas whose editor opens in this view</param>
/// <param name="CreatedWithin">Only lambdas created in this time, up to now</param>
/// <param name="Demos">
/// Whether the installation's demos are listed - and counted - with the rest;
/// they are where the tier asked for is theirs. Left out otherwise, as every
/// count of the panel leaves them out.
/// </param>
public sealed record LambdaQuery(
    string? Search = null,
    LambdaTier? Tier = null,
    LambdaState? State = null,
    IReadOnlyCollection<LambdaFeature>? Uses = null,
    EditorView? View = null,
    TimeSpan? CreatedWithin = null,
    LambdaOrder Order = LambdaOrder.Newest,
    bool Demos = false
)
{

    /// <summary>
    /// How soon a lambda that goes offline or is removed unless it is used
    /// counts as expiring: soon enough that its owner may not notice in time.
    /// </summary>
    public static TimeSpan ExpiringWithin { get; } = TimeSpan.FromDays(7);

}
