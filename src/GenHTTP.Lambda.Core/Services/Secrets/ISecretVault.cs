using GenHTTP.Lambda.Integrations.Secrets;

namespace GenHTTP.Lambda.Services.Secrets;

/// <summary>
/// The secrets as the other services keep them in step with a lambda: what a
/// compiled lambda reads them through, the copy a feature works on, and what
/// goes when a lambda or its secrets do.
/// </summary>
/// <remarks>
/// Apart from <see cref="ISecretService"/>, which is what the API and MCP call
/// and never hands out a value. This one does - to the lambda the value belongs
/// to - so the doors into the platform use the other one. Where the values are
/// kept is the secrets integration's.
/// </remarks>
public interface ISecretVault
{

    /// <summary>
    /// What a compiled lambda is handed to read its secrets with.
    /// </summary>
    SecretReader ReaderFor(long lambdaId, long? featureId);

    /// <summary>
    /// What is stored for the lambda, or for a feature's copy, by name.
    /// </summary>
    ValueTask<IReadOnlyList<SecretEntry>> ListAsync(long lambdaId, long? featureId, CancellationToken cancellation = default);

    /// <summary>
    /// How many secrets the lambda has, or a feature's copy of them.
    /// </summary>
    ValueTask<int> CountAsync(long lambdaId, long? featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Stores a value under a name, replacing the one there.
    /// </summary>
    ValueTask<SecretEntry> StoreAsync(long lambdaId, long? featureId, string name, string value, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a secret, and says whether there was one.
    /// </summary>
    ValueTask<bool> RemoveAsync(long lambdaId, long? featureId, string name, CancellationToken cancellation = default);

    /// <summary>
    /// Replaces a feature's copy of the secrets with the lambda's own.
    /// </summary>
    ValueTask CopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a feature's copy of the secrets.
    /// </summary>
    ValueTask RemoveCopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Deletes every secret of a lambda, its features' copies included.
    /// </summary>
    ValueTask ClearAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// Forgets what is held in memory for a lambda, so the next read loads
    /// what is stored.
    /// </summary>
    void Invalidate(long lambdaId);

}

/// <summary>
/// What the secrets class generated into a lambda calls, once the lambda is
/// loaded: a secret by its name, and whether the caller insists on it.
/// </summary>
/// <remarks>
/// Both forms, since the lambda keeps both: <c>Secret.ReadAsync</c>, which is
/// what agents are taught, and <c>Secret.Read</c>, which code written before
/// calls - and which waits where the integration does not answer at once.
/// </remarks>
/// <param name="Read">The plain call: <c>Secret.Read</c>, <c>Secret.Exists</c></param>
/// <param name="ReadAsync">The awaited one: <c>Secret.ReadAsync</c>, <c>Secret.ExistsAsync</c></param>
public sealed record SecretReader(Func<string, bool, string?> Read, Func<string, bool, CancellationToken, ValueTask<string?>> ReadAsync);
