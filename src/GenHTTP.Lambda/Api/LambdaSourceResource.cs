using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Source;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// Behind the editor key like everything else about a lambda, so only whoever
/// may change the lambda may give its code away. The tag is described here;
/// the public side, <see cref="SourceResource"/>, shares it.
/// </remarks>
[Tag(ApiDocumentation.OpenSource)]
[Summary("Publishing the code of a lambda, for anybody to read, clone, download and star.")]
[Remarks("""
         Publishing is the owner's choice and never happens on its own. What is published is the program: every version kept,
         as the export packs it - its code, resources, documentation and tests - with a `LICENSE`. **Never the data**: no
         database, workspace or secret, and nothing only the owner may know - not the specification of a version, the editor
         key, traffic, logs or visitors. A version is described by its change line only.

         A published source is read at `/source/{publicKey}` on this site, through `GET /sources/…`, and cloned with git from
         `/source/{publicKey}.git`, read only. A source that is not published is not found, like a key nobody has.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class LambdaSourceResource(ISourceService sources, IMetaService meta, ILogger<LambdaSourceResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/source")]
    [Summary("Reads whether the code is published")]
    [return: Summary("What it is published under, if it ever was, and the licenses it can be published under")]
    public OwnSourceResponse Get([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var source = sources.Get(privateKey);

        return new OwnSourceResponse(source == null ? null : SourceSettingsResponse.Of(source),
                                     [.. SourceLicenses.All.Select(LicenseResponse.Of)], SourceLicenses.Default, SourceLicenses.MaxAuthor);
    }

    [ResourceMethod(Method.Put, "lambdas/:privateKey/source")]
    [Summary("Publishes the code of a lambda")]
    [Remarks("""
             Asked again, it changes the license or the author. Every version kept is published at once, and every version saved later as well. What the request leaves out stays
             as it is; published for the first time without a license, it is MIT. The license names the `author` as the holder
             of the copyright - or, without one, "the authors of" the lambda. Whatever somebody downloaded keeps the license it
             came with.
             """)]
    [Response(ResponseStatus.BadRequest, "The license is not one on offer, or the author is too long or more than one line.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("What it is published under now")]
    public SourceSettingsResponse Put([Summary(ApiDocumentation.PrivateKey)] string privateKey, SourceRequest request)
    {
        var source = sources.Publish(privateKey, new SourceDraft(request.License, request.Author));

        logger.LogInformation("Published source of lambda {Lambda} license {License}", source.PublicKey, source.License);

        return SourceSettingsResponse.Of(source);
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/source")]
    [Summary("Takes the published code of a lambda down")]
    [Remarks("""
             From now on it is not found at `/source/{publicKey}`, and cannot be cloned. Its stars and its license are kept for
             when it is published again. Whatever somebody downloaded while it was published stays theirs, under the license it
             came with. Nothing happens if it is not published.
             """)]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public void Delete([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var source = sources.Withdraw(privateKey);

        logger.LogInformation("Unpublished source of lambda {Lambda}", source?.PublicKey ?? meta.PublicKeyOf(privateKey));
    }

}
