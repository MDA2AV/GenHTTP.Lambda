using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Everything the editor does, one endpoint at a time. The private key in the
/// path is what authorizes a call - whoever has it may edit the lambda.
/// </summary>
public sealed class LambdaResource(IMetaService meta)
{

    #region Creation

    /// <summary>
    /// Creates a new lambda and returns its keys.
    /// </summary>
    [ResourceMethod(Method.Post)]
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
    /// Checks whether a key is free before a lambda is created.
    /// </summary>
    [ResourceMethod("keys/:publicKey")]
    public async ValueTask<AvailabilityResponse> CheckKey(string publicKey)
    {
        var availability = await meta.CheckKeyAsync(publicKey);

        return new AvailabilityResponse(availability.PublicKey, availability.Available, availability.Reason);
    }

    /// <summary>
    /// Tells whether a public key is serving, so the frontend can explain an
    /// empty lambda page.
    /// </summary>
    [ResourceMethod("public/:publicKey")]
    public async ValueTask<StatusResponse> GetStatus(string publicKey)
    {
        var status = await meta.GetStatusAsync(publicKey);

        return new StatusResponse(status.PublicKey, status.Exists, status.Deployed);
    }

    #endregion

    #region Lambda

    /// <summary>
    /// Reads a lambda by the private key of its editor.
    /// </summary>
    [ResourceMethod(":privateKey")]
    public async ValueTask<LambdaResponse> Get(string privateKey)
    {
        var lambda = await meta.GetAsync(privateKey) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        return LambdaDescription.Of(lambda);
    }

    /// <summary>
    /// Moves the lambda to another public key.
    /// </summary>
    [ResourceMethod(Method.Put, ":privateKey/key")]
    public async ValueTask<LambdaResponse> ChangeKey(string privateKey, ChangeKeyRequest request)
        => LambdaDescription.Of(await meta.ChangeKeyAsync(privateKey, request.PublicKey));

    /// <summary>
    /// Removes the lambda for good.
    /// </summary>
    [ResourceMethod(Method.Delete, ":privateKey")]
    public async ValueTask Delete(string privateKey) => await meta.DeleteAsync(privateKey);

    #endregion

    #region Versions

    /// <summary>
    /// Lists the stored versions, newest first.
    /// </summary>
    [ResourceMethod(":privateKey/versions")]
    public async ValueTask<List<VersionResponse>> GetVersions(string privateKey)
    {
        var versions = await meta.GetVersionsAsync(privateKey);

        return versions.Select(v => new VersionResponse(v.Version, v.Created)).ToList();
    }

    /// <summary>
    /// Reads the code of a single version.
    /// </summary>
    [ResourceMethod(":privateKey/versions/:version")]
    public async ValueTask<VersionContentResponse> GetVersion(string privateKey, int version)
    {
        var content = await meta.GetVersionAsync(privateKey, version);

        return new VersionContentResponse(content.Version, content.Created, content.Code);
    }

    /// <summary>
    /// Stores the code as a new version, without putting it online.
    /// </summary>
    [ResourceMethod(Method.Post, ":privateKey/versions")]
    public async ValueTask<Result<VersionResponse>> Save(string privateKey, CodeRequest request)
    {
        var version = await meta.SaveAsync(privateKey, request.Code);

        return new Result<VersionResponse>(new VersionResponse(version.Version, version.Created)).Status(ResponseStatus.Created);
    }

    /// <summary>
    /// Compiles the code without storing or deploying it.
    /// </summary>
    [ResourceMethod(Method.Post, ":privateKey/check")]
    public async ValueTask<CompilationResponse> Check(string privateKey, CodeRequest request)
    {
        var outcome = await meta.CheckAsync(privateKey, request.Code);

        return new CompilationResponse(outcome.Success, outcome.Diagnostics);
    }

    #endregion

    #region Deployment

    /// <summary>
    /// Builds a version and makes it the one that is served.
    /// </summary>
    [ResourceMethod(Method.Post, ":privateKey/deployment")]
    public async ValueTask<Result<DeploymentResponse>> Deploy(string privateKey, DeployRequest? request)
    {
        var result = await meta.DeployAsync(privateKey, request?.Version);

        var payload = new DeploymentResponse(result.Success, result.Lambda == null ? null : LambdaDescription.Of(result.Lambda), result.Diagnostics);

        return new Result<DeploymentResponse>(payload).Status(result.Success ? ResponseStatus.Ok : ResponseStatus.UnprocessableEntity);
    }

    /// <summary>
    /// Takes the lambda off the air, keeping its code.
    /// </summary>
    [ResourceMethod(Method.Delete, ":privateKey/deployment")]
    public async ValueTask<LambdaResponse> Undeploy(string privateKey) => LambdaDescription.Of(await meta.UndeployAsync(privateKey));

    #endregion

    #region Mapping


    #endregion

}
