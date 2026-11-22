using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Cloud;

/// <summary>
/// What genhttp.dev is as an installation, beside what its environment says:
/// its terms, whom to write to, where its legal pages are, and that it is the
/// cloud - a public platform, which its operator does not choose otherwise.
/// </summary>
/// <remarks>
/// The installation's own, rather than the platform's: they describe how this
/// installation is run - a shared machine, free lambdas that are taken offline
/// when unused, a company to write to - which somebody running their own does
/// not promise. The legal pages of the site say the same at length.
///
/// The tiers go by their own names here, which the pages translate: production
/// is not sold, but offered to non-profits on request and agreed with
/// companies, so a label saying who pays (Free and Premium, as they were
/// called before) would say something untrue.
/// </remarks>
public static class CloudInstallation
{

    /// <summary>
    /// The options the environment gave, with what is genhttp.dev's own -
    /// the mailboxes unless the environment names others.
    /// </summary>
    public static LambdaOptions Of(LambdaOptions options) => options with
    {
        Terms = Terms,
        Contact = options.Contact ?? "solutions@genhttp.dev",
        AbuseContact = options.AbuseContact ?? "abuse@genhttp.dev",
        LegalPages = true,
        Profile = InstallationProfile.Cloud,
        Profiles = []
    };

    /// <summary>
    /// The terms of use of genhttp.dev, which creating a lambda there accepts.
    /// </summary>
    public const string Terms = """
        Lambdas run on shared infrastructure. By creating one you agree not to deploy
        malware, phishing pages, crypto miners, or anything that attacks, scans or
        floods other systems, and not to publish content you have no right to publish.
        Anyone who knows the editor link can change your lambda, so treat it as a
        password. Lambdas in the sandbox stay online for as long as they are used: one
        that nobody visits and nobody edits for a month is taken offline, and
        removed if nothing happens for two months after that. Anything you deploy
        may be removed at any time.
        """;

}
