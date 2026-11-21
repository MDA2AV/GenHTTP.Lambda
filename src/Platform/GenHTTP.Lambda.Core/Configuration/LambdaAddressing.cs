namespace GenHTTP.Lambda.Configuration;

/// <summary>
/// How a lambda's address is made.
/// </summary>
public enum LambdaAddressing
{

    /// <summary>
    /// Each at a subdomain of the hosting domain named after its public key -
    /// <c>https://quiz.genhttp.run/</c> - so that each is a site of its own:
    /// what it runs in a browser cannot read what the platform or another
    /// lambda keeps there. Needs a wildcard record in the DNS and a wildcard
    /// certificate.
    /// </summary>
    Subdomains,

    /// <summary>
    /// Each below <c>/lambda/</c> on the platform's own host -
    /// <c>http://localhost:8080/lambda/quiz/</c> - which takes nothing but the
    /// one name. The lambdas share the platform's origin, so a page of one can
    /// read what the editor and the panel keep in that browser: for an
    /// installation whose lambdas are trusted, such as one run for oneself.
    /// </summary>
    Paths

}
