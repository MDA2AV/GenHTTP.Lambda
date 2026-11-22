namespace GenHTTP.Lambda.Configuration;

/// <summary>
/// What an installation sets about one tier: how much a lambda in it may
/// have, whether it may connect to other servers by default, and what the
/// tier is called on its pages.
/// </summary>
/// <remarks>
/// The rest of what a tier means - whether it is kept while nobody uses it,
/// whether it may answer at a domain of its own, whether it is the
/// installation's own - is its purpose, and fixed (<c>TierPolicies</c>).
/// These are defaults in <see cref="LambdaOptions"/> and what the operator
/// changes in the administration panel.
/// </remarks>
/// <param name="BuildBytes">How large a version may be: its code and its resources together</param>
/// <param name="DataBytes">The room its data may take: its database and its workspace together</param>
/// <param name="AnalyticsDays">
/// How many days of its analytics its owner is shown. A year is counted of every lambda whatever its tier, so a lambda
/// moved to a tier that shows more shows what was counted before as well.
/// </param>
/// <param name="Connectivity">
/// Whether a lambda in it may connect to other servers unless the operator decided otherwise about it
/// </param>
/// <param name="Label">
/// What the tier is called on the installation's pages, shown as written and in no other language. Nothing to call it
/// by its own name, which the pages translate. The API and the code use the tier's
/// name only.
/// </param>
public sealed record TierSettings(long BuildBytes, long DataBytes, int AnalyticsDays, bool Connectivity, string? Label = null)
{

    /// <summary>
    /// How long a label may be.
    /// </summary>
    public const int MaxLabel = 24;

}
