using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// A feature's copy of the lambda's workspace: what its preview reads and
/// writes, while the lambda's own stays as it is.
/// </summary>
/// <remarks>
/// Answered exactly like the lambda's workspace below <c>/lambdas/{privateKey}/files</c>,
/// under the same quota, with the path of a file sent as a single segment with
/// its slashes encoded. Listed under the tag of the workspace, which
/// <see cref="FileResource"/> describes.
/// </remarks>
[Tag(ApiDocumentation.Workspace)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoFeature, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class FeatureWorkspaceResource(IFeatureService features, IWorkspaceService workspace, IMetaService meta, ILogger<FeatureWorkspaceResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/features/:feature/workspace")]
    [Summary("Lists the files of a feature's workspace")]
    [Remarks("What the feature's preview reads and writes. The lambda's own workspace is not touched by it.")]
    [return: Summary("The files and folders of the feature's copy")]
    public async ValueTask<WorkspaceListing> List([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, false);

        return await workspace.ListAsync(lambdaId, featureId);
    }

    [ResourceMethod("lambdas/:privateKey/features/:feature/workspace/:path")]
    [Summary("Reads a file of a feature's workspace")]
    [Remarks("Up to 32 MB; a larger file is read as it is from `…/{path}/content`.")]
    [Response(ResponseStatus.BadRequest, "The file is larger than 32 MB, or the path is empty or leads outside of the workspace.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The file, base64 encoded")]
    public async ValueTask<FileResponse> Get([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                             [Summary(ApiDocumentation.WorkspacePath)] string path)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, false);

        return await WorkspaceFiles.ReadAsync(workspace, lambdaId, featureId, path, $"/api/v1/lambdas/{{privateKey}}/features/{feature}/workspace");
    }

    [ResourceMethod("lambdas/:privateKey/features/:feature/workspace/:path/content")]
    [Summary("Downloads a file of a feature's workspace")]
    [Response(ResponseStatus.Ok, "The bytes of the file", ContentType = "application/octet-stream")]
    [ResponseHeader(ResponseStatus.Ok, "Content-Disposition", "`attachment`, with the name of the file")]
    public async ValueTask<IResponse> GetContent([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                                 [Summary(ApiDocumentation.WorkspacePath)] string path, IRequest request)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, false);

        return await WorkspaceFiles.SendAsync(workspace, lambdaId, featureId, path, request);
    }

    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/workspace/:path/content")]
    [Summary("Uploads a file into a feature's workspace")]
    [Remarks("As for the lambda's own workspace: of any size, replacing what is there only once the upload is complete.")]
    [RequestBody("The bytes of the file", ContentType = "application/octet-stream")]
    [Response(ResponseStatus.BadRequest, "The file does not fit into the data allowance of the lambda, the upload ended early, or the path cannot be used. Nothing was written.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The lambda's workspace is switched off.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The file as it was written")]
    public async ValueTask<WorkspaceEntry> PutContent([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                                      [Summary(ApiDocumentation.WorkspacePath)] string path, IRequest request)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, true);

        return await WrittenAsync(privateKey, feature, await WorkspaceFiles.ReceiveAsync(workspace, lambdaId, featureId, path, request));
    }

    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/workspace/:path")]
    [Summary("Writes a file into a feature's workspace")]
    [Response(ResponseStatus.BadRequest, "The content is not base64, it does not fit into the data allowance of the lambda, or the path cannot be used. Nothing was written.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The lambda's workspace is switched off.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The file as it was written")]
    public async ValueTask<WorkspaceEntry> Put([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                               [Summary(ApiDocumentation.WorkspacePath)] string path, FileRequest request)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, true);

        return await WrittenAsync(privateKey, feature, await workspace.WriteEncodedAsync(lambdaId, path, request.Content, featureId));
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/features/:feature/workspace/:path")]
    [Summary("Deletes from a feature's workspace")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    [Response(ResponseStatus.BadRequest, "The path is empty or leads outside of the workspace.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask Delete([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                  [Summary(ApiDocumentation.WorkspacePath)] string path)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, true);

        await workspace.DeleteAsync(lambdaId, path, featureId);

        logger.LogInformation("Deleted workspace path {Path} of feature '{Feature}' of lambda {Lambda}", path,
                              await features.NameOfAsync(privateKey, feature), await meta.PublicKeyOfAsync(privateKey));
    }

    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/folders/:path")]
    [Summary("Makes a folder in a feature's workspace")]
    [Response(ResponseStatus.BadRequest, "A file has that path, there is no room for another folder, or the path cannot be used.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The lambda's workspace is switched off.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The feature's copy of the workspace afterwards")]
    public async ValueTask<WorkspaceListing> PutFolder([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                                       [Summary("Where the folder goes within the workspace, sent as one segment with its slashes encoded")] string path)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, true);

        var listing = await WorkspaceFiles.CreateFolderAsync(workspace, lambdaId, featureId, path);

        logger.LogInformation("Created workspace folder {Path} of feature '{Feature}' of lambda {Lambda}", path,
                              await features.NameOfAsync(privateKey, feature), await meta.PublicKeyOfAsync(privateKey));

        return listing;
    }

    private async ValueTask<WorkspaceEntry> WrittenAsync(string privateKey, string feature, WorkspaceEntry written)
    {
        logger.LogInformation("Wrote workspace file {Path} of feature '{Feature}' of lambda {Lambda} size {Size}", written.Path,
                              await features.NameOfAsync(privateKey, feature), await meta.PublicKeyOfAsync(privateKey), written.Size);

        return written;
    }

}
