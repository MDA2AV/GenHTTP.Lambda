using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Guide;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The guide agents read first, and the section the operator writes into it,
/// at <c>/admin/guide</c> behind the admin token - on every installation that
/// has one.
/// </summary>
/// <remarks>
/// The panel's Guide section calls these, and so does an agent the operator
/// handed the token. Writing the section answers with the section alone: the
/// guide is written for the address it was asked at, which is read from the
/// request's headers, and a route that took a body has none left - so the
/// panel reads the guide again.
/// </remarks>
[Tag(ApiDocumentation.Guide)]
[Summary("The guide agents read first over MCP, and a section of the operator's own in it - for whoever runs the installation.")]
[Remarks("""
         `platform_guide`, the MCP tool every agent is told to call first - the build agent included - answers with one Markdown
         document: how the platform works and what it refuses. The operator may write a section of their own into it, with the
         rules of this installation: how lambdas are built here, and what not. It comes last in the guide, after the platform's
         own sections, under its own heading, saying it is the operator's: agents follow it wherever the guide leaves them the
         choice, and it changes nothing the platform requires or refuses. An agent reads it from its next call on.
         """)]
public sealed class GuideResource(IGuideService guide, ILogger<GuideResource> logger)
{

    [ResourceMethod]
    [Summary("Reads the guide")]
    [Remarks("The guide exactly as an agent calling `platform_guide` at this address reads it now, and the operator's section on its own.")]
    [return: Summary("The guide, the operator's section, and what the section may be")]
    public GuideResponse Get(IRequest request) => GuideResponse.Of(guide.Write(RequestOrigin.Of(request)), guide.Section);

    [ResourceMethod(Method.Put)]
    [Summary("Writes the operator's section")]
    [Remarks("""
             Writes the section, or replaces the one there was: a title, which is its heading, and a text in Markdown. Agents read it
             from their next call to `platform_guide` on. The text is trimmed and its line breaks made `\n`; headings inside it take
             `###` or more, since `#` and `##` are the guide's own. `GET` reads the guide with it.
             """)]
    [Response(ResponseStatus.BadRequest, """
                                         The title is empty, longer than 60 characters, more than one line, or the name of one of the platform's own
                                         sections; or the text is empty, larger than 6 KB in UTF-8, holds control characters, a heading of the first
                                         or second level, or a code block that is never closed. Nothing was changed.
                                         """, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The section, as kept")]
    public async ValueTask<GuideSectionResponse> Change(GuideRequest body)
    {
        var section = await guide.ChangeAsync(body.Title, body.Text);

        logger.GuideSectionChanged(section);

        return GuideSectionResponse.Of(section);
    }

    [ResourceMethod(Method.Delete)]
    [Summary("Removes the operator's section")]
    [Remarks("Agents read the platform's guide alone from their next call on. The title and the text are deleted. Nothing happens where there is no section.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    public async ValueTask Remove()
    {
        if (await guide.RemoveAsync() is { } removed)
        {
            logger.GuideSectionRemoved(removed);
        }
    }

}
