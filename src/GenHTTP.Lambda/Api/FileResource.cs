using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;

using GenHTTP.Modules.IO;
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
    /// The largest file that travels as base64 in a JSON document.
    /// </summary>
    /// <remarks>
    /// That way a file is in memory several times over while the request runs
    /// - its bytes, the text they become, the characters of that text. The
    /// workspace does not limit how large one file may be, only the room they
    /// take together, so anything larger goes through <c>…/content</c>, which
    /// streams the bytes as they are.
    /// </remarks>
    private const long EncodedLimit = 32 * 1024 * 1024;

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
        var id = await meta.RequireIdAsync(privateKey);

        var found = await workspace.FindAsync(id, path)
                 ?? throw LambdaException.NotFound($"There is no file called '{path}'.");

        if (found.Length > EncodedLimit)
        {
            throw LambdaException.Invalid($"'{path}' is {found.Length:N0} bytes, more than is sent as base64. GET /api/v1/lambdas/{{privateKey}}/files/{{path}}/content sends it as it is.");
        }

        var file = await workspace.ReadAsync(id, path)
                ?? throw LambdaException.NotFound($"There is no file called '{path}'.");

        return new FileResponse(file.Path, Convert.ToBase64String(file.Content), file.Content.Length);
    }

    /// <summary>
    /// Reads one file as it is, streamed from the disk, however large.
    /// </summary>
    /// <param name="path">The path of the file within the workspace</param>
    [ResourceMethod("lambdas/:privateKey/files/:path/content")]
    public async ValueTask<IResponse> GetContent(string privateKey, string path, IRequest request)
    {
        var found = await workspace.FindAsync(await meta.RequireIdAsync(privateKey), path)
                 ?? throw LambdaException.NotFound($"There is no file called '{path}'.");

        // a name in the workspace may hold anything but a slash, a quote or a
        // line break included, and must not be able to end the header
        var plain = new string(found.Name.Select(c => c is >= ' ' and < (char)127 and not '"' and not '\\' ? c : '_').ToArray());

        return request.Respond()
                      .Content(new FileContent(found))
                      .Header("Content-Disposition", $"attachment; filename=\"{plain}\"; filename*=UTF-8''{Uri.EscapeDataString(found.Name)}")
                      .Header("X-Content-Type-Options", "nosniff")
                      .Build();
    }

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
    {
        // read before the body is, which releases the headers
        var expected = long.TryParse(request.Header.Headers.GetEntry("Content-Length"), out var length) ? length : (long?)null;

        var id = await meta.RequireEditableAsync(privateKey);

        var body = request.GetBody(HeaderAccess.Release)?.AsStream() ?? Stream.Null;

        return await workspace.WriteAsync(id, path, body, expected);
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
