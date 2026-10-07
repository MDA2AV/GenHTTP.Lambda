using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Every lambda on this installation, for whoever runs it.
/// </summary>
/// <remarks>
/// Everything else in this API is authorized by holding the editor link of the
/// one lambda it touches. This resource is the exception: it reads code that
/// belongs to other people and can take their lambdas down, so it asks for a
/// token instead - and answers nothing at all until one is configured, because
/// a panel that is off cannot be left open by accident. The token is checked
/// in front of this resource, see <see cref="AdminAuthentication"/>.
///
/// Lambdas are addressed by their public key here, since that is what an
/// operator is looking at. The editor key is resolved behind it, and each
/// action is then the same one the owner would take - so a deployment started
/// here is the same deployment, recorded as the operator's.
/// </remarks>
public sealed class AdminResource(IMetaService meta, ITelemetryService telemetry, ISettingsService settings, ILimitsService limits,
                                   SiteMeta site, ILogger<AdminResource> logger)
{

    #region Listing

    /// <summary>
    /// Every lambda, newest first.
    /// </summary>
    /// <param name="search">Only lambdas whose key or domain contains this</param>
    /// <param name="tier">Only lambdas of this tier</param>
    [ResourceMethod("lambdas")]
    public AdminListingResponse GetLambdas(string? search, string? tier, int page = 1)
    {
        var wanted = Math.Max(1, page);

        var result = meta.List(search, (wanted - 1) * PageSize, PageSize, string.IsNullOrEmpty(tier) ? null : ParseTier(tier));

        // the counters are held per lambda in memory, so this is a lookup
        // rather than a join - and a lambda nobody has called is simply absent
        var activity = telemetry.DescribeLambdas().ToDictionary(a => a.PublicKey, StringComparer.Ordinal);

        var lambdas = result.Lambdas.Select(l =>
        {
            var seen = activity.GetValueOrDefault(l.PublicKey);

            return new AdminLambda(
                l.PublicKey,
                l.PrivateKey,
                l.Tier,
                l.Created,
                l.Modified,
                l.ActiveVersion,
                l.LatestVersion,
                l.Versions,
                l.DeployedUntil,
                l.KeptUntil,
                seen?.Requests ?? 0,
                seen?.Failed ?? 0,
                seen?.LastSeen,
                l.Domain,
                LambdaDescription.Serves(l.Tier, l.Domain)
            );
        }).ToList();

        var pages = Math.Max(1, (result.Matched + PageSize - 1) / PageSize);

        return new AdminListingResponse(lambdas, result.Total, result.Deployed, result.Matched, Math.Min(wanted, pages), pages);
    }

    #endregion

    #region One lambda

    /// <summary>
    /// Everything about one lambda: what it is, what it has been doing, its
    /// versions and when it was online.
    /// </summary>
    [ResourceMethod("lambdas/:publicKey")]
    public AdminLambdaDetail GetLambda(string publicKey)
    {

        var privateKey = meta.RequirePrivateKey(publicKey);

        return Detail(privateKey);
    }

    /// <summary>
    /// The code of one version of a lambda.
    /// </summary>
    [ResourceMethod("lambdas/:publicKey/versions/:version")]
    public VersionContentResponse GetVersion(string publicKey, int version)
    {

        var content = meta.GetVersion(meta.RequirePrivateKey(publicKey), version);

        return VersionResource.Describe(content);
    }

    /// <summary>
    /// Moves a lambda to another tier.
    /// </summary>
    /// <remarks>
    /// The tier decides whether the lambda may answer at a domain of its own
    /// and whether the sweeps may take it offline. Leaving the premium tier
    /// keeps the domain configured but stops serving it.
    /// </remarks>
    [ResourceMethod(Method.Put, "lambdas/:publicKey/tier")]
    public AdminLambdaDetail ChangeTier(string publicKey, TierRequest body)
    {

        var privateKey = meta.RequirePrivateKey(publicKey);

        var changed = meta.ChangeTier(privateKey, ParseTier(body.Tier));

        logger.LogInformation("Set tier of lambda {Lambda} to {Tier} by operator", changed.PublicKey, changed.Tier);

        return Detail(privateKey);
    }

    /// <summary>
    /// Sets or removes the domain of a lambda, the way its owner would.
    /// </summary>
    [ResourceMethod(Method.Put, "lambdas/:publicKey/domain")]
    public AdminLambdaDetail ChangeDomain(string publicKey, DomainChangeRequest body)
    {

        var privateKey = meta.RequirePrivateKey(publicKey);

        var changed = meta.ChangeDomain(privateKey, body.Domain);

        logger.LogInformation("Set domain of lambda {Lambda} to {Domain} by operator", changed.PublicKey, changed.Domain ?? "(none)");

        return Detail(privateKey);
    }

    /// <summary>
    /// Lists a lambda in the sitemap of the installation, or takes it out.
    /// </summary>
    /// <remarks>
    /// The sitemap names the root of its address below /lambda/ while it is
    /// online. No lambda is listed until the operator lists it, and an owner
    /// cannot list their own.
    /// </remarks>
    [ResourceMethod(Method.Put, "lambdas/:publicKey/sitemap")]
    public AdminLambdaDetail ChangeSitemap(string publicKey, SitemapRequest body)
    {

        var privateKey = meta.RequirePrivateKey(publicKey);

        var changed = meta.ChangeSitemap(privateKey, body.Listed);

        logger.LogInformation("Set sitemap of lambda {Lambda} to {Listed} by operator", changed.PublicKey, changed.InSitemap);

        return Detail(privateKey);
    }

    /// <summary>
    /// Puts a version online - the newest, when none is named.
    /// </summary>
    /// <remarks>
    /// Code that does not compile is answered with 422 and what the compiler
    /// said, and whatever was online before stays online.
    /// </remarks>
    [ResourceMethod(Method.Post, "lambdas/:publicKey/deployment/start")]
    public async ValueTask<Result<DeploymentOutcomeResponse>> StartDeployment(string publicKey, DeploymentRequest? body)
    {

        var result = await meta.DeployAsync(meta.RequirePrivateKey(publicKey), body?.Version, VersionOrigins.Admin);

        logger.Deployed(result, publicKey, body?.Version);

        var payload = new DeploymentOutcomeResponse(result.Success, result.Lambda == null ? null : LambdaDescription.Of(result.Lambda), result.Diagnostics);

        return new Result<DeploymentOutcomeResponse>(payload).Status(result.Success ? ResponseStatus.Ok : ResponseStatus.UnprocessableEntity);
    }

    /// <summary>
    /// Takes a lambda offline without removing it.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas/:publicKey/deployment/stop")]
    public LambdaOverviewResponse StopDeployment(string publicKey)
    {

        var lambda = meta.Undeploy(meta.RequirePrivateKey(publicKey), ActivationEndings.Admin);

        logger.LogInformation("Undeployed lambda {Lambda} by operator", lambda.PublicKey);

        return Summarize(lambda);
    }

    /// <summary>
    /// Removes a lambda and everything stored for it.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:publicKey")]
    public void Delete(string publicKey)
    {

        meta.Delete(meta.RequirePrivateKey(publicKey));

        logger.LogInformation("Deleted lambda {Lambda} by operator", publicKey);
    }

    #endregion

    #region Settings

    /// <summary>
    /// What the operator has switched on or off.
    /// </summary>
    [ResourceMethod("settings")]
    public SettingsModel GetSettings()
    {
        var current = settings.Get();

        return new SettingsModel(current.EnterprisePage, current.BuildBox, current.ChangeBox);
    }

    /// <summary>
    /// Replaces the settings. They apply to the next page anybody opens.
    /// </summary>
    [ResourceMethod(Method.Put, "settings")]
    public SettingsModel ChangeSettings(SettingsModel body)
    {
        var saved = settings.Save(new SiteSettings(body.EnterprisePage, body.BuildBox, body.ChangeBox));

        logger.LogInformation("Updated settings enterprisePage {EnterprisePage} buildBox {BuildBox} changeBox {ChangeBox} by operator",
                              saved.EnterprisePage, saved.BuildBox, saved.ChangeBox);

        return new SettingsModel(saved.EnterprisePage, saved.BuildBox, saved.ChangeBox);
    }

    #endregion

    #region Limits

    /// <summary>
    /// What a lambda may have and do, per tier.
    /// </summary>
    /// <remarks>
    /// Where the operator has not saved them yet, these are the defaults - the
    /// built in ones, or what the environment variables that used to set them
    /// say.
    /// </remarks>
    [ResourceMethod("limits")]
    public LimitsModel GetLimits() => Describe(limits.Get());

    /// <summary>
    /// Replaces the limits, all of them.
    /// </summary>
    /// <remarks>
    /// They apply to what is checked next - a save, an upload, a deploy, a
    /// connection to a database - and take nothing away that a lambda already
    /// has. Every value must be greater than zero, and the premium tier must
    /// not allow less than the free one.
    /// </remarks>
    [ResourceMethod(Method.Put, "limits")]
    public LimitsModel ChangeLimits(LimitsModel body)
    {
        var (saved, previous) = limits.Save(new ProductLimits(
            Tier(body.Free),
            Tier(body.Premium),
            TimeSpan.FromHours(body.OfflineAfterHours),
            TimeSpan.FromHours(body.RemovedAfterHours),
            body.ShowcaseImageBytes,
            body.RequestsPerSecond,
            body.BuildsPerDay
        ));

        var before = LimitsService.Flatten(previous).ToDictionary(l => l.Key, l => l.Value);

        foreach (var (key, value) in LimitsService.Flatten(saved).Where(l => before[l.Key] != l.Value))
        {
            logger.LogInformation("Changed limit {Limit} from {Previous} to {Value} by operator", key, before[key], value);
        }

        return Describe(saved);
    }

    private static TierLimits Tier(TierLimitsModel model)
        => new(model.BuildBytes, model.DataBytes, model.Versions, model.Features);

    private static TierLimitsModel Describe(TierLimits limits)
        => new(limits.BuildBytes, limits.DataBytes, limits.Versions, limits.Features);

    private static LimitsModel Describe(ProductLimits limits)
        => new(Describe(limits.Free), Describe(limits.Premium), (int)limits.OfflineAfter.TotalHours, (int)limits.RemovedAfter.TotalHours,
               limits.ShowcaseImageBytes, limits.RequestsPerSecond, limits.BuildsPerDay);

    #endregion

    #region Helpers

    /// <summary>
    /// How many lambdas a page of the listing holds.
    /// </summary>
    private const int PageSize = 20;

    private AdminLambdaDetail Detail(string privateKey)
    {
        var lambda = meta.Require(privateKey);

        var id = meta.RequireId(privateKey);

        var versions = meta.GetVersions(privateKey);

        var activations = meta.GetActivations(privateKey);

        var now = DateTime.UtcNow;

        return new AdminLambdaDetail(
            LambdaDescription.Of(lambda),
            telemetry.DescribeLambda(id),
            [.. versions.Select(VersionResource.Describe)],
            [.. activations.Select(a => new ActivationResponse(a.Version, a.Started, a.Origin, a.Ended, a.EndedBy,
                                                                (long)((a.Ended ?? now) - a.Started).TotalSeconds))],
            // the tiers the operator may move a lambda to, which is every one
            // but the demos': those are the seeder's to hand out
            [.. Enum.GetValues<LambdaTier>().Where(t => t != LambdaTier.Demo).Select(t => t.ToString())],
            new SitemapResponse(lambda.InSitemap, site.LambdaAddress(lambda.PublicKey))
        );
    }

    private static LambdaTier ParseTier(string tier)
        => Enum.TryParse<LambdaTier>(tier, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed)
         ? parsed
         : throw LambdaException.Invalid($"There is no tier called '{tier}'. There are: {string.Join(", ", Enum.GetNames<LambdaTier>())}.");

    private static LambdaOverviewResponse Summarize(LambdaInfo lambda)
        => new(lambda.PublicKey, lambda.ActiveVersion, lambda.DeployedUntil);

    #endregion

}
