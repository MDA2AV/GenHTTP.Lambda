using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// A file is addressed by its path within the workspace, sent as a single
/// segment with the slashes encoded (<c>logs%2Ftoday.txt</c>), so that a path
/// that happens to end in something this API routes on cannot be mistaken
/// for it. The description of the tag is here; the feature's copy, in
/// <see cref="FeatureWorkspaceResource"/>, shares it.
/// </remarks>
[Tag(ApiDocumentation.Workspace)]
[Summary("The files a lambda keeps: its workspace, which its code reads and writes while it runs.")]
[Remarks("""
         The workspace is data, not part of a version: every version reads and writes the same files, and deploying or rolling
         back leaves them alone. It is where uploads, generated files and large files that are no part of the program - a
         model, a dataset, media - go. It is switched on unless its owner switched it off (`DELETE …/data/workspace`, which
         deletes every file); while it is off, nothing can be written.

         A file is addressed by its path within the workspace, sent as **one** segment with its slashes encoded:
         `GET /lambdas/{privateKey}/files/logs%2Ftoday.txt`. Files are written as they are with `PUT …/content`, of any size, or
         base64 encoded in JSON. Either way, the room it takes counts towards the data allowance of the lambda - workspace and
         database together - in blocks of 4 KB, a folder taking one block.

         A feature has a copy of the workspace, reached the same way below `/lambdas/{privateKey}/features/{feature}/`.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class FileResource(IMetaService meta, IWorkspaceService workspace, ILogger<FileResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/files")]
    [Summary("Lists the files of the workspace")]
    [return: Summary("The files and folders of the workspace")]
    public WorkspaceListing List([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => workspace.List(meta.RequireId(privateKey));

    [ResourceMethod("lambdas/:privateKey/files/:path")]
    [Summary("Reads a file, base64 encoded")]
    [Remarks("""
             The content travels base64 encoded in JSON, so an image or an archive reads the same way as text. Up to 32 MB; a
             larger file is refused here and read as it is from `…/{path}/content`.
             """)]
    [Response(ResponseStatus.BadRequest, "The file is larger than 32 MB, or the path is empty or leads outside of the workspace.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or there is no such file.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The file, base64 encoded")]
    public async ValueTask<FileResponse> Get([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.WorkspacePath)] string path)
        => await WorkspaceFiles.ReadAsync(workspace, meta.RequireId(privateKey), null, path, "/api/v1/lambdas/{privateKey}/files");

    [ResourceMethod("lambdas/:privateKey/files/:path/content")]
    [Summary("Downloads a file as it is, however large")]
    [Remarks("Always sent as bytes to save, whatever the name of the file says, so a page somebody uploaded never runs on this site.")]
    [Response(ResponseStatus.Ok, "The bytes of the file", ContentType = "application/octet-stream")]
    [ResponseHeader(ResponseStatus.Ok, "Content-Disposition", "`attachment`, with the name of the file")]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or there is no such file.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public IResponse GetContent([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.WorkspacePath)] string path, IRequest request)
        => WorkspaceFiles.Send(workspace, meta.RequireId(privateKey), null, path, request);

    [ResourceMethod(Method.Put, "lambdas/:privateKey/files/:path/content")]
    [Summary("Uploads a file as it is")]
    [Remarks("""
             The body is the file. It replaces a file of the same path, and the folders it goes into are made. It is written as
             it arrives and only takes the place of the old one once it is complete: an upload that is refused for want of room,
             or that ends before the `Content-Length` it announced, leaves what was there as it was. The way in for anything too
             large for JSON.
             """)]
    [RequestBody("The bytes of the file", ContentType = "application/octet-stream")]
    [Response(ResponseStatus.BadRequest, "The file does not fit into the data allowance of the lambda, the upload ended early, or the path cannot be used. Nothing was written.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The workspace is switched off.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The file as it was written")]
    public async ValueTask<WorkspaceEntry> PutContent([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.WorkspacePath)] string path,
                                                      IRequest request)
        => Written(privateKey, await WorkspaceFiles.ReceiveAsync(workspace, meta.RequireEditable(privateKey), null, path, request));

    [ResourceMethod(Method.Put, "lambdas/:privateKey/files/:path")]
    [Summary("Writes a file sent base64 encoded")]
    [Remarks("Replaces a file of the same path, and makes the folders it goes into. For a large file, `PUT …/{path}/content` sends it as it is.")]
    [Response(ResponseStatus.BadRequest, "The content is not base64, it does not fit into the data allowance of the lambda, or the path cannot be used. Nothing was written.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The workspace is switched off.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The file as it was written")]
    public async ValueTask<WorkspaceEntry> Put([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.WorkspacePath)] string path,
                                               FileRequest request)
        => Written(privateKey, await workspace.WriteEncodedAsync(meta.RequireEditable(privateKey), path, request.Content));

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/files/:path")]
    [Summary("Deletes a file, or a folder and everything in it")]
    [Remarks("Nothing happens for a path where there is nothing.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    [Response(ResponseStatus.BadRequest, "The path is empty or leads outside of the workspace.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public void Delete([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.WorkspacePath)] string path)
    {
        workspace.Delete(meta.RequireEditable(privateKey), path);

        logger.LogInformation("Deleted workspace path {Path} of lambda {Lambda}", path, meta.PublicKeyOf(privateKey));
    }

    [ResourceMethod(Method.Put, "lambdas/:privateKey/folders/:path")]
    [Summary("Makes a folder, with the folders it goes into")]
    [Remarks("Writing a file makes its folders on its own; this is for a folder that is empty for now. Nothing happens if it is there already.")]
    [Response(ResponseStatus.BadRequest, "A file has that path, there is no room for another folder, or the path cannot be used.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The workspace is switched off.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The workspace afterwards")]
    public WorkspaceListing PutFolder([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary("Where the folder goes within the workspace, sent as one segment with its slashes encoded")] string path)
    {
        var listing = WorkspaceFiles.CreateFolder(workspace, meta.RequireEditable(privateKey), null, path);

        logger.LogInformation("Created workspace folder {Path} of lambda {Lambda}", path, meta.PublicKeyOf(privateKey));

        return listing;
    }

    private WorkspaceEntry Written(string privateKey, WorkspaceEntry written)
    {
        logger.LogInformation("Wrote workspace file {Path} of lambda {Lambda} size {Size}", written.Path, meta.PublicKeyOf(privateKey), written.Size);

        return written;
    }

}
