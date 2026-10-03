using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// Reading and writing the files of a workspace over HTTP - the lambda's own,
/// or a feature's copy of it, which are answered exactly alike.
/// </summary>
/// <remarks>
/// What is left here is what HTTP adds: the body as a stream, the download as
/// a response, and which route to take instead. Encoding a file as base64,
/// and how large one may be for it, is the workspace service's.
/// </remarks>
internal static class WorkspaceFiles
{

    public static async ValueTask<FileResponse> ReadAsync(IWorkspaceService workspace, long lambdaId, long? featureId, string path, string where)
    {
        var file = await workspace.ReadEncodedAsync(lambdaId, path, featureId)
                ?? throw LambdaException.NotFound($"There is no file called '{path}'.");

        if (file.Content == null)
        {
            throw LambdaException.Invalid($"'{path}' is {file.Length:N0} bytes, more than is sent as base64. GET {where}/{{path}}/content sends it as it is.");
        }

        // what travels as base64 is far below what an int holds
        return new FileResponse(file.Path, file.Content, (int)file.Length);
    }

    public static IResponse Send(IWorkspaceService workspace, long lambdaId, long? featureId, string path, IRequest request)
    {
        var found = workspace.Find(lambdaId, path, featureId)
                 ?? throw LambdaException.NotFound($"There is no file called '{path}'.");

        // a name in the workspace may hold anything but a slash, a quote or a
        // line break included, and must not be able to end the header
        var plain = new string(found.Name.Select(c => c is >= ' ' and < (char)127 and not '"' and not '\\' ? c : '_').ToArray());

        return request.Respond()
                      // bytes to download whatever the name says: the API shares its
                      // origin with the editor, and a page somebody uploaded must not run there
                      .Content(Resource.FromFile(found).Type(ContentType.ApplicationOctetStream).Build())
                      .Header("Content-Disposition", $"attachment; filename=\"{plain}\"; filename*=UTF-8''{Uri.EscapeDataString(found.Name)}")
                      .Header("X-Content-Type-Options", "nosniff")
                      .Build();
    }

    /// <summary>
    /// Writes a file from the body as it is - to the disk as it arrives, so a
    /// file of any size costs the server a buffer rather than itself.
    /// </summary>
    public static async ValueTask<WorkspaceEntry> ReceiveAsync(IWorkspaceService workspace, long lambdaId, long? featureId, string path, IRequest request)
    {
        // read before the body is, which releases the headers
        var expected = long.TryParse(request.Header.Headers.GetEntry("Content-Length"), out var length) ? length : (long?)null;

        var body = request.GetBody(HeaderAccess.Release)?.AsStream() ?? Stream.Null;

        return await workspace.WriteAsync(lambdaId, path, body, expected, featureId);
    }

    public static WorkspaceListing CreateFolder(IWorkspaceService workspace, long lambdaId, long? featureId, string path)
    {
        workspace.CreateFolder(lambdaId, path, featureId);

        return workspace.List(lambdaId, featureId);
    }

}
