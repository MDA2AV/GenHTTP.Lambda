using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// A deployment is a process rather than a thing to be edited, so the two
/// ways of changing it are named for what they do instead of being a put and
/// a delete on something that does not quite exist. There is only ever one,
/// hence the singular.
/// </remarks>
[Tag(ApiDocumentation.Deployment)]
[Summary("Whether a lambda is online, putting a version online, and taking it offline.")]
[Remarks("""
         At most one version of a lambda is online at a time, and it serves every request to the lambda's address.
         Deploying compiles the version and switches over to it once it compiles; until then, and if it does not, whatever
         was online stays online. Deploying an older version is how a lambda is rolled back. Neither deploying nor rolling
         back touches the data.

         A free lambda that nobody visits or edits for a while is taken offline by the platform (`deployedUntil` says when),
         and can be deployed again at any time.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class DeploymentResource(IMetaService meta, ILogger<DeploymentResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/deployment")]
    [Summary("Reads what is online")]
    [Remarks("Which version, since when, and until when it stays online unless it is used.")]
    [return: Summary("What is online")]
    public DeploymentResponse Get([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var lambda = meta.Require(privateKey);

        return new DeploymentResponse(lambda.ActiveVersion != null, lambda.ActiveVersion, lambda.DeployedAt, lambda.DeployedUntil);
    }

    [ResourceMethod(Method.Post, "lambdas/:privateKey/deployment/start")]
    [Summary("Puts a version online")]
    [Remarks("""
             Compiles the version and, once it compiles, serves it from the next request on, in place of whatever was online.
             The code that runs once as the lambda starts - outside the handler it returns, migrating its database, say - runs
             now, and what it prints is in the lambda's log. The body is optional: without one, or without a `version`, the
             newest version goes online.
             """)]
    [Response(ResponseStatus.Ok, "The version is online. `diagnostics` may hold warnings of the compiler.")]
    [Response(ResponseStatus.UnprocessableEntity, ApiDocumentation.Diagnostics, Type = typeof(DeploymentOutcomeResponse))]
    [Response(ResponseStatus.BadRequest, "The lambda has no version yet.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or it has no such version - it never existed, or it was not kept.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<Result<DeploymentOutcomeResponse>> Start([Summary(ApiDocumentation.PrivateKey)] string privateKey, DeploymentRequest? request)
    {
        var result = await meta.DeployAsync(privateKey, request?.Version, VersionOrigins.Api);

        logger.Deployed(result, meta.PublicKeyOf(privateKey), request?.Version);

        var payload = new DeploymentOutcomeResponse(result.Success, result.Lambda == null ? null : LambdaDescription.Of(result.Lambda), result.Diagnostics);

        return new Result<DeploymentOutcomeResponse>(payload).Status(result.Success ? ResponseStatus.Ok : ResponseStatus.UnprocessableEntity);
    }

    [ResourceMethod(Method.Post, "lambdas/:privateKey/deployment/stop")]
    [Summary("Takes the lambda offline")]
    [Remarks("""
             From the next request on, its address answers that nothing is online. Its versions and its data are kept, and
             any version can be deployed again. Nothing happens if it is offline already. The previews of its features are
             not affected.
             """)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda, offline")]
    public LambdaResponse Stop([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var lambda = meta.Undeploy(privateKey, ActivationEndings.Stopped);

        logger.LogInformation("Undeployed lambda {Lambda}", lambda.PublicKey);

        return LambdaDescription.Of(lambda);
    }

    [ResourceMethod("lambdas/:privateKey/deployment/history")]
    [Summary("Lists when the lambda was online")]
    [Remarks("""
             Which version, who put it online, how long it stayed and what ended it: the next deployment, its owner, the
             platform because nobody used it, or the operator. The newest entry has no end while it is online. The last 200
             are kept.
             """)]
    [return: Summary("The stretches, newest first")]
    public List<ActivationResponse> History([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var now = DateTime.UtcNow;

        var activations = meta.GetActivations(privateKey);

        return activations.Select(a => new ActivationResponse(a.Version, a.Started, a.Origin, a.Ended, a.EndedBy,
                                                              (long)((a.Ended ?? now) - a.Started).TotalSeconds))
                          .ToList();
    }

}
