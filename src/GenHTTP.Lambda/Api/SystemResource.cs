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
    /// What a new lambda can be started from: nothing much, or a copy of a
    /// demo - described by what somebody would want to build with it.
    /// </summary>
    private static IReadOnlyList<StarterResponse> Describe()
        => [.. DemoCatalog.All.Select(d => new StarterResponse(d.Id, d.Goal, d.Pitch, $"/lambda/{d.Key}/")),
            new StarterResponse(TemplateCatalog.EmptyId, "Something else", "Start from an empty lambda and build whatever you have in mind.", null)];

}
