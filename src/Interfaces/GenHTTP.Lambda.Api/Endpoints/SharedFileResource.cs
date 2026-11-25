using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.SharedFiles;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The shared files, for whoever's page links to one.
/// </summary>
/// <remarks>
/// From the platform's own address, so what is sent is of a type that is
/// drawn or read (<see cref="SharedFileType"/>), never sniffed for another,
/// and - opened on its own, where an SVG is a document - under a policy that
/// runs nothing in it. Any page may load it, a font included, which a browser
/// asks leave for: an app answers at another address than the platform, and
/// a file here is public anyway.
///
/// A page of an app names the file, which may be replaced any time: kept a
/// minute, so the pages of an app are drawn without asking for its style
/// sheet with each of them, and a file that was replaced is the new one soon
/// after. The installation's own pages name what the file holds.
/// </remarks>
[Tag(ApiDocumentation.SharedFiles)]
[Summary("The files whoever runs the installation shares across it - fonts and a logo, a style sheet, a house style to read.")]
[Remarks("""
         For the installation's own pages, for the agents that build on it - the guide names them, in its section `sharedFiles` -
         and for the pages of the apps, which link to one by its full address. A file is public, and read by anybody who has its
         address.
         """)]
public sealed class SharedFileResource(ISharedFiles files)
{

    [ResourceMethod]
    [Summary("Lists the shared files")]
    [Remarks("Public, as the files are. The guide names as many as fit into it.")]
    [return: Summary("The files, by name")]
    public async ValueTask<IReadOnlyList<SharedFileResponse>> List() => [.. (await files.ListAsync()).Select(SharedFileResponse.Of)];

    [ResourceMethod(":name")]
    [Summary("Fetches a shared file")]
    [Remarks("""
             Public, and for any page to load: it is sent with `Access-Control-Allow-Origin: *`. By its plain address it is whatever
             the file holds, kept by a browser for a minute; with `v` as its versioned path names it, it may be cached for good.
             """)]
    [Response(ResponseStatus.Ok, "The file: a picture, a font, a style sheet or a text", ContentType = "*/*")]
    [ResponseHeader(ResponseStatus.Ok, "Cache-Control", "`public, max-age=31536000, immutable` with the version it has now, `public, max-age=60` otherwise")]
    [Response(ResponseStatus.NotFound, "There is no shared file of this name.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<IResponse> Get([Summary("What the file is called")] string name, [Summary("Which content is meant, as the versioned path of the file names it")] string? v,
                                          IRequest request)
    {
        var file = await files.FindAsync(name) ?? throw Missing(name);

        var content = await files.ReadAsync(file) ?? throw Missing(name);

        return request.Respond()
                      .Content(content, new ContentType(file.Type.Type))
                      .Header("Cache-Control", v == file.Version ? "public, max-age=31536000, immutable" : "public, max-age=60")
                      .Header("Access-Control-Allow-Origin", "*")
                      .Header("X-Content-Type-Options", "nosniff")
                      .Header("Content-Security-Policy", "default-src 'none'; style-src 'unsafe-inline'; img-src data:; sandbox")
                      .Build();
    }

    private static LambdaException Missing(string name) => LambdaException.NotFound($"There is no shared file called '{name}'.");

}
