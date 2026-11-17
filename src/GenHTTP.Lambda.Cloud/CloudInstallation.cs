using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Cloud;

/// <summary>
/// What genhttp.dev is as an installation, beside what its environment says:
/// its terms, whom to write to, where its legal pages are, where a visitor
/// has an app built, and what its tiers are called.
/// </summary>
/// <remarks>
/// The installation's own, rather than the platform's: they describe how this
/// installation is run - a shared machine, free lambdas that are taken offline
/// when unused, a company to write to - which somebody running their own does
/// not promise. The legal pages of the site say the same at length.
///
/// The sandbox is called Free and production Premium here, as they were before
/// the tiers were named by purpose: what people pay for is this
/// installation's business, so the words are its own - shown as they are in
/// every language, while the code and the API name the tiers by purpose.
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
        Sandbox = options.Sandbox with { Label = "Free" },
        Production = options.Production with { Label = "Premium" }
    };

    /// <summary>
    /// The terms of use of genhttp.dev, which creating a lambda there accepts.
    /// </summary>
    public const string Terms = """
        Lambdas run on shared infrastructure. By creating one you agree not to deploy
        malware, phishing pages, crypto miners, or anything that attacks, scans or
        floods other systems, and not to publish content you have no right to publish.
        Anyone who knows the editor link can change your lambda, so treat it as a
        password. Free tier lambdas stay online for as long as they are used: one
        that nobody visits and nobody edits for a month is taken offline, and
        removed if nothing happens for two months after that. Anything you deploy
        may be removed at any time.
        """;

}
