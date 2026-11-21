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
    /// <param name="view">How its editor opens for somebody who has not chosen a view of their own</param>
    ValueTask<LambdaInfo> CreateAsync(string? publicKey, string? template = null, EditorView view = EditorView.Full, CancellationToken cancellation = default);

    /// <summary>
    /// Reads a lambda by the private key of its editor.
    /// </summary>
    ValueTask<LambdaInfo?> GetAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// Looks up the deployed lambda behind a public key, if there is one.
    /// </summary>
    ValueTask<ResolvedLambda?> ResolveAsync(string publicKey);

    /// <summary>
    /// Looks up the deployed lambda with the given identity, if it is deployed.
    /// </summary>
    /// <remarks>
    /// For the routes that know a lambda by something other than its key -
    /// its domain - and so should not care when the key changes.
    /// </remarks>
    ValueTask<ResolvedLambda?> ResolveAsync(long id);

    /// <summary>
    /// Lists the stored versions of a lambda, newest first.
    /// </summary>
    ValueTask<IReadOnlyList<LambdaVersionInfo>> GetVersionsAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// Reads a single version with its files.
    /// </summary>
    ValueTask<LambdaVersionContent> GetVersionAsync(string privateKey, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Stores the given files as a new version.
    /// </summary>
    /// <remarks>
    /// Versions are never changed once they are stored: every change to the
    /// code is a new one, and the one online stays exactly what it was.
    /// </remarks>
    /// <param name="note">Why it was written, and which door it came through</param>
    /// <param name="after">
    /// The version this one has to follow directly, if it must: a feature is
    /// merged only on top of the version it is based on, and refused if
    /// another was saved first
    /// </param>
    ValueTask<LambdaVersionInfo> SaveAsync(string privateKey, IReadOnlyList<VersionFile> files, VersionNote? note = null, int? after = null, CancellationToken cancellation = default);

    /// <summary>
    /// Compiles the given files without deploying them.
    /// </summary>
    ValueTask<CompilationOutcome> CheckAsync(string privateKey, IReadOnlyList<VersionFile> files, CancellationToken cancellation = default);

    /// <summary>
    /// Builds the given version (or the latest one) and makes it the active deployment.
    /// </summary>
    /// <param name="origin">Who is putting it online, for the deployment history</param>
    ValueTask<DeploymentResult> DeployAsync(string privateKey, int? version, VersionOrigin? origin = null, CancellationToken cancellation = default);

    /// <summary>
    /// Takes the lambda off the air, keeping its code.
    /// </summary>
    /// <param name="endedBy">Why, for the deployment history: stopped by the owner unless said otherwise</param>
    ValueTask<LambdaInfo> UndeployAsync(string privateKey, ActivationEnding? endedBy = null);

    /// <summary>
    /// Every stretch of time the lambda was online, newest first.
    /// </summary>
    ValueTask<IReadOnlyList<LambdaActivation>> GetActivationsAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// What a version of the lambda filed under the given identity is made
    /// of - its size, what its code reaches for, what its documentation says -
    /// or nothing at all for no version.
    /// </summary>
    /// <remarks>
    /// Read off the version once and kept, since a version never changes.
    /// </remarks>
    VersionFacts GetFacts(long lambdaId, int? version);

    /// <summary>
    /// What has been happening on the installation, counted by day, oldest first.
    /// </summary>
    /// <param name="days">How far back to go</param>
    ValueTask<EventHistory> GetEventsAsync(int days, CancellationToken cancellation = default);

    /// <summary>
    /// Moves the lambda to another public key.
    /// </summary>
    ValueTask<LambdaInfo> ChangeKeyAsync(string privateKey, string? publicKey, CancellationToken cancellation = default);

    /// <summary>
    /// Changes how the editor of the lambda opens for somebody who has not
    /// chosen a view of their own.
    /// </summary>
    ValueTask<LambdaInfo> ChangeViewAsync(string privateKey, EditorView view, CancellationToken cancellation = default);

    /// <summary>
    /// Moves the lambda to another tier. Only ever done by an administrator.
    /// </summary>
    ValueTask<LambdaInfo> ChangeTierAsync(string privateKey, LambdaTier tier, CancellationToken cancellation = default);

    /// <summary>
    /// Sets the domain the lambda answers at, or removes it when nothing is given.
    /// </summary>
    /// <remarks>
    /// Only a lambda in a tier with domains of its own may be given one. Removing one is always allowed,
    /// so a lambda that dropped out of the tier can still let go of it.
    /// </remarks>
    ValueTask<LambdaInfo> ChangeDomainAsync(string privateKey, string? domain, CancellationToken cancellation = default);

    /// <summary>
    /// Removes the lambda, its versions and its workspace.
    /// </summary>
    /// <param name="origin">Who is asking: the operator may delete a lambda they blocked, its owner may not</param>
    ValueTask DeleteAsync(string privateKey, VersionOrigin? origin = null);

    /// <summary>
    /// Blocks the lambda: takes it and the previews of its features offline,
    /// and refuses every change to it from here on, until the block is lifted.
    /// Only ever done by the operator.
    /// </summary>
    /// <remarks>
    /// Its code, its data and its history are kept and can still be read; it
    /// may still be deleted by the operator. Blocked again, it keeps the time
    /// it was first blocked and takes the new reason.
    /// </remarks>
    /// <param name="reason">Why, in the operator's words - shown to the owner as they wrote it</param>
    ValueTask<LambdaInfo> BlockAsync(string privateKey, string? reason);

    /// <summary>
    /// Blocks a lambda, and the addresses given with it for two weeks - every
    /// address read before anything is done, so one that cannot be blocked
    /// (malformed, or one of the installation's own networks) leaves the lambda
    /// as it was too.
    /// </summary>
    ValueTask<LambdaInfo> BlockAsync(string privateKey, string? reason, IReadOnlyList<string> addresses);

    /// <summary>
    /// Lifts the block of a lambda. It stays offline until its owner, or the
    /// operator, puts it online again.
    /// </summary>
    ValueTask<LambdaInfo> UnblockAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// The addresses the lambda was changed from lately, most recent first -
    /// for the operator who blocks it, to block them as well.
    /// </summary>
    ValueTask<IReadOnlyList<EditorAddress>> GetEditorAddressesAsync(string privateKey, CancellationToken cancellation = default);

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
    /// The tier of the lambda filed under the given identity, or nothing when
    /// there is no such lambda.
    /// </summary>
    ValueTask<LambdaTier?> GetTierAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// The identity of a lambda its owner may change, for the services that
    /// store things beside the database - its workspace, say.
    /// </summary>
    /// <remarks>
    /// Refuses a demo, whose editor key is announced: reading one is the
    /// point, and anybody could otherwise replace what it serves - and a
    /// lambda the operator blocked. Notes where the change comes from.
    /// </remarks>
    ValueTask<long> RequireEditableAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// One page of the lambdas on the installation, narrowed and ordered as asked.
    /// </summary>
    ValueTask<LambdaPage> ListAsync(LambdaQuery query, int skip = 0, int take = int.MaxValue, CancellationToken cancellation = default);

    /// <summary>
    /// Which of the platform's features a lambda uses, for the operator's page of it.
    /// </summary>
    ValueTask<LambdaUses> GetUsesAsync(string privateKey, CancellationToken cancellation = default);

    /// <summary>
    /// The editor key behind a public one, for the operations that act on a
    /// lambda without having been given its link.
    /// </summary>
    ValueTask<string?> GetPrivateKeyAsync(string publicKey, CancellationToken cancellation = default);

    /// <summary>
    /// The public key behind an editor key, for naming a lambda where the
    /// editor key must not appear - in a log line.
    /// </summary>
    ValueTask<string?> GetPublicKeyAsync(string privateKey, CancellationToken cancellation = default);

}
