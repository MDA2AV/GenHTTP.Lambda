using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// What the editor needs to know about the platform: the terms, the example it
/// starts from and the vocabulary it can suggest.
/// </summary>
public sealed class SystemResource(LambdaOptions options)
{

    internal const string Terms = """
        Lambdas run on shared infrastructure. By creating one you agree not to deploy
        malware, phishing pages, crypto miners, or anything that attacks, scans or
        floods other systems, and not to publish content you have no right to publish.
        Anyone who knows the editor link can change your lambda, so treat it as a
        password. Free tier lambdas are taken offline a day after they were deployed
        and removed entirely a month after you last touched them. Anything you deploy
        may be removed at any time.
        """;

    /// <summary>
    /// The terms, limits and editor vocabulary of this installation.
    /// </summary>
    [ResourceMethod]
    public PlatformResponse Get() => new(
        Terms,
        CodeTemplate.ForKey("your-key"),
        options.MaxCodeLength,
        (int)options.DeploymentLifetime.TotalHours,
        (int)options.Retention.TotalDays,
        ModuleCatalog.Imports,
        CompletionCatalog.Items
    );

}
