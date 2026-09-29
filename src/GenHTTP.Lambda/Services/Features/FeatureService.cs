using System.Security.Cryptography;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Secrets;
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
                                   IDeploymentService deployments, ISecretVault secrets, LambdaOptions options, LogBook book,
                                   ILogger<FeatureService> logger)
    : IFeatureService
{

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
    /// </remarks>
    private readonly SemaphoreSlim[] _stripes = [.. Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1))];

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

        var code = await storage.ReadFeatureAsync(lambda.Id, entity.Id, cancellation)
                ?? throw LambdaException.NotFound($"The files of the feature '{entity.Name}' are no longer available.");

        return new FeatureContent(Describe(entity, await NewestAsync(database, lambda.Id, cancellation)), code);
    }

    public async ValueTask<(long LambdaId, long FeatureId)> RequireAsync(string privateKey, string feature, bool editable,
                                                                         CancellationToken cancellation = default)
    {
        var lambdaId = editable
            ? await meta.RequireEditableAsync(privateKey, cancellation)
            : await meta.GetIdAsync(privateKey, cancellation) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var entity = await RequireFeatureAsync(database, lambdaId, feature, cancellation);

        return (lambdaId, entity.Id);
    }

    public async ValueTask<ResolvedLambda?> ResolvePreviewAsync(string key, CancellationToken cancellation = default)
    {
        // anything that is not one of our keys is answered without asking
        // the database, since every request to /features/ is asked about
        if (!IsKey(key))
        {
            return null;
        }

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var found = await database.Features.AsNoTracking()
                                  .Where(f => f.Key == key && f.Previewed != null)
                                  .Join(database.Lambdas, f => f.LambdaId, l => l.Id, (f, l) => new { Feature = f, Lambda = l })
                                  .FirstOrDefaultAsync(cancellation);

        if (found == null)
        {
            return null;
        }

        var workspace = await DataSwitches.IsEnabledAsync(database, found.Lambda.Id, DataKinds.Workspace, cancellation);

        return new ResolvedLambda(found.Lambda.Id, found.Lambda.PublicKey, found.Lambda.Tier, found.Feature.BaseVersion,
                                  found.Feature.Previewed!.Value, workspace,
                                  new ResolvedFeature(found.Feature.Id, found.Feature.Key, found.Feature.Preview));
    }

    #endregion

    #region Working on it

    public async ValueTask<FeatureInfo> CreateAsync(string privateKey, FeatureDraft draft, CancellationToken cancellation = default)
    {
        var lambdaId = await meta.RequireEditableAsync(privateKey, cancellation);

        var name = Name(draft.Name);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.FirstAsync(l => l.Id == lambdaId, cancellation);

        if (await database.Features.CountAsync(f => f.LambdaId == lambdaId, cancellation) >= options.MaxFeatures)
        {
            throw LambdaException.Conflict($"A lambda may have {options.MaxFeatures} features open at once. Merge or delete one first.");
        }

        var newest = await NewestAsync(database, lambdaId, cancellation)
                  ?? throw LambdaException.Invalid("There is no version yet to start a feature from. Save one first.");

        var from = draft.Base ?? newest;

        var code = await storage.ReadAsync(lambdaId, from, cancellation);

        if (code == null || !await database.Deployments.AnyAsync(d => d.LambdaId == lambdaId && d.Version == from, cancellation))
        {
            throw LambdaException.NotFound($"Version {from} does not exist. The newest is version {newest}.");
        }

        var now = DateTime.UtcNow;

        var entity = new FeatureEntity
        {
            LambdaId = lambdaId,
            Key = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16)),
            Name = name,
            Specification = MetaService.Tidy(draft.Specification, VersionNote.MaxSpecification),
            BaseVersion = from,
            Origin = draft.Origin,
            Created = now,
            Modified = now
        };

        database.Features.Add(entity);

        // being worked on is being used: a free lambda somebody is changing
        // is not left to expire while they do
        lambda.Modified = now;

        await database.SaveChangesAsync(cancellation);

        try
        {
            await storage.WriteFeatureAsync(lambdaId, entity.Id, code, cancellation);

            // a copy of the data as it is now, so trying the feature cannot
            // touch what the lambda's visitors keep there
            if (await WorkspaceEnabledAsync(database, lambdaId, cancellation))
            {
                await storage.CopyWorkspaceAsync(lambdaId, entity.Id, cancellation);
            }

            // the secrets too: a preview that calls a service the lambda has a key
            // for should be able to, and it can do what it likes with its copy
            if (await DataSwitches.IsEnabledAsync(database, lambdaId, DataKinds.Secrets, cancellation))
            {
                await secrets.CopyAsync(lambdaId, entity.Id, cancellation);
            }
        }
        catch
        {
            await storage.DeleteFeatureAsync(lambdaId, entity.Id, CancellationToken.None);

            database.Features.Remove(entity);

            await database.SaveChangesAsync(CancellationToken.None);

            throw;
        }

        logger.LogInformation("Lambda {LambdaId} started feature {FeatureId} from version {Version}", lambdaId, entity.Id, from);

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
            entity.Specification = MetaService.Tidy(update.Specification, VersionNote.MaxSpecification);
        }

        if (update.Change != null)
        {
            entity.Change = MetaService.Tidy(update.Change, VersionNote.MaxChange);
        }

        var newest = await NewestAsync(database, lambda.Id, cancellation);

        if (update.Base is { } moved && moved != entity.BaseVersion)
        {
            if (!await database.Deployments.AnyAsync(d => d.LambdaId == lambda.Id && d.Version == moved, cancellation))
            {
                throw LambdaException.NotFound($"Version {moved} does not exist. The newest is version {newest}.");
            }

            logger.LogInformation("Feature {FeatureId} of lambda {LambdaId} moved its base from version {From} to {To}",
                                  entity.Id, lambda.Id, entity.BaseVersion, moved);

            entity.BaseVersion = moved;
        }

        entity.Modified = DateTime.UtcNow;

        await database.SaveChangesAsync(cancellation);

        return Describe(entity, newest);
    }

    public async ValueTask<FeatureInfo> SaveAsync(string privateKey, string feature, string code, VersionNote? note = null, int? after = null,
                                                  CancellationToken cancellation = default)
    {
        var files = MetaService.Validate(code);

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

        // what a feature holds becomes a version, so it is held to what a
        // version may hold from the start rather than refused when merged
        MetaService.ValidateAllowance(files, lambda.Tier, options);

        await storage.WriteFeatureAsync(lambda.Id, entity.Id, code, cancellation);

        entity.Specification = MetaService.Tidy(note?.Specification, VersionNote.MaxSpecification) ?? entity.Specification;
        entity.Change = MetaService.Tidy(note?.Change, VersionNote.MaxChange) ?? entity.Change;

        var now = DateTime.UtcNow;

        entity.Revision++;
        entity.Modified = now;
        lambda.Modified = now;

        await database.SaveChangesAsync(cancellation);

        return Describe(entity, await NewestAsync(database, lambda.Id, cancellation));
    }

    public async ValueTask<FeatureDeployment> DeployAsync(string privateKey, string feature, CancellationToken cancellation = default)
    {
        var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

        await using var context = database;

        using var held = turn;

        var code = await storage.ReadFeatureAsync(lambda.Id, entity.Id, cancellation)
                ?? throw LambdaException.NotFound($"The files of the feature '{entity.Name}' are no longer available.");

        var limits = options.WorkspaceOf(lambda.Tier, await WorkspaceEnabledAsync(database, lambda.Id, cancellation));

        var stamp = entity.Preview + 1;

        CompilationOutcome outcome;

        // under the feature's name, like a deployment of the lambda is under
        // the lambda's: building the preview runs its code, and what that
        // prints belongs with the feature
        using (options.CaptureLambdaOutput
               ? LambdaOutput.Enter(new OutputScope(lambda.PublicKey, book, options.MaxOutputLines, lambda.Id, entity.Id))
               : null)
        {
            outcome = await deployments.PreviewAsync(lambda.Id, entity.Id, stamp, code, limits, cancellation);
        }

        if (outcome.Success)
        {
            try
            {
                // what the preview serves from now on, until it is deployed again -
                // a restart included - however much the feature changes meanwhile
                await storage.WritePreviewAsync(lambda.Id, entity.Id, code, cancellation);

                var now = DateTime.UtcNow;

                entity.Preview = stamp;
                entity.PreviewOf = entity.Revision;
                entity.Previewed = now;
                entity.Modified = now;
                lambda.Modified = now;

                await database.SaveChangesAsync(cancellation);
            }
            catch
            {
                // built, but not recorded: the next deployment would take the
                // same stamp for the build already there, and serve it
                deployments.EvictPreview(lambda.Id, entity.Id);
                throw;
            }
        }

        return new FeatureDeployment(outcome.Success, Describe(entity, await NewestAsync(database, lambda.Id, cancellation)), outcome.Diagnostics);
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

        return Describe(entity, await NewestAsync(database, lambda.Id, cancellation));
    }

    public async ValueTask<FeatureInfo> RefreshDataAsync(string privateKey, string feature, CancellationToken cancellation = default)
    {
        var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

        await using var context = database;

        using var held = turn;

        if (await WorkspaceEnabledAsync(database, lambda.Id, cancellation))
        {
            await storage.CopyWorkspaceAsync(lambda.Id, entity.Id, cancellation);
        }

        if (await DataSwitches.IsEnabledAsync(database, lambda.Id, DataKinds.Secrets, cancellation))
        {
            await secrets.CopyAsync(lambda.Id, entity.Id, cancellation);
        }

        // built again on its next request, so code that read the data into
        // memory when it started reads the fresh copy rather than writing the
        // old one back over it
        deployments.EvictPreview(lambda.Id, entity.Id);

        entity.Modified = DateTime.UtcNow;

        await database.SaveChangesAsync(cancellation);

        return Describe(entity, await NewestAsync(database, lambda.Id, cancellation));
    }

    #endregion

    #region Finishing it

    public async ValueTask<FeatureMerge> MergeAsync(string privateKey, string feature, VersionNote? note = null, bool deploy = false,
                                                    CancellationToken cancellation = default)
    {
        var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

        await using var context = database;

        using var held = turn;

        var newest = await NewestAsync(database, lambda.Id, cancellation);

        if (newest != entity.BaseVersion)
        {
            throw LambdaException.Conflict(Behind(entity, newest));
        }

        var code = await storage.ReadFeatureAsync(lambda.Id, entity.Id, cancellation)
                ?? throw LambdaException.NotFound($"The files of the feature '{entity.Name}' are no longer available.");

        if (code == await storage.ReadAsync(lambda.Id, entity.BaseVersion, cancellation))
        {
            throw LambdaException.Invalid($"The feature '{entity.Name}' holds exactly what version {entity.BaseVersion} holds, so there is nothing to merge. Change it first, or delete it.");
        }

        // a version is only made of what compiles: the feature is kept to be
        // fixed, rather than turned into a version nobody can put online
        var check = await meta.CheckAsync(privateKey, code, cancellation);

        if (!check.Success)
        {
            return new FeatureMerge(false, null, check.Diagnostics, null);
        }

        var origin = note?.Origin ?? VersionOrigins.Api;

        var merged = new VersionNote(string.IsNullOrWhiteSpace(note?.Specification) ? entity.Specification : note.Specification,
                                     string.IsNullOrWhiteSpace(note?.Change) ? entity.Change ?? $"Merges the feature '{entity.Name}'" : note.Change,
                                     origin);

        // refused if a version was saved since the base was checked above, by
        // the same turn every save of the lambda takes
        var version = await meta.SaveAsync(privateKey, code, merged, after: entity.BaseVersion, cancellation);

        // the version is there from here on: what follows is seen through
        // however the caller fares, or a feature would stay behind that was merged
        await RemoveAsync(database, lambda.Id, entity, CancellationToken.None);

        logger.LogInformation("Feature {FeatureId} of lambda {LambdaId} was merged as version {Version}", entity.Id, lambda.Id, version.Version);

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
                logger.LogWarning(e, "Version {Version} of lambda {LambdaId}, merged from a feature, could not be deployed", version.Version, lambda.Id);

                deployment = new DeploymentResult(false, null, [CompilationDiagnostic.Error($"Merged as version {version.Version}, but it could not be put online: {e.Message}")]);
            }
        }

        return new FeatureMerge(true, version, [], deployment);
    }

    public async ValueTask DeleteAsync(string privateKey, string feature, CancellationToken cancellation = default)
    {
        var (database, lambda, entity, turn) = await LockedAsync(privateKey, feature, cancellation);

        await using var context = database;

        using var held = turn;

        await RemoveAsync(database, lambda.Id, entity, cancellation);

        logger.LogInformation("Feature {FeatureId} of lambda {LambdaId} was deleted", entity.Id, lambda.Id);
    }

    public async ValueTask<int> RunMaintenanceAsync(DateTime now, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var quiet = now - options.DeploymentLifetime;

        // the same rule the lambda's own deployment lives by: online while
        // somebody works on it, offline once nobody has for the whole window
        var stale = await database.Features.AsNoTracking()
                                  .Where(f => f.Previewed != null && f.Modified < quiet && f.Lambda!.Tier == LambdaTier.Free)
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

        // offline in the database first, so no request builds one again
        foreach (var feature in stale)
        {
            if (!await database.Features.AnyAsync(f => f.Id == feature.Id && f.Previewed != null, cancellation))
            {
                deployments.EvictPreview(feature.LambdaId, feature.Id);
            }
        }

        logger.LogInformation("Maintenance took {Count} preview(s) of features offline", taken);

        return taken;
    }

    public async ValueTask<int> SweepAsync(CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        // a row is written before its folder and removed before it, so a
        // folder without a row is one left behind - by a request that was
        // still writing to the copy of the data when its feature went
        var known = (await database.Features.AsNoTracking().Select(f => f.Id).ToListAsync(cancellation)).ToHashSet();

        var swept = 0;

        foreach (var (lambdaId, featureId) in storage.ListFeatures())
        {
            if (!known.Contains(featureId))
            {
                deployments.EvictPreview(lambdaId, featureId);

                await storage.DeleteFeatureAsync(lambdaId, featureId, cancellation);

                swept++;
            }
        }

        if (swept > 0)
        {
            logger.LogInformation("Maintenance removed the files of {Count} feature(s) that no longer exist", swept);
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
        => new(feature.Key, feature.Name, feature.Specification, feature.Change, feature.BaseVersion, newest, feature.Origin,
               feature.Created, feature.Modified, feature.Previewed != null, feature.Previewed != null && feature.PreviewOf == feature.Revision,
               feature.Previewed, feature.Revision);

    /// <summary>
    /// Whether a key is one this service could have handed out: 32 lower case
    /// hexadecimal digits.
    /// </summary>
    internal static bool IsKey(string? key)
        => key is { Length: 32 } && key.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

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
    private async ValueTask RemoveAsync(LambdaDbContext database, long lambdaId, FeatureEntity feature, CancellationToken cancellation)
    {
        database.Features.Remove(feature);

        await database.SaveChangesAsync(cancellation);

        deployments.EvictPreview(lambdaId, feature.Id);

        await storage.DeleteFeatureAsync(lambdaId, feature.Id, CancellationToken.None);

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
    private async ValueTask<(LambdaDbContext Database, LambdaEntity Lambda, FeatureEntity Feature, IDisposable Turn)> LockedAsync(
        string privateKey, string feature, CancellationToken cancellation)
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

            var stripe = _stripes[(int)((ulong)id % (ulong)_stripes.Length)];

            await stripe.WaitAsync(cancellation);

            try
            {
                var entity = await database.Features.FirstOrDefaultAsync(f => f.Id == id, cancellation)
                          ?? throw LambdaException.NotFound("This feature does not exist (or has been merged or deleted).");

                var lambda = await database.Lambdas.FirstAsync(l => l.Id == lambdaId, cancellation);

                return (database, lambda, entity, new Turn(stripe));
            }
            catch
            {
                stripe.Release();
                throw;
            }
        }
        catch
        {
            await database.DisposeAsync();
            throw;
        }
    }

    private sealed class Turn(SemaphoreSlim stripe) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                stripe.Release();
            }
        }
    }

    private static async ValueTask<LambdaEntity> RequireLambdaAsync(LambdaDbContext database, string privateKey, CancellationToken cancellation)
        => await database.Lambdas.AsNoTracking().FirstOrDefaultAsync(l => l.PrivateKey == privateKey, cancellation)
        ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

    private static async ValueTask<FeatureEntity> RequireFeatureAsync(LambdaDbContext database, long lambdaId, string feature,
                                                                      CancellationToken cancellation)
    {
        var key = feature.Trim().ToLowerInvariant();

        return await database.Features.AsNoTracking().FirstOrDefaultAsync(f => f.LambdaId == lambdaId && f.Key == key, cancellation)
            ?? throw LambdaException.NotFound($"This lambda has no feature '{feature}' (it may have been merged or deleted). read_lambda lists the ones it has.");
    }

    private static async ValueTask<int?> NewestAsync(LambdaDbContext database, long lambdaId, CancellationToken cancellation)
        => await database.Deployments.Where(d => d.LambdaId == lambdaId).MaxAsync(d => (int?)d.Version, cancellation);

    private static ValueTask<bool> WorkspaceEnabledAsync(LambdaDbContext database, long lambdaId, CancellationToken cancellation)
        => DataSwitches.IsEnabledAsync(database, lambdaId, DataKinds.Workspace, cancellation);

    #endregion

}
