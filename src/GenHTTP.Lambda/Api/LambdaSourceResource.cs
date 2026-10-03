using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Source;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Whether the source of one lambda is published, for its owner.
/// </summary>
/// <remarks>
/// Behind the editor key like everything else about a lambda, so only whoever
/// may change the lambda may give its code away. Publishing is the owner's
/// choice and never happens on its own: nothing is published until this is
/// asked for, and what is published is the program - never the data.
/// </remarks>
public sealed class LambdaSourceResource(ISourceService sources, IMetaService meta, ILogger<LambdaSourceResource> logger)
{

    /// <summary>
    /// Whether the source is published and under which license, and the
    /// licenses it can be published under.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/source")]
    public OwnSourceResponse Get(string privateKey)
    {
        var source = sources.Get(privateKey);

        return new OwnSourceResponse(source == null ? null : SourceSettingsResponse.Of(source),
                                     [.. SourceLicenses.All.Select(LicenseResponse.Of)], SourceLicenses.Default, SourceLicenses.MaxAuthor);
    }

    /// <summary>
    /// Publishes the source of the lambda - every version's code, assets,
    /// documentation and tests, never its data - or changes the license it is
    /// published under.
    /// </summary>
    /// <remarks>
    /// What the request leaves out stays as it is; published for the first
    /// time without a license, it is MIT. An empty author names nobody, and
    /// the license then names "the authors of" the lambda.
    /// </remarks>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/source")]
    public SourceSettingsResponse Put(string privateKey, SourceRequest request)
    {
        var source = sources.Publish(privateKey, new SourceDraft(request.License, request.Author));

        logger.LogInformation("Published source of lambda {Lambda} license {License}", source.PublicKey, source.License);

        return SourceSettingsResponse.Of(source);
    }

    /// <summary>
    /// Takes the source down. Its stars are kept for when it is published
    /// again. Whatever somebody downloaded while it was published stays theirs
    /// under the license it came with.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/source")]
    public void Delete(string privateKey)
    {
        var source = sources.Withdraw(privateKey);

        logger.LogInformation("Unpublished source of lambda {Lambda}", source?.PublicKey ?? meta.PublicKeyOf(privateKey));
    }

}
