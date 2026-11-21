using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <summary>
/// What an installation says about itself once a day: how it is set up and
/// how much it is used, never what its lambdas are or hold.
/// </summary>
/// <remarks>
/// Counts and switches only. Nothing in here names a lambda, an owner or a
/// visitor - no key, no domain, no title, no code, no record, no address -
/// and nothing that is not about the installation as a whole, apart from the
/// size of the largest lambda's data. Not even the address the installation
/// is reached at: it names whoever runs it, which is more than a count of how
/// installations are run needs (decided by the owner).
///
/// This is the wire shape: the receiver reads it by its field names, so a
/// field is added rather than renamed, and <see cref="Schema"/> goes up when
/// one changes its meaning.
/// </remarks>
/// <param name="Schema">The version of this shape</param>
/// <param name="Installation">A random identifier made once per installation and kept in its database, so the reports of one installation can be told from another's</param>
/// <param name="Version">The version of GenHTTP Lambda running</param>
/// <param name="Sent">When the report was made</param>
/// <param name="System">The machine it runs on</param>
/// <param name="Server">How it serves</param>
/// <param name="Configuration">What the operator switched on or off</param>
/// <param name="Lambdas">How many lambdas there are, and in which state</param>
/// <param name="Data">How much room the lambdas take</param>
/// <param name="Switches">How many lambdas use each of the platform's features</param>
/// <param name="Activity">How the lambdas were changed recently</param>
public sealed record UsageReport(
    int Schema,
    string Installation,
    string Version,
    DateTime Sent,
    UsageSystem System,
    UsageServer Server,
    UsageConfiguration Configuration,
    UsageLambdas Lambdas,
    UsageData Data,
    UsageSwitches Switches,
    UsageActivity Activity
)
{

    /// <summary>
    /// The version of the shape this platform sends.
    /// </summary>
    public const int CurrentSchema = 1;

}
