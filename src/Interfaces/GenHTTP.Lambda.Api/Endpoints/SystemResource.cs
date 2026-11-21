using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Settings;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

[Tag(ApiDocumentation.Platform)]
[Summary("What this installation is like: its terms, its limits, what a lambda can start from, and where lambdas answer.")]
[Remarks("Public. Read it first: the limits and the address of lambdas differ between installations, and may change.")]
public sealed class SystemResource(ILimitsService limits, IBuildService builds, ILambdaAddresses addresses, IBrandingService branding,
                                    ISettingsService settings, IDatabaseProvider database, LambdaOptions options)
{

    [ResourceMethod]
    [Summary("Describes this installation")]
    [Remarks("""
             Its terms, which creating a lambda accepts where it has any; the tier a new lambda starts in and what every tier
             lets a lambda have (`defaultTier`, `tiers`), with what the installation calls each; what a new lambda can start
             from; whether the build agent is on offer, and on which models and efforts; where a lambda answers (`lambdaUrl`); who runs it, as far as its
             pages have to say so (`operator`); how it presents itself on them (`brand`) and whom it speaks to (`profile`); and
             what the editor suggests while code is written - the namespaces a lambda has imported and the types and snippets
             it may use.
             """)]
    [return: Summary("The terms, limits, starters and vocabulary of this installation")]
    public PlatformResponse Get()
    {
        var current = limits.Get();

        return new(
            options.Terms,
            Describe(),
            limits.PolicyOf(current.DefaultTier).MaxVersionBytes,
            (int)current.OfflineAfter.TotalHours,
            (int)current.RemovedAfter.TotalDays,
            ModuleCatalog.ImportsFor(database.Binding),
            CompletionCatalog.Items,
            new BuildAvailability(builds.BuildsOffered(), builds.PerDay, builds.Offer()),
            addresses.Template,
            new OperatorResponse(options.Contact, options.AbuseContact, options.LegalPages, options.StartPage),
            TierDescription.Of(limits, current.DefaultTier),
            [.. limits.TiersWhere(p => p.Assignable).Select(t => TierDescription.Of(limits, t))],
            BrandResponse.Of(branding),
            InstallationProfiles.NameOf(settings.Get().Profile)
        );
    }

    /// <remarks>
    /// Cached for good when asked for by the address the pages have, which
    /// names what the picture is: a new logo, or a new name drawn as a
    /// letter, is a new address. Asked for by any other, it is the picture as
    /// it is now, and asked about again. An SVG opened on its own would be a
    /// document of this origin, so it is served with a policy that runs
    /// nothing in it; as a picture on a page, nothing in it runs anyway.
    /// </remarks>
    [ResourceMethod("logo")]
    [Summary("Reads the picture of the installation")]
    [Remarks("""
             Public. The picture its pages show beside its name and as the icon of the browser tab: the logo its operator uploaded,
             or the first letter of its name on a plate where they named it without one. Link it by `brand.logo`, which changes
             with it, so it may be cached for good.
             """)]
    [Response(ResponseStatus.Ok, "The picture: a PNG, JPEG, GIF, WebP or SVG image", ContentType = "image/*")]
    [ResponseHeader(ResponseStatus.Ok, "Cache-Control", "`public, max-age=31536000, immutable` by the address `brand.logo` names, `no-cache` by any other")]
    [Response(ResponseStatus.NotFound, "The installation goes by the platform's own name and mark, which the pages draw themselves.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public IResponse Logo([Summary("What the picture is, as `brand.logo` names it")] string? v, IRequest request)
    {
        var brand = branding.Current;

        var picture = brand.Picture
                   ?? throw LambdaException.NotFound("This installation has no picture of its own: its pages draw the platform's mark.");

        var current = v != null && brand.PicturePath!.EndsWith($"?v={v}", StringComparison.Ordinal);

        return request.Respond()
                      .Content(picture.Content, new ContentType(picture.Type))
                      .Header("Cache-Control", current ? "public, max-age=31536000, immutable" : "no-cache")
                      .Header("X-Content-Type-Options", "nosniff")
                      .Header("Content-Security-Policy", "default-src 'none'; style-src 'unsafe-inline'; img-src data:; sandbox")
                      .Build();
    }

    /// <summary>
    /// What a new lambda can be started from: nothing much, or a copy of a
    /// demo - described by what somebody would want to build with it.
    /// </summary>
    private IReadOnlyList<StarterResponse> Describe()
        => [.. DemoCatalog.All.Select(d => new StarterResponse(d.Id, d.Goal, d.Pitch, addresses.Of(d.Key))),
            new StarterResponse(TemplateCatalog.EmptyId, "Something else", "Start from an empty lambda and build whatever you have in mind.", null)];

}
