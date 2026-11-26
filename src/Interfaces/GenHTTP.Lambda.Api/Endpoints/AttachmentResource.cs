using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Agents;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The files that travel with messages to the agent: uploaded before the
/// message that names them, and read back - with the files the agent hands
/// back - by their ids.
/// </summary>
/// <remarks>
/// Uploading needs no key, as asking for a build does not; what keeps it
/// from being free storage is that a file the agent does not take is refused,
/// one nobody sends with a message goes after an hour, and all the files that
/// wait together are bounded. An id is random: holding it is what reading the
/// file takes, as holding the id of a build is.
/// </remarks>
[Tag(ApiDocumentation.Builds)]
[Summary("The files a message to the agent comes with, and those the agent hands back.")]
[Remarks("""
         A file is uploaded first and named by its `id` in the message - `attachments` of `POST /builds` or of
         `POST /lambdas/{privateKey}/agent/start`. Which kinds and how large, and how many one message takes, is in
         `build.offer.attachments` of `GET /system`; without, the agent of this installation takes none. A file nobody sends
         is removed after an hour; one that was sent is kept with its message, for as long as the platform keeps that, so a later
         message can refer to it - and the agent can read it, and put it into the app by its id. The operator may open every file,
         as they read every prompt.
         """)]
public sealed class AttachmentResource(IAgentService agents)
{

    [ResourceMethod(Method.Post)]
    [Summary("Uploads a file for a message to the agent")]
    [Remarks("The body is the file as it is. Its name and kind go in the query, since the body says neither.")]
    [Response(ResponseStatus.Created, "The file was kept, for an hour unless a message names it.")]
    [Response(ResponseStatus.BadRequest, "The file is empty.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "The agent of this installation takes no files - or there is no agent.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.RequestEntityTooLarge, "The file is larger than the agent takes.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.UnsupportedMediaType, "The agent takes no files of this kind.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.ServiceUnavailable, "Too many files wait to be sent. Try again in a while.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The file, by the id a message names it by")]
    public async ValueTask<Result<RunFile>> Upload([Summary("What the file is called, as on the device it came from")] string? name,
                                                  [Summary("What kind of file it is, as a media type: `image/png`")] string? type,
                                                  Stream body)
    {
        using var read = new MemoryStream();

        await body.CopyToAsync(read);

        return new Result<RunFile>(await agents.AttachAsync(name, type, read.ToArray())).Status(ResponseStatus.Created);
    }

    [ResourceMethod(":id")]
    [Summary("Reads a file that travelled with a message")]
    [Remarks("""
             A picture is answered as what it is, to be shown; every other file to be saved (`Content-Disposition: attachment`), as
             a stream of bytes - never as a page or a script of this site, whatever it holds.
             """)]
    [Response(ResponseStatus.Ok, "The file", ContentType = "*/*")]
    [Response(ResponseStatus.NotFound, "There is no file of this id, any more.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<IResponse> Get([Summary("The `id` of the file")] string id, IRequest request)
        => RunFiles.Serve(request, await agents.ReadFileAsync(id) ?? throw new ProviderException(ResponseStatus.NotFound, "There is no such file any more."));

}
