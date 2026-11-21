using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What a tier lets a lambda have and how it keeps it, as this installation set it.")]
public sealed record TierResponse(
    [Summary("`Sandbox` for trying something, `Production` for an app people use, `System` for what the installation itself depends on, or `Demo` for one of its demos")]
    string Tier,
    [Summary("What this installation calls the tier on its pages, shown as written - `Premium` for production on genhttp.dev; absent where it goes by its own name")]
    string? Label,
    [Summary("What a version may come to, its code and its resources together, in bytes")]
    long VersionBytes,
    [Summary("The room its data may take, its database and its workspace together, in bytes")]
    long DataBytes,
    [Summary("How many days of its analytics its owner is shown, of the year that is kept")]
    int AnalyticsDays,
    [Summary("Whether a lambda in it may connect to other servers while the operator decided nothing about it")]
    bool Connectivity,
    [Summary("Whether a lambda in it may answer at a domain of its own")]
    bool OwnDomain,
    [Summary("Whether a lambda in it is taken offline once nobody visits or edits it, and deleted later; otherwise it is kept")]
    bool ExpiresWhenUnused,
    [Summary("Whether it is the installation's own: never blocked, never in the showcase, and not deleted with the editor key")]
    bool Installation,
    [Summary("The tier an app in it would be moved to for more, which only the operator does; absent where there is none")]
    string? Above,
    [Summary("What the installation calls that tier; absent where it goes by its own name, or there is none")]
    string? AboveLabel
);

/// <summary>
/// Describes a tier for the API.
/// </summary>
/// <remarks>
/// The tier is named by its identifier only; the label is a word for the
/// pages, and never what a client sends back.
/// </remarks>
public static class TierDescription
{

    public static TierResponse Of(TierPolicy policy, TierPolicy? above) => new(
        policy.Tier.ToString(),
        policy.Label,
        policy.MaxVersionBytes,
        policy.MaxDataBytes,
        policy.AnalyticsDays,
        policy.Connectivity,
        policy.OwnDomain,
        policy.ExpiresWhenUnused,
        policy.Installation,
        above?.Tier.ToString(),
        above?.Label
    );

    public static TierResponse Of(ILimitsService limits, LambdaTier tier) => Of(limits.PolicyOf(tier), limits.Above(tier));

    /// <summary>
    /// Reads the tier a lambda is to be moved to, in any case.
    /// </summary>
    /// <remarks>
    /// <c>Free</c> and <c>Premium</c>, what the sandbox and production were
    /// called before, are read as them still, so a script written against an
    /// earlier release keeps working; nothing answers with them.
    /// </remarks>
    public static LambdaTier Parse(string tier)
    {
        var name = tier.Trim();

        if (name.Equals("Free", StringComparison.OrdinalIgnoreCase))
        {
            return LambdaTier.Sandbox;
        }

        if (name.Equals("Premium", StringComparison.OrdinalIgnoreCase))
        {
            return LambdaTier.Production;
        }

        return Enum.TryParse<LambdaTier>(name, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed) && !int.TryParse(name, out _)
            ? parsed
            : throw LambdaException.Invalid($"There is no tier called '{tier}'. There are: {string.Join(", ", Enum.GetNames<LambdaTier>())}.");
    }

}
