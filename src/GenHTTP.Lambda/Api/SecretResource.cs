using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Secrets;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The secrets of a lambda - API keys, passwords, tokens - which its code reads
/// with <c>Secret.Read("NAME")</c> and nobody else reads at all.
/// </summary>
/// <remarks>
/// A value is written and never read back: not here, not in the editor, not by
/// an agent. What can be seen is the name, when it was set, and whether the
/// code reads it - and which names the code reads that have no value yet.
/// Secrets are data: every version reads the same, deploying and rolling back
/// leave them alone, and a feature works on a copy of them. They are off until
/// they are switched on (<c>PUT /lambdas/{privateKey}/data/secrets</c>), and
/// switching them off deletes them.
/// </remarks>
public sealed class SecretResource(ISecretService secrets)
{

    #region The lambda's

    /// <summary>
    /// The secrets by name, and which names the code reads.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/secrets")]
    public async ValueTask<SecretListingResponse> List(string privateKey)
        => Describe(await secrets.ListAsync(privateKey));

    /// <summary>
    /// Stores a value under a name, replacing the one there.
    /// </summary>
    /// <remarks>
    /// The lambda reads it from its next call to <c>Secret.Read</c> on, without
    /// being deployed again. Refused while secrets are switched off.
    /// </remarks>
    /// <param name="name">Letters, digits and underscores, not starting with a digit: <c>STRIPE_KEY</c></param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/secrets/:name")]
    public async ValueTask<SecretResponse> Put(string privateKey, string name, SecretRequest request)
        => Describe(await secrets.SetAsync(privateKey, name, request.Value));

    /// <summary>
    /// Removes a secret.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/secrets/:name")]
    public async ValueTask Delete(string privateKey, string name) => await secrets.DeleteAsync(privateKey, name);

    #endregion

    #region A feature's copy

    /// <summary>
    /// A feature's copy of the secrets, which its preview reads.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/features/:feature/secrets")]
    public async ValueTask<SecretListingResponse> ListOfFeature(string privateKey, string feature)
        => Describe(await secrets.ListAsync(privateKey, feature));

    /// <summary>
    /// Stores a value in a feature's copy, for trying the feature with - a
    /// key for a sandbox, say. The lambda's own is not touched.
    /// </summary>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/secrets/:name")]
    public async ValueTask<SecretResponse> PutOfFeature(string privateKey, string feature, string name, SecretRequest request)
        => Describe(await secrets.SetAsync(privateKey, name, request.Value, feature));

    /// <summary>
    /// Removes a secret from a feature's copy.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/features/:feature/secrets/:name")]
    public async ValueTask DeleteOfFeature(string privateKey, string feature, string name) => await secrets.DeleteAsync(privateKey, name, feature);

    #endregion

    internal static SecretListingResponse Describe(SecretListing listing)
        => new(listing.Enabled, [.. listing.Secrets.Select(Describe)], [.. listing.Used], [.. listing.Missing], [.. listing.Optional], listing.Limit);

    internal static SecretResponse Describe(SecretInfo secret) => new(secret.Name, secret.Created, secret.Changed, secret.Used);

}
