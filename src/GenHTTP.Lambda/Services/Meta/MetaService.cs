using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Storage;

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

    private LambdaOptions Options { get; }

    private ILogger Logger { get; }

    #endregion

    #region Initialization

    public MetaService(IDbContextFactory<LambdaDbContext> databases, IStorageService storage, IDeploymentService deployments,
        LambdaOptions options, ILogger<MetaService> logger)
    {
        Databases = databases;
        Storage = storage;
        Deployments = deployments;
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

        var expired = await database.Lambdas.Where(l => l.Modified < abandoned)
                                    .ToListAsync(cancellation);

        foreach (var lambda in expired)
        {
            await RemoveAsync(database, lambda, cancellation);
        }

        var stale = now - Options.DeploymentLifetime;

        var running = await database.Lambdas.Where(l => l.ActiveVersion != null)
                                    .ToListAsync(cancellation);

        var undeployed = 0;

        foreach (var lambda in running)
        {
            var deployed = await database.Deployments.Where(d => d.LambdaId == lambda.Id && d.Version == lambda.ActiveVersion)
                                         .Select(d => (DateTime?)d.Created)
                                         .FirstOrDefaultAsync(cancellation);

            if (deployed != null && deployed > stale)
            {
                continue;
            }

            lambda.ActiveVersion = null;

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

        if (code.Length > Options.MaxCodeLength)
        {
            throw LambdaException.Invalid($"The code must not exceed {Options.MaxCodeLength} characters.");
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

        database.Lambdas.Remove(lambda);

        await database.SaveChangesAsync(cancellation);

        await Storage.DeleteAsync(lambda.Id, cancellation);
    }

    private static async ValueTask<LambdaInfo> DescribeAsync(LambdaDbContext database, LambdaEntity lambda, CancellationToken cancellation)
    {
        var latest = await database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                   .MaxAsync(d => (int?)d.Version, cancellation);

        return new LambdaInfo(lambda.PublicKey, lambda.PrivateKey, lambda.Tier.ToString(), lambda.Created, lambda.Modified, lambda.ActiveVersion, latest);
    }

    private static async ValueTask<IReadOnlyList<LambdaVersionInfo>> ListVersionsAsync(LambdaDbContext database, long lambdaId, CancellationToken cancellation)
        => await database.Deployments.AsNoTracking()
                         .Where(d => d.LambdaId == lambdaId)
                         .OrderByDescending(d => d.Version)
                         .Select(d => new LambdaVersionInfo(d.Version, d.Created))
                         .ToListAsync(cancellation);

    #endregion

}
