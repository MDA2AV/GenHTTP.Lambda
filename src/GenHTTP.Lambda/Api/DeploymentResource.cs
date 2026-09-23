using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Whether a lambda is online, and putting it on or taking it off.
/// </summary>
/// <remarks>
/// A deployment is a process rather than a thing to be edited, so the two
/// ways of changing it are named for what they do instead of being a put and
/// a delete on something that does not quite exist. There is only ever one,
/// hence the singular.
/// </remarks>
public sealed class DeploymentResource(IMetaService meta)
{

    /// <summary>
    /// What is online, and for how long.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/deployment")]
    public async ValueTask<DeploymentResponse> Get(string privateKey)
    {
        var lambda = await meta.RequireAsync(privateKey);

        return new DeploymentResponse(lambda.ActiveVersion != null, lambda.ActiveVersion, lambda.DeployedAt, lambda.DeployedUntil);
    }

    /// <summary>
    /// Builds a version and makes it the one that is served.
    /// </summary>
    /// <remarks>
    /// Code that does not compile is answered with 422 and what the compiler
    /// said, and whatever was online before stays online.
    /// </remarks>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/deployment/start")]
    public async ValueTask<Result<DeploymentOutcomeResponse>> Start(string privateKey, DeploymentRequest? request)
    {
        var result = await meta.DeployAsync(privateKey, request?.Version, VersionOrigins.Api);

        var payload = new DeploymentOutcomeResponse(result.Success, result.Lambda == null ? null : LambdaDescription.Of(result.Lambda), result.Diagnostics);

        return new Result<DeploymentOutcomeResponse>(payload).Status(result.Success ? ResponseStatus.Ok : ResponseStatus.UnprocessableEntity);
    }

    /// <summary>
    /// Takes the lambda off the air, keeping its code.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/deployment/stop")]
    public async ValueTask<LambdaResponse> Stop(string privateKey)
        => LambdaDescription.Of(await meta.UndeployAsync(privateKey, ActivationEndings.Stopped));

    /// <summary>
    /// Every stretch of time the lambda was online, newest first.
    /// </summary>
    /// <remarks>
    /// Which version, who put it up, how long it stayed and what ended it -
    /// the next deployment, the owner, the sweep that takes down what nobody
    /// uses, or the operator. The newest entry has no end while it is live.
    /// </remarks>
    [ResourceMethod("lambdas/:privateKey/deployment/history")]
    public async ValueTask<List<ActivationResponse>> History(string privateKey)
    {
        var now = DateTime.UtcNow;

        var activations = await meta.GetActivationsAsync(privateKey);

        return activations.Select(a => new ActivationResponse(a.Version, a.Started, a.Origin, a.Ended, a.EndedBy,
                                                              (long)((a.Ended ?? now) - a.Started).TotalSeconds))
                          .ToList();
    }

}
