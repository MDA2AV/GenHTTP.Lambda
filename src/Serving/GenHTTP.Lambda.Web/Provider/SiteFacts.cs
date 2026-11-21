namespace GenHTTP.Lambda.Web;

/// <summary>
/// What the server knows and the build could not, as <c>SiteFacts</c> in <c>site.ts</c>.
/// </summary>
/// <param name="Before">What a lambda's address is shown with before its key: "", or "localhost:8080/lambda/"</param>
/// <param name="After">And after it: ".genhttp.run", or ""</param>
/// <param name="Expires">Whether a new lambda is taken offline when unused - it starts in the sandbox - so the pages say how long it is kept</param>
/// <param name="Profile">Who the installation is for, which the page was rendered for: <c>cloud</c>, <c>home</c> or <c>enterprise</c></param>
public sealed record SiteFacts(string Origin, string Host, string Before, string After, int LifetimeHours, int OfflineDays, int RetentionDays,
                               bool Expires = true, string Profile = "home", ShowcaseListing? Showcase = null);
