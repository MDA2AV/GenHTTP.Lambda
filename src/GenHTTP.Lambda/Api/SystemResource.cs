using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Settings;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

[Tag(ApiDocumentation.Platform)]
[Summary("What this installation is like: its terms, its limits, what a lambda can start from, and where lambdas answer.")]
[Remarks("Public. Read it first: the limits and the address of lambdas differ between installations, and may change.")]
public sealed class SystemResource(ILimitsService limits, IBuildService builds, ISettingsService settings, ILambdaAddresses addresses)
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

    [ResourceMethod]
    [Summary("Describes this installation")]
    [Remarks("""
             Its terms, which creating a lambda accepts; the limits of a free lambda; what a new lambda can start from; whether
             the build agent is on offer; where a lambda answers (`lambdaUrl`); and what the editor suggests while code is
             written - the namespaces a lambda has imported and the types and snippets it may use.
             """)]
    [return: Summary("The terms, limits, starters and vocabulary of this installation")]
    public PlatformResponse Get()
    {
        var current = limits.Get();

        return new(
            Terms,
            Describe(),
            current.Free.BuildBytes,
            (int)current.OfflineAfter.TotalHours,
            (int)current.RemovedAfter.TotalDays,
            ModuleCatalog.Imports,
            CompletionCatalog.Items,
            new BuildAvailability(builds.BuildsOffered(), builds.PerDay, builds.HasSecondModel),
            addresses.Template
        );
    }

    /// <remarks>
    /// Apart from the platform description, which carries the whole editor
    /// vocabulary and is too much to ask for on every page.
    /// </remarks>
    [ResourceMethod("features")]
    [Summary("Reads which pages the site links to")]
    [Remarks("What the site asks on every page. Unrelated to the features of a lambda.")]
    [return: Summary("Which pages are linked")]
    public FeaturesResponse GetFeatures()
    {
        var current = settings.Get();

        return new FeaturesResponse(current.EnterprisePage);
    }

    /// <summary>
    /// What a new lambda can be started from: nothing much, or a copy of a
    /// demo - described by what somebody would want to build with it.
    /// </summary>
    private IReadOnlyList<StarterResponse> Describe()
        => [.. DemoCatalog.All.Select(d => new StarterResponse(d.Id, d.Goal, d.Pitch, addresses.Of(d.Key))),
            new StarterResponse(TemplateCatalog.EmptyId, "Something else", "Start from an empty lambda and build whatever you have in mind.", null)];

}
