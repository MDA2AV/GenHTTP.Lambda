using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Api.Model;

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
