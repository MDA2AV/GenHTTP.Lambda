using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Secrets;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The secrets of a lambda: API tokens, credentials and the like, which its
/// code reads by name and nobody reads back.
/// </summary>
/// <remarks>
/// Secrets are off until switched on (<c>PUT /lambdas/{privateKey}/data/secrets</c>).
/// They can be listed by name, set, replaced and deleted, and that is all: no
/// call answers with a value, for the owner as little as for an agent. They are
/// stored encrypted, belong to the lambda rather than to a version - every
/// version reads the same, and none of them brings an earlier one back - and a
/// feature works on a copy of them, under
/// <c>/lambdas/{privateKey}/features/{feature}/secrets</c>. An exported project
/// takes them from environment variables of the same name.
/// </remarks>
public sealed class SecretResource(ISecretService secrets)
{

    #region The lambda's

    /// <summary>
    /// Lists the secrets by name, with when each was set. Never a value.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/secrets")]
    public async ValueTask<SecretListingResponse> List(string privateKey)
        => Describe(await secrets.ListAsync(privateKey));

    /// <summary>
    /// Sets a secret, or replaces its value.
    /// </summary>
    /// <remarks>
    /// Takes effect at once, without a deploy: the code reads it the next time
    /// it asks. Refused while secrets are switched off. The name is the shape
    /// of an environment variable - letters, digits and underscores, not
    /// starting with a digit - and a line break at the end of the value, which
    /// a paste brings along, is dropped.
    /// </remarks>
    /// <param name="name">What the code asks for it by, such as <c>STRIPE_API_KEY</c></param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/secrets/:name")]
    public async ValueTask<SecretResponse> Put(string privateKey, string name, SecretRequest request)
        => Describe(await secrets.SetAsync(privateKey, null, name, request.Value));

    /// <summary>
    /// Deletes a secret. Code that reads it fails from its next request on.
    /// </summary>
    /// <param name="name">The secret</param>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/secrets/:name")]
    public async ValueTask Delete(string privateKey, string name)
        => await secrets.DeleteAsync(privateKey, null, name);

    #endregion

    #region A feature's copy

    /// <summary>
    /// Lists the secrets of a feature's copy, by name.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/features/:feature/secrets")]
    public async ValueTask<SecretListingResponse> ListCopy(string privateKey, string feature)
        => Describe(await secrets.ListAsync(privateKey, feature));

    /// <summary>
    /// Sets a secret in the feature's copy only, for its preview to read. The
    /// lambda's own are not touched, and this copy is thrown away with the feature.
    /// </summary>
    /// <param name="name">What the code asks for it by</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/secrets/:name")]
    public async ValueTask<SecretResponse> PutCopy(string privateKey, string feature, string name, SecretRequest request)
        => Describe(await secrets.SetAsync(privateKey, feature, name, request.Value));

    /// <summary>
    /// Deletes a secret from the feature's copy.
    /// </summary>
    /// <param name="name">The secret</param>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/features/:feature/secrets/:name")]
    public async ValueTask DeleteCopy(string privateKey, string feature, string name)
        => await secrets.DeleteAsync(privateKey, feature, name);

    #endregion

    internal static SecretResponse Describe(SecretInfo secret) => new(secret.Name, secret.Created, secret.Updated);

    private static SecretListingResponse Describe(SecretListing listing)
        => new(listing.Enabled, listing.Limit, [.. listing.Secrets.Select(Describe)]);

}
