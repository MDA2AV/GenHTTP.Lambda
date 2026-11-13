namespace GenHTTP.Lambda.Cloud;

/// <summary>
/// The terms of use of genhttp.dev, which creating a lambda there accepts.
/// </summary>
/// <remarks>
/// The installation's own, rather than the platform's: they describe how this
/// installation is run - a shared machine, free lambdas that are taken
/// offline when unused - which somebody running their own does not promise.
/// The legal pages say the same at length.
/// </remarks>
public static class CloudTerms
{

    public const string Text = """
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
