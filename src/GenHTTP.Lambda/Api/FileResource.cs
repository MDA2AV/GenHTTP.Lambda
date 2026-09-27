using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The files a lambda keeps in its private directory.
/// </summary>
/// <remarks>
/// A file is addressed by its path within the workspace, sent as a single
/// segment with the slashes encoded (<c>logs%2Ftoday.txt</c>), so that a path
/// that happens to end in something this API routes on cannot be mistaken
/// for it.
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
    /// reads the same way as one holding text.
    /// </remarks>
    [ResourceMethod("lambdas/:privateKey/files/:path")]
    public async ValueTask<FileResponse> Get(string privateKey, string path)
    {
        var file = await workspace.ReadAsync(await meta.RequireIdAsync(privateKey), path)
                ?? throw LambdaException.NotFound($"There is no file called '{path}'.");

        return new FileResponse(file.Path, Convert.ToBase64String(file.Content), file.Content.Length);
    }

    /// <summary>
    /// Writes a file, replacing it if it is already there.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/files/:path")]
    public async ValueTask<WorkspaceEntry> Put(string privateKey, string path, FileRequest request)
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

        return await workspace.WriteAsync(await meta.RequireEditableAsync(privateKey), path, stream);
    }

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
    {
        var id = await meta.RequireEditableAsync(privateKey);

        await workspace.CreateFolderAsync(id, path);

        return await workspace.ListAsync(id);
    }

}
