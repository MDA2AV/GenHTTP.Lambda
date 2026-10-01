namespace GenHTTP.Lambda.Services.Secrets;

/// <summary>
/// The secrets of a lambda, as its owner and agents manage them: by name, and
/// never read back.
/// </summary>
/// <remarks>
/// A value goes in and never comes out again through here - not to the editor,
/// not to the API, not to an agent. Only the lambda reads it, with
/// <c>Secret.Read</c>. So a secret is replaced rather than edited, and whoever
/// holds the editor link can use a key without ever being able to copy it.
///
/// Where a feature is named, its copy is meant: taken from the lambda's when
/// the feature began, read by its preview, and thrown away when it is merged.
/// </remarks>
public interface ISecretService
{

    /// <summary>
    /// Which secrets there are, and which the code reads.
    /// </summary>
    SecretListing List(string privateKey, string? feature = null);

    /// <summary>
    /// Stores a value under a name, replacing the one there. Refused while the
    /// lambda has secrets switched off.
    /// </summary>
    SecretInfo Set(string privateKey, string name, string value, string? feature = null);

    /// <summary>
    /// Removes a secret.
    /// </summary>
    void Delete(string privateKey, string name, string? feature = null);

}
