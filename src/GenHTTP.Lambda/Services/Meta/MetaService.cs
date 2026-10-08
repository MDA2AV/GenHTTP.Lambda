using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The meta data side of a lambda: keys, versions and which of them is live.
/// Code itself is handed to the storage service, building it to the deployment service.
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
    /// Never held across an await, which is why a deployment compiles before
    /// it takes one (see <see cref="DeployAsync"/>).
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

    private VersionFactsCache Facts { get; }

    private EventReader Events { get; }

    private ILogger Logger { get; }

    /// <summary>
    /// The lambdas being served, by the key in their path.
    /// </summary>
    private ResolutionCache<string> ByKey { get; }

    /// <summary>
    /// The lambdas being served, by id - how a lambda's own domain finds it.
    /// </summary>
    private ResolutionCache<long> ById { get; }

    #endregion

    #region Initialization

    public MetaService(IDbContextFactory<LambdaDbContext> databases, IStorageService storage, IDeploymentService deployments,
        LambdaOptions options, ILimitsService limits, ILogBook book, IDatabaseVault databaseVault, LambdaHosting hosting, LambdaHistory history,
        LambdaLifetime lifetime, LambdaRemoval removal, LambdaDescriber describer, VersionFactsCache facts, EventReader events,
        DatabaseChanges changes, ILogger<MetaService> logger)
    {
        ByKey = new ResolutionCache<string>(changes);
        ById = new ResolutionCache<long>(changes);

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
        Facts = facts;
        Events = events;
        Logger = logger;
    }

    #endregion

    #region Settings

    public KeyStatus DescribeKey(string? publicKey) => Hosting.DescribeKey(publicKey);

    public LambdaInfo ChangeKey(string privateKey, string? publicKey) => Hosting.ChangeKey(privateKey, publicKey);

    public LambdaInfo ChangeDomain(string privateKey, string? domain) => Hosting.ChangeDomain(privateKey, domain);

    public LambdaInfo ChangeTier(string privateKey, LambdaTier tier) => Hosting.ChangeTier(privateKey, tier);

    public LambdaInfo ChangeView(string privateKey, EditorView view)
    {
        using var database = Databases.CreateDbContext();

        var lambda = LambdaGuard.Require(database, privateKey);

        LambdaGuard.EnsureEditable(lambda);

        if (lambda.View != view)
        {
            var previous = lambda.View;

            lambda.View = view;
            lambda.Modified = DateTime.UtcNow;

            database.SaveChanges();

            Logger.LogInformation("Changed view of lambda {Lambda} #{LambdaId} from {Previous} to {View}",
                                  lambda.PublicKey, lambda.Id, previous, view);
        }

        return Describer.Describe(database, lambda);
    }

    #endregion

    #region Lifecycle

    public LambdaInfo Create(string? publicKey, string? template = null, EditorView view = EditorView.Full)
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

        using var database = Databases.CreateDbContext();

        var now = DateTime.UtcNow;

        for (var attempt = 0; attempt < KeyAttempts; attempt++)
        {
            LambdaKeys.TryNormalize(requested ? publicKey : LambdaKeys.CreatePublicKey(), out var key, out _);

            var entity = new LambdaEntity
            {
                PublicKey = key,
                PrivateKey = LambdaKeys.CreatePrivateKey(),
                Tier = LambdaTier.Free,
                View = view,
                Created = now,
                Modified = now
            };

            database.Lambdas.Add(entity);

            try
            {
                database.SaveChanges();
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

            Seed(database, entity, template, now);

            // after the insert rather than before: the id the event refers to
            // is the one the database just handed out
            History.Record(database, entity, LambdaEvents.Created);

            database.SaveChanges();

            // a copy of a demo that keeps its records in a database starts
            // with one, or the first thing it does is fail for want of it
            if (DemoCatalog.Find(template)?.Database == true)
            {
                DatabaseVault.Create(entity.Id);
            }

            Logger.LogInformation("Created lambda {Lambda} #{LambdaId}", entity.PublicKey, entity.Id);

            return Describer.Describe(database, entity);
        }

        throw LambdaException.Conflict("Unable to find a free key, please try again.");
    }

    private void Seed(LambdaDbContext database, LambdaEntity lambda, string? template, DateTime now)
    {
        var demo = DemoCatalog.Find(template);

        var note = new VersionNote(Change: demo != null ? $"Started as a copy of the demo '{demo.Id}'" : "Started empty",
                                   Origin: VersionOrigins.Template);

        History.Append(database, lambda, TemplateCatalog.ForKey(template, lambda.PublicKey), now, note);

        if (demo?.Database == true)
        {
            database.DataStores.Add(new DataStoreEntity { LambdaId = lambda.Id, Kind = DataKinds.DatabaseId, Enabled = true, Changed = now });
        }
    }

    public void Delete(string privateKey)
    {
        using var database = Databases.CreateDbContext();

        var lambda = LambdaGuard.Require(database, privateKey);

        LambdaGuard.EnsureEditable(lambda);

        Removal.Remove(database, lambda);

        Logger.LogInformation("Deleted lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);
    }

    #endregion

    #region Reading

    public LambdaInfo? Get(string privateKey)
    {
        using var database = Databases.CreateDbContext();

        var lambda = database.Lambdas.FirstOrDefault(l => l.PrivateKey == privateKey);

        return lambda == null ? null : Describer.Describe(database, lambda);
    }

    /// <remarks>
    /// Asked on every request a lambda serves, so it is answered from memory
    /// (see <see cref="ResolutionCache{TKey}"/>) and, where it has to ask the
    /// database, asks synchronously: SQLite answers synchronously either way,
    /// and the asynchronous path only adds to what the answer costs.
    /// </remarks>
    public ResolvedLambda? Resolve(string publicKey)
        => ByKey.Resolve(publicKey, key => Resolve(database => database.Lambdas.AsNoTracking().FirstOrDefault(l => l.PublicKey == key)));

    public ResolvedLambda? Resolve(long id)
        => ById.Resolve(id, wanted => Resolve(database => database.Lambdas.AsNoTracking().FirstOrDefault(l => l.Id == wanted)));

    private ResolvedLambda? Resolve(Func<LambdaDbContext, LambdaEntity?> find)
    {
        using var database = Databases.CreateDbContext();

        var lambda = find(database);

        if (lambda?.ActiveVersion == null)
        {
            return null;
        }

        var deployment = database.Deployments.AsNoTracking()
                                 .FirstOrDefault(d => d.LambdaId == lambda.Id && d.Version == lambda.ActiveVersion);

        if (deployment == null)
        {
            return null;
        }

        var workspace = DataSwitches.IsEnabled(database, lambda.Id, DataKinds.Workspace);

        return new ResolvedLambda(lambda.Id, lambda.PublicKey, lambda.Tier, deployment.Version, deployment.Created, workspace);
    }

    public IReadOnlyList<LambdaVersionInfo> GetVersions(string privateKey)
    {
        using var database = Databases.CreateDbContext();

        var lambda = LambdaGuard.Require(database, privateKey);

        return database.Deployments.AsNoTracking()
                       .Where(d => d.LambdaId == lambda.Id)
                       .OrderByDescending(d => d.Version)
                       .Select(d => new LambdaVersionInfo(d.Version, d.Created, d.Specification, d.Change, d.Origin))
                       .ToList();
    }

    public LambdaVersionContent GetVersion(string privateKey, int version)
    {
        using var database = Databases.CreateDbContext();

        var lambda = LambdaGuard.Require(database, privateKey);

        var deployment = database.Deployments.AsNoTracking()
                                 .FirstOrDefault(d => d.LambdaId == lambda.Id && d.Version == version)
                      ?? throw LambdaException.NotFound($"Version {version} does not exist.");

        var code = Storage.Read(lambda.Id, version)
                ?? throw LambdaException.NotFound($"The code of version {version} is no longer available.");

        return new LambdaVersionContent(deployment.Version, deployment.Created, code, deployment.Specification, deployment.Change, deployment.Origin);
    }

    public IReadOnlyList<LambdaActivation> GetActivations(string privateKey)
    {
        using var database = Databases.CreateDbContext();

        var lambda = LambdaGuard.Require(database, privateKey);

        return database.Activations.AsNoTracking()
                       .Where(a => a.LambdaId == lambda.Id)
                       .OrderByDescending(a => a.Started)
                       .ThenByDescending(a => a.Id)
                       .Select(a => new LambdaActivation(a.Version, a.Started, a.Origin, a.Ended, a.EndedBy))
                       .ToList();
    }

    public VersionFacts GetFacts(long lambdaId, int? version) => Facts.Of(lambdaId, version);

    public EventHistory GetEvents(int days) => Events.History(days);

    public LambdaPage List(string? search = null, int skip = 0, int take = int.MaxValue, LambdaTier? tier = null)
    {
        using var database = Databases.CreateDbContext();

        var total = database.Lambdas.AsNoTracking().Count();

        var deployed = database.Lambdas.AsNoTracking().Count(l => l.ActiveVersion != null);

        var query = database.Lambdas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            // the key and the domain are what an administrator has to go on
            // that is not the code itself, and what the panel shows
            query = query.Where(l => EF.Functions.Like(l.PublicKey, $"%{term}%")
                                  || (l.Domain != null && EF.Functions.Like(l.Domain, $"%{term}%")));
        }

        if (tier is { } wanted)
        {
            query = query.Where(l => l.Tier == wanted);
        }

        var matched = query.Count();

        var lambdas = query.OrderByDescending(l => l.Created)
                           .Skip(skip)
                           .Take(take)
                           .ToList();

        var counts = database.Deployments.AsNoTracking()
                             .GroupBy(d => d.LambdaId)
                             .Select(g => new { LambdaId = g.Key, Count = g.Count() })
                             .ToDictionary(g => g.LambdaId, g => g.Count);

        var latest = database.Deployments.AsNoTracking()
                             .GroupBy(d => d.LambdaId)
                             .Select(g => new { LambdaId = g.Key, Version = g.Max(d => d.Version) })
                             .ToDictionary(g => g.LambdaId, g => (int?)g.Version);

        return new LambdaPage([.. lambdas.Select(l => Describer.Overview(l, latest.GetValueOrDefault(l.Id), counts.GetValueOrDefault(l.Id)))],
                              matched, total, deployed);
    }

    public LambdaCounts Count()
    {
        using var database = Databases.CreateDbContext();

        return new LambdaCounts(
            database.Lambdas.Count(),
            database.Lambdas.Count(l => l.ActiveVersion != null),
            database.Deployments.Count()
        );
    }

    #endregion

    #region Editing

    public LambdaVersionInfo Save(string privateKey, string code, VersionNote? note = null, int? after = null)
    {
        var files = VersionInput.Validate(code);

        using var database = Databases.CreateDbContext();

        var (lambda, held) = Locked(database, privateKey);

        using var turn = held;

        note ??= new VersionNote(Origin: VersionOrigins.Api);

        LambdaGuard.EnsureEditable(lambda, note.Origin);

        if (after is { } expected)
        {
            var newest = database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                 .Max(d => (int?)d.Version);

            if (newest != expected)
            {
                throw LambdaException.Conflict($"Version {newest} was saved in the meantime; this was meant to follow version {expected}.");
            }
        }

        VersionInput.ValidateAllowance(files, lambda.Tier, Limits);

        var version = History.Append(database, lambda, code, DateTime.UtcNow, note);

        Logger.LogInformation("Saved lambda #{LambdaId} version {Version}", lambda.Id, version.Version);

        return version;
    }

    public async ValueTask<CompilationOutcome> CheckAsync(string privateKey, string code, CancellationToken cancellation = default)
    {
        var files = VersionInput.Validate(code);

        long id;

        WorkspaceLimits limits;

        using (var database = Databases.CreateDbContext())
        {
            var lambda = LambdaGuard.Require(database, privateKey);

            VersionInput.ValidateAllowance(files, lambda.Tier, Limits);

            (id, limits) = (lambda.Id, WorkspaceOf(database, lambda));
        }

        return await Deployments.ValidateAsync(code, id, limits, cancellation);
    }

    /// <remarks>
    /// Compiled before the lambda's turn is taken, and written once it is: the
    /// turn is a lock, held by a thread, and compiling takes seconds away
    /// from it (see <see cref="_turns"/>). Two deployments of one lambda can
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

        WorkspaceLimits limits;

        using (var database = Databases.CreateDbContext())
        {
            var lambda = LambdaGuard.Require(database, privateKey);

            LambdaGuard.EnsureEditable(lambda, origin);

            target = version ?? database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                        .Max(d => (int?)d.Version)
                  ?? throw LambdaException.Invalid("There is nothing to deploy yet, save the code first.");

            if (!database.Deployments.Any(d => d.LambdaId == lambda.Id && d.Version == target))
            {
                throw LambdaException.NotFound($"Version {target} does not exist.");
            }

            (id, publicKey, limits) = (lambda.Id, lambda.PublicKey, WorkspaceOf(database, lambda));
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
            outcome = await Deployments.ActivateAsync(id, target, limits, cancellation);
        }

        return Activated(privateKey, target, origin, outcome);
    }

    /// <summary>
    /// Writes down what a deployment came to, in the lambda's turn.
    /// </summary>
    private DeploymentResult Activated(string privateKey, int target, string? origin, CompilationOutcome outcome)
    {
        using var database = Databases.CreateDbContext();

        var (lambda, held) = Locked(database, privateKey);

        using var turn = held;

        if (!outcome.Success)
        {
            Logger.LogInformation("Rejected deployment of lambda #{LambdaId} errors {Count}", lambda.Id, outcome.Diagnostics.Count);

            return new DeploymentResult(false, Describer.Describe(database, lambda), outcome.Diagnostics);
        }

        // pruned while it was being compiled, by saves landing in between
        if (!database.Deployments.Any(d => d.LambdaId == lambda.Id && d.Version == target))
        {
            throw LambdaException.NotFound($"Version {target} does not exist anymore.");
        }

        var now = DateTime.UtcNow;

        lambda.ActiveVersion = target;
        lambda.Deployed = now;
        lambda.Modified = now;

        History.Activate(database, lambda.Id, target, now, origin ?? VersionOrigins.Api);

        History.Record(database, lambda, LambdaEvents.Deployed);

        database.SaveChanges();

        History.PruneActivations(database, lambda.Id);

        return new DeploymentResult(true, Describer.Describe(database, lambda), outcome.Diagnostics);
    }

    public LambdaInfo Undeploy(string privateKey, string? endedBy = null)
    {
        using var database = Databases.CreateDbContext();

        var (lambda, held) = Locked(database, privateKey);

        using var turn = held;

        LambdaGuard.EnsureEditable(lambda, endedBy == ActivationEndings.Admin ? VersionOrigins.Admin : null);

        if (lambda.ActiveVersion != null)
        {
            lambda.ActiveVersion = null;
            lambda.Deployed = null;

            History.Close(database, lambda.Id, DateTime.UtcNow, endedBy ?? ActivationEndings.Stopped);

            History.Record(database, lambda, LambdaEvents.Undeployed);

            database.SaveChanges();

            Deployments.Evict(lambda.Id);

            Logger.LogInformation("Undeployed lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);
        }

        return Describer.Describe(database, lambda);
    }

    #endregion

    #region Maintenance

    public MaintenanceReport RunMaintenance(DateTime now) => Lifetime.Sweep(now);

    #endregion

    #region Lookups

    public long RequireEditable(string privateKey)
    {
        using var database = Databases.CreateDbContext();

        var lambda = database.Lambdas.AsNoTracking().FirstOrDefault(l => l.PrivateKey == privateKey)
                  ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        LambdaGuard.EnsureEditable(lambda);

        return lambda.Id;
    }

    public long? GetId(string privateKey)
    {
        using var database = Databases.CreateDbContext();

        return database.Lambdas.AsNoTracking()
                       .Where(l => l.PrivateKey == privateKey)
                       .Select(l => (long?)l.Id)
                       .FirstOrDefault();
    }

    public WorkspaceLimits? GetWorkspaceLimits(long lambdaId)
    {
        using var database = Databases.CreateDbContext();

        var tier = database.Lambdas.AsNoTracking()
                           .Where(l => l.Id == lambdaId)
                           .Select(l => (LambdaTier?)l.Tier)
                           .FirstOrDefault();

        if (tier == null)
        {
            return null;
        }

        return Limits.WorkspaceOf(tier.Value, DataSwitches.IsEnabled(database, lambdaId, DataKinds.Workspace));
    }

    public string? GetPrivateKey(string publicKey)
    {
        if (!LambdaKeys.TryNormalize(publicKey, out var normalized, out _))
        {
            return null;
        }

        using var database = Databases.CreateDbContext();

        return database.Lambdas.AsNoTracking()
                       .Where(l => l.PublicKey == normalized)
                       .Select(l => l.PrivateKey)
                       .FirstOrDefault();
    }

    public string? GetPublicKey(string privateKey)
    {
        using var database = Databases.CreateDbContext();

        return database.Lambdas.AsNoTracking()
                       .Where(l => l.PrivateKey == privateKey)
                       .Select(l => l.PublicKey)
                       .FirstOrDefault();
    }

    #endregion

    #region Helpers

    /// <summary>
    /// What the lambda may keep in its workspace, which is compiled into it.
    /// </summary>
    private WorkspaceLimits WorkspaceOf(LambdaDbContext database, LambdaEntity lambda)
        => Limits.WorkspaceOf(lambda.Tier, DataSwitches.IsEnabled(database, lambda.Id, DataKinds.Workspace));

    /// <summary>
    /// The lambda of the key, read once it is its turn to change (see <see cref="_turns" />).
    /// </summary>
    /// <returns>The lambda, and the turn - to be disposed of once the change is written</returns>
    private (LambdaEntity Lambda, IDisposable Turn) Locked(LambdaDbContext database, string privateKey)
    {
        var id = database.Lambdas.AsNoTracking()
                         .Where(l => l.PrivateKey == privateKey)
                         .Select(l => (long?)l.Id)
                         .FirstOrDefault()
              ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        var turn = _turns.Take(id);

        try
        {
            // read after waiting, so what it decides on is what the change
            // before it left behind
            return (LambdaGuard.Require(database, privateKey), turn);
        }
        catch
        {
            turn.Dispose();
            throw;
        }
    }

    #endregion

}
