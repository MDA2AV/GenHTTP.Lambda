using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The lambdas themselves: made, read, renamed, removed.
/// </summary>
/// <remarks>
/// A lambda is addressed by its editor key rather than its public one, because
/// the key in the path is what authorizes the call - whoever has it may edit
/// the lambda. Its versions, deployment, files and code live in resources of
/// their own below the same path.
/// </remarks>
public sealed class LambdaResource(IMetaService meta)
{

    /// <summary>
    /// Creates a new lambda and returns its keys.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas")]
    public async ValueTask<Result<LambdaResponse>> Create(CreateLambdaRequest request)
    {
        if (!request.AcceptedTerms)
        {
            throw LambdaException.Invalid("The terms of service need to be accepted.");
        }

        var lambda = await meta.CreateAsync(request.PublicKey, request.Template);

        return new Result<LambdaResponse>(LambdaDescription.Of(lambda)).Status(ResponseStatus.Created);
    }

    /// <summary>
    /// Reads a lambda by the private key of its editor.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey")]
    public async ValueTask<LambdaResponse> Get(string privateKey)
        => LambdaDescription.Of(await meta.RequireAsync(privateKey));

    /// <summary>
    /// Changes a lambda. What the request leaves out stays as it is.
    /// </summary>
    /// <remarks>
    /// The public key is the only thing there is to change so far. Moving it
    /// moves the address the lambda answers at, and the old one is free for
    /// anybody to claim afterwards.
    /// </remarks>
    [ResourceMethod(Method.Patch, "lambdas/:privateKey")]
    public async ValueTask<LambdaResponse> Update(string privateKey, UpdateLambdaRequest request)
    {
        var lambda = request.PublicKey is { } publicKey
                   ? await meta.ChangeKeyAsync(privateKey, publicKey)
                   : await meta.RequireAsync(privateKey);

        return LambdaDescription.Of(lambda);
    }

    /// <summary>
    /// Removes the lambda for good.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey")]
    public async ValueTask Delete(string privateKey) => await meta.DeleteAsync(privateKey);

    /// <summary>
    /// The lambda as a project that can be opened and run.
    /// </summary>
    /// <remarks>
    /// A way out. Whatever somebody writes here runs on a machine they do not
    /// own, for as long as it is left running; being able to take it away is
    /// the difference between a place to build something and a place it is
    /// stuck.
    /// </remarks>
    /// <param name="privateKey">The lambda being taken away</param>
    [ResourceMethod("lambdas/:privateKey/export")]
    public async ValueTask<IResponse> Export(string privateKey, IRequest request)
    {
        var lambda = await meta.RequireAsync(privateKey);

        if (lambda.LatestVersion is not { } latest)
        {
            throw LambdaException.NotFound("That lambda has nothing saved to take away yet.");
        }

        var content = await meta.GetVersionAsync(privateKey, latest);

        var zip = ProjectPacker.Pack(lambda.PublicKey, LambdaSource.Parse(content.Code));

        return request.Respond()
                      .Content(new ZippedProject(zip))
                      .Header("Content-Disposition", $"attachment; filename=\"{lambda.PublicKey}.zip\"")
                      .Build();
    }

}
