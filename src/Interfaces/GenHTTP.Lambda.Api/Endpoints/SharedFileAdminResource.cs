using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.SharedFiles;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The shared files as the operator manages them, at <c>/admin/shared</c>
/// behind the admin token - on every installation that has one.
/// </summary>
/// <remarks>
/// The panel's Shared files section calls these, and its Branding section to
/// share a font; so does an agent the operator handed the token.
/// </remarks>
[Tag(ApiDocumentation.SharedFiles)]
public sealed class SharedFileAdminResource(ISharedFileService files, ILogger<SharedFileAdminResource> logger)
{

    [ResourceMethod]
    [Summary("Lists the shared files")]
    [return: Summary("The files with what of the installation uses them, and what one may be")]
    public async ValueTask<SharedFilesResponse> List() => SharedFilesResponse.Of(await files.ListAsync());

    [ResourceMethod(Method.Put, ":name")]
    [Summary("Shares a file")]
    [Remarks("""
             The body is the file as it is. Its name says what it is by its end - a picture (`.png`, `.jpg`, `.gif`, `.webp`, `.svg`), a
             font (`.woff2`, `.woff`, `.ttf`, `.otf`), a style sheet (`.css`) or a text (`.md`, `.txt`, `.csv`, `.json`) - and its content
             has to be that; neither pages nor scripts are shared. A file of that name is replaced: whatever uses it - the branding,
             the guide, the pages that link to it - has the new one from then on.

             Make sure you may put it on the web: a font's license often says where it may be used.
             """)]
    [RequestBody("The file", ContentType = "*/*")]
    [Response(ResponseStatus.BadRequest, """
                                         The name is none - letters and digits without accents, dashes, underscores and dots, at most 60 of them - or ends in
                                         none of the types; the body is empty, larger than 2 MB, or not what the name says; or there are 40 files already, or
                                         they would be larger than 20 MB together. Nothing was changed.
                                         """, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "There is a file whose name differs from this one only in case.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The file, as it is shared")]
    public async ValueTask<SharedFileResponse> Write([Summary("What the file is called")] string name, Stream body)
    {
        var content = await Bodies.ReadAsync(body, SharedFileChecks.MaxBytes, $"A shared file must not be larger than {SharedFileChecks.MaxBytes / 1024 / 1024} MB.");

        var (file, replaced) = await files.WriteAsync(name, content);

        logger.SharedFileWritten(file.File, replaced);

        return SharedFileResponse.Of(file);
    }

    [ResourceMethod(Method.Delete, ":name")]
    [Summary("Deletes a shared file")]
    [Remarks("For good: a page that links to it goes without. Refused while the branding or the guide uses it, which `uses` says.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    [Response(ResponseStatus.NotFound, "There is no shared file of this name.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The branding or the guide uses it. Nothing was deleted.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask Delete([Summary("What the file is called")] string name)
    {
        var deleted = await files.DeleteAsync(name);

        logger.SharedFileDeleted(deleted);
    }

}
