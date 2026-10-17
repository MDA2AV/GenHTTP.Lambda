using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Services.Settings;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// Behind the editor key like everything else about a lambda, so only
/// whoever may change the lambda may put it in front of everybody. Apart from
/// the versions and the deployment on purpose: presenting a lambda is
/// something done once it works, not a step of making it work. The tag is
/// described here; the public listing, <see cref="ShowcaseResource"/>, shares it.
/// </remarks>
[Tag(ApiDocumentation.Showcase)]
[Summary("The showcase: lambdas their owners chose to show, for anybody to browse - and putting one there.")]
[Remarks("""
         Showcasing is the owner's choice and never happens on its own. An entry has a title, a description and a picture,
         and is listed (`GET /showcases`) only while its lambda is online. The listing is public and names lambdas by their
         public key and address only - never an editor key.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class LambdaShowcaseResource(IShowcaseService showcases, IMetaService meta, ILimitsService limits, ILambdaAddresses addresses,
                                           ILogger<LambdaShowcaseResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/showcase")]
    [Summary("Reads the showcase entry of a lambda")]
    [return: Summary("The entry, if the lambda has one, and the limits of an entry")]
    public OwnShowcaseResponse Get([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var showcase = showcases.Get(privateKey);

        return new OwnShowcaseResponse(showcase == null ? null : ShowcaseResponse.Of(showcase, addresses), Limits(limits));
    }

    [ResourceMethod(Method.Put, "lambdas/:privateKey/showcase")]
    [Summary("Puts a lambda into the showcase")]
    [Remarks("""
             Or changes what its entry says. A new entry needs a picture: a PNG, JPEG, GIF or WebP image, base64 encoded, no larger than the limits say. A
             changed entry keeps its picture unless another is sent. The title (60 characters, one line) and the description
             (280 characters) are required either way, and should read plainly: what it is and what a visitor can do with it.
             The entry is listed from now on, while the lambda is online.
             """)]
    [Response(ResponseStatus.BadRequest, "The title or the description is missing or too long, or the picture is missing, too large, not base64 or not an image of a kind that is taken.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The entry, as the showcase lists it")]
    public ShowcaseResponse Put([Summary(ApiDocumentation.PrivateKey)] string privateKey, ShowcaseRequest request)
    {
        byte[]? image = null;

        if (!string.IsNullOrEmpty(request.Image))
        {
            try
            {
                image = Convert.FromBase64String(request.Image);
            }
            catch (FormatException)
            {
                throw LambdaException.Invalid("The picture has to be base64 encoded.");
            }
        }

        var saved = showcases.Save(privateKey, new ShowcaseDraft(request.Title, request.Description, image));

        logger.LogInformation("Added lambda {Lambda} to showcase as '{Title}'", saved.PublicKey, saved.Title);

        return ShowcaseResponse.Of(saved, addresses);
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/showcase")]
    [Summary("Takes a lambda out of the showcase")]
    [Remarks("Its entry and its picture are deleted. Nothing happens if it has none.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public void Delete([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        showcases.Remove(privateKey);

        logger.LogInformation("Removed lambda {Lambda} from showcase", meta.PublicKeyOf(privateKey));
    }

    internal static ShowcaseLimitsResponse Limits(ILimitsService limits)
        => new(ShowcaseLimits.MaxTitle, ShowcaseLimits.MaxDescription, limits.Get().ShowcaseImageBytes,
               ["image/png", "image/jpeg", "image/gif", "image/webp"], ShowcaseLimits.Tone);

}
