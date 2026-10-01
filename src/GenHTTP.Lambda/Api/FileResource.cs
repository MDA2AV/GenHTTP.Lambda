using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

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
public sealed class FileResource(IMetaService meta, IWorkspaceService workspace, ILogger<FileResource> logger)
{

    /// <summary>
    /// Lists every file and folder.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/files")]
    public WorkspaceListing List(string privateKey)
        => workspace.List(meta.RequireId(privateKey));

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
        => await WorkspaceFiles.ReadAsync(workspace, meta.RequireId(privateKey), null, path, "/api/v1/lambdas/{privateKey}/files");

    /// <summary>
    /// Reads one file as it is, streamed from the disk, however large.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod("lambdas/:privateKey/files/:path/content")]
    public IResponse GetContent(string privateKey, string path, IRequest request)
        => WorkspaceFiles.Send(workspace, meta.RequireId(privateKey), null, path, request);

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
        => Written(privateKey, await WorkspaceFiles.ReceiveAsync(workspace, meta.RequireEditable(privateKey), null, path, request));

    /// <summary>
    /// Writes a file, replacing it if it is already there.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/files/:path")]
    public async ValueTask<WorkspaceEntry> Put(string privateKey, string path, FileRequest request)
        => Written(privateKey, await WorkspaceFiles.WriteAsync(workspace, meta.RequireEditable(privateKey), null, path, request));

    /// <summary>
    /// Removes a file, or a folder and everything in it.
    /// </summary>
    /// <param name="path">The path within the workspace</param>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/files/:path")]
    public void Delete(string privateKey, string path)
    {
        workspace.Delete(meta.RequireEditable(privateKey), path);

        logger.LogInformation("Deleted {Path} from the workspace of lambda {Lambda}", path, meta.PublicKeyOf(privateKey));
    }

    /// <summary>
    /// Makes a folder, so files can be put into it.
    /// </summary>
    /// <param name="path">Where it goes within the workspace</param>
    /// <returns>The workspace afterwards</returns>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/folders/:path")]
    public WorkspaceListing PutFolder(string privateKey, string path)
    {
        var listing = WorkspaceFiles.CreateFolder(workspace, meta.RequireEditable(privateKey), null, path);

        logger.LogInformation("Created the folder {Path} in the workspace of lambda {Lambda}", path, meta.PublicKeyOf(privateKey));

        return listing;
    }

    private WorkspaceEntry Written(string privateKey, WorkspaceEntry written)
    {
        logger.LogInformation("Wrote {Path} ({Size:N0} bytes) to the workspace of lambda {Lambda}", written.Path, written.Size, meta.PublicKeyOf(privateKey));

        return written;
    }

}
