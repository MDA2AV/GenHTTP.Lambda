using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The meta data side of a lambda: keys, versions and which of them is live.
/// Files themselves are handed to the storage service, building them to the deployment service.
/// </summary>
/// <remarks>
/// The face every door into a lambda talks to, and the part that decides what
/// is done and in which order: it finds the lambda, checks that it may be
/// changed, takes its turn where a change needs one, and leaves the rest to
/// the units beside it. <see cref="LambdaHosting"/> changes where a lambda
/// answers and on what terms, <see cref="LambdaHistory"/> writes down its
/// versions, its stretches online and its events, <see cref="LambdaLifetime"/>
/// says when it goes offline and away and sweeps, <see cref="LambdaRemoval"/>
/// takes everything with it when it goes, <see cref="LambdaDescriber"/> says
/// what the outside is told, and <see cref="LambdaGuard"/> and
/// <see cref="VersionInput"/> say what is allowed.
/// </remarks>
public sealed class MetaService : IMetaService
{
    private const int KeyAttempts = 8;

    /// <summary>
    /// Takes turns over what adds a version to a lambda or changes what it has
    /// online, one lambda at a time.
    /// </summary>
    /// <remarks>
    /// Two saves at once used to be able to pick the same number for their
    /// version, and a feature being merged has to find the newest version to
    /// be the one it is based on at the moment its own is added - not a moment
    /// before, with somebody else's save landing in between.
    ///
    /// Held across the awaits of the records it reads and writes, which a
    /// semaphore allows, but not across compiling: a deployment compiles
    /// before it takes one (see <see cref="DeployAsync"/>), so a save of the
    /// same lambda does not wait seconds for it.
    /// </remarks>
    private readonly Turns _turns = new();

    #region Get-/Setters

    private IDbContextFactory<LambdaDbContext> Databases { get; }

    private IStorageService Storage { get; }

    private IDeploymentService Deployments { get; }

    private LambdaOptions Options { get; }

    private ILimitsService Limits { get; }

    private ILogBook Book { get; }

    private IDatabaseVault DatabaseVault { get; }

    private LambdaHosting Hosting { get; }

    private LambdaHistory History { get; }

    private LambdaLifetime Lifetime { get; }

    private LambdaRemoval Removal { get; }

    private LambdaDescriber Describer { get; }

    private LambdaListing Listing { get; }

    private VersionFactsCache Facts { get; }

    private EventReader Events { get; }

    private EditorAddresses Editors { get; }

    private ILogger Logger { get; }

    /// <summary>
    /// The lambdas being served, by the key in their path.
    /// </summary>
    private ResolutionCache<string> ByKey { get; }

    /// <summary>
    /// The lambdas being served, by id - how a lambda's own domain finds it.
    /// </summary>
    private ResolutionCache<long> ById { get; }

    /// <summary>
    /// What the caches load with, made once rather than on every request
    /// that finds what they hold.
    /// </summary>
    private Func<string, ValueTask<ResolvedLambda?>> LoadByKey { get; }

    private Func<long, ValueTask<ResolvedLambda?>> LoadById { get; }

    #endregion

    #region Initialization

    public MetaService(IDbContextFactory<LambdaDbContext> databases, IStorageService storage, IDeploymentService deployments,
        LambdaOptions options, ILimitsService limits, ILogBook book, IDatabaseVault databaseVault, LambdaHosting hosting, LambdaHistory history,
        LambdaLifetime lifetime, LambdaRemoval removal, LambdaDescriber describer, LambdaListing listing, VersionFactsCache facts, EventReader events,
        EditorAddresses editors, DatabaseChanges changes, ILogger<MetaService> logger)
    {
        ByKey = new ResolutionCache<string>(changes);
        ById = new ResolutionCache<long>(changes);

        LoadByKey = key => LoadAsync(database => database.Lambdas.AsNoTracking().FirstOrDefaultAsync(l => l.PublicKey == key));
        LoadById = id => LoadAsync(database => database.Lambdas.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id));

        Databases = databases;
        Storage = storage;
        Deployments = deployments;
        Options = options;
        Limits = limits;
        Book = book;
        DatabaseVault = databaseVault;
        Hosting = hosting;
        History = history;
        Lifetime = lifetime;
        Removal = removal;
        Describer = describer;
        Listing = listing;
        Facts = facts;
        Events = events;
        Editors = editors;
        Logger = logger;
    }

    #endregion

    #region Settings

    public ValueTask<KeyStatus> DescribeKeyAsync(string? publicKey, CancellationToken cancellation = default)
        => Hosting.DescribeKeyAsync(publicKey, cancellation);

    public ValueTask<LambdaInfo> ChangeKeyAsync(string privateKey, string? publicKey, CancellationToken cancellation = default)
        => Hosting.ChangeKeyAsync(privateKey, publicKey, cancellation);

    public ValueTask<LambdaInfo> ChangeDomainAsync(string privateKey, string? domain, CancellationToken cancellation = default)
        => Hosting.ChangeDomainAsync(privateKey, domain, cancellation);

    public ValueTask<LambdaInfo> ChangeTierAsync(string privateKey, LambdaTier tier, CancellationToken cancellation = default)
        => Hosting.ChangeTierAsync(privateKey, tier, cancellation);

    public async ValueTask<LambdaInfo> ChangeViewAsync(string privateKey, EditorView view, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await LambdaGuard.RequireAsync(database, privateKey, cancellation);

        LambdaGuard.EnsureEditable(lambda, Limits.PolicyOf(lambda.Tier));

        if (lambda.View != view)
        {
            var previous = lambda.View;

            lambda.View = view;
            lambda.Modified = DateTime.UtcNow;

            await database.SaveChangesAsync(cancellation);

            Logger.LogInformation("Changed view of lambda {Lambda} #{LambdaId} from {Previous} to {View}",
                                  lambda.PublicKey, lambda.Id, previous, view);
        }

        return await Describer.DescribeAsync(database, lambda, CancellationToken.None);
    }

    #endregion

    #region Lifecycle

    public async ValueTask<LambdaInfo> CreateAsync(string? publicKey, string? template = null, EditorView view = EditorView.Full, CancellationToken cancellation = default)
    {
        var requested = !string.IsNullOrWhiteSpace(publicKey);

        if (template != null && !TemplateCatalog.Exists(template))
        {
            throw LambdaException.Invalid($"There is nothing called '{template}' to start from. Leave it out for an empty lambda, or name a demo: {string.Join(", ", DemoCatalog.All.Select(d => d.Id))}.");
        }

        if (requested)
        {
            if (!LambdaKeys.TryNormalize(publicKey, out var wanted, out var reason))
            {
                throw LambdaException.Invalid(reason!);
            }

            if (LambdaKeys.IsDemo(wanted))
            {
                throw LambdaException.Invalid(LambdaKeys.DemoReason);
            }
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
                Tier = Limits.Get().DefaultTier,
                View = view,
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

            // the lambda is there from here on, so what it starts with is
            // written whoever cancels
            await SeedAsync(database, entity, template, now);

            // after the insert rather than before: the id the event refers to
            // is the one the database just handed out
            await History.RecordAsync(database, entity, LambdaEvents.Created);

            await database.SaveChangesAsync(CancellationToken.None);

            // a copy of a demo that keeps its records in a database starts
            // with one, or the first thing it does is fail for want of it
            if (DemoCatalog.Find(template)?.Database == true)
            {
                // the lambda is there from here on, so its database is made whoever cancels
                await DatabaseVault.CreateAsync(entity.Id, CancellationToken.None);
            }

            Logger.LogInformation("Created lambda {Lambda} #{LambdaId}", entity.PublicKey, entity.Id);

            return await Describer.DescribeAsync(database, entity, CancellationToken.None);
        }

        throw LambdaException.Conflict("Unable to find a free key, please try again.");
    }

    private async ValueTask SeedAsync(LambdaDbContext database, LambdaEntity lambda, string? template, DateTime now)
    {
        var demo = DemoCatalog.Find(template);

        var note = new VersionNote(Change: demo != null ? $"Started as a copy of the demo '{demo.Id}'" : "Started empty",
                                   Origin: VersionOrigins.Template);

        await History.AppendAsync(database, lambda, TemplateCatalog.ForKey(template, lambda.PublicKey), now, note);

        if (demo?.Database == true)
        {
            database.DataStores.Add(new DataStoreEntity { LambdaId = lambda.Id, Kind = DataKinds.DatabaseId, Enabled = true, Changed = now });
        }
    }

    public async ValueTask DeleteAsync(string privateKey, string? origin = null)
    {
        await using var database = await Databases.CreateDbContextAsync();

        var lambda = await LambdaGuard.RequireAsync(database, privateKey);

        var policy = Limits.PolicyOf(lambda.Tier);

        // the operator may delete what they blocked, which is one of the two
        // ways out of a block, and a system lambda, which nobody else may;
        // a demo stays the installation's either way
        if (origin == VersionOrigins.Admin && !policy.ReadOnly)
        {
            await Removal.RemoveAsync(database, lambda);

            Logger.LogInformation("Deleted lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);

            return;
        }

        LambdaGuard.EnsureEditable(lambda, policy);

        if (!policy.DeletedByOwner)
        {
            throw LambdaException.Forbidden($"'{lambda.PublicKey}' is one of the installation's own lambdas ({policy.Name}), which the platform depends on: "
                                          + "it is not deleted with its editor key. Only the operator of the installation can delete it.");
        }

        await Removal.RemoveAsync(database, lambda);

        Logger.LogInformation("Deleted lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);
    }

    #endregion

    #region Reading

    public async ValueTask<LambdaInfo?> GetAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.FirstOrDefaultAsync(l => l.PrivateKey == privateKey, cancellation);

        return lambda == null ? null : await Describer.DescribeAsync(database, lambda, cancellation);
    }

    /// <remarks>
    /// Asked on every request a lambda serves, so it is answered from memory
    /// (see <see cref="ResolutionCache{TKey}"/>) - a task that has completed,
    /// without a query - and only the first request after a write asks the
    /// database, which over SQLite answers on the thread that asks as well.
    /// No cancellation token: what is loaded is kept for every request after.
    /// </remarks>
    public ValueTask<ResolvedLambda?> ResolveAsync(string publicKey) => ByKey.ResolveAsync(publicKey, LoadByKey);

    public ValueTask<ResolvedLambda?> ResolveAsync(long id) => ById.ResolveAsync(id, LoadById);

    private async ValueTask<ResolvedLambda?> LoadAsync(Func<LambdaDbContext, Task<LambdaEntity?>> find)
    {
        await using var database = await Databases.CreateDbContextAsync();

        var lambda = await find(database);

        // blocking takes it offline as well; this is for a deployment that
        // was being written down at the very moment the block was
        if (lambda?.ActiveVersion == null || lambda.Blocked != null)
        {
            return null;
        }

        var deployment = await database.Deployments.AsNoTracking()
                                       .FirstOrDefaultAsync(d => d.LambdaId == lambda.Id && d.Version == lambda.ActiveVersion);

        if (deployment == null)
        {
            return null;
        }

        var workspace = await DataSwitches.IsEnabledAsync(database, lambda.Id, DataKinds.Workspace);

        return new ResolvedLambda(lambda.Id, lambda.PublicKey, lambda.Tier, deployment.Version, deployment.Created, workspace,
                                  AnalyticsSince: lambda.AnalyticsSince, Connectivity: ConnectivityRule.Allows(Limits.PolicyOf(lambda.Tier), lambda.Connectivity));
    }

    public async ValueTask<IReadOnlyList<LambdaVersionInfo>> GetVersionsAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await LambdaGuard.RequireAsync(database, privateKey, cancellation);

        return await database.Deployments.AsNoTracking()
                             .Where(d => d.LambdaId == lambda.Id)
                             .OrderByDescending(d => d.Version)
                             .Select(d => new LambdaVersionInfo(d.Version, d.Created, d.Specification, d.Change, d.Origin))
                             .ToListAsync(cancellation);
    }

    public async ValueTask<LambdaVersionContent> GetVersionAsync(string privateKey, int version, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await LambdaGuard.RequireAsync(database, privateKey, cancellation);

        var deployment = await database.Deployments.AsNoTracking()
                                       .FirstOrDefaultAsync(d => d.LambdaId == lambda.Id && d.Version == version, cancellation)
                      ?? throw LambdaException.NotFound($"Version {version} does not exist.");

        var files = Storage.Read(lambda.Id, version)
                 ?? throw LambdaException.NotFound($"The code of version {version} is no longer available.");

        return new LambdaVersionContent(deployment.Version, deployment.Created, files, deployment.Specification, deployment.Change, deployment.Origin);
    }

    public async ValueTask<IReadOnlyList<LambdaActivation>> GetActivationsAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await LambdaGuard.RequireAsync(database, privateKey, cancellation);

        return await database.Activations.AsNoTracking()
                             .Where(a => a.LambdaId == lambda.Id)
                             .OrderByDescending(a => a.Started)
                             .ThenByDescending(a => a.Id)
                             .Select(a => new LambdaActivation(a.Version, a.Started, a.Origin, a.Ended, a.EndedBy))
                             .ToListAsync(cancellation);
    }

    public VersionFacts GetFacts(long lambdaId, int? version) => Facts.Of(lambdaId, version);

    public ValueTask<EventHistory> GetEventsAsync(int days, CancellationToken cancellation = default) => Events.HistoryAsync(days, cancellation);

    public ValueTask<LambdaPage> ListAsync(LambdaQuery query, int skip = 0, int take = int.MaxValue, CancellationToken cancellation = default)
        => Listing.ListAsync(query, skip, take, cancellation);

    public async ValueTask<LambdaUses> GetUsesAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.AsNoTracking().FirstOrDefaultAsync(l => l.PrivateKey == privateKey, cancellation)
                     ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        return (await Listing.UsesOfAsync(database, [lambda], cancellation))[lambda.Id];
    }

    public async ValueTask<LambdaCounts> CountAsync(CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        // one after another: a context answers one query at a time
        var lambdas = await database.Lambdas.CountAsync(cancellation);
        var deployed = await database.Lambdas.CountAsync(l => l.ActiveVersion != null, cancellation);
        var versions = await database.Deployments.CountAsync(cancellation);

        return new LambdaCounts(lambdas, deployed, versions);
    }

    #endregion

    #region Editing

    /// <remarks>
    /// Kept before the lambda's turn is taken, and written once it is: what a
    /// version holds that the lambda does not keep yet can be a hundred
    /// megabytes, which a save of the same lambda would otherwise wait for
    /// (see <see cref="_turns"/>). What would be refused is refused before
    /// anything is kept, and checked again in the turn.
    /// </remarks>
    public async ValueTask<LambdaVersionInfo> SaveAsync(string privateKey, IReadOnlyList<VersionFile> files, VersionNote? note = null, int? after = null,
                                                        CancellationToken cancellation = default)
    {
        VersionInput.Validate(files);

        note ??= new VersionNote(Origin: VersionOrigins.Api);

        long id;

        await using (var database = await Databases.CreateDbContextAsync(cancellation))
        {
            var lambda = await LambdaGuard.RequireAsync(database, privateKey, cancellation);

            LambdaGuard.EnsureEditable(lambda, Limits.PolicyOf(lambda.Tier), note.Origin);

            VersionInput.ValidateAllowance(files, lambda.Tier, Limits);

            id = lambda.Id;
        }

        var kept = await Storage.KeepAsync(id, files, cancellation);

        await using var context = await Databases.CreateDbContextAsync(cancellation);

        var (locked, held) = await LockedAsync(context, privateKey, cancellation);

        using var turn = held;

        LambdaGuard.EnsureEditable(locked, Limits.PolicyOf(locked.Tier), note.Origin);

        if (after is { } expected)
        {
            var newest = await context.Deployments.Where(d => d.LambdaId == locked.Id)
                                      .MaxAsync(d => (int?)d.Version, cancellation);

            if (newest != expected)
            {
                throw LambdaException.Conflict($"Version {newest} was saved in the meantime; this was meant to follow version {expected}.");
            }
        }

        // moved to another tier meanwhile, perhaps
        VersionInput.ValidateAllowance(kept, locked.Tier, Limits);

        // written whoever cancels from here on (see LambdaHistory)
        var version = await History.AppendAsync(context, locked, kept, DateTime.UtcNow, note);

        Logger.LogInformation("Saved lambda #{LambdaId} version {Version}", locked.Id, version.Version);

        return version;
    }

    public async ValueTask<CompilationOutcome> CheckAsync(string privateKey, IReadOnlyList<VersionFile> files, CancellationToken cancellation = default)
    {
        VersionInput.Validate(files);

        long id;

        BuildSettings settings;

        await using (var database = await Databases.CreateDbContextAsync(cancellation))
        {
            var lambda = await LambdaGuard.RequireAsync(database, privateKey, cancellation);

            VersionInput.ValidateAllowance(files, lambda.Tier, Limits);

            (id, settings) = (lambda.Id, await SettingsOfAsync(database, lambda, cancellation));
        }

        return await Deployments.ValidateAsync(files, id, settings, cancellation);
    }

    /// <remarks>
    /// Compiled before the lambda's turn is taken, and written once it is:
    /// compiling takes seconds, which a save of the same lambda would wait
    /// for (see <see cref="_turns"/>). Two deployments of one lambda can
    /// therefore finish in either order. What is served stays what the
    /// database says is online all the same, because a request compares the
    /// build it finds with the version online and builds again where they
    /// differ.
    /// </remarks>
    public async ValueTask<DeploymentResult> DeployAsync(string privateKey, int? version, string? origin = null, CancellationToken cancellation = default)
    {
        long id;

        string publicKey;

        int target;

        BuildSettings settings;

        await using (var database = await Databases.CreateDbContextAsync(cancellation))
        {
            var lambda = await LambdaGuard.RequireAsync(database, privateKey, cancellation);

            LambdaGuard.EnsureEditable(lambda, Limits.PolicyOf(lambda.Tier), origin);

            target = version ?? await database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                              .MaxAsync(d => (int?)d.Version, cancellation)
                  ?? throw LambdaException.Invalid("There is nothing to deploy yet, save the code first.");

            if (!await database.Deployments.AnyAsync(d => d.LambdaId == lambda.Id && d.Version == target, cancellation))
            {
                throw LambdaException.NotFound($"Version {target} does not exist.");
            }

            (id, publicKey, settings) = (lambda.Id, lambda.PublicKey, await SettingsOfAsync(database, lambda, cancellation));
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
               ? LambdaOutput.Enter(new OutputScope(publicKey, Book, Options.MaxOutputLines, id))
               : null)
        {
            outcome = await Deployments.ActivateAsync(id, target, settings, cancellation);
        }

        return await ActivatedAsync(privateKey, target, origin, outcome);
    }

    /// <summary>
    /// Writes down what a deployment came to, in the lambda's turn.
    /// </summary>
    /// <remarks>
    /// Without a cancellation token: what was built is written down once it
    /// was, or the version online and what serves it would differ.
    /// </remarks>
    private async ValueTask<DeploymentResult> ActivatedAsync(string privateKey, int target, string? origin, CompilationOutcome outcome)
    {
        await using var database = await Databases.CreateDbContextAsync();

        var (lambda, held) = await LockedAsync(database, privateKey);

        using var turn = held;

        // blocked while it was being compiled: what was built never goes online
        if (lambda.Blocked != null)
        {
            Deployments.Evict(lambda.Id);

            LambdaGuard.EnsureUnblocked(lambda);
        }

        if (!outcome.Success)
        {
            Logger.LogInformation("Rejected deployment of lambda #{LambdaId} errors {Count}", lambda.Id, outcome.Diagnostics.Count);

            return new DeploymentResult(false, await Describer.DescribeAsync(database, lambda), outcome.Diagnostics);
        }

        // pruned while it was being compiled, by saves landing in between
        if (!await database.Deployments.AnyAsync(d => d.LambdaId == lambda.Id && d.Version == target))
        {
            throw LambdaException.NotFound($"Version {target} does not exist anymore.");
        }

        var now = DateTime.UtcNow;

        lambda.ActiveVersion = target;
        lambda.Deployed = now;
        lambda.Modified = now;

        await History.ActivateAsync(database, lambda.Id, target, now, origin ?? VersionOrigins.Api);

        await History.RecordAsync(database, lambda, LambdaEvents.Deployed);

        await database.SaveChangesAsync();

        await History.PruneActivationsAsync(database, lambda.Id);

        return new DeploymentResult(true, await Describer.DescribeAsync(database, lambda), outcome.Diagnostics);
    }

    /// <remarks>
    /// Without a cancellation token: what is let go of is written down whoever
    /// leaves, so the lambda is not left online with nothing serving it.
    /// </remarks>
    public async ValueTask<LambdaInfo> UndeployAsync(string privateKey, string? endedBy = null)
    {
        await using var database = await Databases.CreateDbContextAsync();

        var (lambda, held) = await LockedAsync(database, privateKey);

        using var turn = held;

        LambdaGuard.EnsureEditable(lambda, Limits.PolicyOf(lambda.Tier), endedBy == ActivationEndings.Admin ? VersionOrigins.Admin : null);

        if (lambda.ActiveVersion != null)
        {
            lambda.ActiveVersion = null;
            lambda.Deployed = null;

            await History.CloseAsync(database, lambda.Id, DateTime.UtcNow, endedBy ?? ActivationEndings.Stopped);

            await History.RecordAsync(database, lambda, LambdaEvents.Undeployed);

            await database.SaveChangesAsync();

            Deployments.Evict(lambda.Id);

            Logger.LogInformation("Undeployed lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);
        }

        return await Describer.DescribeAsync(database, lambda);
    }

    #endregion

    #region Blocking

    /// <remarks>
    /// Without a cancellation token: a block half written would leave the
    /// lambda frozen and its drafts online, or the other way round.
    /// </remarks>
    public async ValueTask<LambdaInfo> BlockAsync(string privateKey, string? reason)
    {
        await using var database = await Databases.CreateDbContextAsync();

        var (lambda, held) = await LockedAsync(database, privateKey);

        using var turn = held;

        var policy = Limits.PolicyOf(lambda.Tier);

        // the installation's own: a demo nobody else can change, a system
        // lambda the platform depends on - which an operator who means to
        // stop it moves to another tier first, as a decision of its own
        if (!policy.Blockable)
        {
            throw LambdaException.Forbidden(policy.ReadOnly
                ? $"'{lambda.PublicKey}' is a demo, which the installation keeps and nobody else can change: there is nothing to block."
                : $"'{lambda.PublicKey}' is in the tier '{policy.Name}', which the platform depends on and which cannot be blocked. Move it to another tier first to block it.");
        }

        var now = DateTime.UtcNow;

        // blocked again: since when stays, the reason is the newest
        lambda.Blocked ??= now;
        lambda.BlockReason = VersionInput.Tidy(reason, MaxBlockReason);

        if (lambda.ActiveVersion != null)
        {
            lambda.ActiveVersion = null;
            lambda.Deployed = null;

            await History.CloseAsync(database, lambda.Id, now, ActivationEndings.Admin);

            await History.RecordAsync(database, lambda, LambdaEvents.Undeployed);
        }

        // its drafts run its code as well, at addresses of their own
        foreach (var feature in await database.Features.Where(f => f.LambdaId == lambda.Id && f.Previewed != null).ToListAsync())
        {
            feature.Previewed = null;
            feature.Modified = now;
        }

        await database.SaveChangesAsync();

        Deployments.EvictAll(lambda.Id);

        Logger.LogInformation("Blocked lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);

        return await Describer.DescribeAsync(database, lambda);
    }

    public async ValueTask<LambdaInfo> UnblockAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var (lambda, held) = await LockedAsync(database, privateKey, cancellation);

        using var turn = held;

        if (lambda.Blocked != null)
        {
            lambda.Blocked = null;
            lambda.BlockReason = null;

            // the time it was blocked does not count against a sandbox lambda:
            // it gets the whole of its time again from here
            lambda.Modified = DateTime.UtcNow;

            await database.SaveChangesAsync(cancellation);

            Logger.LogInformation("Unblocked lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);
        }

        return await Describer.DescribeAsync(database, lambda, CancellationToken.None);
    }

    public async ValueTask<IReadOnlyList<EditorAddress>> GetEditorAddressesAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await LambdaGuard.RequireAsync(database, privateKey, cancellation);

        return await Editors.OfAsync(database, lambda.Id, cancellation);
    }

    /// <summary>
    /// How long the operator's note on a block may be.
    /// </summary>
    private const int MaxBlockReason = 500;

    #endregion

    #region Maintenance

    public async ValueTask<MaintenanceReport> RunMaintenanceAsync(DateTime now, CancellationToken cancellation = default)
    {
        var report = await Lifetime.SweepAsync(now, cancellation);

        await using var database = await Databases.CreateDbContextAsync(cancellation);

        await Editors.PruneAsync(database, now, cancellation);

        return report;
    }

    #endregion

    #region Lookups

    public async ValueTask<long> RequireEditableAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.AsNoTracking().FirstOrDefaultAsync(l => l.PrivateKey == privateKey, cancellation)
                  ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        LambdaGuard.EnsureEditable(lambda, Limits.PolicyOf(lambda.Tier));

        await Editors.NoteAsync(lambda.Id);

        return lambda.Id;
    }

    public async ValueTask<long?> GetIdAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        return await database.Lambdas.AsNoTracking()
                             .Where(l => l.PrivateKey == privateKey)
                             .Select(l => (long?)l.Id)
                             .FirstOrDefaultAsync(cancellation);
    }

    public async ValueTask<LambdaTier?> GetTierAsync(long lambdaId, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        return await database.Lambdas.AsNoTracking()
                             .Where(l => l.Id == lambdaId)
                             .Select(l => (LambdaTier?)l.Tier)
                             .FirstOrDefaultAsync(cancellation);
    }

    public async ValueTask<WorkspaceLimits?> GetWorkspaceLimitsAsync(long lambdaId, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        var tier = await database.Lambdas.AsNoTracking()
                                 .Where(l => l.Id == lambdaId)
                                 .Select(l => (LambdaTier?)l.Tier)
                                 .FirstOrDefaultAsync(cancellation);

        if (tier == null)
        {
            return null;
        }

        return Limits.WorkspaceOf(tier.Value, await DataSwitches.IsEnabledAsync(database, lambdaId, DataKinds.Workspace, cancellation));
    }

    public async ValueTask<string?> GetPrivateKeyAsync(string publicKey, CancellationToken cancellation = default)
    {
        if (!LambdaKeys.TryRead(publicKey, out var normalized))
        {
            return null;
        }

        await using var database = await Databases.CreateDbContextAsync(cancellation);

        return await database.Lambdas.AsNoTracking()
                             .Where(l => l.PublicKey == normalized)
                             .Select(l => l.PrivateKey)
                             .FirstOrDefaultAsync(cancellation);
    }

    public async ValueTask<string?> GetPublicKeyAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await Databases.CreateDbContextAsync(cancellation);

        return await database.Lambdas.AsNoTracking()
                             .Where(l => l.PrivateKey == privateKey)
                             .Select(l => l.PublicKey)
                             .FirstOrDefaultAsync(cancellation);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// What the lambda is compiled with: whether it has a workspace, and
    /// whether it may connect to other servers.
    /// </summary>
    private async ValueTask<BuildSettings> SettingsOfAsync(LambdaDbContext database, LambdaEntity lambda, CancellationToken cancellation)
        => new(await DataSwitches.IsEnabledAsync(database, lambda.Id, DataKinds.Workspace, cancellation),
               ConnectivityRule.Allows(Limits.PolicyOf(lambda.Tier), lambda.Connectivity));

    /// <summary>
    /// The lambda of the key, read once it is its turn to change (see <see cref="_turns" />).
    /// </summary>
    /// <param name="cancellation">Stops it before the turn is taken; the turn itself is waited for whoever cancels</param>
    /// <returns>The lambda, and the turn - to be disposed of once the change is written</returns>
    private async ValueTask<(LambdaEntity Lambda, IDisposable Turn)> LockedAsync(LambdaDbContext database, string privateKey, CancellationToken cancellation = default)
    {
        var id = await database.Lambdas.AsNoTracking()
                               .Where(l => l.PrivateKey == privateKey)
                               .Select(l => (long?)l.Id)
                               .FirstOrDefaultAsync(cancellation)
              ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        var turn = await _turns.TakeAsync(id);

        try
        {
            // read after waiting, so what it decides on is what the change
            // before it left behind
            return (await LambdaGuard.RequireAsync(database, privateKey, cancellation), turn);
        }
        catch
        {
            turn.Dispose();
            throw;
        }
    }

    #endregion

}
