using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The files a lambda keeps in its private directory - its workspace, which is
/// the lambda's data rather than a part of any version.
/// </summary>
/// <remarks>
/// A file is addressed by its path within the workspace, sent as a single
/// segment with the slashes encoded (<c>logs%2Ftoday.txt</c>), so that a path
/// that happens to end in something this API routes on cannot be mistaken
/// for it. A feature's copy of the workspace is reached the same way below
/// <c>/lambdas/{privateKey}/features/{feature}/workspace</c>.
/// </remarks>
public sealed class FileResource(IMetaService meta, IWorkspaceService workspace)
{

    /// <summary>
    /// Lists every file and folder.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/files")]
    public async ValueTask<WorkspaceListing> List(string privateKey)
        => await workspace.ListAsync(await meta.RequireIdAsync(privateKey));

    /// <summary>
    /// Reads one file.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    /// <remarks>
    /// The content travels base64 encoded in a JSON document like everything
    /// else this API answers, so a workspace holding an image or an archive
    /// reads the same way as one holding text. Up to 32 MB; anything larger
    /// is read from <c>…/content</c>.
    /// </remarks>
    [ResourceMethod("lambdas/:privateKey/files/:path")]
    public async ValueTask<FileResponse> Get(string privateKey, string path)
        => await WorkspaceFiles.ReadAsync(workspace, await meta.RequireIdAsync(privateKey), null, path, "/api/v1/lambdas/{privateKey}/files");

    /// <summary>
    /// Reads one file as it is, streamed from the disk, however large.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod("lambdas/:privateKey/files/:path/content")]
    public async ValueTask<IResponse> GetContent(string privateKey, string path, IRequest request)
        => await WorkspaceFiles.StreamAsync(workspace, await meta.RequireIdAsync(privateKey), null, path, request);

    /// <summary>
    /// Writes a file from the body as it is, replacing it if it is already
    /// there.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    /// <remarks>
    /// Written to the disk as it arrives, so a file of any size costs the
    /// server a buffer rather than itself - the way in for a model, a dataset
    /// or anything else too large for a JSON document.
    /// </remarks>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/files/:path/content")]
    public async ValueTask<WorkspaceEntry> PutContent(string privateKey, string path, IRequest request)
        => await WorkspaceFiles.ReceiveAsync(workspace, await meta.RequireEditableAsync(privateKey), null, path, request);

    /// <summary>
    /// Writes a file, replacing it if it is already there.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/files/:path")]
    public async ValueTask<WorkspaceEntry> Put(string privateKey, string path, FileRequest request)
        => await WorkspaceFiles.WriteAsync(workspace, await meta.RequireEditableAsync(privateKey), null, path, request);

    /// <summary>
    /// Removes a file, or a folder and everything in it.
    /// </summary>
    /// <param name="path">The path within the workspace</param>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/files/:path")]
    public async ValueTask Delete(string privateKey, string path)
        => await workspace.DeleteAsync(await meta.RequireEditableAsync(privateKey), path);

    /// <summary>
    /// Makes a folder, so files can be put into it.
    /// </summary>
    /// <param name="path">Where it goes within the workspace</param>
    /// <returns>The workspace afterwards</returns>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/folders/:path")]
    public async ValueTask<WorkspaceListing> PutFolder(string privateKey, string path)
        => await WorkspaceFiles.CreateFolderAsync(workspace, await meta.RequireEditableAsync(privateKey), null, path);

}
