using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Telemetry;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Every lambda on this installation, for whoever runs it.
/// </summary>
/// <remarks>
/// Everything else in this API is authorized by holding the editor link of the
/// one lambda it touches. This resource is the exception: it reads code that
/// belongs to other people and can take their lambdas down, so it asks for a
/// token instead - and answers nothing at all until one is configured, because
/// a panel that is off cannot be left open by accident.
/// </remarks>
public sealed class AdminResource(IMetaService meta, LambdaTelemetry telemetry, LambdaOptions options)
{

    #region Functionality

    /// <summary>
    /// Every lambda, newest first.
    /// </summary>
    [ResourceMethod("lambdas")]
    public async ValueTask<AdminListingResponse> GetLambdas(string? search, int? page, IRequest request)
    {
        Authorize(request);

        var wanted = Math.Max(1, page ?? 1);

        var result = await meta.ListAsync(search, (wanted - 1) * PageSize, PageSize);

        // the counters are held per lambda in memory, so this is a lookup
        // rather than a join - and a lambda nobody has called is simply absent
        var activity = telemetry.Describe().ToDictionary(a => a.PublicKey, StringComparer.Ordinal);

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
                seen?.LastSeen
            );
        }).ToList();

        var pages = Math.Max(1, (result.Matched + PageSize - 1) / PageSize);

        return new AdminListingResponse(lambdas, result.Total, result.Deployed, result.Matched, Math.Min(wanted, pages), pages);
    }

    /// <summary>
    /// The code of one version of a lambda.
    /// </summary>
    /// <remarks>
    /// Which versions there are is in the listing, as the latest one each
    /// lambda has.
    /// </remarks>
    [ResourceMethod("lambdas/:publicKey/versions/:version")]
    public async ValueTask<VersionContentResponse> GetVersion(string publicKey, int version, IRequest request)
    {
        Authorize(request);

        var content = await meta.GetVersionAsync(await meta.RequirePrivateKeyAsync(publicKey), version);

        return VersionResource.Describe(content);
    }

    /// <summary>
    /// Takes a lambda offline without removing it.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas/:publicKey/deployment/stop")]
    public async ValueTask<LambdaOverviewResponse> StopDeployment(string publicKey, IRequest request)
    {
        Authorize(request);

        var lambda = await meta.UndeployAsync(await meta.RequirePrivateKeyAsync(publicKey), ActivationEndings.Admin);

        return Summarize(lambda);
    }

    /// <summary>
    /// Removes a lambda and everything stored for it.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:publicKey")]
    public async ValueTask Delete(string publicKey, IRequest request)
    {
        Authorize(request);

        await meta.DeleteAsync(await meta.RequirePrivateKeyAsync(publicKey));
    }

    /// <summary>
    /// How many lambdas a page of the listing holds.
    /// </summary>
    private const int PageSize = 20;

    private void Authorize(IRequest request) => AdminGate.Require(request, options);

    private static LambdaOverviewResponse Summarize(LambdaInfo lambda)
        => new(lambda.PublicKey, lambda.ActiveVersion, lambda.DeployedUntil);

    #endregion

}
