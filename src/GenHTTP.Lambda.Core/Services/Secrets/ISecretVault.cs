using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Secrets;

/// <summary>
/// The secrets as the other services keep them in step with a lambda: what a
/// compiled lambda reads them through, the copy a feature works on, and what
/// goes when a lambda or its secrets do.
/// </summary>
/// <remarks>
/// Apart from <see cref="ISecretService"/>, which is what the API and MCP call
/// and never hands out a value. This one does - to the lambda the value belongs
/// to - so the doors into the platform use the other one.
/// </remarks>
public interface ISecretVault
{

    /// <summary>
    /// What a compiled lambda is handed to read its secrets with.
    /// </summary>
    Func<string, bool, string?> ReaderFor(long lambdaId, long? featureId);

    /// <summary>
    /// What is stored for the lambda, or for a feature's copy, by name.
    /// </summary>
    IReadOnlyList<SecretEntity> List(long lambdaId, long? featureId);

    /// <summary>
    /// How many secrets the lambda has, or a feature's copy of them.
    /// </summary>
    int Count(long lambdaId, long? featureId);

    /// <summary>
    /// Stores a value under a name, replacing the one there.
    /// </summary>
    SecretEntity Store(long lambdaId, long? featureId, string name, string value);

    /// <summary>
    /// Removes a secret, and says whether there was one.
    /// </summary>
    bool Remove(long lambdaId, long? featureId, string name);

    /// <summary>
    /// Replaces a feature's copy of the secrets with the lambda's own.
    /// </summary>
    void Copy(long lambdaId, long featureId);

    /// <summary>
    /// Deletes every secret of a lambda, its features' copies included.
    /// </summary>
    void Clear(long lambdaId);

    /// <summary>
    /// Forgets what is held in memory for a lambda, so the next read loads
    /// what is stored.
    /// </summary>
    void Invalidate(long lambdaId);

}
