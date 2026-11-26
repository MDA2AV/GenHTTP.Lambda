using System.Collections.Concurrent;
using System.Security.Cryptography;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Agents;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Connectivity;
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
                                   IDeploymentService deployments, ISecretVault secrets, IDatabaseVault stores, IWorkspaceVault workspaces, LambdaOptions options, ILimitsService limits, ILogBook book,
                                   DatabaseChanges changes, IAgentJournal journal, ILogger<FeatureService> logger)
    : IFeatureService
{

    /// <summary>
    /// The previews being served, by their key, see <see cref="ResolutionCache{TKey}"/>.
    /// </summary>
    private readonly ResolutionCache<string> _previews = new(changes);

    private Func<string, ValueTask<ResolvedLambda?>>? _previewLoader;

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
    /// Held while the change is read and written, across the awaits of the
    /// records, but not across what compiles or copies: that is done first,
    /// and the turn is taken to write down what it came to (see
    /// <see cref="DeployAsync"/> and <see cref="MergeAsync"/>). A merge takes
    /// the lambda's turn inside the feature's, never the other way round.
    /// </remarks>
    private readonly Turns _turns = new();

    /// <summary>
    /// Takes turns over copying the data of a feature, one copy at a time.
    /// </summary>
    /// <remarks>
    /// Apart from the turn to change, which a save would otherwise wait for
    /// across the copy - and that can be gigabytes; two copies into the same
    /// feature would write over each other's staging.
    /// </remarks>
    private readonly SemaphoreSlim[] _copying = [.. Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1))];

    /// <summary>
    /// The builds of previews handed out, so two deployments of one preview
    /// under way at once are never given the same number.
    /// </summary>
    private readonly ConcurrentDictionary<long, int> _stamps = [];

    #region Reading

    public async ValueTask<IReadOnlyList<FeatureInfo>> ListAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireLambdaAsync(database, privateKey, cancellation);

        var newest = await NewestAsync(database, lambda.Id, cancellation);

        var features = await database.Features.AsNoTracking()
                                     .Where(f => f.LambdaId == lambda.Id)
                                     .OrderByDescending(f => f.Modified)
                                     .ToListAsync(cancellation);

        return [.. features.Select(f => Describe(f, newest))];
    }

    public async ValueTask<FeatureContent> GetAsync(string privateKey, string feature, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireLambdaAsync(database, privateKey, cancellation);

        var entity = await RequireFeatureAsync(database, lambda.Id, feature, cancellation);

        var files = storage.ReadFeature(lambda.Id, entity.Id)
                 ?? throw LambdaException.NotFound($"The files of the feature '{entity.Name}' are no longer available.");

        return new FeatureContent(Describe(entity, await NewestAsync(database, lambda.Id, cancellation)), files);
    }

    public async ValueTask<(long LambdaId, long FeatureId)> RequireAsync(string privateKey, string feature, bool editable, CancellationToken cancellation = default)
    {
        var lambdaId = editable
            ? await meta.RequireEditableAsync(privateKey, cancellation)
            : await meta.GetIdAsync(privateKey, cancellation) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var entity = await RequireFeatureAsync(database, lambdaId, feature, cancellation);

        return (lambdaId, entity.Id);
    }

    public async ValueTask<string?> GetNameAsync(string privateKey, string feature, CancellationToken cancellation = default)
    {
        var key = feature.Trim().ToLowerInvariant();

        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.Features.AsNoTracking()
                             .Where(f => f.Key == key && f.Lambda!.PrivateKey == privateKey)
                             .Select(f => f.Name)
                             .FirstOrDefaultAsync(cancellation);
    }

    public ValueTask<ResolvedLambda?> ResolvePreviewAsync(string key)
    {
        // anything that is not one of our keys is answered without asking
        // the database or taking room in the cache, since every request to
        // /features/ is asked about
        if (!IsKey(key))
        {
            return ValueTask.FromResult<ResolvedLambda?>(null);
        }

        return _previews.ResolveAsync(key, PreviewLoader);
    }

    /// <summary>
    /// What the previews are loaded with, made once rather than on every
    /// request to one.
    /// </summary>
    private Func<string, ValueTask<ResolvedLambda?>> PreviewLoader => _previewLoader ??= LoadPreviewAsync;

    /// <summary>
    /// Reads the preview of a feature, like the lambdas it stands beside
    /// (see <see cref="MetaService.ResolveAsync(string)"/>).
    /// </summary>
    private async ValueTask<ResolvedLambda?> LoadPreviewAsync(string key)
    {
        await using var database = await databases.CreateDbContextAsync();

        var found = await database.Features.AsNoTracking()
                                  .Where(f => f.Key == key && f.Previewed != null)
                                  .Join(database.Lambdas, f => f.LambdaId, l => l.Id, (f, l) => new { Feature = f, Lambda = l })
                                  .FirstOrDefaultAsync();

        // blocking takes the previews offline; this is for one that was put
        // online at the very moment the lambda was blocked
        if (found == null || found.Lambda.Blocked != null)
        {
            return null;
        }

        var workspace = await DataSwitches.IsEnabledAsync(database, found.Lambda.Id, DataKinds.Workspace);

        // built as the lambda is, so a preview may connect where the lambda
        // may - and is not built again on its first request for being told
        // otherwise than its deployment was
        return new ResolvedLambda(found.Lambda.Id, found.Lambda.PublicKey, found.Lambda.Tier, found.Feature.BaseVersion,
                                  found.Feature.Previewed!.Value, workspace,
                                  new ResolvedFeature(found.Feature.Id, found.Feature.Key, found.Feature.Preview),
                                  Connectivity: ConnectivityRule.Allows(limits.PolicyOf(found.Lambda.Tier), found.Lambda.Connectivity));
    }

    #endregion

    #region Working on it

    public async ValueTask<FeatureInfo> CreateAsync(string privateKey, FeatureDraft draft, CancellationToken cancellation = default)
    {
        var lambdaId = await meta.RequireEditableAsync(privateKey, cancellation);

        var name = Name(draft.Name);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.FirstAsync(l => l.Id == lambdaId, cancellation);

        var allowed = limits.Get().Features;

        if (await database.Features.CountAsync(f => f.LambdaId == lambdaId, cancellation) >= allowed)
        {
            throw LambdaException.Conflict($"A lambda may have {allowed} features open at once. Merge or delete one first.");
        }

        var newest = await NewestAsync(database, lambdaId, cancellation)
                  ?? throw LambdaException.Invalid("There is no version yet to start a feature from. Save one first.");

        var from = draft.Base ?? newest;

        var files = storage.Read(lambdaId, from);

        if (files == null || !await database.Deployments.AnyAsync(d => d.LambdaId == lambdaId && d.Version == from, cancellation))
        {
            throw LambdaException.NotFound($"Version {from} does not exist. The newest is version {newest}.");
        }

        var now = DateTime.UtcNow;

        var taken = await database.Features.Where(f => f.LambdaId == lambdaId).Select(f => f.Branch).ToListAsync(cancellation);

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

        // being worked on is being used: a sandbox lambda somebody is changing
        // is not left to expire while they do
        lambda.Modified = now;

        try
        {
            await database.SaveChangesAsync(cancellation);
        }
        catch (DbUpdateException) when (draft.Branch == null)
        {
            // another feature took the same branch in the meantime: the next free one
            entity.Branch = FeatureBranches.For(name, await database.Features.AsNoTracking().Where(f => f.LambdaId == lambdaId).Select(f => f.Branch).ToListAsync(cancellation));

            await database.SaveChangesAsync(cancellation);
        }
        catch (DbUpdateException)
        {
            throw LambdaException.Conflict($"Another feature of this lambda is at the branch '{draft.Branch}'.");
        }

        try
        {
            // a manifest of what the version holds: nothing of it is copied
            await storage.WriteFeatureAsync(lambdaId, entity.Id, files);

            await CopyDataAsync(lambdaId, entity.Id, await WorkspaceEnabledAsync(database, lambdaId, cancellation), cancellation);
        }
        catch
        {
            // what was begun is taken back whoever cancelled it
            await RemoveDataAsync(lambdaId, entity.Id);

            storage.DeleteFeature(lambdaId, entity.Id);

            database.Features.Remove(entity);

            await database.SaveChangesAsync(CancellationToken.None);

            throw;
        }

        logger.LogInformation("Created feature #{FeatureId} of lambda #{LambdaId} base {Version}", entity.Id, lambdaId, from);

        await journal.RecordAsync(draft.Author, new AgentEffect(AgentEffectKind.Started, lambdaId, Version: from, Feature: entity.Key, Name: entity.Name), CancellationToken.None);

        return Describe(entity, newest);
    }

    public async ValueTask<FeatureInfo> UpdateAsync(string privateKey, string feature, FeatureUpdate update, CancellationToken cancellation = default)
    {
        var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

        await using var context = database;

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

        var newest = await NewestAsync(database, lambda.Id, cancellation);

        if (update.Base is { } moved && moved != entity.BaseVersion)
        {
            if (!await database.Deployments.AnyAsync(d => d.LambdaId == lambda.Id && d.Version == moved, cancellation))
            {
                throw LambdaException.NotFound($"Version {moved} does not exist. The newest is version {newest}.");
            }

            logger.LogInformation("Moved base of feature #{FeatureId} of lambda #{LambdaId} from {From} to {To}",
                                  entity.Id, lambda.Id, entity.BaseVersion, moved);

            entity.BaseVersion = moved;
        }

        entity.Modified = DateTime.UtcNow;

        await database.SaveChangesAsync(cancellation);

        return Describe(entity, newest);
    }

    /// <remarks>
    /// What the files hold that the lambda does not keep yet is kept before
    /// the feature's turn is taken, so a save of the same feature does not
    /// wait for it (see <see cref="_turns"/>); in the turn, the feature's
    /// manifest is written.
    /// </remarks>
    public async ValueTask<FeatureInfo> SaveAsync(string privateKey, string feature, IReadOnlyList<VersionFile> files, VersionNote? note = null, int? after = null,
                                                  CancellationToken cancellation = default)
    {
        VersionInput.Validate(files);

        // refuses a demo, whose key is announced and which nobody may change,
        // and a feature that is not there - before anything is kept for it
        var (lambdaId, _) = await RequireAsync(privateKey, feature, true, cancellation);

        // what a feature holds becomes a version, so it is held to what a
        // version may hold from the start rather than refused when merged -
        // and before anything is kept for it
        if (await meta.GetTierAsync(lambdaId, cancellation) is { } tier)
        {
            VersionInput.ValidateAllowance(files, tier, limits);
        }

        var kept = await storage.KeepAsync(lambdaId, files, cancellation);

        var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

        await using var context = database;

        using var held = turn;

        // a feature keeps no history: a save made from what somebody else has
        // since replaced would replace their work without anybody knowing
        if (after is { } read && read != entity.Revision)
        {
            throw LambdaException.Conflict($"The feature '{entity.Name}' was saved again since it was read (by the agent, perhaps, or in another window). "
                                         + "Read it again and make the change on top of what it holds now.");
        }

        // moved to another tier meanwhile, perhaps
        VersionInput.ValidateAllowance(kept, lambda.Tier, limits);

        await storage.WriteFeatureAsync(lambda.Id, entity.Id, kept);

        entity.Specification = VersionInput.Tidy(note?.Specification, VersionNote.MaxSpecification) ?? entity.Specification;
        entity.Change = VersionInput.Tidy(note?.Change, VersionNote.MaxChange) ?? entity.Change;

        // who wrote what it holds now - every door saves through here, so a
        // save the agent did not make clears what it had written, and what
        // was stored as model and effort before goes with it
        entity.Writer = note?.Author?.Writer;
        entity.AgentRun = note?.Author?.Run;
        entity.AgentOptions = note?.Author is { } author ? StoredOptions.Write(author.Options) : null;
        entity.AgentModel = null;
        entity.AgentEffort = null;

        var now = DateTime.UtcNow;

        entity.Revision++;
        entity.Modified = now;
        lambda.Modified = now;

        // the files are written, so what says which revision they are is too
        await database.SaveChangesAsync(CancellationToken.None);

        await journal.RecordAsync(note?.Author, new AgentEffect(AgentEffectKind.Drafted, lambda.Id, Feature: entity.Key, Name: entity.Name), CancellationToken.None);

        return Describe(entity, await NewestAsync(database, lambda.Id, CancellationToken.None));
    }

    /// <remarks>
    /// Compiled outside the feature's turn, which a save would otherwise wait
    /// for across the seconds that takes (see <see cref="_turns"/>): the build
    /// is numbered in the turn, compiled, and written down in the turn again -
    /// unless a deployment started later finished first, which is then what
    /// stays online.
    /// </remarks>
    public async ValueTask<FeatureDeployment> DeployAsync(string privateKey, string feature, CancellationToken cancellation = default)
    {
        long lambdaId, featureId;

        string publicKey;

        IReadOnlyList<VersionFile> files;

        int stamp, revision;

        BuildSettings settings;

        {
            var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

            await using var context = database;

            using var held = turn;

            files = storage.ReadFeature(lambda.Id, entity.Id)
                 ?? throw LambdaException.NotFound($"The files of the feature '{entity.Name}' are no longer available.");

            // built as the lambda is, so a preview may connect where the lambda may
            settings = new BuildSettings(await WorkspaceEnabledAsync(database, lambda.Id, cancellation),
                                         ConnectivityRule.Allows(limits.PolicyOf(lambda.Tier), lambda.Connectivity));

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
            outcome = await deployments.PreviewAsync(lambdaId, featureId, stamp, files, settings, cancellation);
        }

        try
        {
            return await PreviewedAsync(privateKey, feature, stamp, revision, files, outcome);
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
    /// <remarks>
    /// Without a cancellation token: what was built is written down once it
    /// was, or the next deployment would take its stamp for the build already
    /// there.
    /// </remarks>
    private async ValueTask<FeatureDeployment> PreviewedAsync(string privateKey, string feature, int stamp, int revision, IReadOnlyList<VersionFile> files,
                                                              CompilationOutcome outcome)
    {
        var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature);

        await using var context = database;

        using var held = turn;

        if (outcome.Success && stamp > entity.Preview)
        {
            try
            {
                // what the preview serves from now on, until it is deployed again -
                // a restart included - however much the feature changes meanwhile:
                // a manifest of what the feature holds, nothing copied
                await storage.WritePreviewAsync(lambda.Id, entity.Id, files);

                var now = DateTime.UtcNow;

                entity.Preview = stamp;
                entity.PreviewOf = revision;
                entity.Previewed = now;
                entity.Modified = now;
                lambda.Modified = now;

                await database.SaveChangesAsync();
            }
            catch
            {
                // built, but not recorded: the next deployment would take the
                // same stamp for the build already there, and serve it
                deployments.EvictPreview(lambda.Id, entity.Id);
                throw;
            }
        }

        return new FeatureDeployment(outcome.Success, Describe(entity, await NewestAsync(database, lambda.Id)), outcome.Diagnostics);
    }

    public async ValueTask<FeatureInfo> UndeployAsync(string privateKey, string feature, CancellationToken cancellation = default)
    {
        var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

        await using var context = database;

        using var held = turn;

        if (entity.Previewed != null)
        {
            entity.Previewed = null;
            entity.Modified = DateTime.UtcNow;

            await database.SaveChangesAsync(cancellation);

            deployments.EvictPreview(lambda.Id, entity.Id);
        }

        return Describe(entity, await NewestAsync(database, lambda.Id, CancellationToken.None));
    }

    public async ValueTask<FeatureInfo> RefreshDataAsync(string privateKey, string feature, CancellationToken cancellation = default)
    {
        long lambdaId, featureId;

        bool workspace;

        {
            var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

            await using var context = database;

            using var held = turn;

            (lambdaId, featureId, workspace) = (lambda.Id, entity.Id, await WorkspaceEnabledAsync(database, lambda.Id, cancellation));
        }

        await CopyDataAsync(lambdaId, featureId, workspace, cancellation);

        {
            // the copy is made: what follows is seen through whoever cancels
            var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature);

            await using var context = database;

            using var held = turn;

            // built again on its next request, so code that read the data into
            // memory when it started reads the fresh copy rather than writing the
            // old one back over it
            deployments.EvictPreview(lambda.Id, entity.Id);

            entity.Modified = DateTime.UtcNow;

            await database.SaveChangesAsync();

            return Describe(entity, await NewestAsync(database, lambda.Id));
        }
    }

    /// <summary>
    /// Copies the data of the lambda into the feature as it is now, so trying
    /// the feature cannot touch what the lambda's visitors keep.
    /// </summary>
    /// <summary>
    /// Removes a feature's copy of the data - its secrets, its database, its
    /// workspace - wherever the integrations keep them, letting go of the
    /// connections to its database before its folder goes.
    /// </summary>
    private async ValueTask RemoveDataAsync(long lambdaId, long featureId)
    {
        await secrets.RemoveCopyAsync(lambdaId, featureId);

        await stores.RemoveCopyAsync(lambdaId, featureId);

        await workspaces.RemoveCopyAsync(lambdaId, featureId);
    }

    /// <remarks>
    /// The workspace and the database can be gigabytes, so they are copied
    /// away from the reactor and in the feature's turn to be copied (see
    /// <see cref="_copying"/>) - not in its turn to change, which a save
    /// would wait for meanwhile.
    /// </remarks>
    private async ValueTask CopyDataAsync(long lambdaId, long featureId, bool workspace, CancellationToken cancellation)
    {
        var copying = _copying[(int)((ulong)featureId % (ulong)_copying.Length)];

        await copying.WaitAsync(cancellation);

        try
        {
            if (workspace)
            {
                await workspaces.CopyAsync(lambdaId, featureId, cancellation);
            }

            // and the secrets, so the preview can call what the lambda calls
            await secrets.CopyAsync(lambdaId, featureId, cancellation);

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
    /// Checked outside the feature's turn, which is not held across the
    /// compiler (see <see cref="_turns"/>), and merged in it once the check
    /// passed - refused if the feature was saved meanwhile, since what was
    /// checked would then not be what is merged.
    /// </remarks>
    public async ValueTask<FeatureMerge> MergeAsync(string privateKey, string feature, VersionNote? note = null, bool deploy = false,
                                                    CancellationToken cancellation = default)
    {
        IReadOnlyList<VersionFile> files;

        string name;

        int revision;

        {
            var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

            await using var context = database;

            using var held = turn;

            var newest = await NewestAsync(database, lambda.Id, cancellation);

            if (newest != entity.BaseVersion)
            {
                throw LambdaException.Conflict(Behind(entity, newest));
            }

            files = storage.ReadFeature(lambda.Id, entity.Id)
                 ?? throw LambdaException.NotFound($"The files of the feature '{entity.Name}' are no longer available.");

            // told by what the manifests name, without reading what they hold
            if (storage.Read(lambda.Id, entity.BaseVersion) is { } based && LambdaSource.Same(files, based))
            {
                throw LambdaException.Invalid($"The feature '{entity.Name}' holds exactly what version {entity.BaseVersion} holds, so there is nothing to merge. Change it first, or delete it.");
            }

            name = entity.Name;

            revision = entity.Revision;
        }

        // a version is only made of what compiles: the feature is kept to be
        // fixed, rather than turned into a version nobody can put online
        var check = await meta.CheckAsync(privateKey, files, cancellation);

        if (!check.Success)
        {
            return new FeatureMerge(name, false, null, check.Diagnostics, null);
        }

        var (version, lambdaId, featureId, origin) = await MergedAsync(privateKey, feature, note, files, revision);

        // the version is there from here on: what follows is seen through
        // however the caller fares, or a feature would stay behind that was merged
        await RemoveAsync(lambdaId, featureId);

        // for whoever merged it, which need not be whoever wrote it
        await journal.RecordAsync(note?.Author, new AgentEffect(AgentEffectKind.Merged, lambdaId, version.Version, Feature: feature, Name: name), CancellationToken.None);

        DeploymentResult? deployment = null;

        if (deploy)
        {
            try
            {
                deployment = await meta.DeployAsync(privateKey, version.Version, origin, note?.Author, CancellationToken.None);
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
    /// Makes the feature the next version, in its turn - its data is
    /// removed once the turn is over.
    /// </summary>
    /// <remarks>
    /// Without a cancellation token: a version made of the feature and a
    /// feature left behind would be merged twice.
    /// </remarks>
    private async ValueTask<(LambdaVersionInfo Version, long LambdaId, long FeatureId, VersionOrigin Origin)> MergedAsync(string privateKey, string feature, VersionNote? note,
                                                                                                                  IReadOnlyList<VersionFile> files, int revision)
    {
        var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature);

        await using var context = database;

        using var held = turn;

        if (entity.Revision != revision)
        {
            throw LambdaException.Conflict($"The feature '{entity.Name}' was saved while it was being checked, so what was checked is not what it holds now. Merge it again.");
        }

        var origin = note?.Origin ?? VersionOrigin.Api;

        // written by whoever saved into the feature last, whoever merges it:
        // the owner putting the build agent's draft online makes the agent's
        // work a version
        var merged = new VersionNote(string.IsNullOrWhiteSpace(note?.Specification) ? entity.Specification : note.Specification,
                                     string.IsNullOrWhiteSpace(note?.Change) ? entity.Change ?? $"Merges the feature '{entity.Name}'" : note.Change,
                                     origin, AuthorOf(entity), Merged: true);

        // refused if a version was saved since the feature was based on its
        // newest, by the same turn every save of the lambda takes - the
        // feature's manifest made the version's, its content kept already
        var version = await meta.SaveAsync(privateKey, files, merged, after: entity.BaseVersion);

        await ForgetAsync(database, lambda.Id, entity);

        logger.LogInformation("Merged feature #{FeatureId} of lambda #{LambdaId} as version {Version}", entity.Id, lambda.Id, version.Version);

        return (version, lambda.Id, entity.Id, origin);
    }

    public async ValueTask<string> DeleteAsync(string privateKey, string feature, VersionAuthor? author = null, CancellationToken cancellation = default)
    {
        var (lambdaId, featureId, name) = await DeletedAsync(privateKey, feature, cancellation);

        // seen through however the caller fares, once the feature is gone
        await RemoveAsync(lambdaId, featureId);

        logger.LogInformation("Deleted feature #{FeatureId} of lambda #{LambdaId}", featureId, lambdaId);

        await journal.RecordAsync(author, new AgentEffect(AgentEffectKind.Discarded, lambdaId, Feature: feature, Name: name), CancellationToken.None);

        return name;
    }

    /// <summary>
    /// Deletes the feature, in its turn - its data is removed once the turn
    /// is over.
    /// </summary>
    private async ValueTask<(long LambdaId, long FeatureId, string Name)> DeletedAsync(string privateKey, string feature, CancellationToken cancellation)
    {
        var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

        await using var context = database;

        using var held = turn;

        await ForgetAsync(database, lambda.Id, entity);

        return (lambda.Id, entity.Id, entity.Name);
    }

    public async ValueTask<int> RunMaintenanceAsync(DateTime now, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var quiet = now - limits.Get().OfflineAfter;

        // the same rule the lambda's own deployment lives by: online while
        // somebody works on it, offline once nobody has for the whole window
        // - in a tier that is not kept while unused, and never counted from
        // before the lambda moved into it
        var expiring = limits.TiersWhere(p => p.ExpiresWhenUnused);

        var stale = await database.Features.AsNoTracking()
                                  .Where(f => f.Previewed != null && f.Modified < quiet && expiring.Contains(f.Lambda!.Tier)
                                              && (f.Lambda.TierChanged == null || f.Lambda.TierChanged < quiet))
                                  .Select(f => new { f.Id, f.LambdaId })
                                  .ToListAsync(cancellation);

        if (stale.Count == 0)
        {
            return 0;
        }

        var ids = stale.Select(f => f.Id).ToList();

        // asked again as it is written, so a preview deployed a moment ago -
        // which made it no longer stale - is left online
        var taken = await database.Features
                                  .Where(f => ids.Contains(f.Id) && f.Previewed != null && f.Modified < quiet)
                                  .ExecuteUpdateAsync(u => u.SetProperty(f => f.Previewed, (DateTime?)null), cancellation);

        // offline in the database first, so no request builds one again - and
        // let go of whoever cancels, now that it is
        foreach (var feature in stale)
        {
            if (!await database.Features.AnyAsync(f => f.Id == feature.Id && f.Previewed != null, CancellationToken.None))
            {
                deployments.EvictPreview(feature.LambdaId, feature.Id);
            }
        }

        logger.LogInformation("Undeployed {Count} feature(s) by maintenance", taken);

        return taken;
    }

    public async ValueTask<int> SweepAsync(CancellationToken cancellation = default)
    {
        HashSet<long> known;

        // a row is written before its folder and removed before it, so a
        // folder without a row is one left behind - by a request that was
        // still writing to the copy of the data when its feature went
        await using (var database = await databases.CreateDbContextAsync(cancellation))
        {
            known = await database.Features.AsNoTracking().Select(f => f.Id).ToHashSetAsync(cancellation);
        }

        var swept = 0;

        foreach (var (lambdaId, featureId) in storage.ListFeatures())
        {
            if (!known.Contains(featureId))
            {
                deployments.EvictPreview(lambdaId, featureId);

                await RemoveDataAsync(lambdaId, featureId);

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

    /// <summary>
    /// Who wrote what a feature holds, where the platform knows it.
    /// </summary>
    private static VersionAuthor? AuthorOf(FeatureEntity feature)
        => feature.Writer is { } writer ? new VersionAuthor(writer, StoredOptions.Of(feature.AgentOptions, feature.AgentModel, feature.AgentEffort), feature.AgentRun) : null;

    private static FeatureInfo Describe(FeatureEntity feature, int? newest)
    {
        var options = feature.Writer != null ? StoredOptions.Of(feature.AgentOptions, feature.AgentModel, feature.AgentEffort) : null;

        return new(feature.Key, feature.Name, feature.Branch, feature.Specification, feature.Change, feature.BaseVersion, newest, VersionOriginNames.Name(feature.Origin),
                   feature.Created, feature.Modified, feature.Previewed != null, feature.Previewed != null && feature.PreviewOf == feature.Revision,
                   feature.Previewed, feature.Revision, VersionWriterNames.Name(feature.Writer), options?.GetValueOrDefault(AgentModelNames.ModelOption),
                   options?.GetValueOrDefault(AgentModelNames.EffortOption), options is { Count: > 0 } ? options : null);
    }

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

    /// <summary>
    /// Removes the row of a feature, in its turn, so no request finds its
    /// preview from then on.
    /// </summary>
    private async ValueTask ForgetAsync(LambdaDbContext database, long lambdaId, FeatureEntity feature)
    {
        database.Features.Remove(feature);

        await database.SaveChangesAsync();

        deployments.EvictPreview(lambdaId, feature.Id);
    }

    /// <summary>
    /// Removes what a feature whose row is gone kept: its copy of the data,
    /// wherever the integrations keep it, and its files.
    /// </summary>
    /// <remarks>
    /// After its turn, which a save would otherwise wait for across what the
    /// integrations take to remove. A copy of its data that its turn left behind -
    /// one being written by a request that found the preview a moment
    /// before - is what the sweep removes. The handler is let go twice,
    /// because such a request may still be building it, and would otherwise
    /// leave a handler behind for a feature that no longer exists.
    /// </remarks>
    private async ValueTask RemoveAsync(long lambdaId, long featureId)
    {
        await RemoveDataAsync(lambdaId, featureId);

        storage.DeleteFeature(lambdaId, featureId);

        deployments.EvictPreview(lambdaId, featureId);
    }

    /// <summary>
    /// The lambda and the feature, read once it is the feature's turn to change.
    /// </summary>
    /// <param name="cancellation">Stops it before the turn is taken; the turn itself is waited for whoever cancels</param>
    /// <returns>
    /// The context both were read with and are to be written through, the
    /// lambda, the feature, and the turn - to be disposed of once the change
    /// is written
    /// </returns>
    private async ValueTask<(LambdaDbContext Database, LambdaEntity Lambda, FeatureEntity Feature, IDisposable Turn)> LockedAsync(string privateKey, string feature,
                                                                                                                                 CancellationToken cancellation = default)
    {
        // refuses a demo, whose key is announced and which nobody may change
        var lambdaId = await meta.RequireEditableAsync(privateKey, cancellation);

        var database = await databases.CreateDbContextAsync(cancellation);

        try
        {
            var id = (await RequireFeatureAsync(database, lambdaId, feature, cancellation)).Id;

            // looked up again once it is its turn, so what the change decides
            // on is what the change before it left behind
            database.ChangeTracker.Clear();

            var turn = await _turns.TakeAsync(id);

            try
            {
                var entity = await database.Features.FirstOrDefaultAsync(f => f.Id == id, cancellation)
                          ?? throw LambdaException.NotFound("This feature does not exist (or has been merged or deleted).");

                var lambda = await database.Lambdas.FirstAsync(l => l.Id == lambdaId, cancellation);

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
            await database.DisposeAsync();
            throw;
        }
    }

    private static async ValueTask<LambdaEntity> RequireLambdaAsync(LambdaDbContext database, string privateKey, CancellationToken cancellation)
        => await database.Lambdas.AsNoTracking().FirstOrDefaultAsync(l => l.PrivateKey == privateKey, cancellation)
        ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

    private static async ValueTask<FeatureEntity> RequireFeatureAsync(LambdaDbContext database, long lambdaId, string feature, CancellationToken cancellation)
    {
        var key = feature.Trim().ToLowerInvariant();

        return await database.Features.AsNoTracking().FirstOrDefaultAsync(f => f.LambdaId == lambdaId && f.Key == key, cancellation)
            ?? throw LambdaException.NotFound($"This lambda has no feature '{feature}' (it may have been merged or deleted). read_lambda lists the ones it has.");
    }

    private static async ValueTask<int?> NewestAsync(LambdaDbContext database, long lambdaId, CancellationToken cancellation = default)
        => await database.Deployments.Where(d => d.LambdaId == lambdaId).MaxAsync(d => (int?)d.Version, cancellation);

    private static ValueTask<bool> WorkspaceEnabledAsync(LambdaDbContext database, long lambdaId, CancellationToken cancellation)
        => DataSwitches.IsEnabledAsync(database, lambdaId, DataKinds.Workspace, cancellation);

    #endregion

}
