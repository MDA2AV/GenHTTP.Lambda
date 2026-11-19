using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Blocking;
using GenHTTP.Lambda.Services.Census;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Source;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Integrations;
using GenHTTP.Lambda.Services.UsageReports;
using GenHTTP.Lambda.Services.Workspace;

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
public sealed class AdminResource(IMetaService meta, ITelemetryService telemetry, ISettingsService settings, ILimitsService limits,
                                   IAddressBlocks blocks, IConnectivityService connectivity, IUsageReportService usageReports, IIntegrationService integrations,
                                   ICensusService census, IShowcaseService showcases, ISourceService sources, IFeatureService features,
                                   IWorkspaceService workspace, ISecretService secrets, IDataService data, ILogBook book,
                                   ILambdaAddresses addresses, LambdaOptions options, ILogger<AdminResource> logger)
{
    private readonly LambdaSummaries _summaries = new(meta, workspace, secrets, data, telemetry, book, limits, connectivity);

    #region Listing

    [ResourceMethod("lambdas")]
    [Summary("Lists every lambda")]
    [Remarks("""
             Twenty to a page, the newest first unless `sort` says otherwise, the installation's demos left out unless they are asked
             for, with which of the platform's features each uses and
             what it has been doing since the server came up, which is counted in memory. Every filter narrows what the others
             left: `uses=database,showcase` lists the lambdas that use both. Only what the platform keeps narrows it - the requests
             and errors are not filters, since they are counted in memory.
             """)]
    [Response(ResponseStatus.BadRequest, "There is no such tier, state, feature, view or order.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("A page of lambdas")]
    public AdminListingResponse GetLambdas([Summary("Only lambdas whose public key or domain contains this")] string? search,
                                           [Summary("Only lambdas of this tier: `Sandbox`, `Production`, `System` or `Demo`")] string? tier,
                                           [Summary("Only lambdas in this state: `online`, `offline` (and not blocked), `blocked`, or `expiring` - going offline or removed within seven days unless they are used")] string? state,
                                           [Summary("Only lambdas that use every one of these, separated by commas: `database`, `workspace`, `secrets`, `analytics`, `showcase`, `source` (published), `domain`, `drafts` (a feature open), `connects` (may connect to other servers) and `requested` (asks to, and waits)")] string? uses,
                                           [Summary("Only lambdas whose editor opens in this view: `simple` or `full`")] string? view,
                                           [Summary("Only lambdas created in the last this many days")] int? created,
                                           [Summary("The order: `newest` (the default), `oldest`, `changed`, `visited` or `key`")] string? sort,
                                           [Summary("Whether the installation's demos are listed and counted with the rest; they are left out unless this or `tier=Demo` asks for them, as the overview's counts leave them out")] bool demos = false,
                                           [Summary("The page, counting from 1")] int page = 1)
    {
        var wanted = Math.Max(1, page);

        var query = new LambdaQuery(
            search,
            string.IsNullOrEmpty(tier) ? null : TierDescription.Parse(tier),
            Parse<LambdaState>(state, "state"),
            string.IsNullOrWhiteSpace(uses) ? null : [.. uses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(u => Parse<LambdaFeature>(u, "feature")!.Value)],
            Parse<EditorView>(view, "view"),
            created is > 0 ? TimeSpan.FromDays(created.Value) : null,
            Parse<LambdaOrder>(sort, "order") ?? LambdaOrder.Newest,
            demos
        );

        var result = meta.List(query, (wanted - 1) * PageSize, PageSize);

        var pages = Math.Max(1, (result.Matched + PageSize - 1) / PageSize);

        return new AdminListingResponse(Describe(result.Lambdas), result.Total, result.Deployed, result.Matched, Math.Min(wanted, pages), pages);
    }

    /// <summary>
    /// The lambdas of a page, with what they have been doing since the server came up.
    /// </summary>
    private List<AdminLambda> Describe(IReadOnlyList<LambdaOverview> lambdas)
    {
        // the counters are held per lambda in memory, so this is a lookup
        // rather than a join - and a lambda nobody has called is simply absent
        var activity = telemetry.DescribeLambdas().ToDictionary(a => a.PublicKey, StringComparer.Ordinal);

        return [.. lambdas.Select(l =>
        {
            var seen = activity.GetValueOrDefault(l.PublicKey);

            return new AdminLambda(
                l.PublicKey,
                l.PrivateKey,
                l.Tier,
                l.Policy.Label,
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
                l.Domain != null && l.Policy.OwnDomain,
                l.Blocked,
                ConnectivityResponse.Of(l.Connectivity),
                l.Uses == null ? null : AdminLambdaUses.Of(l.Uses),
                l.Visited
            );
        })];
    }

    /// <summary>
    /// A value of the listing's query by its name in any case, or a refusal that names what there is.
    /// </summary>
    private static T? Parse<T>(string? value, string what) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<T>(value.Trim(), ignoreCase: true, out var parsed) && Enum.IsDefined(parsed) && !int.TryParse(value, out _))
        {
            return parsed;
        }

        throw LambdaException.Invalid($"There is no {what} '{value}'. There are: {string.Join(", ", Enum.GetNames<T>().Select(n => n.ToLowerInvariant()))}.");
    }

    #endregion

    #region Overview

    [ResourceMethod("overview")]
    [Summary("Reads the installation at a glance")]
    [Remarks("""
             What the operator's dashboards draw that is not about the machine: the lambdas counted - in which state and tier, how
             recent, which of the platform's features they use, where their versions came from - what waits for a decision, the
             room left on the volume of the data directory, the newest lambdas and the ones that failed in the last hour. The
             demos are counted apart and in nothing else. A few counts on the platform's database; the free space is read as it
             is asked for.
             """)]
    [return: Summary("The installation at a glance")]
    public AdminOverviewResponse GetOverview(IRequest request)
    {
        var counted = census.Count();

        var held = blocks.List();

        var you = You(request);

        var report = usageReports.Status;

        var newest = meta.List(new LambdaQuery(), 0, NewestShown);

        var failing = telemetry.DescribeLambdas()
                               .Where(a => a.HourFailed > 0)
                               .OrderByDescending(a => a.HourFailed)
                               .Take(FailingShown)
                               .ToList();

        return new AdminOverviewResponse(
            AdminCensusResponse.Of(counted),
            new AdminAttentionResponse(
                counted.Uses.Requested,
                counted.RequestedSince,
                held.Count,
                you != null && held.Any(b => b.Address == you),
                report.Enabled,
                report.Enabled ? report.Failure : null
            ),
            census.Disk() is var (size, free) && size != null && free != null ? new AdminDiskResponse(size.Value, free.Value) : null,
            Describe(newest.Lambdas),
            failing
        );
    }

    [ResourceMethod("storage")]
    [Summary("Reads the room the installation has, and what takes it")]
    [Remarks("""
             The volume of the data directory - how large, how much is free, read as this is asked for - and what the directory
             holds, by part and by lambda, as last measured. Measuring walks over every file, so it is done every hour in the
             background, and the first time it is asked for where it never was; `measured` says when.
             """)]
    [return: Summary("The room, and what takes it")]
    public async ValueTask<AdminStorageResponse> GetStorage()
        => AdminStorageResponse.Of(await census.StorageAsync());

    [ResourceMethod(Method.Post, "storage/measure")]
    [Summary("Measures what takes the room again, now")]
    [Remarks("""
             Walks over every file of the data directory, as the hourly measurement does, and answers once it is done - which on
             a large installation takes a moment. Changes nothing; a walk already running is waited for rather than started again.
             """)]
    [return: Summary("The room, and what takes it, as measured now")]
    public async ValueTask<AdminStorageResponse> MeasureStorage()
    {
        await census.MeasureAsync();

        return AdminStorageResponse.Of(await census.StorageAsync());
    }

    /// <summary>
    /// How many of the newest lambdas the overview names.
    /// </summary>
    private const int NewestShown = 6;

    /// <summary>
    /// How many of the lambdas that failed in the last hour the overview names.
    /// </summary>
    private const int FailingShown = 8;

    #endregion

    #region One lambda

    [ResourceMethod("lambdas/:publicKey")]
    [Summary("Reads everything about one lambda")]
    [Remarks("What it is - its editor key included - what it has been doing, its versions and when it was online.")]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("Everything about the lambda")]
    public async ValueTask<AdminLambdaDetail> GetLambda([Summary("The public key of the lambda")] string publicKey, IRequest request)
    {

        var privateKey = meta.RequirePrivateKey(publicKey);

        return await DetailAsync(privateKey, request);
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
             The tier is what the lambda is for, and decides its limits, whether it may answer at a domain of its own, whether it
             is taken offline and deleted when nobody uses it, whether it connects to other servers while nothing was decided
             about it - as `GET /admin/limits` sets each - and whether what it has online is compiled as the server starts
             (production and system) rather than on its first request. Only the policy changes: the lambda keeps running, nothing of its data
             is moved, and it is built again only where the move changes whether it may connect. A tier without domains keeps
             the domain configured but stops serving it; what the lambda has beyond the new tier's limits is kept and can still
             be read and put online, and only what it saves or writes next is held to them. Moved to a tier that takes unused
             lambdas offline, it is given the whole time from the move. A `System` lambda is the installation's own: it cannot be
             blocked, is never in the showcase and is not deleted with its editor key. Nothing is moved into or out of the demo
             tier by hand. `Free` and `Premium`, what the sandbox and production were called before, are still read as them.
             """)]
    [Response(ResponseStatus.BadRequest, "There is no such tier.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, "The demo tier is the installation's own. Nothing is moved into or out of it by hand.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The lambda is blocked, and the tier asked for cannot be: the block is lifted first.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda afterwards")]
    public async ValueTask<AdminLambdaDetail> ChangeTier([Summary("The public key of the lambda")] string publicKey, TierRequest body, IRequest request)
    {

        var privateKey = meta.RequirePrivateKey(publicKey);

        var changed = meta.ChangeTier(privateKey, TierDescription.Parse(body.Tier));

        logger.LogInformation("Set tier of lambda {Lambda} to {Tier} by operator", changed.PublicKey, changed.Tier);

        return await DetailAsync(privateKey, request);
    }

    [ResourceMethod(Method.Put, "lambdas/:publicKey/domain")]
    [Summary("Sets the domain of a lambda")]
    [Remarks("Under the same rules as `PUT /lambdas/{privateKey}/domain`: only a lambda whose tier has domains of its own - production or system - may have one. Nothing, or an empty domain, removes it.")]
    [Response(ResponseStatus.BadRequest, "That is not a domain that can be used.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "Another lambda answers at that domain.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda afterwards")]
    public async ValueTask<AdminLambdaDetail> ChangeDomain([Summary("The public key of the lambda")] string publicKey, DomainChangeRequest body, IRequest request)
    {

        var privateKey = meta.RequirePrivateKey(publicKey);

        var changed = meta.ChangeDomain(privateKey, body.Domain);

        logger.LogInformation("Set domain of lambda {Lambda} to {Domain} by operator", changed.PublicKey, changed.Domain ?? "(none)");

        return await DetailAsync(privateKey, request);
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
    [Remarks("As its owner would, for good - a blocked lambda as well, which its owner cannot delete. Demos cannot be deleted: the installation keeps them.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask Delete([Summary("The public key of the lambda")] string publicKey)
    {
        await meta.DeleteAsync(meta.RequirePrivateKey(publicKey), VersionOrigins.Admin);

        logger.LogInformation("Deleted lambda {Lambda} by operator", publicKey);
    }

    #endregion

    #region Blocking

    [ResourceMethod(Method.Post, "lambdas/:publicKey/block")]
    [Summary("Blocks a lambda")]
    [Remarks("""
             Takes it offline, and the previews of its features, and refuses every change to it from then on - through the
             editor, the API, MCP and git, with `Blocked` (403) and a message that says so. Its code, its data and its history
             are kept and can still be read and exported; nothing about it can be changed, put online or deleted by its owner
             until the block is lifted. The operator may still delete it, and a blocked lambda is never removed for going unused.
             Its published source is not found while the block lasts. The reason is shown to its owner as it is written, in the
             editor and as `blockReason` of the lambda.

             The addresses named are refused as well, for two weeks, before anything else is done for them - usually some of the
             `editorAddresses` the lambda was changed from. Blocking a lambda that is blocked already keeps the time it was
             first blocked and takes the new reason.
             """)]
    [Response(ResponseStatus.BadRequest, "One of the addresses is not one, or is on one of the installation's own networks, which are never blocked. Nothing was blocked.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, "The lambda is a demo, which the installation keeps: there is nothing to block.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda afterwards")]
    public async ValueTask<AdminLambdaDetail> Block([Summary("The public key of the lambda")] string publicKey, BlockRequest? body, IRequest request)
    {
        var privateKey = meta.RequirePrivateKey(publicKey);

        var addresses = body?.Addresses ?? [];

        // read before anything is done, so an address that cannot be
        // blocked leaves the lambda as it was too
        foreach (var address in addresses)
        {
            NetworkAddress.ReadBlockable(address, options.UnblockableNetworks);
        }

        var blocked = meta.Block(privateKey, body?.Reason);

        if (addresses.Count > 0)
        {
            blocks.Block(addresses, body?.Reason, blocked.PublicKey);
        }

        logger.LogInformation("Blocked lambda {Lambda} addresses {Addresses} by operator", blocked.PublicKey, addresses.Count);

        return await DetailAsync(privateKey, request);
    }

    [ResourceMethod(Method.Delete, "lambdas/:publicKey/block")]
    [Summary("Lifts the block of a lambda")]
    [Remarks("""
             Its owner can change it again. It stays offline until its owner, or the operator, puts it online again, and its free
             tier starts its time again from now. The blocks of the addresses it was changed from are left as they are:
             they are lifted in `DELETE /admin/blocks/{id}`, or by themselves after two weeks.
             """)]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda afterwards")]
    public async ValueTask<AdminLambdaDetail> Unblock([Summary("The public key of the lambda")] string publicKey, IRequest request)
    {
        var privateKey = meta.RequirePrivateKey(publicKey);

        var lambda = meta.Unblock(privateKey);

        logger.LogInformation("Unblocked lambda {Lambda} by operator", lambda.PublicKey);

        return await DetailAsync(privateKey, request);
    }

    [ResourceMethod("blocks")]
    [Summary("Lists the blocked addresses")]
    [Remarks("""
             Newest first. A blocked address is answered with 403 before anything else is done for it - on the platform and at
             every lambda - apart from the administration panel, so an operator who blocked their own address can lift the
             block. Each is let go of two weeks after it was blocked. What each refused is counted in memory, since the server
             came up.
             """)]
    [return: Summary("The blocks, and the address of whoever asks")]
    public AddressBlockListing GetBlocks(IRequest request)
        => new([.. blocks.List().Select(Describe)], You(request));

    [ResourceMethod(Method.Delete, "blocks/:id")]
    [Summary("Lifts the block of an address")]
    [Remarks("From the next request on. A lambda it was blocked for stays blocked.")]
    [Response(ResponseStatus.NoContent, "The block is lifted.")]
    [Response(ResponseStatus.NotFound, "There is no block with this id - it may have been lifted, or let go of by itself.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public void RemoveBlock([Summary("The id of the block, from `GET /admin/blocks`")] long id)
    {
        if (!blocks.Remove(id))
        {
            throw LambdaException.NotFound($"There is no block #{id}. It may have been lifted already, or let go of by itself.");
        }

        logger.LogInformation("Removed address block #{Block} by operator", id);
    }

    private static AddressBlockResponse Describe(AddressBlock block)
        => new(block.Id, block.Address, block.Reason, block.PublicKey, block.Created, block.Expires, block.Refused, block.LastRefused);

    /// <summary>
    /// The address of the request as a block would cover it, or nothing for
    /// one on the installation's own networks, which is never blocked.
    /// </summary>
    private string? You(IRequest request)
        => request.Client.Address is { } address && !NetworkAddress.IsOwn(address, options.UnblockableNetworks)
         ? NetworkAddress.Describe(NetworkAddress.RangeOf(address))
         : null;

    #endregion

    #region Connectivity

    [ResourceMethod("connectivity")]
    [Summary("Lists the requests to connect to other servers")]
    [Remarks("""
             The lambdas whose owner asked for their code to connect to other servers and waits for a decision, the longest
             waiting first, with what they said it is for - and the lambdas allowed so far, the most recently allowed first, to
             be looked at again. A lambda whose tier connects by default - production, as the limits set it - may connect without
             asking, and is not listed unless it asked as well.
             """)]
    [return: Summary("What waits, and what was allowed")]
    public AdminConnectivityListing GetConnectivity()
    {
        var listed = connectivity.List();

        return new AdminConnectivityListing([.. listed.Where(l => l.Connectivity.Status == ConnectivityStatus.Requested).Select(Describe)],
                                      [.. listed.Where(l => l.Connectivity.Status == ConnectivityStatus.Allowed).Select(Describe)]);
    }

    [ResourceMethod(Method.Put, "lambdas/:publicKey/connectivity")]
    [Summary("Decides whether a lambda may connect to other servers")]
    [Remarks("""
             Allows it - asked for or not - or declines it, which takes back what was allowed, and what its tier allows by default
             as well: a decision about the lambda comes before its tier's default. The note is
             shown to the owner as it is written, in the editor and as `connectivity.note` of the lambda; the owner may ask again
             after a decline.

             It takes effect from the next request, without a deploy: the lambda and the previews of its features are built
             again under the decision. Taken back, code that connects stops compiling, and the version online answers with an
             error until it is changed. Moving a lambda to a tier that connects by default forgets a request that waits.
             """)]
    [Response(ResponseStatus.Forbidden, "The lambda is a demo, which does not connect to other servers, or it is blocked, of which nothing changes but its block.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "Declining, where nothing was asked for and nothing is allowed.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this public key.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda afterwards")]
    public async ValueTask<AdminLambdaDetail> DecideConnectivity([Summary("The public key of the lambda")] string publicKey, ConnectivityDecision body, IRequest request)
    {
        var privateKey = meta.RequirePrivateKey(publicKey);

        connectivity.Decide(privateKey, body.Allow, body.Note);

        logger.LogInformation(body.Allow ? "Allowed connectivity for lambda {Lambda} by operator" : "Declined connectivity for lambda {Lambda} by operator", publicKey);

        return await DetailAsync(privateKey, request);
    }

    private static AdminConnectivityEntry Describe(ConnectivityListing listing)
        => new(listing.PublicKey, listing.Tier, listing.ActiveVersion, listing.Blocked, ConnectivityResponse.Of(listing.Connectivity), listing.Connects);

    #endregion

    #region Settings

    [ResourceMethod("settings")]
    [Summary("Reads what the operator has switched on or off")]
    [return: Summary("The settings of the installation")]
    public SettingsModel GetSettings()
    {
        var current = settings.Get();

        return new SettingsModel(current.BuildBox, current.ChangeBox);
    }

    [ResourceMethod(Method.Put, "settings")]
    [Summary("Replaces the settings")]
    [Remarks("All of them. They apply to the next page anybody opens.")]
    [return: Summary("The settings, as they were saved")]
    public SettingsModel ChangeSettings(SettingsModel body)
    {
        var saved = settings.Save(new SiteSettings(body.BuildBox, body.ChangeBox));

        logger.LogInformation("Updated settings buildBox {BuildBox} changeBox {ChangeBox} by operator", saved.BuildBox, saved.ChangeBox);

        return new SettingsModel(saved.BuildBox, saved.ChangeBox);
    }

    #endregion

    #region Usage report

    [ResourceMethod("usage-report")]
    [Summary("Reads the usage report")]
    [Remarks("""
             Whether the installation sends its usage report, where to, and the last one exactly as it was sent. Reports are sent
             once a day, the first shortly after the server starts; switching them on or off is the environment's
             (`LAMBDA_USAGE_REPORTS`), not a setting here.
             """)]
    [return: Summary("The usage report and how the last one fared")]
    public UsageReportResponse GetUsageReport()
    {
        var status = usageReports.Status;

        return new UsageReportResponse(status.Enabled, status.Endpoint, status.Attempted, status.Sent, status.Failure, status.Report);
    }

    #endregion

    #region Integrations

    [ResourceMethod("integrations")]
    [Summary("Lists what the installation keeps its data in")]
    [Remarks("""
             The database the platform keeps its records, its analytics and the lambdas' databases in, where the lambdas' files
             are kept, and where their secrets are - each with what it says about where, and whether it is the platform's own,
             which every installation has unless its host chooses another. Chosen by the host as it starts (`LAMBDA_DATABASE` on the community edition), not
             here. Never a password, a key or a connection string.
             """)]
    [return: Summary("The database, the files and the secrets, in that order")]
    public IReadOnlyList<IntegrationResponse> GetIntegrations() => [.. integrations.List().Select(IntegrationResponse.Of)];

    #endregion

    #region Limits

    [ResourceMethod("limits")]
    [Summary("Reads what a lambda may have and do, per tier")]
    [Remarks("""
             Where the operator has not saved them yet, these are the defaults - the built in ones, or what the host of the
             installation sets (genhttp.dev calls its sandbox `Free` and production `Premium`). What a tier is for is fixed: the
             sandbox is taken offline once unused, production and system lambdas are kept and may answer at a domain of their
             own, and system lambdas are the installation's own. What is set here is how much each may have, whether it
             connects to other servers by default, and what it is called on the pages.
             """)]
    [return: Summary("The limits of the installation")]
    public LimitsModel GetLimits() => Describe(limits.Get());

    [ResourceMethod(Method.Put, "limits")]
    [Summary("Replaces the limits, all of them")]
    [Remarks("""
             They apply to what is checked next - a save, an upload, a write to a workspace, a connection to a database, the next
             sweep - and take nothing away that a lambda already has; nothing is built again, but where a tier's connectivity
             default changes. Every number must be greater than zero, no tier may show more days of analytics than are kept (a
             year), production must not allow less than the sandbox, a label is at most 24 characters, and a new lambda starts in
             the sandbox or in production. An empty label is the tier's own name, which the pages translate.
             """)]
    [Response(ResponseStatus.BadRequest, "A value is zero or less, more days of analytics than are kept, production would allow less than the sandbox, a label is too long, or new lambdas would start in a tier of the installation's own. Nothing was changed.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The limits, as they were saved")]
    public LimitsModel ChangeLimits(LimitsModel body)
    {
        var (saved, previous) = limits.Save(new ProductLimits(
            TierDescription.Parse(body.DefaultTier),
            Tier(body.Sandbox),
            Tier(body.Production),
            Tier(body.System),
            body.Versions,
            body.Features,
            TimeSpan.FromHours(body.OfflineAfterHours),
            TimeSpan.FromHours(body.RemovedAfterHours),
            body.ShowcaseImageBytes,
            body.RequestsPerSecond,
            body.BuildsPerDay
        ));

        var before = LimitsService.Flatten(previous).ToDictionary(l => l.Key, l => l.Value);

        foreach (var (key, value) in LimitsService.Flatten(saved).Where(l => before.GetValueOrDefault(l.Key) != l.Value))
        {
            logger.LogInformation("Changed limit {Limit} from {Previous} to {Value} by operator", key, before.GetValueOrDefault(key) ?? "(none)", value);
        }

        return Describe(saved);
    }

    private static TierSettings Tier(TierLimitsModel model)
        => new(model.BuildBytes, model.DataBytes, model.AnalyticsDays, model.Connectivity, model.Label);

    private static TierLimitsModel Describe(TierSettings limits)
        => new(limits.BuildBytes, limits.DataBytes, limits.AnalyticsDays, limits.Connectivity, limits.Label);

    private static LimitsModel Describe(ProductLimits limits)
        => new(limits.DefaultTier.ToString(), Describe(limits.Sandbox), Describe(limits.Production), Describe(limits.System), limits.Versions, limits.Features,
               (int)limits.OfflineAfter.TotalHours, (int)limits.RemovedAfter.TotalHours, limits.ShowcaseImageBytes, limits.RequestsPerSecond, limits.BuildsPerDay);

    #endregion

    #region Helpers

    /// <summary>
    /// How many lambdas a page of the listing holds.
    /// </summary>
    private const int PageSize = 20;

    private async ValueTask<AdminLambdaDetail> DetailAsync(string privateKey, IRequest request)
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
            [.. limits.TiersWhere(p => p.Assignable).Select(t => t.ToString())],
            lambda.Blocked is { } since ? new LambdaBlockResponse(since, lambda.BlockReason) : null,
            [.. meta.GetEditorAddresses(privateKey).Select(Describe)],
            You(request),
            connectivity.Connects(id, lambda.ActiveVersion, lambda.LatestVersion),
            await _summaries.OfAsync(privateKey),
            AdminLambdaUses.Of(meta.GetUses(privateKey)),
            showcases.Get(privateKey) is { } entry ? ShowcaseResponse.Of(entry, addresses) : null,
            sources.Get(privateKey) is { } source ? SourceSettingsResponse.Of(source) : null,
            [.. features.List(privateKey).OrderByDescending(f => f.Modified).Select(FeatureResource.Describe)]
        );
    }

    private EditorAddressResponse Describe(EditorAddress editor)
    {
        // written by the platform, so always an address
        var address = System.Net.IPAddress.Parse(editor.Address);

        return new(editor.Address, NetworkAddress.Describe(NetworkAddress.RangeOf(address)), editor.FirstSeen, editor.LastSeen, blocks.Covers(address));
    }

    private static LambdaOverviewResponse Summarize(LambdaInfo lambda)
        => new(lambda.PublicKey, lambda.ActiveVersion, lambda.DeployedUntil);

    #endregion

}
