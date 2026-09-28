using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Workspace;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

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
public sealed class FeatureWorkspaceResource(IFeatureService features, IWorkspaceService workspace)
{

    /// <summary>
    /// Lists every file and folder of the feature's copy of the workspace.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/features/:feature/workspace")]
    public async ValueTask<WorkspaceListing> List(string privateKey, string feature)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, false);

        return await workspace.ListAsync(lambdaId, featureId);
    }

    /// <summary>
    /// Reads one file, base64 encoded, up to 32 MB.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod("lambdas/:privateKey/features/:feature/workspace/:path")]
    public async ValueTask<FileResponse> Get(string privateKey, string feature, string path)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, false);

        return await WorkspaceFiles.ReadAsync(workspace, lambdaId, featureId, path, $"/api/v1/lambdas/{{privateKey}}/features/{feature}/workspace");
    }

    /// <summary>
    /// Reads one file as it is, streamed from the disk, however large.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod("lambdas/:privateKey/features/:feature/workspace/:path/content")]
    public async ValueTask<IResponse> GetContent(string privateKey, string feature, string path, IRequest request)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, false);

        return await WorkspaceFiles.StreamAsync(workspace, lambdaId, featureId, path, request);
    }

    /// <summary>
    /// Writes a file from the body as it is, streamed to the disk however large.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/workspace/:path/content")]
    public async ValueTask<WorkspaceEntry> PutContent(string privateKey, string feature, string path, IRequest request)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, true);

        return await WorkspaceFiles.ReceiveAsync(workspace, lambdaId, featureId, path, request);
    }

    /// <summary>
    /// Writes a file, base64 encoded, replacing it if it is already there.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/workspace/:path")]
    public async ValueTask<WorkspaceEntry> Put(string privateKey, string feature, string path, FileRequest request)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, true);

        return await WorkspaceFiles.WriteAsync(workspace, lambdaId, featureId, path, request);
    }

    /// <summary>
    /// Removes a file, or a folder and everything in it.
    /// </summary>
    /// <param name="path">The path within the workspace</param>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/features/:feature/workspace/:path")]
    public async ValueTask Delete(string privateKey, string feature, string path)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, true);

        await workspace.DeleteAsync(lambdaId, path, featureId);
    }

    /// <summary>
    /// Makes a folder, so files can be put into it.
    /// </summary>
    /// <param name="path">Where it goes within the workspace</param>
    /// <returns>The workspace afterwards</returns>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/folders/:path")]
    public async ValueTask<WorkspaceListing> PutFolder(string privateKey, string feature, string path)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, true);

        return await WorkspaceFiles.CreateFolderAsync(workspace, lambdaId, featureId, path);
    }

}
