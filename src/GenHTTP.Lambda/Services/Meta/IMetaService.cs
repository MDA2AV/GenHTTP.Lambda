using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Creates, reads and updates lambdas. Everything the editor does goes through here.
/// </summary>
public interface IMetaService
{

    /// <summary>
    /// Tells whether a public key could be claimed, and whether a lambda is
    /// already answering there.
    /// </summary>
    ValueTask<KeyStatus> DescribeKeyAsync(string? publicKey, CancellationToken cancellation = default);

    /// <summary>
    /// Creates a new lambda, generating a public key if none was requested and
    /// seeding it with the given template (the default one if none was named).
    /// </summary>
    ValueTask<LambdaInfo> CreateAsync(string? publicKey, string? template = null, CancellationToken cancellation = default);

    /// <summary>
    /// Reads a lambda by the private key of its editor.
    /// </summary>
    ValueTask<LambdaInfo?> GetAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// Looks up the deployed lambda behind a public key, if there is one.
    /// </summary>
    ValueTask<ResolvedLambda?> ResolveAsync(string publicKey, CancellationToken cancellation = default);

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
    /// <param name="note">Why it was written, and which door it came through</param>
    ValueTask<LambdaVersionInfo> SaveAsync(string privateKey, string code, VersionNote? note = null, CancellationToken cancellation = default);

    /// <summary>
    /// Compiles the given code without deploying it.
    /// </summary>
    ValueTask<CompilationOutcome> CheckAsync(string privateKey, string code, CancellationToken cancellation = default);

    /// <summary>
    /// Builds the given version (or the latest one) and makes it the active deployment.
    /// </summary>
    /// <param name="origin">Who is putting it online, for the deployment history</param>
    ValueTask<DeploymentResult> DeployAsync(string privateKey, int? version, string? origin = null, CancellationToken cancellation = default);

    /// <summary>
    /// Takes the lambda off the air, keeping its code.
    /// </summary>
    /// <param name="endedBy">Why, for the deployment history: stopped by the owner unless said otherwise</param>
    ValueTask<LambdaInfo> UndeployAsync(string privateKey, string? endedBy = null, CancellationToken cancellation = default);

    /// <summary>
    /// Every stretch of time the lambda was online, newest first.
    /// </summary>
    ValueTask<IReadOnlyList<LambdaActivation>> GetActivationsAsync(string privateKey, CancellationToken cancellation = default);

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

    /// <summary>
    /// Counts the lambdas, the deployed ones and the versions kept for them.
    /// </summary>
    ValueTask<LambdaCounts> CountAsync(CancellationToken cancellation = default);

    /// <summary>
    /// The identity a lambda is filed under, for the services that store things
    /// beside the database. Null when the key belongs to nothing.
    /// </summary>
    ValueTask<long?> GetIdAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// One page of the lambdas on the installation, newest first.
    /// </summary>
    /// <param name="search">Narrows the listing to public keys containing this</param>
    ValueTask<LambdaPage> ListAsync(string? search = null, int skip = 0, int take = int.MaxValue, CancellationToken cancellation = default);

    /// <summary>
    /// The editor key behind a public one, for the operations that act on a
    /// lambda without having been given its link.
    /// </summary>
    ValueTask<string?> GetPrivateKeyAsync(string publicKey, CancellationToken cancellation = default);

}
