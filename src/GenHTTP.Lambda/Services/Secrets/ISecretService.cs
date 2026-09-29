namespace GenHTTP.Lambda.Services.Secrets;

/// <summary>
/// What is listed of a lambda's secrets: whether they are on, and what they are
/// called.
/// </summary>
/// <param name="Enabled">Whether the owner switched secrets on</param>
/// <param name="Limit">How many secrets there may be</param>
/// <param name="Secrets">The secrets, by name</param>
public sealed record SecretListing(bool Enabled, int Limit, IReadOnlyList<SecretInfo> Secrets);

/// <summary>
/// The secrets of a lambda as its owner, the API and an agent manage them:
/// by editor key, and for a feature by its key.
/// </summary>
/// <remarks>
/// Write only. A value can be set and replaced and a secret can be deleted,
/// and there is no way to ask for one - not from here, not from the editor,
/// not from an agent. Only the code of the lambda reads them, by name.
/// </remarks>
public interface ISecretService
{

    /// <summary>
    /// The secrets of the lambda - or, with a feature, of that feature's copy.
    /// </summary>
    ValueTask<SecretListing> ListAsync(string privateKey, string? feature = null, CancellationToken cancellation = default);

    /// <summary>
    /// Sets a secret, or replaces its value. Refused while secrets are off.
    /// </summary>
    ValueTask<SecretInfo> SetAsync(string privateKey, string? feature, string name, string value, CancellationToken cancellation = default);

    /// <summary>
    /// Deletes a secret.
    /// </summary>
    ValueTask DeleteAsync(string privateKey, string? feature, string name, CancellationToken cancellation = default);

}
