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
internal static class WorkspaceFiles
{

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

    public static async ValueTask<FileResponse> ReadAsync(IWorkspaceService workspace, long lambdaId, long? featureId, string path, string where)
    {
        var found = workspace.Find(lambdaId, path, featureId)
                 ?? throw LambdaException.NotFound($"There is no file called '{path}'.");

        if (found.Length > EncodedLimit)
        {
            throw LambdaException.Invalid($"'{path}' is {found.Length:N0} bytes, more than is sent as base64. GET {where}/{{path}}/content sends it as it is.");
        }

        var file = await workspace.ReadAsync(lambdaId, path, featureId)
                ?? throw LambdaException.NotFound($"There is no file called '{path}'.");

        return new FileResponse(file.Path, Convert.ToBase64String(file.Content), file.Content.Length);
    }

    public static IResponse Send(IWorkspaceService workspace, long lambdaId, long? featureId, string path, IRequest request)
    {
        var found = workspace.Find(lambdaId, path, featureId)
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

    public static async ValueTask<WorkspaceEntry> WriteAsync(IWorkspaceService workspace, long lambdaId, long? featureId, string path, FileRequest request)
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

        return await workspace.WriteAsync(lambdaId, path, stream, featureId: featureId);
    }

    public static WorkspaceListing CreateFolder(IWorkspaceService workspace, long lambdaId, long? featureId, string path)
    {
        workspace.CreateFolder(lambdaId, path, featureId);

        return workspace.List(lambdaId, featureId);
    }

}
