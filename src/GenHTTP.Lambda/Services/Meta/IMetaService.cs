using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Workspace;

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
    /// Looks up the deployed lambda with the given identity, if it is deployed.
    /// </summary>
    /// <remarks>
    /// For the routes that know a lambda by something other than its key -
    /// its domain - and so should not care when the key changes.
    /// </remarks>
    ValueTask<ResolvedLambda?> ResolveAsync(long id, CancellationToken cancellation = default);

    /// <summary>
    /// Lists the stored versions of a lambda, newest first.
    /// </summary>
    ValueTask<IReadOnlyList<LambdaVersionInfo>> GetVersionsAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// Reads a single version including its code.
    /// </summary>
    ValueTask<LambdaVersionContent> GetVersionAsync(string privateKey, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Stores the given code as a new version, which becomes the newest.
    /// </summary>
    /// <param name="note">Why it was written, and which door it came through</param>
    ValueTask<LambdaVersionInfo> SaveAsync(string privateKey, string code, VersionNote? note = null, CancellationToken cancellation = default);

    /// <summary>
    /// Saves the given code over the newest version, which is the one being
    /// worked on.
    /// </summary>
    /// <remarks>
    /// Refused for any other version: those are history, kept as they were so
    /// there is something to go back to. What is online does not change until
    /// the version is deployed again, even where this is the version online.
    /// Code that is what the version holds already only updates its notes.
    /// </remarks>
    /// <param name="version">The number of the newest version, said so a version saved by somebody else meanwhile is not overwritten</param>
    /// <param name="note">The notes to replace the ones it has, where given, and which door it came through</param>
    ValueTask<LambdaVersionInfo> UpdateAsync(string privateKey, int version, string code, VersionNote? note = null, CancellationToken cancellation = default);

    /// <summary>
    /// Starts a new version as a copy of an existing one - the newest unless
    /// another is named - which becomes the newest.
    /// </summary>
    /// <param name="note">What the copy is for; left out, it keeps the specification of the one it copies and says what it is a copy of</param>
    ValueTask<LambdaVersionInfo> CopyAsync(string privateKey, int? version, VersionNote? note = null, CancellationToken cancellation = default);

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
    /// Moves the lambda to another tier. Only ever done by an administrator.
    /// </summary>
    ValueTask<LambdaInfo> ChangeTierAsync(string privateKey, LambdaTier tier, CancellationToken cancellation = default);

    /// <summary>
    /// Sets the domain the lambda answers at, or removes it when nothing is given.
    /// </summary>
    /// <remarks>
    /// Only a premium lambda may be given one. Removing one is always allowed,
    /// so a lambda that dropped out of the tier can still let go of it.
    /// </remarks>
    ValueTask<LambdaInfo> ChangeDomainAsync(string privateKey, string? domain, CancellationToken cancellation = default);

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
    /// What the lambda filed under the given identity may keep in its
    /// workspace, which its tier decides and its owner can switch off. Null
    /// when there is no such lambda.
    /// </summary>
    ValueTask<WorkspaceLimits?> GetWorkspaceLimitsAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// The identity of a lambda its owner may change, for the services that
    /// store things beside the database - its workspace, say.
    /// </summary>
    /// <remarks>
    /// Refuses a demo, whose editor key is announced: reading one is the
    /// point, and anybody could otherwise replace what it serves.
    /// </remarks>
    ValueTask<long> RequireEditableAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// One page of the lambdas on the installation, newest first.
    /// </summary>
    /// <param name="search">Narrows the listing to public keys or domains containing this</param>
    /// <param name="tier">Narrows the listing to one tier</param>
    ValueTask<LambdaPage> ListAsync(string? search = null, int skip = 0, int take = int.MaxValue, LambdaTier? tier = null,
                                    CancellationToken cancellation = default);

    /// <summary>
    /// The editor key behind a public one, for the operations that act on a
    /// lambda without having been given its link.
    /// </summary>
    ValueTask<string?> GetPrivateKeyAsync(string publicKey, CancellationToken cancellation = default);

}
