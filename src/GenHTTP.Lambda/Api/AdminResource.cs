using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;

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
[Tag(ApiDocumentation.Administration)]
[Summary("Every lambda on the installation, its settings and its limits - for whoever runs it.")]
[Remarks("""
         Lambdas are addressed by their **public** key here, and every action is the one the owner would take, recorded as the
         operator's. The answers carry editor keys. The settings and the limits apply to the whole installation, from the next
         request on.
         """)]
[RequestHeader(AdminAuthentication.Header, AdminAuthentication.HeaderDescription, Required = true)]
[Response(ResponseStatus.Unauthorized, AdminAuthentication.MissingDescription)]
[Response(ResponseStatus.Forbidden, AdminAuthentication.WrongDescription)]
public sealed class AdminResource(IMetaService meta, ITelemetryService telemetry, ISettingsService settings, ILimitsService limits,
                                   ILogger<AdminResource> logger)
{

    #region Listing

    [ResourceMethod("lambdas")]
    [Summary("Lists every lambda")]
    [Remarks("Newest first, twenty to a page, with what each has been doing since the server came up, which is counted in memory.")]
    [Response(ResponseStatus.BadRequest, "There is no such tier.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("A page of lambdas")]
    public AdminListingResponse GetLambdas([Summary("Only lambdas whose public key or domain contains this")] string? search,
                                           [Summary("Only lambdas of this tier: `Free`, `Premium` or `Demo`")] string? tier,
                                           [Summary("The page, counting from 1")] int page = 1)
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

    [ResourceMethod("lambdas/:publicKey")]
    [Summary("Reads everything about one lambda")]
    [Remarks("What it is - its editor key included - what it has been doing, its versions and when it was online.")]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("Everything about the lambda")]
    public AdminLambdaDetail GetLambda([Summary("The public key of the lambda")] string publicKey)
    {

        var privateKey = meta.RequirePrivateKey(publicKey);

        return Detail(privateKey);
    }

    [ResourceMethod("lambdas/:publicKey/versions/:version")]
    [Summary("Reads a version of a lambda with its files")]
    [Response(ResponseStatus.NotFound, "No lambda has this public key, or it has no such version.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The version with its files")]
    public VersionContentResponse GetVersion([Summary("The public key of the lambda")] string publicKey, [Summary(ApiDocumentation.VersionNumber)] int version)
    {

        var content = meta.GetVersion(meta.RequirePrivateKey(publicKey), version);

        return VersionResource.Describe(content);
    }

    [ResourceMethod(Method.Put, "lambdas/:publicKey/tier")]
    [Summary("Moves a lambda to another tier")]
    [Remarks("""
             The tier decides the limits of the lambda, whether it may answer at a domain of its own, and whether it is taken
             offline and deleted when nobody uses it. Leaving the premium tier keeps the domain configured, but stops serving it;
             what the lambda has beyond the free limits is kept, and only what it saves next is held to them. Nothing is moved
             into or out of the demo tier by hand.
             """)]
    [Response(ResponseStatus.BadRequest, "There is no such tier.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, "The demo tier is the installation's own. Nothing is moved into or out of it by hand.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda afterwards")]
    public AdminLambdaDetail ChangeTier([Summary("The public key of the lambda")] string publicKey, TierRequest body)
    {

        var privateKey = meta.RequirePrivateKey(publicKey);

        var changed = meta.ChangeTier(privateKey, ParseTier(body.Tier));

        logger.LogInformation("Set tier of lambda {Lambda} to {Tier} by operator", changed.PublicKey, changed.Tier);

        return Detail(privateKey);
    }

    [ResourceMethod(Method.Put, "lambdas/:publicKey/domain")]
    [Summary("Sets the domain of a lambda")]
    [Remarks("Under the same rules as `PUT /lambdas/{privateKey}/domain`: only a premium lambda may have one. Nothing, or an empty domain, removes it.")]
    [Response(ResponseStatus.BadRequest, "That is not a domain that can be used.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "Another lambda answers at that domain.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda afterwards")]
    public AdminLambdaDetail ChangeDomain([Summary("The public key of the lambda")] string publicKey, DomainChangeRequest body)
    {

        var privateKey = meta.RequirePrivateKey(publicKey);

        var changed = meta.ChangeDomain(privateKey, body.Domain);

        logger.LogInformation("Set domain of lambda {Lambda} to {Domain} by operator", changed.PublicKey, changed.Domain ?? "(none)");

        return Detail(privateKey);
    }

    [ResourceMethod(Method.Post, "lambdas/:publicKey/deployment/start")]
    [Summary("Puts a version of a lambda online")]
    [Remarks("The newest, unless another is named - as its owner would, but for demos as well, and recorded as the operator's.")]
    [Response(ResponseStatus.Ok, "The version is online.")]
    [Response(ResponseStatus.UnprocessableEntity, ApiDocumentation.Diagnostics, Type = typeof(DeploymentOutcomeResponse))]
    [Response(ResponseStatus.NotFound, "No lambda has this public key, or it has no such version.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<Result<DeploymentOutcomeResponse>> StartDeployment([Summary("The public key of the lambda")] string publicKey, DeploymentRequest? body)
    {

        var result = await meta.DeployAsync(meta.RequirePrivateKey(publicKey), body?.Version, VersionOrigins.Admin);

        logger.Deployed(result, publicKey, body?.Version);

        var payload = new DeploymentOutcomeResponse(result.Success, result.Lambda == null ? null : LambdaDescription.Of(result.Lambda), result.Diagnostics);

        return new Result<DeploymentOutcomeResponse>(payload).Status(result.Success ? ResponseStatus.Ok : ResponseStatus.UnprocessableEntity);
    }

    [ResourceMethod(Method.Post, "lambdas/:publicKey/deployment/stop")]
    [Summary("Takes a lambda offline without deleting it")]
    [Remarks("Its history says the operator took it down. Its owner may deploy it again.")]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda afterwards")]
    public LambdaOverviewResponse StopDeployment([Summary("The public key of the lambda")] string publicKey)
    {

        var lambda = meta.Undeploy(meta.RequirePrivateKey(publicKey), ActivationEndings.Admin);

        logger.LogInformation("Undeployed lambda {Lambda} by operator", lambda.PublicKey);

        return Summarize(lambda);
    }

    [ResourceMethod(Method.Delete, "lambdas/:publicKey")]
    [Summary("Deletes a lambda and everything kept for it")]
    [Remarks("As its owner would, for good. Demos cannot be deleted: the installation keeps them.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public void Delete([Summary("The public key of the lambda")] string publicKey)
    {

        meta.Delete(meta.RequirePrivateKey(publicKey));

        logger.LogInformation("Deleted lambda {Lambda} by operator", publicKey);
    }

    #endregion

    #region Settings

    [ResourceMethod("settings")]
    [Summary("Reads what the operator has switched on or off")]
    [return: Summary("The settings of the installation")]
    public SettingsModel GetSettings()
    {
        var current = settings.Get();

        return new SettingsModel(current.EnterprisePage, current.BuildBox, current.ChangeBox);
    }

    [ResourceMethod(Method.Put, "settings")]
    [Summary("Replaces the settings")]
    [Remarks("All of them. They apply to the next page anybody opens.")]
    [return: Summary("The settings, as they were saved")]
    public SettingsModel ChangeSettings(SettingsModel body)
    {
        var saved = settings.Save(new SiteSettings(body.EnterprisePage, body.BuildBox, body.ChangeBox));

        logger.LogInformation("Updated settings enterprisePage {EnterprisePage} buildBox {BuildBox} changeBox {ChangeBox} by operator",
                              saved.EnterprisePage, saved.BuildBox, saved.ChangeBox);

        return new SettingsModel(saved.EnterprisePage, saved.BuildBox, saved.ChangeBox);
    }

    #endregion

    #region Limits

    [ResourceMethod("limits")]
    [Summary("Reads what a lambda may have and do, per tier")]
    [Remarks("Where the operator has not saved them yet, these are the defaults - the built in ones, or what the environment variables that used to set them say.")]
    [return: Summary("The limits of the installation")]
    public LimitsModel GetLimits() => Describe(limits.Get());

    [ResourceMethod(Method.Put, "limits")]
    [Summary("Replaces the limits, all of them")]
    [Remarks("""
             They apply to what is checked next - a save, an upload, a deploy, a connection to a database - and take nothing away
             that a lambda already has. Every value must be greater than zero, no tier may show more days of analytics than are
             kept (a year), and the premium tier must not allow less than the free one.
             """)]
    [Response(ResponseStatus.BadRequest, "A value is zero or less, more days of analytics than are kept, or the premium tier would allow less than the free one. Nothing was changed.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The limits, as they were saved")]
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
        => new(model.BuildBytes, model.DataBytes, model.Versions, model.Features, model.AnalyticsDays);

    private static TierLimitsModel Describe(TierLimits limits)
        => new(limits.BuildBytes, limits.DataBytes, limits.Versions, limits.Features, limits.AnalyticsDays);

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
            [.. Enum.GetValues<LambdaTier>().Where(t => t != LambdaTier.Demo).Select(t => t.ToString())]
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
