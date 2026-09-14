using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Telemetry;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The meta data side of a lambda: keys, versions and which of them is live.
/// Code itself is handed to the storage service, building it to the deployment service.
/// </summary>
public sealed class MetaService : IMetaService
{
    private const int KeyAttempts = 8;

    #region Get-/Setters

    private IDbContextFactory<LambdaDbContext> Databases { get; }

    private IStorageService Storage { get; }

    private IDeploymentService Deployments { get; }

    private LambdaTelemetry Activity { get; }

    private LambdaOptions Options { get; }

    private ILogger Logger { get; }

    #endregion

    #region Initialization

    public MetaService(IDbContextFactory<LambdaDbContext> databases, IStorageService storage, IDeploymentService deployments,
        LambdaTelemetry activity, LambdaOptions options, ILogger<MetaService> logger)
    {
        Databases = databases;
        Storage = storage;
        Deployments = deployments;
        Activity = activity;
        Options = options;
        Logger = logger;
    }

    #endregion

    #region Keys

    public async ValueTask<KeyAvailability> CheckKeyAsync(string? publicKey, CancellationToken cancellation = default)
    {
        if (!LambdaKeys.TryNormalize(publicKey, out var normalized, out var reason))
        {
            return new KeyAvailability(normalized, false, reason);
        }

        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var taken = await database.Lambdas.AnyAsync(l => l.PublicKey == normalized, cancellation);

        return new KeyAvailability(normalized, !taken, taken ? "This key is already in use." : null);
    }

    public async ValueTask<LambdaInfo> ChangeKeyAsync(string privateKey, string? publicKey, CancellationToken cancellation = default)
    {
        if (!LambdaKeys.TryNormalize(publicKey, out var normalized, out var reason))
        {
            throw LambdaException.Invalid(reason!);
        }

        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        if (lambda.PublicKey == normalized)
        {
            return await DescribeAsync(database, lambda, cancellation);
        }

        if (await database.Lambdas.AnyAsync(l => l.PublicKey == normalized, cancellation))
        {
            throw LambdaException.Conflict("This key is already in use.");
        }

        var previous = lambda.PublicKey;

        lambda.PublicKey = normalized;
        lambda.Modified = DateTime.UtcNow;

        await database.SaveChangesAsync(cancellation);

        Logger.LogInformation("Lambda {LambdaId} moved from '{Previous}' to '{Current}'", lambda.Id, previous, normalized);

        return await DescribeAsync(database, lambda, cancellation);
    }

    #endregion

    #region Lifecycle

    public async ValueTask<LambdaInfo> CreateAsync(string? publicKey, string? template = null, CancellationToken cancellation = default)
    {
        var requested = !string.IsNullOrWhiteSpace(publicKey);

        if (template != null && !TemplateCatalog.Exists(template))
        {
            throw LambdaException.Invalid($"There is no template called '{template}'.");
        }

        if (requested && !LambdaKeys.TryNormalize(publicKey, out _, out var reason))
        {
            throw LambdaException.Invalid(reason!);
        }

        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var now = DateTime.UtcNow;

        for (var attempt = 0; attempt < KeyAttempts; attempt++)
        {
            LambdaKeys.TryNormalize(requested ? publicKey : LambdaKeys.CreatePublicKey(), out var key, out _);

            var entity = new LambdaEntity
            {
                PublicKey = key,
                PrivateKey = LambdaKeys.CreatePrivateKey(),
                Tier = LambdaTier.Free,
                Created = now,
                Modified = now
            };

            database.Lambdas.Add(entity);

            try
            {
                await database.SaveChangesAsync(cancellation);
            }
            catch (DbUpdateException) when (!requested)
            {
                // generated key collided with an existing one, try the next
                database.Entry(entity).State = EntityState.Detached;
                continue;
            }
            catch (DbUpdateException)
            {
                throw LambdaException.Conflict("This key is already in use.");
            }

            await SeedAsync(database, entity, template, now, cancellation);

            Logger.LogInformation("Created lambda {LambdaId} at '{PublicKey}'", entity.Id, entity.PublicKey);

            return await DescribeAsync(database, entity, cancellation);
        }

        throw LambdaException.Conflict("Unable to find a free key, please try again.");
    }

    public async ValueTask DeleteAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        await RemoveAsync(database, lambda, cancellation);

        Logger.LogInformation("Deleted lambda {LambdaId} at '{PublicKey}'", lambda.Id, lambda.PublicKey);
    }

    #endregion

    #region Reading

    public async ValueTask<LambdaInfo?> GetAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.FirstOrDefaultAsync(l => l.PrivateKey == privateKey, cancellation);

        return lambda == null ? null : await DescribeAsync(database, lambda, cancellation);
    }

    public async ValueTask<ResolvedLambda?> ResolveAsync(string publicKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.AsNoTracking()
                                   .FirstOrDefaultAsync(l => l.PublicKey == publicKey, cancellation);

        if (lambda?.ActiveVersion == null)
        {
            return null;
        }

        var deployment = await database.Deployments.AsNoTracking()
                                       .FirstOrDefaultAsync(d => d.LambdaId == lambda.Id && d.Version == lambda.ActiveVersion, cancellation);

        if (deployment == null)
        {
            return null;
        }

        return new ResolvedLambda(lambda.Id, lambda.PublicKey, lambda.Tier.ToString(), deployment.Version, deployment.Created);
    }

    public async ValueTask<PublicStatus> GetStatusAsync(string publicKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.AsNoTracking()
                                   .FirstOrDefaultAsync(l => l.PublicKey == publicKey, cancellation);

        return new PublicStatus(publicKey, lambda != null, lambda?.ActiveVersion != null);
    }

    public async ValueTask<IReadOnlyList<LambdaVersionInfo>> GetVersionsAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        return await ListVersionsAsync(database, lambda.Id, cancellation);
    }

    public async ValueTask<LambdaVersionContent> GetVersionAsync(string privateKey, int version, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        var deployment = await database.Deployments.AsNoTracking()
                                       .FirstOrDefaultAsync(d => d.LambdaId == lambda.Id && d.Version == version, cancellation)
                      ?? throw LambdaException.NotFound($"Version {version} does not exist.");

        var code = await Storage.ReadAsync(lambda.Id, version, cancellation)
                ?? throw LambdaException.NotFound($"The code of version {version} is no longer available.");

        return new LambdaVersionContent(deployment.Version, deployment.Created, code);
    }

    #endregion

    #region Editing

    public async ValueTask<LambdaVersionInfo> SaveAsync(string privateKey, string code, CancellationToken cancellation = default)
    {
        Validate(code);

        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        var version = await AppendAsync(database, lambda, code, DateTime.UtcNow, cancellation);

        Logger.LogInformation("Saved version {Version} of lambda {LambdaId}", version.Version, lambda.Id);

        return version;
    }

    public async ValueTask<CompilationOutcome> CheckAsync(string privateKey, string code, CancellationToken cancellation = default)
    {
        Validate(code);

        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        return await Deployments.ValidateAsync(code, lambda.Id, cancellation);
    }

    public async ValueTask<DeploymentResult> DeployAsync(string privateKey, int? version, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        var target = version ?? await database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                              .MaxAsync(d => (int?)d.Version, cancellation)
                  ?? throw LambdaException.Invalid("There is nothing to deploy yet, save the code first.");

        if (!await database.Deployments.AnyAsync(d => d.LambdaId == lambda.Id && d.Version == target, cancellation))
        {
            throw LambdaException.NotFound($"Version {target} does not exist.");
        }

        var outcome = await Deployments.ActivateAsync(lambda.Id, target, cancellation);

        if (!outcome.Success)
        {
            Logger.LogInformation("Deployment of lambda {LambdaId} was rejected with {Count} error(s)", lambda.Id, outcome.Diagnostics.Count);

            return new DeploymentResult(false, await DescribeAsync(database, lambda, cancellation), outcome.Diagnostics);
        }

        lambda.ActiveVersion = target;
        lambda.Deployed = DateTime.UtcNow;
        lambda.Modified = DateTime.UtcNow;

        await database.SaveChangesAsync(cancellation);

        return new DeploymentResult(true, await DescribeAsync(database, lambda, cancellation), outcome.Diagnostics);
    }

    public async ValueTask<LambdaInfo> UndeployAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        if (lambda.ActiveVersion != null)
        {
            lambda.ActiveVersion = null;
            lambda.Deployed = null;

            await database.SaveChangesAsync(cancellation);

            Deployments.Evict(lambda.Id);

            Logger.LogInformation("Undeployed lambda {LambdaId} at '{PublicKey}'", lambda.Id, lambda.PublicKey);
        }

        return await DescribeAsync(database, lambda, cancellation);
    }

    #endregion

    #region Maintenance

    public async ValueTask<MaintenanceReport> RunMaintenanceAsync(DateTime now, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var abandoned = now - Options.Retention;

        // examples are the installation's own, and being untouched is their
        // normal state rather than a sign that nobody wants them
        var expired = await database.Lambdas.Where(l => !l.IsExample && l.Modified < abandoned)
                                    .ToListAsync(cancellation);

        foreach (var lambda in expired)
        {
            await RemoveAsync(database, lambda, cancellation);
        }

        var stale = now - Options.DeploymentLifetime;

        var running = await database.Lambdas.Where(l => !l.IsExample && l.ActiveVersion != null)
                                    .ToListAsync(cancellation);

        var undeployed = 0;

        foreach (var lambda in running)
        {
            // a lambda deployed before the column existed has no date; it is
            // treated as deployed now rather than swept on the next pass
            if (lambda.Deployed is null)
            {
                lambda.Deployed = now;
                continue;
            }

            if (lambda.Deployed > stale)
            {
                continue;
            }

            lambda.ActiveVersion = null;
            lambda.Deployed = null;

            Deployments.Evict(lambda.Id);

            undeployed++;
        }

        if (undeployed > 0)
        {
            await database.SaveChangesAsync(cancellation);
        }

        if (undeployed > 0 || expired.Count > 0)
        {
            Logger.LogInformation("Maintenance undeployed {Undeployed} and deleted {Deleted} lambda(s)", undeployed, expired.Count);
        }

        return new MaintenanceReport(undeployed, expired.Count);
    }

    #endregion

    #region Helpers

    private void Validate(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw LambdaException.Invalid("The code must not be empty.");
        }

        var files = LambdaSource.Parse(code);

        if (LambdaSource.Validate(files) is { } complaint)
        {
            throw LambdaException.Invalid(complaint);
        }

        // the limit counts what was written rather than what it is stored as,
        // so splitting a lambda into files does not spend any of it on the
        // envelope those files are kept in
        if (LambdaSource.Length(files) > Options.MaxCodeLength)
        {
            throw LambdaException.Invalid($"The code must not exceed {Options.MaxCodeLength} characters.");
        }

        if (LambdaSource.AssetBytes(files) > Options.MaxAssetBytes)
        {
            throw LambdaException.Invalid($"The assets must not exceed {Options.MaxAssetBytes / 1024} KB in total.");
        }
    }

    public async ValueTask<LambdaCounts> CountAsync(CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        return new LambdaCounts(
            await database.Lambdas.CountAsync(cancellation),
            await database.Lambdas.CountAsync(l => l.ActiveVersion != null, cancellation),
            await database.Deployments.CountAsync(cancellation)
        );
    }

    public async ValueTask<long?> GetIdAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        return await database.Lambdas.AsNoTracking()
                             .Where(l => l.PrivateKey == privateKey)
                             .Select(l => (long?)l.Id)
                             .FirstOrDefaultAsync(cancellation);
    }

    public async ValueTask<LambdaPage> ListAsync(string? search = null, int skip = 0, int take = int.MaxValue, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var total = await database.Lambdas.AsNoTracking().CountAsync(cancellation);

        var deployed = await database.Lambdas.AsNoTracking().CountAsync(l => l.ActiveVersion != null, cancellation);

        var query = database.Lambdas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            // the public key is the only thing an administrator has to go on
            // that is not the code itself, and it is what the panel shows
            query = query.Where(l => EF.Functions.Like(l.PublicKey, $"%{term}%"));
        }

        var matched = await query.CountAsync(cancellation);

        var lambdas = await query.OrderByDescending(l => l.Created)
                                 .Skip(skip)
                                 .Take(take)
                                 .ToListAsync(cancellation);

        var counts = await database.Deployments.AsNoTracking()
                                   .GroupBy(d => d.LambdaId)
                                   .Select(g => new { LambdaId = g.Key, Count = g.Count() })
                                   .ToDictionaryAsync(g => g.LambdaId, g => g.Count, cancellation);

        var latest = await database.Deployments.AsNoTracking()
                                   .GroupBy(d => d.LambdaId)
                                   .Select(g => new { LambdaId = g.Key, Version = g.Max(d => d.Version) })
                                   .ToDictionaryAsync(g => g.LambdaId, g => (int?)g.Version, cancellation);

        return new LambdaPage([.. lambdas.Select(l => new LambdaOverview(
            l.PublicKey,
            l.PrivateKey,
            l.Tier.ToString(),
            l.Created,
            l.Modified,
            l.ActiveVersion,
            latest.GetValueOrDefault(l.Id),
            counts.GetValueOrDefault(l.Id),
            l.ActiveVersion != null ? l.Deployed + Options.DeploymentLifetime : null,
            l.Modified + Options.Retention
        ))], matched, total, deployed);
    }

    public async ValueTask<string?> GetPrivateKeyAsync(string publicKey, CancellationToken cancellation = default)
    {
        if (!LambdaKeys.TryNormalize(publicKey, out var normalized, out _))
        {
            return null;
        }

        await using var database = await Databases.CreateDbContextAsync(cancellation);

        return await database.Lambdas.AsNoTracking()
                             .Where(l => l.PublicKey == normalized)
                             .Select(l => l.PrivateKey)
                             .FirstOrDefaultAsync(cancellation);
    }

    private static async ValueTask<LambdaEntity> RequireAsync(LambdaDbContext database, string privateKey, CancellationToken cancellation)
        => await database.Lambdas.FirstOrDefaultAsync(l => l.PrivateKey == privateKey, cancellation)
        ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

    private async ValueTask SeedAsync(LambdaDbContext database, LambdaEntity lambda, string? template, DateTime now, CancellationToken cancellation)
        => await AppendAsync(database, lambda, TemplateCatalog.ForKey(template, lambda.PublicKey), now, cancellation);

    private async ValueTask<LambdaVersionInfo> AppendAsync(LambdaDbContext database, LambdaEntity lambda, string code, DateTime now, CancellationToken cancellation)
    {
        var version = await database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                    .MaxAsync(d => (int?)d.Version, cancellation) + 1 ?? 1;

        await Storage.WriteAsync(lambda.Id, version, code, cancellation);

        database.Deployments.Add(new DeploymentEntity
        {
            LambdaId = lambda.Id,
            Version = version,
            Created = now
        });

        lambda.Modified = now;

        await database.SaveChangesAsync(cancellation);

        await PruneAsync(database, lambda, cancellation);

        return new LambdaVersionInfo(version, now);
    }

    /// <summary>
    /// Keeps the version history bounded, never touching the version that is live.
    /// </summary>
    private async ValueTask PruneAsync(LambdaDbContext database, LambdaEntity lambda, CancellationToken cancellation)
    {
        var obsolete = await database.Deployments.Where(d => d.LambdaId == lambda.Id && d.Version != lambda.ActiveVersion)
                                     .OrderByDescending(d => d.Version)
                                     .Skip(Options.MaxVersions)
                                     .ToListAsync(cancellation);

        if (obsolete.Count == 0)
        {
            return;
        }

        foreach (var deployment in obsolete)
        {
            await Storage.DeleteVersionAsync(lambda.Id, deployment.Version, cancellation);
        }

        database.Deployments.RemoveRange(obsolete);

        await database.SaveChangesAsync(cancellation);
    }

    private async ValueTask RemoveAsync(LambdaDbContext database, LambdaEntity lambda, CancellationToken cancellation)
    {
        Deployments.Evict(lambda.Id);

        // a deleted lambda takes its numbers with it rather than leaving a row
        // in the activity list that nothing can be looked up from any more
        Activity.Evict(lambda.Id);

        database.Lambdas.Remove(lambda);

        await database.SaveChangesAsync(cancellation);

        await Storage.DeleteAsync(lambda.Id, cancellation);
    }

    private async ValueTask<LambdaInfo> DescribeAsync(LambdaDbContext database, LambdaEntity lambda, CancellationToken cancellation)
    {
        var latest = await database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                   .MaxAsync(d => (int?)d.Version, cancellation);

        // the two deadlines the maintenance job will act on, so the editor can
        // say when rather than leaving it to be discovered
        var until = lambda.Deployed + Options.DeploymentLifetime;

        return new LambdaInfo(lambda.PublicKey, lambda.PrivateKey, lambda.Tier.ToString(), lambda.Created, lambda.Modified,
                              lambda.ActiveVersion, latest, lambda.Deployed, lambda.ActiveVersion != null ? until : null,
                              lambda.Modified + Options.Retention);
    }

    private static async ValueTask<IReadOnlyList<LambdaVersionInfo>> ListVersionsAsync(LambdaDbContext database, long lambdaId, CancellationToken cancellation)
        => await database.Deployments.AsNoTracking()
                         .Where(d => d.LambdaId == lambdaId)
                         .OrderByDescending(d => d.Version)
                         .Select(d => new LambdaVersionInfo(d.Version, d.Created))
                         .ToListAsync(cancellation);

    #endregion

}
