namespace GenHTTP.Lambda.Services.Secrets;

/// <summary>
/// One secret, as anybody but the code that reads it may know it: what it is
/// called, and when it was set. Never what it holds.
/// </summary>
/// <param name="Name">What the code asks for it by</param>
/// <param name="Created">When it was first set</param>
/// <param name="Updated">When its value was last set</param>
public sealed record SecretInfo(string Name, DateTime Created, DateTime Updated);

/// <summary>
/// How the code of a lambda reaches its secrets: the two things
/// <c>Secret</c> in a snippet is made of.
/// </summary>
/// <remarks>
/// Plain delegates, because the assembly a snippet is compiled into cannot see
/// this application - only what the framework has.
/// </remarks>
/// <param name="Enabled">Whether the lambda has secrets switched on, asked again on every call</param>
/// <param name="Read">A secret's value, or nothing if there is none of that name; refuses when secrets are off or the value cannot be opened</param>
public sealed record SecretAccess(Func<bool> Enabled, Func<string, string?> Read);

/// <summary>
/// The secrets of the lambdas: the only place that seals and opens their
/// values.
/// </summary>
/// <remarks>
/// Works on ids and knows nothing of editor keys or features by name, so that
/// everything that has to do something with a lambda's secrets - the API, the
/// features that copy them, the deployments that read them, the data service
/// that switches them off - can use it without any of them depending on
/// another. Whether the owner switched secrets on is asked of it only for
/// reading; it is the caller that refuses to write while they are off.
///
/// A value goes in and comes out of here in exactly one direction: written by
/// <see cref="SetAsync" />, read by the <see cref="SecretAccess" /> a lambda is
/// given. Nothing lists a value.
/// </remarks>
public interface ISecretVault
{

    /// <summary>
    /// The secrets of a lambda, or of a feature's copy of them, by name.
    /// </summary>
    ValueTask<IReadOnlyList<SecretInfo>> ListAsync(long lambdaId, long? featureId = null, CancellationToken cancellation = default);

    /// <summary>
    /// How many there are.
    /// </summary>
    ValueTask<int> CountAsync(long lambdaId, long? featureId = null, CancellationToken cancellation = default);

    /// <summary>
    /// Sets a secret, replacing its value if there is one of that name.
    /// </summary>
    ValueTask<SecretInfo> SetAsync(long lambdaId, long? featureId, string name, string value, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a secret.
    /// </summary>
    /// <returns>Whether there was one</returns>
    ValueTask<bool> DeleteAsync(long lambdaId, long? featureId, string name, CancellationToken cancellation = default);

    /// <summary>
    /// Gives a feature a copy of the lambda's secrets as they are now,
    /// replacing whatever it had.
    /// </summary>
    ValueTask CopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Removes every secret of a lambda - the copies its features have
    /// included.
    /// </summary>
    ValueTask ClearAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// Says that whether a lambda has secrets on has changed, so what was read
    /// about it is read again.
    /// </summary>
    void Forget(long lambdaId);

    /// <summary>
    /// What the code of a lambda, or of a feature's preview, reads its secrets with.
    /// </summary>
    SecretAccess AccessFor(long lambdaId, long? featureId = null);

}
