using GenHTTP.Api.Protocol;

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
/// its slashes encoded.
/// </remarks>
public sealed class FeatureWorkspaceResource(IFeatureService features, IWorkspaceService workspace, IMetaService meta, ILogger<FeatureWorkspaceResource> logger)
{

    /// <summary>
    /// Lists every file and folder of the feature's copy of the workspace.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/features/:feature/workspace")]
    public WorkspaceListing List(string privateKey, string feature)
    {
        var (lambdaId, featureId) = features.Require(privateKey, feature, false);

        return workspace.List(lambdaId, featureId);
    }

    /// <summary>
    /// Reads one file, base64 encoded, up to 32 MB.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod("lambdas/:privateKey/features/:feature/workspace/:path")]
    public async ValueTask<FileResponse> Get(string privateKey, string feature, string path)
    {
        var (lambdaId, featureId) = features.Require(privateKey, feature, false);

        return await WorkspaceFiles.ReadAsync(workspace, lambdaId, featureId, path, $"/api/v1/lambdas/{{privateKey}}/features/{feature}/workspace");
    }

    /// <summary>
    /// Reads one file as it is, streamed from the disk, however large.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod("lambdas/:privateKey/features/:feature/workspace/:path/content")]
    public IResponse GetContent(string privateKey, string feature, string path, IRequest request)
    {
        var (lambdaId, featureId) = features.Require(privateKey, feature, false);

        return WorkspaceFiles.Send(workspace, lambdaId, featureId, path, request);
    }

    /// <summary>
    /// Writes a file from the body as it is, streamed to the disk however large.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/workspace/:path/content")]
    public async ValueTask<WorkspaceEntry> PutContent(string privateKey, string feature, string path, IRequest request)
    {
        var (lambdaId, featureId) = features.Require(privateKey, feature, true);

        return Written(privateKey, feature, await WorkspaceFiles.ReceiveAsync(workspace, lambdaId, featureId, path, request));
    }

    /// <summary>
    /// Writes a file, base64 encoded, replacing it if it is already there.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/workspace/:path")]
    public async ValueTask<WorkspaceEntry> Put(string privateKey, string feature, string path, FileRequest request)
    {
        var (lambdaId, featureId) = features.Require(privateKey, feature, true);

        return Written(privateKey, feature, await workspace.WriteEncodedAsync(lambdaId, path, request.Content, featureId));
    }

    /// <summary>
    /// Removes a file, or a folder and everything in it.
    /// </summary>
    /// <param name="path">The path within the workspace</param>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/features/:feature/workspace/:path")]
    public void Delete(string privateKey, string feature, string path)
    {
        var (lambdaId, featureId) = features.Require(privateKey, feature, true);

        workspace.Delete(lambdaId, path, featureId);

        logger.LogInformation("Deleted workspace path {Path} of feature '{Feature}' of lambda {Lambda}", path,
                              features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey));
    }

    /// <summary>
    /// Makes a folder, so files can be put into it.
    /// </summary>
    /// <param name="path">Where it goes within the workspace</param>
    /// <returns>The workspace afterwards</returns>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/folders/:path")]
    public WorkspaceListing PutFolder(string privateKey, string feature, string path)
    {
        var (lambdaId, featureId) = features.Require(privateKey, feature, true);

        var listing = WorkspaceFiles.CreateFolder(workspace, lambdaId, featureId, path);

        logger.LogInformation("Created workspace folder {Path} of feature '{Feature}' of lambda {Lambda}", path,
                              features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey));

        return listing;
    }

    private WorkspaceEntry Written(string privateKey, string feature, WorkspaceEntry written)
    {
        logger.LogInformation("Wrote workspace file {Path} of feature '{Feature}' of lambda {Lambda} size {Size}", written.Path,
                              features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey), written.Size);

        return written;
    }

}
