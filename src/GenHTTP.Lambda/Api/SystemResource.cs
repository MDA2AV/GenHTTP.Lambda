using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// What the editor needs to know about the platform: the terms, the example it
/// starts from, the vocabulary it can suggest and whether it can build things.
/// </summary>
public sealed class SystemResource(LambdaOptions options, BuildService builds)
{

    internal const string Terms = """
        Lambdas run on shared infrastructure. By creating one you agree not to deploy
        malware, phishing pages, crypto miners, or anything that attacks, scans or
        floods other systems, and not to publish content you have no right to publish.
        Anyone who knows the editor link can change your lambda, so treat it as a
        password. Free tier lambdas stay online for as long as they are used: one
        that nobody visits and nobody edits for a month is taken offline, and
        removed if nothing happens for two months after that. Anything you deploy
        may be removed at any time.
        """;

    /// <summary>
    /// The terms, limits and editor vocabulary of this installation.
    /// </summary>
    [ResourceMethod]
    public PlatformResponse Get() => new(
        Terms,
        Describe(),
        options.MaxCodeLength,
        (int)options.DeploymentLifetime.TotalHours,
        (int)options.Retention.TotalDays,
        ModuleCatalog.Imports,
        CompletionCatalog.Items,
        new BuildAvailability(builds.Available, builds.PerDay, builds.HasSecondModel)
    );

    /// <summary>
    /// The template catalogue, with a stand in key so the assistant can show
    /// the code of a template before a lambda exists to fill it with.
    /// </summary>
    /// <remarks>
    /// Hidden templates are described here rather than left out: a link that
    /// names one still has to arrive at an editor that can say what it is. It
    /// is the assistant's list they are kept out of, not the catalogue.
    /// </summary>
    private static IReadOnlyList<TemplateGroupResponse> Describe()
        => [.. TemplateCatalog.Groups.Select(g => new TemplateGroupResponse(g.Id, g.Name, g.Description,
               [.. g.Templates.Select(t => new TemplateResponse(t.Id, t.Name, t.Description,
                   TemplateCatalog.ForKey(t.Id, "your-key"), t.Hidden))]))];

}
