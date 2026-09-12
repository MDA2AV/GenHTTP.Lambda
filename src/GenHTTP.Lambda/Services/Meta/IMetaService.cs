using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Creates, reads and updates lambdas. Everything the editor does goes through here.
/// </summary>
public interface IMetaService
{

    /// <summary>
    /// Checks whether the given public key could be used for a new lambda.
    /// </summary>
    ValueTask<KeyAvailability> CheckKeyAsync(string? publicKey, CancellationToken cancellation = default);

    /// <summary>
    /// Creates a new lambda, generating a public key if none was requested.
    /// </summary>
    ValueTask<LambdaInfo> CreateAsync(string? publicKey, CancellationToken cancellation = default);

    /// <summary>
    /// Reads a lambda by the private key of its editor.
    /// </summary>
    ValueTask<LambdaInfo?> GetAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// Looks up the deployed lambda behind a public key, if there is one.
    /// </summary>
    ValueTask<ResolvedLambda?> ResolveAsync(string publicKey, CancellationToken cancellation = default);

    /// <summary>
    /// Tells whether a public key exists and is currently serving.
    /// </summary>
    ValueTask<PublicStatus> GetStatusAsync(string publicKey, CancellationToken cancellation = default);

    /// <summary>
    /// Lists the stored versions of a lambda, newest first.
    /// </summary>
    ValueTask<IReadOnlyList<LambdaVersionInfo>> GetVersionsAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// Reads a single version including its code.
    /// </summary>
    ValueTask<LambdaVersionContent> GetVersionAsync(string privateKey, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Stores the given code as a new version.
    /// </summary>
    ValueTask<LambdaVersionInfo> SaveAsync(string privateKey, string code, CancellationToken cancellation = default);

    /// <summary>
    /// Compiles the given code without deploying it.
    /// </summary>
    ValueTask<CompilationOutcome> CheckAsync(string privateKey, string code, CancellationToken cancellation = default);

    /// <summary>
    /// Builds the given version (or the latest one) and makes it the active deployment.
    /// </summary>
    ValueTask<DeploymentResult> DeployAsync(string privateKey, int? version, CancellationToken cancellation = default);

    /// <summary>
    /// Takes the lambda off the air, keeping its code.
    /// </summary>
    ValueTask<LambdaInfo> UndeployAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// Moves the lambda to another public key.
    /// </summary>
    ValueTask<LambdaInfo> ChangeKeyAsync(string privateKey, string? publicKey, CancellationToken cancellation = default);

    /// <summary>
    /// Removes the lambda, its versions and its workspace.
    /// </summary>
    ValueTask DeleteAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// Undeploys and removes lambdas that have outlived their tier.
    /// </summary>
    ValueTask<MaintenanceReport> RunMaintenanceAsync(DateTime now, CancellationToken cancellation = default);

}
