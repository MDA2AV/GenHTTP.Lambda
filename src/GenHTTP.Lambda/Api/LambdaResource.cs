using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Workspace;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Everything the editor does, one endpoint at a time. The private key in the
/// path is what authorizes a call - whoever has it may edit the lambda.
/// </summary>
public sealed class LambdaResource(IMetaService meta, IWorkspaceService workspace)
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

        var files = LambdaSource.Parse(content.Code);

        return new VersionContentResponse(content.Version, content.Created, files[0].Code, files);
    }

    /// <summary>
    /// Stores the code as a new version, without putting it online.
    /// </summary>
    [ResourceMethod(Method.Post, ":privateKey/versions")]
    public async ValueTask<Result<VersionResponse>> Save(string privateKey, CodeRequest request)
    {
        var version = await meta.SaveAsync(privateKey, Combine(request));

        return new Result<VersionResponse>(new VersionResponse(version.Version, version.Created)).Status(ResponseStatus.Created);
    }

    /// <summary>
    /// What every name in the given code means, for the colours in the editor.
    /// </summary>
    /// <remarks>
    /// Behind the editor key like the check below it, because it runs the
    /// compiler: an endpoint that binds arbitrary C# for anyone who asks is a
    /// way to spend a server.
    /// </remarks>
    [ResourceMethod(Method.Post, ":privateKey/semantics")]
    public async ValueTask<SemanticsResponse> Semantics(string privateKey, CodeRequest request)
    {
        await RequireAsync(privateKey);

        var tokens = SemanticClassifier.Classify(request.Code ?? string.Empty);

        return new SemanticsResponse([.. tokens.Select(t => new SemanticToken(t.Line, t.Column, t.Length, t.Kind))]);
    }

    /// <summary>
    /// What could be written where the caret is.
    /// </summary>
    /// <param name="privateKey">The lambda being edited</param>
    /// <param name="request">The code and where in it the caret sits</param>
    [ResourceMethod(Method.Post, ":privateKey/completions")]
    public async ValueTask<CompletionsResponse> Completions(string privateKey, CompletionRequest request)
    {
        await RequireAsync(privateKey);

        var found = CompletionResolver.Resolve(request.Code, request.Line, request.Column);

        return new CompletionsResponse([.. found.Select(c => new ResolvedCompletionResponse(c.Label, c.Kind, c.Detail, c.Documentation))]);
    }

    /// <summary>
    /// Compiles the code without storing or deploying it.
    /// </summary>
    [ResourceMethod(Method.Post, ":privateKey/check")]
    public async ValueTask<CompilationResponse> Check(string privateKey, CodeRequest request)
    {
        var outcome = await meta.CheckAsync(privateKey, Combine(request));

        return new CompilationResponse(outcome.Success, outcome.Diagnostics);
    }

    /// <summary>
    /// Turns what was submitted into the single blob a version is stored as.
    /// </summary>
    /// <remarks>
    /// A caller may send files or the one snippet it used to send, and older
    /// clients still send only the snippet. Sending both is answered by the
    /// files, because a client that knows about them meant them.
    /// </remarks>
    private static string Combine(CodeRequest request)
    {
        if (request.Files is not { Count: > 0 } files)
        {
            return request.Code ?? string.Empty;
        }

        if (LambdaSource.Validate(files) is { } complaint)
        {
            throw LambdaException.Invalid(complaint);
        }

        return LambdaSource.Serialize(files);
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

    #region Workspace

    /// <summary>
    /// Lists the files a lambda keeps in its private directory.
    /// </summary>
    [ResourceMethod(":privateKey/files")]
    public async ValueTask<WorkspaceListing> GetFiles(string privateKey)
        => await workspace.ListAsync(await ResolveIdAsync(privateKey));

    /// <summary>
    /// Reads one file.
    /// </summary>
    /// <param name="path">The name of the file, relative to the workspace</param>
    /// <remarks>
    /// The content travels base64 encoded in a JSON document like everything
    /// else this API answers, so a workspace holding an image or an archive
    /// reads the same way as one holding text.
    /// </remarks>
    [ResourceMethod(":privateKey/files/content")]
    public async ValueTask<FileResponse> GetFile(string privateKey, string path)
    {
        var file = await workspace.ReadAsync(await ResolveIdAsync(privateKey), path)
                ?? throw LambdaException.NotFound($"There is no file called '{path}'.");

        return new FileResponse(file.Path, Convert.ToBase64String(file.Content), file.Content.Length);
    }

    /// <summary>
    /// Writes a file, replacing it if it is already there.
    /// </summary>
    /// <param name="path">The name of the file, relative to the workspace</param>
    [ResourceMethod(Method.Put, ":privateKey/files/content")]
    public async ValueTask<WorkspaceEntry> PutFile(string privateKey, string path, FileRequest request)
    {
        byte[] content;

        try
        {
            content = Convert.FromBase64String(request.Content ?? string.Empty);
        }
        catch (FormatException)
        {
            throw LambdaException.Invalid("The content of a file has to be base64 encoded.");
        }

        using var stream = new MemoryStream(content);

        return await workspace.WriteAsync(await ResolveIdAsync(privateKey), path, stream);
    }

    /// <summary>
    /// Removes a file.
    /// </summary>
    /// <param name="path">The name of the file, relative to the workspace</param>
    [ResourceMethod(Method.Delete, ":privateKey/files/content")]
    public async ValueTask DeleteFile(string privateKey, string path)
        => await workspace.DeleteAsync(await ResolveIdAsync(privateKey), path);

    /// <summary>
    /// Turns the editor key into the identity the workspace is filed under,
    /// which doubles as the check that the caller owns the lambda.
    /// </summary>
    private async ValueTask RequireAsync(string privateKey)
        => _ = await meta.GetIdAsync(privateKey)
        ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

    private async ValueTask<long> ResolveIdAsync(string privateKey)
        => await meta.GetIdAsync(privateKey)
        ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        #endregion

}
