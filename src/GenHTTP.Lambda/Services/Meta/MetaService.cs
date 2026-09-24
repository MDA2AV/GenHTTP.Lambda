using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
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

    private LogBook Book { get; }

    private ILogger Logger { get; }

    #endregion

    #region Initialization

    public MetaService(IDbContextFactory<LambdaDbContext> databases, IStorageService storage, IDeploymentService deployments,
        LambdaTelemetry activity, LambdaOptions options, LogBook book, ILogger<MetaService> logger)
    {
        Databases = databases;
        Storage = storage;
        Deployments = deployments;
        Activity = activity;
        Options = options;
        Book = book;
        Logger = logger;
    }

    #endregion

    #region Keys

    public async ValueTask<KeyStatus> DescribeKeyAsync(string? publicKey, CancellationToken cancellation = default)
    {
        // looked up even when it could not be claimed: a key that is refused
        // today may still belong to a lambda from before the rule was made
        var valid = LambdaKeys.TryNormalize(publicKey, out var normalized, out var reason);

        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.AsNoTracking()
                                   .Where(l => l.PublicKey == normalized)
                                   .Select(l => new { l.ActiveVersion })
                                   .FirstOrDefaultAsync(cancellation);

        if (valid && lambda != null)
        {
            reason = "This key is already in use.";
        }

        return new KeyStatus(normalized, valid, lambda != null, lambda?.ActiveVersion != null, reason);
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

            // after the insert rather than before: the id the event refers to
            // is the one the database just handed out
            Record(database, entity, LambdaEvents.Created);

            await database.SaveChangesAsync(cancellation);

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

        return new LambdaVersionContent(deployment.Version, deployment.Created, code, deployment.Specification, deployment.Change, deployment.Origin);
    }

    public async ValueTask<IReadOnlyList<LambdaActivation>> GetActivationsAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        return await database.Activations.AsNoTracking()
                             .Where(a => a.LambdaId == lambda.Id)
                             .OrderByDescending(a => a.Started)
                             .ThenByDescending(a => a.Id)
                             .Select(a => new LambdaActivation(a.Version, a.Started, a.Origin, a.Ended, a.EndedBy))
                             .ToListAsync(cancellation);
    }

    #endregion

    #region Editing

    public async ValueTask<LambdaVersionInfo> SaveAsync(string privateKey, string code, VersionNote? note = null, CancellationToken cancellation = default)
    {
        Validate(code);

        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        var version = await AppendAsync(database, lambda, code, DateTime.UtcNow, note ?? new VersionNote(Origin: VersionOrigins.Api), cancellation);

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

    public async ValueTask<DeploymentResult> DeployAsync(string privateKey, int? version, string? origin = null, CancellationToken cancellation = default)
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

        /*
         * Under the lambda's own name, because activating it runs its code:
         * everything outside the handler it returns happens once, here, and a
         * print from there is how somebody watches their own start up. A
         * request would carry this mark on its own - the concern that serves
         * one sets it - but a deployment is not a request.
         */
        CompilationOutcome outcome;

        using (Options.CaptureLambdaOutput
               ? LambdaOutput.Enter(new OutputScope(lambda.PublicKey, Book, Options.MaxOutputLines, lambda.Id))
               : null)
        {
            outcome = await Deployments.ActivateAsync(lambda.Id, target, cancellation);
        }

        if (!outcome.Success)
        {
            Logger.LogInformation("Deployment of lambda {LambdaId} was rejected with {Count} error(s)", lambda.Id, outcome.Diagnostics.Count);

            return new DeploymentResult(false, await DescribeAsync(database, lambda, cancellation), outcome.Diagnostics);
        }

        var now = DateTime.UtcNow;

        lambda.ActiveVersion = target;
        lambda.Deployed = now;
        lambda.Modified = now;

        await CloseActivationAsync(database, lambda.Id, now, ActivationEndings.Replaced, cancellation);

        database.Activations.Add(new ActivationEntity
        {
            LambdaId = lambda.Id,
            Version = target,
            Started = now,
            Origin = origin ?? VersionOrigins.Api
        });

        Record(database, lambda, LambdaEvents.Deployed);

        await database.SaveChangesAsync(cancellation);

        await PruneActivationsAsync(database, lambda.Id, cancellation);

        return new DeploymentResult(true, await DescribeAsync(database, lambda, cancellation), outcome.Diagnostics);
    }

    public async ValueTask<LambdaInfo> UndeployAsync(string privateKey, string? endedBy = null, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        if (lambda.ActiveVersion != null)
        {
            lambda.ActiveVersion = null;
            lambda.Deployed = null;

            await CloseActivationAsync(database, lambda.Id, DateTime.UtcNow, endedBy ?? ActivationEndings.Stopped, cancellation);

            Record(database, lambda, LambdaEvents.Undeployed);

            await database.SaveChangesAsync(cancellation);

            Deployments.Evict(lambda.Id);

            Logger.LogInformation("Undeployed lambda {LambdaId} at '{PublicKey}'", lambda.Id, lambda.PublicKey);
        }

        return await DescribeAsync(database, lambda, cancellation);
    }

    #endregion

    #region Maintenance

    /// <summary>
    /// Writes what the counters in memory know about who has been called.
    /// </summary>
    /// <remarks>
    /// Only forwards, and only for rows that would move: a restart empties the
    /// counters, and a lambda that has had no traffic since should keep the
    /// date it already had rather than be pushed back to the epoch.
    /// </remarks>
    private async ValueTask RecordUseAsync(LambdaDbContext database, CancellationToken cancellation)
    {
        var seen = Activity.Describe()
                           .Where(a => a.LastSeen != null)
                           .ToDictionary(a => a.PublicKey, a => a.LastSeen!.Value, StringComparer.Ordinal);

        if (seen.Count == 0)
        {
            return;
        }

        var keys = seen.Keys.ToList();

        var rows = await database.Lambdas.Where(l => keys.Contains(l.PublicKey)).ToListAsync(cancellation);

        var moved = 0;

        foreach (var row in rows)
        {
            if (seen.TryGetValue(row.PublicKey, out var last) && (row.LastSeen == null || last > row.LastSeen))
            {
                row.LastSeen = last;
                moved++;
            }
        }

        if (moved > 0)
        {
            await database.SaveChangesAsync(cancellation);
        }
    }

    /// <summary>
    /// When a lambda last had any attention of either kind.
    /// </summary>
    /// <remarks>
    /// Both clocks run from here, so what a visitor is told about how long
    /// something has left is measured the same way the sweep measures it.
    /// Anything else would show a date that passes without anything happening.
    /// </remarks>
    private static DateTime Quiet(LambdaEntity lambda)
        => lambda.LastSeen > lambda.Modified ? lambda.LastSeen.Value : lambda.Modified;

    public async ValueTask<MaintenanceReport> RunMaintenanceAsync(DateTime now, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        /*
         * What counts as use, written down before anything is decided by it.
         *
         * Requests are counted in memory, because a database write per request
         * to move a timestamp would be absurd; this is where the last of them
         * reaches the row. It runs first so that a lambda busy right up to
         * this moment is not swept by figures taken before its traffic was
         * recorded.
         */
        await RecordUseAsync(database, cancellation);

        var abandoned = now - Options.Retention;

        // examples are the installation's own, and being untouched is their
        // normal state rather than a sign that nobody wants them
        var expired = await database.Lambdas
                                    .Where(l => !l.IsExample && l.Modified < abandoned
                                             && (l.LastSeen == null || l.LastSeen < abandoned))
                                    .ToListAsync(cancellation);

        foreach (var lambda in expired)
        {
            await RemoveAsync(database, lambda, cancellation);
        }

        var quiet = now - Options.DeploymentLifetime;

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

            /*
             * The later of the two kinds of attention a lambda can get.
             *
             * Being edited counts and being visited counts, and it only goes
             * offline once neither has happened for the whole window. The
             * deployment's own age is deliberately not in here: how long ago
             * something was put online says nothing about whether anybody
             * wants it, and using it as the test is what used to take working
             * lambdas down overnight.
             */
            var used = lambda.LastSeen > lambda.Modified ? lambda.LastSeen.Value : lambda.Modified;

            if (used > quiet)
            {
                continue;
            }

            lambda.ActiveVersion = null;
            lambda.Deployed = null;

            await CloseActivationAsync(database, lambda.Id, now, ActivationEndings.Expired, cancellation);

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
            l.ActiveVersion != null ? Quiet(l) + Options.DeploymentLifetime : null,
            Quiet(l) + Options.Retention
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
    {
        var name = TemplateCatalog.Groups.SelectMany(g => g.Templates)
                                  .FirstOrDefault(t => t.Id == template)?.Name;

        var note = new VersionNote(Change: name != null ? $"Started from the template '{name}'" : "Started from the default template",
                                   Origin: VersionOrigins.Template);

        await AppendAsync(database, lambda, TemplateCatalog.ForKey(template, lambda.PublicKey), now, note, cancellation);
    }

    private async ValueTask<LambdaVersionInfo> AppendAsync(LambdaDbContext database, LambdaEntity lambda, string code, DateTime now, VersionNote note, CancellationToken cancellation)
    {
        var version = await database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                    .MaxAsync(d => (int?)d.Version, cancellation) + 1 ?? 1;

        var specification = Tidy(note.Specification, VersionNote.MaxSpecification);

        var change = Tidy(note.Change, VersionNote.MaxChange);

        await Storage.WriteAsync(lambda.Id, version, code, cancellation);

        database.Deployments.Add(new DeploymentEntity
        {
            LambdaId = lambda.Id,
            Version = version,
            Created = now,
            Specification = specification,
            Change = change,
            Origin = note.Origin
        });

        lambda.Modified = now;

        Record(database, lambda, LambdaEvents.Saved);

        await database.SaveChangesAsync(cancellation);

        await PruneAsync(database, lambda, cancellation);

        return new LambdaVersionInfo(version, now, specification, change, note.Origin);
    }

    /// <summary>
    /// A note as it is kept: trimmed, nothing where there was only space, and
    /// cut rather than refused where it runs long.
    /// </summary>
    /// <remarks>
    /// Cut rather than refused because the note is the least important part
    /// of a save. An agent that wrote working code and a paragraph too many
    /// about it should lose the end of the paragraph, not the code.
    /// </remarks>
    private static string? Tidy(string? text, int most)
    {
        var trimmed = text?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return trimmed.Length <= most ? trimmed : string.Concat(trimmed.AsSpan(0, most - 2), " …");
    }

    /// <summary>
    /// Ends whatever stretch of being online is still open.
    /// </summary>
    private static async ValueTask CloseActivationAsync(LambdaDbContext database, long lambdaId, DateTime now, string endedBy, CancellationToken cancellation)
    {
        var open = await database.Activations.Where(a => a.LambdaId == lambdaId && a.Ended == null)
                                 .ToListAsync(cancellation);

        foreach (var activation in open)
        {
            activation.Ended = now;
            activation.EndedBy = endedBy;
        }
    }

    /// <summary>
    /// How many stretches of being online are remembered per lambda.
    /// </summary>
    /// <remarks>
    /// An agent iterating on something deploys a great deal, and the history
    /// is for reading back what happened lately, not for keeping every
    /// deployment a lambda ever had.
    /// </remarks>
    private const int MaxActivations = 200;

    private static async ValueTask PruneActivationsAsync(LambdaDbContext database, long lambdaId, CancellationToken cancellation)
    {
        var obsolete = await database.Activations.Where(a => a.LambdaId == lambdaId && a.Ended != null)
                                     .OrderByDescending(a => a.Started)
                                     .ThenByDescending(a => a.Id)
                                     .Skip(MaxActivations)
                                     .ToListAsync(cancellation);

        if (obsolete.Count > 0)
        {
            database.Activations.RemoveRange(obsolete);

            await database.SaveChangesAsync(cancellation);
        }
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

    /// <summary>
    /// Notes that something happened, to be read long after it did.
    /// </summary>
    /// <remarks>
    /// Added to the same context as the change it describes, so it is written
    /// in the same transaction: an event recorded for a save that then failed
    /// would be a lie, and one written separately could be lost on its own.
    ///
    /// Examples are skipped. The installation seeds its own on every boot, and
    /// counting those would bury the activity the figures are meant to show.
    /// </remarks>
    private static void Record(LambdaDbContext database, LambdaEntity lambda, string kind)
    {
        if (lambda.IsExample)
        {
            return;
        }

        database.Events.Add(new EventEntity
        {
            Kind = kind,
            LambdaId = lambda.Id,
            PublicKey = lambda.PublicKey,
            Occurred = DateTime.UtcNow
        });
    }

    private async ValueTask RemoveAsync(LambdaDbContext database, LambdaEntity lambda, CancellationToken cancellation)
    {
        Deployments.Evict(lambda.Id);

        // a deleted lambda takes its numbers with it rather than leaving a row
        // in the activity list that nothing can be looked up from any more
        Activity.Evict(lambda.Id);

        Record(database, lambda, LambdaEvents.Deleted);

        // the key cascades in the schema, but only where the connection has
        // foreign keys switched on - said here so it does not depend on that
        await database.Activations.Where(a => a.LambdaId == lambda.Id).ExecuteDeleteAsync(cancellation);

        await database.Showcases.Where(s => s.LambdaId == lambda.Id).ExecuteDeleteAsync(cancellation);

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
        var until = Quiet(lambda) + Options.DeploymentLifetime;

        return new LambdaInfo(lambda.PublicKey, lambda.PrivateKey, lambda.Tier.ToString(), lambda.Created, lambda.Modified,
                              lambda.ActiveVersion, latest, lambda.Deployed, lambda.ActiveVersion != null ? until : null,
                              Quiet(lambda) + Options.Retention);
    }

    private static async ValueTask<IReadOnlyList<LambdaVersionInfo>> ListVersionsAsync(LambdaDbContext database, long lambdaId, CancellationToken cancellation)
        => await database.Deployments.AsNoTracking()
                         .Where(d => d.LambdaId == lambdaId)
                         .OrderByDescending(d => d.Version)
                         .Select(d => new LambdaVersionInfo(d.Version, d.Created, d.Specification, d.Change, d.Origin))
                         .ToListAsync(cancellation);

    #endregion

}
