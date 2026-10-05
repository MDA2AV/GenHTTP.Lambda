using System.Collections.Concurrent;
using System.Security.Cryptography;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Features;

/// <summary>
/// Keeps the features of the lambdas: their notes in the database, their files
/// and their copy of the data on disk, and their previews in the deployment
/// service beside the lambdas themselves.
/// </summary>
public sealed class FeatureService(IDbContextFactory<LambdaDbContext> databases, IMetaService meta, IStorageService storage,
                                   IDeploymentService deployments, ISecretVault secrets, IDatabaseVault stores, LambdaOptions options, ILimitsService limits, ILogBook book,
                                   DatabaseChanges changes, ILogger<FeatureService> logger)
    : IFeatureService
{

    /// <summary>
    /// The previews being served, by their key, see <see cref="ResolutionCache{TKey}"/>.
    /// </summary>
    private readonly ResolutionCache<string> _previews = new(changes);

    /// <summary>
    /// How long the name of a feature may be.
    /// </summary>
    public const int MaxName = 80;

    /// <summary>
    /// Takes turns over what changes one feature, one feature at a time.
    /// </summary>
    /// <remarks>
    /// A save landing while the feature is being merged would be lost with the
    /// feature, and one landing while the preview is deployed would leave the
    /// preview serving something nobody saved. Striped, like the lambdas'.
    ///
    /// Plain locks, held only while the change is read and written: never
    /// across an await, so what compiles or copies does that first and takes
    /// the turn to write down what it came to (see <see cref="DeployAsync"/>
    /// and <see cref="MergeAsync"/>).
    /// </remarks>
    private readonly Lock[] _stripes = [.. Enumerable.Range(0, 64).Select(_ => new Lock())];

    /// <summary>
    /// Takes turns over copying the data of a feature, one copy at a time.
    /// </summary>
    /// <remarks>
    /// Waited for asynchronously because it is held across the copy, which
    /// leaves the reactor (see Offload); two copies into the same feature
    /// would write over each other's staging.
    /// </remarks>
    private readonly SemaphoreSlim[] _copying = [.. Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1))];

    /// <summary>
    /// The builds of previews handed out, so two deployments of one preview
    /// under way at once are never given the same number.
    /// </summary>
    private readonly ConcurrentDictionary<long, int> _stamps = [];

    #region Reading

    public IReadOnlyList<FeatureInfo> List(string privateKey)
    {
        using var database = databases.CreateDbContext();

        var lambda = RequireLambda(database, privateKey);

        var newest = Newest(database, lambda.Id);

        var features = database.Features.AsNoTracking()
                               .Where(f => f.LambdaId == lambda.Id)
                               .OrderByDescending(f => f.Modified)
                               .ToList();

        return [.. features.Select(f => Describe(f, newest))];
    }

    public FeatureContent Get(string privateKey, string feature)
    {
        using var database = databases.CreateDbContext();

        var lambda = RequireLambda(database, privateKey);

        var entity = RequireFeature(database, lambda.Id, feature);

        var code = storage.ReadFeature(lambda.Id, entity.Id)
                ?? throw LambdaException.NotFound($"The files of the feature '{entity.Name}' are no longer available.");

        return new FeatureContent(Describe(entity, Newest(database, lambda.Id)), code);
    }

    public (long LambdaId, long FeatureId) Require(string privateKey, string feature, bool editable)
    {
        var lambdaId = editable
            ? meta.RequireEditable(privateKey)
            : meta.GetId(privateKey) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        using var database = databases.CreateDbContext();

        var entity = RequireFeature(database, lambdaId, feature);

        return (lambdaId, entity.Id);
    }

    public string? GetName(string privateKey, string feature)
    {
        var key = feature.Trim().ToLowerInvariant();

        using var database = databases.CreateDbContext();

        return database.Features.AsNoTracking()
                       .Where(f => f.Key == key && f.Lambda!.PrivateKey == privateKey)
                       .Select(f => f.Name)
                       .FirstOrDefault();
    }

    public ResolvedLambda? ResolvePreview(string key)
    {
        // anything that is not one of our keys is answered without asking
        // the database or taking room in the cache, since every request to
        // /features/ is asked about
        if (!IsKey(key))
        {
            return null;
        }

        return _previews.Resolve(key, LoadPreview);
    }

    /// <summary>
    /// Reads the preview of a feature, synchronously like the lambdas it
    /// stands beside (see <see cref="MetaService.Resolve(string)"/>).
    /// </summary>
    private ResolvedLambda? LoadPreview(string key)
    {
        using var database = databases.CreateDbContext();

        var found = database.Features.AsNoTracking()
                            .Where(f => f.Key == key && f.Previewed != null)
                            .Join(database.Lambdas, f => f.LambdaId, l => l.Id, (f, l) => new { Feature = f, Lambda = l })
                            .FirstOrDefault();

        if (found == null)
        {
            return null;
        }

        var workspace = DataSwitches.IsEnabled(database, found.Lambda.Id, DataKinds.Workspace);

        return new ResolvedLambda(found.Lambda.Id, found.Lambda.PublicKey, found.Lambda.Tier, found.Feature.BaseVersion,
                                  found.Feature.Previewed!.Value, workspace,
                                  new ResolvedFeature(found.Feature.Id, found.Feature.Key, found.Feature.Preview));
    }

    #endregion

    #region Working on it

    public async ValueTask<FeatureInfo> CreateAsync(string privateKey, FeatureDraft draft, CancellationToken cancellation = default)
    {
        var lambdaId = meta.RequireEditable(privateKey);

        var name = Name(draft.Name);

        using var database = databases.CreateDbContext();

        var lambda = database.Lambdas.First(l => l.Id == lambdaId);

        var allowed = limits.Of(lambda.Tier).Features;

        if (database.Features.Count(f => f.LambdaId == lambdaId) >= allowed)
        {
            throw LambdaException.Conflict($"A lambda may have {allowed} features open at once. Merge or delete one first.");
        }

        var newest = Newest(database, lambdaId)
                  ?? throw LambdaException.Invalid("There is no version yet to start a feature from. Save one first.");

        var from = draft.Base ?? newest;

        var code = storage.Read(lambdaId, from);

        if (code == null || !database.Deployments.Any(d => d.LambdaId == lambdaId && d.Version == from))
        {
            throw LambdaException.NotFound($"Version {from} does not exist. The newest is version {newest}.");
        }

        var now = DateTime.UtcNow;

        var taken = database.Features.Where(f => f.LambdaId == lambdaId).Select(f => f.Branch).ToList();

        var entity = new FeatureEntity
        {
            LambdaId = lambdaId,
            Key = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16)),
            Name = name,
            Branch = Branch(draft.Branch, name, taken),
            Specification = VersionInput.Tidy(draft.Specification, VersionNote.MaxSpecification),
            BaseVersion = from,
            Origin = draft.Origin,
            Created = now,
            Modified = now
        };

        database.Features.Add(entity);

        // being worked on is being used: a free lambda somebody is changing
        // is not left to expire while they do
        lambda.Modified = now;

        try
        {
            database.SaveChanges();
        }
        catch (DbUpdateException) when (draft.Branch == null)
        {
            // another feature took the same branch in the meantime: the next free one
            entity.Branch = FeatureBranches.For(name, [.. database.Features.AsNoTracking().Where(f => f.LambdaId == lambdaId).Select(f => f.Branch)]);

            database.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw LambdaException.Conflict($"Another feature of this lambda is at the branch '{draft.Branch}'.");
        }

        try
        {
            storage.WriteFeature(lambdaId, entity.Id, code);

            await CopyDataAsync(lambdaId, entity.Id, WorkspaceEnabled(database, lambdaId), cancellation);
        }
        catch
        {
            database.Secrets.Where(s => s.FeatureId == entity.Id).ExecuteDelete();

            stores.RemoveCopy(lambdaId, entity.Id);

            storage.DeleteFeature(lambdaId, entity.Id);

            database.Features.Remove(entity);

            database.SaveChanges();

            throw;
        }

        logger.LogInformation("Created feature #{FeatureId} of lambda #{LambdaId} base {Version}", entity.Id, lambdaId, from);

        return Describe(entity, newest);
    }

    public FeatureInfo Update(string privateKey, string feature, FeatureUpdate update)
    {
        var (database, lambda, entity, turn) = Locked(privateKey, feature);

        using var context = database;

        using var held = turn;

        if (update.Name != null)
        {
            entity.Name = Name(update.Name);
        }

        // left out, a note stays; sent empty, it is cleared
        if (update.Specification != null)
        {
            entity.Specification = VersionInput.Tidy(update.Specification, VersionNote.MaxSpecification);
        }

        if (update.Change != null)
        {
            entity.Change = VersionInput.Tidy(update.Change, VersionNote.MaxChange);
        }

        var newest = Newest(database, lambda.Id);

        if (update.Base is { } moved && moved != entity.BaseVersion)
        {
            if (!database.Deployments.Any(d => d.LambdaId == lambda.Id && d.Version == moved))
            {
                throw LambdaException.NotFound($"Version {moved} does not exist. The newest is version {newest}.");
            }

            logger.LogInformation("Moved base of feature #{FeatureId} of lambda #{LambdaId} from {From} to {To}",
                                  entity.Id, lambda.Id, entity.BaseVersion, moved);

            entity.BaseVersion = moved;
        }

        entity.Modified = DateTime.UtcNow;

        database.SaveChanges();

        return Describe(entity, newest);
    }

    public FeatureInfo Save(string privateKey, string feature, string code, VersionNote? note = null, int? after = null)
    {
        var files = VersionInput.Validate(code);

        var (database, lambda, entity, turn) = Locked(privateKey, feature);

        using var context = database;

        using var held = turn;

        // a feature keeps no history: a save made from what somebody else has
        // since replaced would replace their work without anybody knowing
        if (after is { } read && read != entity.Revision)
        {
            throw LambdaException.Conflict($"The feature '{entity.Name}' was saved again since it was read (by the agent, perhaps, or in another window). "
                                         + "Read it again and make the change on top of what it holds now.");
        }

        // what a feature holds becomes a version, so it is held to what a
        // version may hold from the start rather than refused when merged
        VersionInput.ValidateAllowance(files, lambda.Tier, limits);

        storage.WriteFeature(lambda.Id, entity.Id, code);

        entity.Specification = VersionInput.Tidy(note?.Specification, VersionNote.MaxSpecification) ?? entity.Specification;
        entity.Change = VersionInput.Tidy(note?.Change, VersionNote.MaxChange) ?? entity.Change;

        var now = DateTime.UtcNow;

        entity.Revision++;
        entity.Modified = now;
        lambda.Modified = now;

        database.SaveChanges();

        return Describe(entity, Newest(database, lambda.Id));
    }

    /// <remarks>
    /// Compiled outside the feature's turn, which is a lock and is not held
    /// across the seconds that takes (see <see cref="_stripes"/>): the build
    /// is numbered in the turn, compiled, and written down in the turn again -
    /// unless a deployment started later finished first, which is then what
    /// stays online.
    /// </remarks>
    public async ValueTask<FeatureDeployment> DeployAsync(string privateKey, string feature, CancellationToken cancellation = default)
    {
        long lambdaId, featureId;

        string publicKey, code;

        int stamp, revision;

        WorkspaceLimits workspace;

        {
            var (database, lambda, entity, turn) = Locked(privateKey, feature);

            using var context = database;

            using var held = turn;

            code = storage.ReadFeature(lambda.Id, entity.Id)
                ?? throw LambdaException.NotFound($"The files of the feature '{entity.Name}' are no longer available.");

            workspace = limits.WorkspaceOf(lambda.Tier, WorkspaceEnabled(database, lambda.Id));

            // handed out here, so one started meanwhile is given the next
            stamp = _stamps.AddOrUpdate(entity.Id, entity.Preview + 1, (_, last) => Math.Max(last, entity.Preview) + 1);

            (lambdaId, featureId, publicKey, revision) = (lambda.Id, entity.Id, lambda.PublicKey, entity.Revision);
        }

        CompilationOutcome outcome;

        // under the feature's name, like a deployment of the lambda is under
        // the lambda's: building the preview runs its code, and what that
        // prints belongs with the feature
        using (options.CaptureLambdaOutput
               ? LambdaOutput.Enter(new OutputScope(publicKey, book, options.MaxOutputLines, lambdaId, featureId))
               : null)
        {
            outcome = await deployments.PreviewAsync(lambdaId, featureId, stamp, code, workspace, cancellation);
        }

        try
        {
            return Previewed(privateKey, feature, stamp, revision, code, outcome);
        }
        catch (LambdaException)
        {
            // merged or deleted while it was being built: nothing serves it
            deployments.EvictPreview(lambdaId, featureId);
            throw;
        }
    }

    /// <summary>
    /// Writes down what a deployment of the preview came to, in the feature's turn.
    /// </summary>
    private FeatureDeployment Previewed(string privateKey, string feature, int stamp, int revision, string code, CompilationOutcome outcome)
    {
        var (database, lambda, entity, turn) = Locked(privateKey, feature);

        using var context = database;

        using var held = turn;

        if (outcome.Success && stamp > entity.Preview)
        {
            try
            {
                // what the preview serves from now on, until it is deployed again -
                // a restart included - however much the feature changes meanwhile
                storage.WritePreview(lambda.Id, entity.Id, code);

                var now = DateTime.UtcNow;

                entity.Preview = stamp;
                entity.PreviewOf = revision;
                entity.Previewed = now;
                entity.Modified = now;
                lambda.Modified = now;

                database.SaveChanges();
            }
            catch
            {
                // built, but not recorded: the next deployment would take the
                // same stamp for the build already there, and serve it
                deployments.EvictPreview(lambda.Id, entity.Id);
                throw;
            }
        }

        return new FeatureDeployment(outcome.Success, Describe(entity, Newest(database, lambda.Id)), outcome.Diagnostics);
    }

    public FeatureInfo Undeploy(string privateKey, string feature)
    {
        var (database, lambda, entity, turn) = Locked(privateKey, feature);

        using var context = database;

        using var held = turn;

        if (entity.Previewed != null)
        {
            entity.Previewed = null;
            entity.Modified = DateTime.UtcNow;

            database.SaveChanges();

            deployments.EvictPreview(lambda.Id, entity.Id);
        }

        return Describe(entity, Newest(database, lambda.Id));
    }

    public async ValueTask<FeatureInfo> RefreshDataAsync(string privateKey, string feature, CancellationToken cancellation = default)
    {
        long lambdaId, featureId;

        bool workspace;

        {
            var (database, lambda, entity, turn) = Locked(privateKey, feature);

            using var context = database;

            using var held = turn;

            (lambdaId, featureId, workspace) = (lambda.Id, entity.Id, WorkspaceEnabled(database, lambda.Id));
        }

        await CopyDataAsync(lambdaId, featureId, workspace, cancellation);

        {
            var (database, lambda, entity, turn) = Locked(privateKey, feature);

            using var context = database;

            using var held = turn;

            // built again on its next request, so code that read the data into
            // memory when it started reads the fresh copy rather than writing the
            // old one back over it
            deployments.EvictPreview(lambda.Id, entity.Id);

            entity.Modified = DateTime.UtcNow;

            database.SaveChanges();

            return Describe(entity, Newest(database, lambda.Id));
        }
    }

    /// <summary>
    /// Copies the data of the lambda into the feature as it is now, so trying
    /// the feature cannot touch what the lambda's visitors keep.
    /// </summary>
    /// <remarks>
    /// The workspace and the database can be gigabytes, so they are copied
    /// away from the reactor and in the feature's turn to be copied (see
    /// <see cref="_copying"/>) - not in its turn to change, which is a lock.
    /// </remarks>
    private async ValueTask CopyDataAsync(long lambdaId, long featureId, bool workspace, CancellationToken cancellation)
    {
        var copying = _copying[(int)((ulong)featureId % (ulong)_copying.Length)];

        await copying.WaitAsync(cancellation);

        try
        {
            if (workspace)
            {
                await storage.CopyWorkspaceAsync(lambdaId, featureId, cancellation);
            }

            // and the secrets, so the preview can call what the lambda calls
            secrets.Copy(lambdaId, featureId);

            // and the database, so the preview can change its records - and
            // its schema - without the lambda's visitors noticing
            await stores.CopyAsync(lambdaId, featureId, cancellation);
        }
        finally
        {
            copying.Release();
        }
    }

    #endregion

    #region Finishing it

    /// <remarks>
    /// Checked outside the feature's turn, which is a lock and is not held
    /// across the compiler (see <see cref="_stripes"/>), and merged in it
    /// once the check passed - refused if the feature was saved meanwhile,
    /// since what was checked would then not be what is merged.
    /// </remarks>
    public async ValueTask<FeatureMerge> MergeAsync(string privateKey, string feature, VersionNote? note = null, bool deploy = false,
                                                    CancellationToken cancellation = default)
    {
        string code;

        string name;

        int revision;

        {
            var (database, lambda, entity, turn) = Locked(privateKey, feature);

            using var context = database;

            using var held = turn;

            var newest = Newest(database, lambda.Id);

            if (newest != entity.BaseVersion)
            {
                throw LambdaException.Conflict(Behind(entity, newest));
            }

            code = storage.ReadFeature(lambda.Id, entity.Id)
                ?? throw LambdaException.NotFound($"The files of the feature '{entity.Name}' are no longer available.");

            if (code == storage.Read(lambda.Id, entity.BaseVersion))
            {
                throw LambdaException.Invalid($"The feature '{entity.Name}' holds exactly what version {entity.BaseVersion} holds, so there is nothing to merge. Change it first, or delete it.");
            }

            name = entity.Name;

            revision = entity.Revision;
        }

        // a version is only made of what compiles: the feature is kept to be
        // fixed, rather than turned into a version nobody can put online
        var check = await meta.CheckAsync(privateKey, code, cancellation);

        if (!check.Success)
        {
            return new FeatureMerge(name, false, null, check.Diagnostics, null);
        }

        var (version, lambdaId, origin) = Merged(privateKey, feature, note, code, revision);

        DeploymentResult? deployment = null;

        if (deploy)
        {
            try
            {
                deployment = await meta.DeployAsync(privateKey, version.Version, origin, CancellationToken.None);
            }
            catch (Exception e)
            {
                // merged all the same, which is what the answer has to say
                logger.LogWarning(e, "Failed to deploy lambda #{LambdaId} version {Version} after merge", lambdaId, version.Version);

                deployment = new DeploymentResult(false, null, [CompilationDiagnostic.Error($"Merged as version {version.Version}, but it could not be put online: {e.Message}")]);
            }
        }

        return new FeatureMerge(name, true, version, [], deployment);
    }

    /// <summary>
    /// Makes the feature the next version, in its turn.
    /// </summary>
    private (LambdaVersionInfo Version, long LambdaId, string Origin) Merged(string privateKey, string feature, VersionNote? note, string code, int revision)
    {
        var (database, lambda, entity, turn) = Locked(privateKey, feature);

        using var context = database;

        using var held = turn;

        if (entity.Revision != revision)
        {
            throw LambdaException.Conflict($"The feature '{entity.Name}' was saved while it was being checked, so what was checked is not what it holds now. Merge it again.");
        }

        var origin = note?.Origin ?? VersionOrigins.Api;

        var merged = new VersionNote(string.IsNullOrWhiteSpace(note?.Specification) ? entity.Specification : note.Specification,
                                     string.IsNullOrWhiteSpace(note?.Change) ? entity.Change ?? $"Merges the feature '{entity.Name}'" : note.Change,
                                     origin);

        // refused if a version was saved since the feature was based on its
        // newest, by the same turn every save of the lambda takes
        var version = meta.Save(privateKey, code, merged, after: entity.BaseVersion);

        // the version is there from here on: what follows is seen through
        // however the caller fares, or a feature would stay behind that was merged
        Remove(database, lambda.Id, entity);

        logger.LogInformation("Merged feature #{FeatureId} of lambda #{LambdaId} as version {Version}", entity.Id, lambda.Id, version.Version);

        return (version, lambda.Id, origin);
    }

    public string Delete(string privateKey, string feature)
    {
        var (database, lambda, entity, turn) = Locked(privateKey, feature);

        using var context = database;

        using var held = turn;

        Remove(database, lambda.Id, entity);

        logger.LogInformation("Deleted feature #{FeatureId} of lambda #{LambdaId}", entity.Id, lambda.Id);

        return entity.Name;
    }

    public int RunMaintenance(DateTime now)
    {
        using var database = databases.CreateDbContext();

        var quiet = now - limits.Get().OfflineAfter;

        // the same rule the lambda's own deployment lives by: online while
        // somebody works on it, offline once nobody has for the whole window
        var stale = database.Features.AsNoTracking()
                            .Where(f => f.Previewed != null && f.Modified < quiet && f.Lambda!.Tier == LambdaTier.Free)
                            .Select(f => new { f.Id, f.LambdaId })
                            .ToList();

        if (stale.Count == 0)
        {
            return 0;
        }

        var ids = stale.Select(f => f.Id).ToList();

        // asked again as it is written, so a preview deployed a moment ago -
        // which made it no longer stale - is left online
        var taken = database.Features
                            .Where(f => ids.Contains(f.Id) && f.Previewed != null && f.Modified < quiet)
                            .ExecuteUpdate(u => u.SetProperty(f => f.Previewed, (DateTime?)null));

        // offline in the database first, so no request builds one again
        foreach (var feature in stale)
        {
            if (!database.Features.Any(f => f.Id == feature.Id && f.Previewed != null))
            {
                deployments.EvictPreview(feature.LambdaId, feature.Id);
            }
        }

        logger.LogInformation("Undeployed {Count} feature(s) by maintenance", taken);

        return taken;
    }

    public int Sweep()
    {
        using var database = databases.CreateDbContext();

        // a row is written before its folder and removed before it, so a
        // folder without a row is one left behind - by a request that was
        // still writing to the copy of the data when its feature went
        var known = (database.Features.AsNoTracking().Select(f => f.Id).ToList()).ToHashSet();

        var swept = 0;

        foreach (var (lambdaId, featureId) in storage.ListFeatures())
        {
            if (!known.Contains(featureId))
            {
                deployments.EvictPreview(lambdaId, featureId);

                stores.RemoveCopy(lambdaId, featureId);

                storage.DeleteFeature(lambdaId, featureId);

                swept++;
            }
        }

        if (swept > 0)
        {
            logger.LogInformation("Removed files of {Count} deleted feature(s) by maintenance", swept);
        }

        return swept;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// What somebody is told who tries to merge a feature that is not based on
    /// the newest version - which is also how to get it there.
    /// </summary>
    internal static string Behind(FeatureEntity feature, int? newest)
        => $"The feature '{feature.Name}' is based on version {feature.BaseVersion}, but the newest version is {newest}: merging it now would undo "
         + $"what {(newest - feature.BaseVersion == 1 ? $"version {newest}" : $"versions {feature.BaseVersion + 1} to {newest}")} changed. "
         + $"Bring those changes into the feature first - read version {newest} (read_lambda with version, or GET /api/v1/lambdas/{{privateKey}}/versions/{newest}), "
         + $"compare it with version {feature.BaseVersion}, and change the feature to match - then move its base to {newest} "
         + $"(update_feature with base: {newest}, or PATCH the feature with base) and merge again.";

    private static FeatureInfo Describe(FeatureEntity feature, int? newest)
        => new(feature.Key, feature.Name, feature.Branch, feature.Specification, feature.Change, feature.BaseVersion, newest, feature.Origin,
               feature.Created, feature.Modified, feature.Previewed != null, feature.Previewed != null && feature.PreviewOf == feature.Revision,
               feature.Previewed, feature.Revision);

    /// <summary>
    /// Whether a key is one this service could have handed out: 32 lower case
    /// hexadecimal digits.
    /// </summary>
    internal static bool IsKey(string? key)
        => key is { Length: 32 } && key.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    /// <summary>
    /// The branch a new feature is in the lambda's repository: the one asked
    /// for, which has to be free, or one named after it.
    /// </summary>
    /// <param name="taken">The branches of the lambda's other features</param>
    private static string Branch(string? wanted, string name, IReadOnlyList<string> taken)
    {
        if (wanted == null)
        {
            return FeatureBranches.For(name, taken);
        }

        if (FeatureBranches.Check(wanted) is { } complaint)
        {
            throw LambdaException.Invalid(complaint);
        }

        if (FeatureBranches.Clashes(wanted, taken))
        {
            throw LambdaException.Conflict($"Another feature of this lambda is at the branch '{wanted}', or at one that cannot sit beside it.");
        }

        return wanted;
    }

    private static string Name(string? name)
    {
        var trimmed = name?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            throw LambdaException.Invalid("A feature needs a name: what it is, in a few words.");
        }

        if (trimmed.Length > MaxName)
        {
            throw LambdaException.Invalid($"The name of a feature must not be longer than {MaxName} characters.");
        }

        if (trimmed.Contains('\n'))
        {
            throw LambdaException.Invalid("The name of a feature has to fit on one line.");
        }

        return trimmed;
    }

    /// <remarks>
    /// The row goes first, so no request finds the preview from then on; the
    /// handler is let go twice, because a request that found it a moment
    /// before may still be building it, and would otherwise leave a handler
    /// behind for a feature that no longer exists.
    /// </remarks>
    private void Remove(LambdaDbContext database, long lambdaId, FeatureEntity feature)
    {
        // the copy of the secrets cascades in the schema, but only where the
        // connection has foreign keys switched on
        database.Secrets.Where(s => s.FeatureId == feature.Id).ExecuteDelete();

        database.Features.Remove(feature);

        database.SaveChanges();

        secrets.Invalidate(lambdaId);

        deployments.EvictPreview(lambdaId, feature.Id);

        // its copy of the database is let go of before its folder goes
        stores.RemoveCopy(lambdaId, feature.Id);

        storage.DeleteFeature(lambdaId, feature.Id);

        deployments.EvictPreview(lambdaId, feature.Id);
    }

    /// <summary>
    /// The lambda and the feature, read once it is the feature's turn to change.
    /// </summary>
    /// <returns>
    /// The context both were read with and are to be written through, the
    /// lambda, the feature, and the turn - to be disposed of once the change
    /// is written
    /// </returns>
    private (LambdaDbContext Database, LambdaEntity Lambda, FeatureEntity Feature, IDisposable Turn) Locked(string privateKey, string feature)
    {
        // refuses a demo, whose key is announced and which nobody may change
        var lambdaId = meta.RequireEditable(privateKey);

        var database = databases.CreateDbContext();

        try
        {
            var id = RequireFeature(database, lambdaId, feature).Id;

            // looked up again once it is its turn, so what the change decides
            // on is what the change before it left behind
            database.ChangeTracker.Clear();

            var turn = new Turn(_stripes[(int)((ulong)id % (ulong)_stripes.Length)]);

            try
            {
                var entity = database.Features.FirstOrDefault(f => f.Id == id)
                          ?? throw LambdaException.NotFound("This feature does not exist (or has been merged or deleted).");

                var lambda = database.Lambdas.First(l => l.Id == lambdaId);

                return (database, lambda, entity, turn);
            }
            catch
            {
                turn.Dispose();
                throw;
            }
        }
        catch
        {
            database.Dispose();
            throw;
        }
    }

    /// <summary>
    /// A stripe taken, given back once.
    /// </summary>
    private sealed class Turn : IDisposable
    {
        private readonly Lock _stripe;

        private int _released;

        public Turn(Lock stripe)
        {
            _stripe = stripe;
            _stripe.Enter();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                _stripe.Exit();
            }
        }
    }

    private static LambdaEntity RequireLambda(LambdaDbContext database, string privateKey)
        => database.Lambdas.AsNoTracking().FirstOrDefault(l => l.PrivateKey == privateKey)
        ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

    private static FeatureEntity RequireFeature(LambdaDbContext database, long lambdaId, string feature)
    {
        var key = feature.Trim().ToLowerInvariant();

        return database.Features.AsNoTracking().FirstOrDefault(f => f.LambdaId == lambdaId && f.Key == key)
            ?? throw LambdaException.NotFound($"This lambda has no feature '{feature}' (it may have been merged or deleted). read_lambda lists the ones it has.");
    }

    private static int? Newest(LambdaDbContext database, long lambdaId)
        => database.Deployments.Where(d => d.LambdaId == lambdaId).Max(d => (int?)d.Version);

    private static bool WorkspaceEnabled(LambdaDbContext database, long lambdaId)
        => DataSwitches.IsEnabled(database, lambdaId, DataKinds.Workspace);

    #endregion

}
