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
    KeyStatus DescribeKey(string? publicKey);

    /// <summary>
    /// Creates a new lambda, generating a public key if none was requested and
    /// seeding it with the given template (the default one if none was named).
    /// </summary>
    /// <param name="view">How its editor opens for somebody who has not chosen a view of their own</param>
    LambdaInfo Create(string? publicKey, string? template = null, EditorView view = EditorView.Full);

    /// <summary>
    /// Reads a lambda by the private key of its editor.
    /// </summary>
    LambdaInfo? Get(string privateKey);

    /// <summary>
    /// Looks up the deployed lambda behind a public key, if there is one.
    /// </summary>
    ResolvedLambda? Resolve(string publicKey);

    /// <summary>
    /// Looks up the deployed lambda with the given identity, if it is deployed.
    /// </summary>
    /// <remarks>
    /// For the routes that know a lambda by something other than its key -
    /// its domain - and so should not care when the key changes.
    /// </remarks>
    ResolvedLambda? Resolve(long id);

    /// <summary>
    /// Lists the stored versions of a lambda, newest first.
    /// </summary>
    IReadOnlyList<LambdaVersionInfo> GetVersions(string privateKey);

    /// <summary>
    /// Reads a single version including its code.
    /// </summary>
    LambdaVersionContent GetVersion(string privateKey, int version);

    /// <summary>
    /// Stores the given code as a new version.
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
    LambdaVersionInfo Save(string privateKey, string code, VersionNote? note = null, int? after = null);

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
    LambdaInfo Undeploy(string privateKey, string? endedBy = null);

    /// <summary>
    /// Every stretch of time the lambda was online, newest first.
    /// </summary>
    IReadOnlyList<LambdaActivation> GetActivations(string privateKey);

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
    EventHistory GetEvents(int days);

    /// <summary>
    /// Moves the lambda to another public key.
    /// </summary>
    LambdaInfo ChangeKey(string privateKey, string? publicKey);

    /// <summary>
    /// Changes how the editor of the lambda opens for somebody who has not
    /// chosen a view of their own.
    /// </summary>
    LambdaInfo ChangeView(string privateKey, EditorView view);

    /// <summary>
    /// Moves the lambda to another tier. Only ever done by an administrator.
    /// </summary>
    LambdaInfo ChangeTier(string privateKey, LambdaTier tier);

    /// <summary>
    /// Lists the lambda in the sitemap of the installation, or takes it out.
    /// Only ever done by an administrator.
    /// </summary>
    LambdaInfo ChangeSitemap(string privateKey, bool listed);

    /// <summary>
    /// The public keys of the lambdas the operator listed in the sitemap that
    /// are online, for the sitemap.
    /// </summary>
    IReadOnlyList<string> ListSitemap();

    /// <summary>
    /// Sets the domain the lambda answers at, or removes it when nothing is given.
    /// </summary>
    /// <remarks>
    /// Only a premium lambda may be given one. Removing one is always allowed,
    /// so a lambda that dropped out of the tier can still let go of it.
    /// </remarks>
    LambdaInfo ChangeDomain(string privateKey, string? domain);

    /// <summary>
    /// Removes the lambda, its versions and its workspace.
    /// </summary>
    void Delete(string privateKey);

    /// <summary>
    /// Undeploys and removes lambdas that have outlived their tier.
    /// </summary>
    MaintenanceReport RunMaintenance(DateTime now);

    /// <summary>
    /// Counts the lambdas, the deployed ones and the versions kept for them.
    /// </summary>
    LambdaCounts Count();

    /// <summary>
    /// The identity a lambda is filed under, for the services that store things
    /// beside the database. Null when the key belongs to nothing.
    /// </summary>
    long? GetId(string privateKey);

    /// <summary>
    /// What the lambda filed under the given identity may keep in its
    /// workspace, which its tier decides and its owner can switch off. Null
    /// when there is no such lambda.
    /// </summary>
    WorkspaceLimits? GetWorkspaceLimits(long lambdaId);

    /// <summary>
    /// The identity of a lambda its owner may change, for the services that
    /// store things beside the database - its workspace, say.
    /// </summary>
    /// <remarks>
    /// Refuses a demo, whose editor key is announced: reading one is the
    /// point, and anybody could otherwise replace what it serves.
    /// </remarks>
    long RequireEditable(string privateKey);

    /// <summary>
    /// One page of the lambdas on the installation, newest first.
    /// </summary>
    /// <param name="search">Narrows the listing to public keys or domains containing this</param>
    /// <param name="tier">Narrows the listing to one tier</param>
    LambdaPage List(string? search = null, int skip = 0, int take = int.MaxValue, LambdaTier? tier = null);

    /// <summary>
    /// The editor key behind a public one, for the operations that act on a
    /// lambda without having been given its link.
    /// </summary>
    string? GetPrivateKey(string publicKey);

    /// <summary>
    /// The public key behind an editor key, for naming a lambda where the
    /// editor key must not appear - in a log line.
    /// </summary>
    string? GetPublicKey(string privateKey);

}
