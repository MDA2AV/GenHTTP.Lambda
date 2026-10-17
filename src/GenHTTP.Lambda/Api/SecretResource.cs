using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// A value is written and never read back: not here, not in the editor, not by
/// an agent. There is deliberately no operation that answers with one.
/// </remarks>
[Tag(ApiDocumentation.Secrets)]
[Summary("The secrets of a lambda - API keys, passwords, tokens - which its code reads and nobody else does.")]
[Remarks("""
         The code reads a secret with `Secret.Read("NAME")`, or asks whether it is there with `Secret.Exists("NAME")`. A value
         is **written and never read back**: no operation of this API answers with one, and neither does the editor. What can
         be seen is the name, when it was set, whether the code reads it - and which names the code reads that have no value
         yet (`missing`), which is what somebody still has to enter.

         Secrets are data: every version reads the same, deploying and rolling back leave them alone, and a feature works on a
         copy of them. They are off until they are switched on (`PUT /lambdas/{privateKey}/data/secrets`), and switching them
         off deletes them. A lambda may keep 100.

         The values are kept encrypted at rest, and a lambda reads a changed value from its next `Secret.Read` on.
         An exported project reads them from environment variables of the same names.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class SecretResource(ISecretService secrets, IMetaService meta, IFeatureService features, ILogger<SecretResource> logger)
{

    #region The lambda's

    [ResourceMethod("lambdas/:privateKey/secrets")]
    [Summary("Lists the secrets by name")]
    [Remarks("Never their values. The names the code reads are found in the code of the version online and of the newest one.")]
    [return: Summary("The secrets by name, and the names the code reads")]
    public SecretListingResponse List([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => Describe(secrets.List(privateKey));

    [ResourceMethod(Method.Put, "lambdas/:privateKey/secrets/:name")]
    [Summary("Stores a secret")]
    [Remarks("""
             The lambda reads it from its next `Secret.Read` on, without being deployed again. The value cannot be read back
             afterwards - not even by whoever stored it. Its features keep the copies they have.
             """)]
    [Response(ResponseStatus.BadRequest, "The name is not one a secret may have, or the value is empty or larger than 32 KB.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "Secrets are switched off, or the lambda has as many secrets as it may keep.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The secret, without its value")]
    public SecretResponse Put([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                              [Summary("Its name, as the code reads it: letters, digits and underscores, not starting with a digit, up to 128 characters - `STRIPE_KEY`. Case sensitive.")] string name,
                              SecretRequest request)
    {
        var secret = secrets.Set(privateKey, name, request.Value);

        logger.LogInformation("Set secret {Name} of lambda {Lambda}", secret.Name, meta.PublicKeyOf(privateKey));

        return Describe(secret);
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/secrets/:name")]
    [Summary("Deletes a secret")]
    [Remarks("From its next request on, code that reads it fails, and code that asks `Secret.Exists` is told it is not there.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or it has no secret of that name.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public void Delete([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary("Its name, case sensitive")] string name)
    {
        secrets.Delete(privateKey, name);

        logger.LogInformation("Deleted secret {Name} of lambda {Lambda}", name.Trim(), meta.PublicKeyOf(privateKey));
    }

    #endregion

    #region A feature's copy

    [ResourceMethod("lambdas/:privateKey/features/:feature/secrets")]
    [Summary("Lists a feature's copy of the secrets")]
    [Remarks("The copy its preview reads. The names the code reads are found in the feature's files.")]
    [Response(ResponseStatus.NotFound, ApiDocumentation.NoFeature, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The feature's secrets by name, and the names its code reads")]
    public SecretListingResponse ListOfFeature([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature)
        => Describe(secrets.List(privateKey, feature));

    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/secrets/:name")]
    [Summary("Stores a secret in a feature's copy")]
    [Remarks("For trying the feature with - a key for a sandbox, say. The lambda's own secrets are not touched, and the copy is thrown away when the feature is merged.")]
    [Response(ResponseStatus.BadRequest, "The name is not one a secret may have, or the value is empty or larger than 32 KB.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "Secrets are switched off, or the copy has as many secrets as a lambda may keep.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, ApiDocumentation.NoFeature, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The secret, without its value")]
    public SecretResponse PutOfFeature([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                       [Summary("Its name, as the code reads it: letters, digits and underscores, not starting with a digit. Case sensitive.")] string name,
                                       SecretRequest request)
    {
        var secret = secrets.Set(privateKey, name, request.Value, feature);

        logger.LogInformation("Set secret {Name} of feature '{Feature}' of lambda {Lambda}", secret.Name,
                              features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey));

        return Describe(secret);
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/features/:feature/secrets/:name")]
    [Summary("Deletes a secret from a feature's copy")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, it has no such feature, or the copy has no secret of that name.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public void DeleteOfFeature([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                [Summary("Its name, case sensitive")] string name)
    {
        secrets.Delete(privateKey, name, feature);

        logger.LogInformation("Deleted secret {Name} of feature '{Feature}' of lambda {Lambda}", name.Trim(),
                              features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey));
    }

    #endregion

    internal static SecretListingResponse Describe(SecretListing listing)
        => new(listing.Enabled, [.. listing.Secrets.Select(Describe)], [.. listing.Used], [.. listing.Missing], [.. listing.Optional], listing.Limit);

    internal static SecretResponse Describe(SecretInfo secret) => new(secret.Name, secret.Created, secret.Changed, secret.Used);

}
