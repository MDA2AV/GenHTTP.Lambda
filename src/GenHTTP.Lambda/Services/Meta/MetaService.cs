using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;

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

    /// <summary>
    /// Takes turns over what adds a version to a lambda or changes what it has
    /// online, one lambda at a time.
    /// </summary>
    /// <remarks>
    /// Two saves at once used to be able to pick the same number for their
    /// version, and a feature being merged has to find the newest version to
    /// be the one it is based on at the moment its own is added - not a moment
    /// before, with somebody else's save landing in between. Striped rather
    /// than one per lambda so it does not grow with them; two lambdas sharing
    /// a stripe merely wait for each other.
    ///
    /// Plain locks, held only while the change is read and written - never
    /// across an await, which is why a deployment compiles before it takes
    /// one (see <see cref="DeployAsync"/>). A lock is held by a thread, and a
    /// reactor that awaited under one would hand it to whatever request it
    /// resumed next.
    /// </remarks>
    private readonly Lock[] _stripes = [.. Enumerable.Range(0, 64).Select(_ => new Lock())];

    #region Get-/Setters

    private IDbContextFactory<LambdaDbContext> Databases { get; }

    private IStorageService Storage { get; }

    private IDeploymentService Deployments { get; }

    private LambdaTelemetry Activity { get; }

    private LambdaOptions Options { get; }

    private LimitsService Limits { get; }

    private LogBook Book { get; }

    private DomainRegistry Domains { get; }

    private SecretVault Secrets { get; }

    private DatabaseVault DatabaseVault { get; }

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
        LambdaTelemetry activity, LambdaOptions options, LimitsService limits, LogBook book, DomainRegistry domains, SecretVault secrets, DatabaseVault databaseVault,
        DatabaseChanges changes, ILogger<MetaService> logger)
    {
        ByKey = new ResolutionCache<string>(changes);
        ById = new ResolutionCache<long>(changes);

        Domains = domains;
        Secrets = secrets;
        DatabaseVault = databaseVault;
        Databases = databases;
        Storage = storage;
        Deployments = deployments;
        Activity = activity;
        Options = options;
        Limits = limits;
        Book = book;
        Logger = logger;
    }

    #endregion

    #region Keys

    public KeyStatus DescribeKey(string? publicKey)
    {
        // looked up even when it could not be claimed: a key that is refused
        // today may still belong to a lambda from before the rule was made
        var valid = LambdaKeys.TryNormalize(publicKey, out var normalized, out var reason);

        using var database = Databases.CreateDbContext();

        var lambda = database.Lambdas.AsNoTracking()
                             .Where(l => l.PublicKey == normalized)
                             .Select(l => new { l.ActiveVersion })
                             .FirstOrDefault();

        if (valid && lambda != null)
        {
            reason = "This key is already in use.";
        }
        else if (valid && LambdaKeys.IsDemo(normalized))
        {
            // said here as well as refused on creation, so the page that
            // checks a key while it is typed says why before anybody submits
            valid = false;
            reason = LambdaKeys.DemoReason;
        }

        return new KeyStatus(normalized, valid, lambda != null, lambda?.ActiveVersion != null, reason);
    }

    public LambdaInfo ChangeKey(string privateKey, string? publicKey)
    {
        if (!LambdaKeys.TryNormalize(publicKey, out var normalized, out var reason))
        {
            throw LambdaException.Invalid(reason!);
        }

        using var database = Databases.CreateDbContext();

        var lambda = Require(database, privateKey);

        EnsureEditable(lambda);

        if (lambda.PublicKey == normalized)
        {
            return Describe(database, lambda);
        }

        if (LambdaKeys.IsDemo(normalized))
        {
            throw LambdaException.Invalid(LambdaKeys.DemoReason);
        }

        if (database.Lambdas.Any(l => l.PublicKey == normalized))
        {
            throw LambdaException.Conflict("This key is already in use.");
        }

        var previous = lambda.PublicKey;

        lambda.PublicKey = normalized;
        lambda.Modified = DateTime.UtcNow;

        database.SaveChanges();

        Logger.LogInformation("Changed public key of lambda #{LambdaId} from {Previous} to {Lambda}", lambda.Id, previous, normalized);

        return Describe(database, lambda);
    }

    public LambdaInfo ChangeView(string privateKey, EditorView view)
    {
        using var database = Databases.CreateDbContext();

        var lambda = Require(database, privateKey);

        EnsureEditable(lambda);

        if (lambda.View != view)
        {
            var previous = lambda.View;

            lambda.View = view;
            lambda.Modified = DateTime.UtcNow;

            database.SaveChanges();

            Logger.LogInformation("Changed view of lambda {Lambda} #{LambdaId} from {Previous} to {View}",
                                  lambda.PublicKey, lambda.Id, previous, view);
        }

        return Describe(database, lambda);
    }

    #endregion

    #region Hosting

    public LambdaInfo ChangeTier(string privateKey, LambdaTier tier)
    {
        using var database = Databases.CreateDbContext();

        var lambda = Require(database, privateKey);

        if (lambda.Tier != tier && (tier == LambdaTier.Demo || lambda.Tier == LambdaTier.Demo))
        {
            // the seeder retires every demo it no longer knows, so a lambda
            // moved in by hand would be deleted on the next start; one moved
            // out would be moved back
            throw LambdaException.Forbidden("The demo tier belongs to the installation's own demos. Nothing is moved into it or out of it by hand.");
        }

        if (lambda.Tier != tier)
        {
            var previous = lambda.Tier;

            lambda.Tier = tier;
            lambda.Modified = DateTime.UtcNow;

            database.SaveChanges();

            // how large its database may grow is its tier's to say, from the
            // next connection on
            DatabaseVault.Invalidate(lambda.Id);

            // a domain is served or not by the tier, so the tier moving can
            // take one on or off the air without the domain itself changing
            Domains.Reload();

            Logger.LogInformation("Changed tier of lambda {Lambda} #{LambdaId} from {Previous} to {Tier}",
                                  lambda.PublicKey, lambda.Id, previous, tier);
        }

        return Describe(database, lambda);
    }

    public LambdaInfo ChangeDomain(string privateKey, string? domain)
    {
        string? normalized = null;

        if (!string.IsNullOrWhiteSpace(domain) && !DomainNames.TryNormalize(domain, Options, out normalized, out var reason))
        {
            throw LambdaException.Invalid(reason!);
        }

        using var database = Databases.CreateDbContext();

        var lambda = Require(database, privateKey);

        EnsureEditable(lambda);

        if (lambda.Domain == normalized)
        {
            return Describe(database, lambda);
        }

        if (normalized != null)
        {
            if (lambda.Tier != LambdaTier.Premium)
            {
                throw LambdaException.Forbidden("A domain of its own is part of the premium tier, which this lambda is not in.");
            }

            if (database.Lambdas.Any(l => l.Domain == normalized && l.Id != lambda.Id))
            {
                throw LambdaException.Conflict("This domain is already used by another lambda.");
            }
        }

        var previous = lambda.Domain;

        lambda.Domain = normalized;
        lambda.Modified = DateTime.UtcNow;

        try
        {
            database.SaveChanges();
        }
        catch (DbUpdateException)
        {
            // claimed by somebody else between the check above and now
            throw LambdaException.Conflict("This domain is already used by another lambda.");
        }

        Domains.Reload();

        Logger.LogInformation("Changed domain of lambda {Lambda} #{LambdaId} from {Previous} to {Domain}",
                              lambda.PublicKey, lambda.Id, previous ?? "(none)", normalized ?? "(none)");

        return Describe(database, lambda);
    }

    /// <summary>
    /// Whether the tier of a lambda keeps it, rather than the sweeps deciding.
    /// </summary>
    /// <remarks>
    /// Demos because they are the installation's own; premium lambdas
    /// because they answer at somebody's domain, and a site going offline
    /// because it had a quiet month is not something anybody would pay for.
    /// </remarks>
    private static bool Kept(LambdaEntity lambda) => lambda.Tier is LambdaTier.Demo or LambdaTier.Premium;

    /// <summary>
    /// Refuses a change to a demo unless the installation itself is making it.
    /// </summary>
    /// <remarks>
    /// The editor key of a demo is announced so that anybody can read it, which
    /// makes holding the key mean nothing about being allowed to change it.
    /// Checked here rather than in front of the API, so that no door into a
    /// lambda - the editor, the REST API or an agent - can forget it.
    /// </remarks>
    /// <param name="origin">Who is asking: the seeder and the operator may, nobody else</param>
    private static void EnsureEditable(LambdaEntity lambda, string? origin = null)
    {
        if (lambda.Tier == LambdaTier.Demo && origin is not (VersionOrigins.System or VersionOrigins.Admin))
        {
            throw LambdaException.Forbidden(ReadOnly(lambda.PublicKey));
        }
    }

    /// <summary>
    /// What somebody is told who tries to change a demo, which is also what
    /// to do instead.
    /// </summary>
    internal static string ReadOnly(string publicKey)
        => $"'{publicKey}' is a demo and read only: read its code, files and logs as much as you like. " +
           $"To change it, create a lambda of your own from it - create_lambda (POST /api/v1/lambdas) with template '{publicKey}'.";

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
            Record(database, entity, LambdaEvents.Created);

            database.SaveChanges();

            // a copy of a demo that keeps its records in a database starts
            // with one, or the first thing it does is fail for want of it
            if (DemoCatalog.Find(template)?.Database == true)
            {
                DatabaseVault.Create(entity.Id);
            }

            Logger.LogInformation("Created lambda {Lambda} #{LambdaId}", entity.PublicKey, entity.Id);

            return Describe(database, entity);
        }

        throw LambdaException.Conflict("Unable to find a free key, please try again.");
    }

    public void Delete(string privateKey)
    {
        using var database = Databases.CreateDbContext();

        var lambda = Require(database, privateKey);

        EnsureEditable(lambda);

        Remove(database, lambda);

        Logger.LogInformation("Deleted lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);
    }

    #endregion

    #region Reading

    public LambdaInfo? Get(string privateKey)
    {
        using var database = Databases.CreateDbContext();

        var lambda = database.Lambdas.FirstOrDefault(l => l.PrivateKey == privateKey);

        return lambda == null ? null : Describe(database, lambda);
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

        var lambda = Require(database, privateKey);

        return ListVersions(database, lambda.Id);
    }

    public LambdaVersionContent GetVersion(string privateKey, int version)
    {
        using var database = Databases.CreateDbContext();

        var lambda = Require(database, privateKey);

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

        var lambda = Require(database, privateKey);

        return database.Activations.AsNoTracking()
                       .Where(a => a.LambdaId == lambda.Id)
                       .OrderByDescending(a => a.Started)
                       .ThenByDescending(a => a.Id)
                       .Select(a => new LambdaActivation(a.Version, a.Started, a.Origin, a.Ended, a.EndedBy))
                       .ToList();
    }

    #endregion

    #region Editing

    public LambdaVersionInfo Save(string privateKey, string code, VersionNote? note = null, int? after = null)
    {
        var files = Validate(code);

        using var database = Databases.CreateDbContext();

        var (lambda, held) = Locked(database, privateKey);

        using var turn = held;

        note ??= new VersionNote(Origin: VersionOrigins.Api);

        EnsureEditable(lambda, note.Origin);

        if (after is { } expected)
        {
            var newest = database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                 .Max(d => (int?)d.Version);

            if (newest != expected)
            {
                throw LambdaException.Conflict($"Version {newest} was saved in the meantime; this was meant to follow version {expected}.");
            }
        }

        ValidateAllowance(files, lambda.Tier, Limits);

        var version = Append(database, lambda, code, DateTime.UtcNow, note);

        Logger.LogInformation("Saved lambda #{LambdaId} version {Version}", lambda.Id, version.Version);

        return version;
    }

    public async ValueTask<CompilationOutcome> CheckAsync(string privateKey, string code, CancellationToken cancellation = default)
    {
        var files = Validate(code);

        long id;

        WorkspaceLimits limits;

        using (var database = Databases.CreateDbContext())
        {
            var lambda = Require(database, privateKey);

            ValidateAllowance(files, lambda.Tier, Limits);

            (id, limits) = (lambda.Id, WorkspaceOf(database, lambda));
        }

        return await Deployments.ValidateAsync(code, id, limits, cancellation);
    }

    /// <remarks>
    /// Compiled before the lambda's turn is taken, and written once it is: the
    /// turn is a lock, held by a thread, and compiling takes seconds away
    /// from it (see <see cref="_stripes"/>). Two deployments of one lambda can
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
            var lambda = Require(database, privateKey);

            EnsureEditable(lambda, origin);

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

            return new DeploymentResult(false, Describe(database, lambda), outcome.Diagnostics);
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

        CloseActivation(database, lambda.Id, now, ActivationEndings.Replaced);

        database.Activations.Add(new ActivationEntity
        {
            LambdaId = lambda.Id,
            Version = target,
            Started = now,
            Origin = origin ?? VersionOrigins.Api
        });

        Record(database, lambda, LambdaEvents.Deployed);

        database.SaveChanges();

        PruneActivations(database, lambda.Id);

        return new DeploymentResult(true, Describe(database, lambda), outcome.Diagnostics);
    }

    public LambdaInfo Undeploy(string privateKey, string? endedBy = null)
    {
        using var database = Databases.CreateDbContext();

        var (lambda, held) = Locked(database, privateKey);

        using var turn = held;

        EnsureEditable(lambda, endedBy == ActivationEndings.Admin ? VersionOrigins.Admin : null);

        if (lambda.ActiveVersion != null)
        {
            lambda.ActiveVersion = null;
            lambda.Deployed = null;

            CloseActivation(database, lambda.Id, DateTime.UtcNow, endedBy ?? ActivationEndings.Stopped);

            Record(database, lambda, LambdaEvents.Undeployed);

            database.SaveChanges();

            Deployments.Evict(lambda.Id);

            Logger.LogInformation("Undeployed lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);
        }

        return Describe(database, lambda);
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
    private void RecordUse(LambdaDbContext database)
    {
        var seen = Activity.Describe()
                           .Where(a => a.LastSeen != null)
                           .ToDictionary(a => a.PublicKey, a => a.LastSeen!.Value, StringComparer.Ordinal);

        if (seen.Count == 0)
        {
            return;
        }

        var keys = seen.Keys.ToList();

        var rows = database.Lambdas.Where(l => keys.Contains(l.PublicKey)).ToList();

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
            database.SaveChanges();
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

    public MaintenanceReport RunMaintenance(DateTime now)
    {
        using var database = Databases.CreateDbContext();

        /*
         * What counts as use, written down before anything is decided by it.
         *
         * Requests are counted in memory, because a database write per request
         * to move a timestamp would be absurd; this is where the last of them
         * reaches the row. It runs first so that a lambda busy right up to
         * this moment is not swept by figures taken before its traffic was
         * recorded.
         */
        RecordUse(database);

        var abandoned = now - Limits.Get().RemovedAfter;

        // demos are the installation's own, and being untouched is their
        // normal state rather than a sign that nobody wants them; premium
        // lambdas are kept by their tier (see Kept)
        var expired = database.Lambdas
                              .Where(l => l.Tier != LambdaTier.Demo && l.Tier != LambdaTier.Premium && l.Modified < abandoned
                                             && (l.LastSeen == null || l.LastSeen < abandoned))
                                    .ToList();

        foreach (var lambda in expired)
        {
            Remove(database, lambda);
        }

        var quiet = now - Limits.Get().OfflineAfter;

        var running = database.Lambdas.Where(l => l.Tier != LambdaTier.Demo && l.Tier != LambdaTier.Premium && l.ActiveVersion != null)
                              .ToList();

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

            CloseActivation(database, lambda.Id, now, ActivationEndings.Expired);

            Deployments.Evict(lambda.Id);

            undeployed++;
        }

        if (undeployed > 0)
        {
            database.SaveChanges();
        }

        if (undeployed > 0 || expired.Count > 0)
        {
            Logger.LogInformation("Undeployed {Undeployed} and deleted {Deleted} lambda(s) by maintenance", undeployed, expired.Count);
        }

        return new MaintenanceReport(undeployed, expired.Count);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Checks what can be checked without knowing whose code it is.
    /// </summary>
    /// <returns>The files, for the checks that do need to know</returns>
    internal static IReadOnlyList<LambdaFile> Validate(string? code)
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

        return files;
    }

    /// <summary>
    /// Holds the code and the assets to what the tier of the lambda allows.
    /// </summary>
    /// <remarks>
    /// Only ever when a version is written. A lambda that leaves the premium
    /// tier keeps the versions it has, and can put any of them online again;
    /// what it cannot do is save a new one until it fits. A refusal names
    /// what the premium tier allows, so whoever reads it knows there is more.
    /// A feature is held to the same, since what it holds becomes a version.
    /// </remarks>
    internal static void ValidateAllowance(IReadOnlyList<LambdaFile> files, LambdaTier tier, LimitsService limits)
    {
        // the limit counts what was written rather than what it is stored as,
        // so splitting a lambda into files does not spend any of it on the
        // envelope those files are kept in
        var code = limits.MaxCodeLengthOf(tier);

        if (LambdaSource.Length(files) > code)
        {
            throw LambdaException.Invalid($"The code must not exceed {code:N0} characters.{Beyond(tier, code, limits.MaxCodeLengthOf(LambdaTier.Premium), $"{limits.MaxCodeLengthOf(LambdaTier.Premium):N0} characters")}");
        }

        // the documentation and the tests are carried the same way as the
        // assets - a copy in every version - so they share the allowance
        var assets = limits.MaxAssetBytesOf(tier);

        var context = LambdaSource.ContextBytes(files);

        if (LambdaSource.AssetBytes(files) + context > assets)
        {
            var what = context > 0 ? "The assets, the documentation and the tests" : "The assets";

            throw LambdaException.Invalid($"{what} must not exceed {Readable(assets)} in total.{Beyond(tier, assets, limits.MaxAssetBytesOf(LambdaTier.Premium), Readable(limits.MaxAssetBytesOf(LambdaTier.Premium)))} A large file that is not code - a model, a dataset, media - belongs in the workspace, which is kept apart from the versions.");
        }
    }

    private static string Beyond(LambdaTier tier, long allowed, long premium, string readable)
        => tier != LambdaTier.Premium && premium > allowed ? $" A lambda in the premium tier may have {readable}." : string.Empty;

    private static string Readable(long bytes)
        => bytes % (1024 * 1024) == 0 ? $"{bytes / 1024 / 1024} MB" : $"{bytes / 1024} KB";

    public LambdaCounts Count()
    {
        using var database = Databases.CreateDbContext();

        return new LambdaCounts(
            database.Lambdas.Count(),
            database.Lambdas.Count(l => l.ActiveVersion != null),
            database.Deployments.Count()
        );
    }

    public long RequireEditable(string privateKey)
    {
        using var database = Databases.CreateDbContext();

        var lambda = database.Lambdas.AsNoTracking().FirstOrDefault(l => l.PrivateKey == privateKey)
                  ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        EnsureEditable(lambda);

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

    /// <summary>
    /// What the lambda may keep in its workspace, which is compiled into it.
    /// </summary>
    private WorkspaceLimits WorkspaceOf(LambdaDbContext database, LambdaEntity lambda)
        => Limits.WorkspaceOf(lambda.Tier, DataSwitches.IsEnabled(database, lambda.Id, DataKinds.Workspace));

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

        return new LambdaPage([.. lambdas.Select(l => new LambdaOverview(
            l.PublicKey,
            l.PrivateKey,
            l.Tier.ToString(),
            l.Created,
            l.Modified,
            l.ActiveVersion,
            latest.GetValueOrDefault(l.Id),
            counts.GetValueOrDefault(l.Id),
            DeployedUntil(l),
            KeptUntil(l),
            l.Domain
        ))], matched, total, deployed);
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

    private static LambdaEntity Require(LambdaDbContext database, string privateKey)
        => database.Lambdas.FirstOrDefault(l => l.PrivateKey == privateKey)
        ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

    /// <summary>
    /// The lambda of the key, read once it is its turn to change (see <see cref="_stripes" />).
    /// </summary>
    /// <returns>The lambda, and the turn - to be disposed of once the change is written</returns>
    private (LambdaEntity Lambda, IDisposable Turn) Locked(LambdaDbContext database, string privateKey)
    {
        var id = database.Lambdas.AsNoTracking()
                         .Where(l => l.PrivateKey == privateKey)
                         .Select(l => (long?)l.Id)
                         .FirstOrDefault()
              ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        var turn = new Turn(_stripes[(int)((ulong)id % (ulong)_stripes.Length)]);

        try
        {
            // read after waiting, so what it decides on is what the change
            // before it left behind
            return (Require(database, privateKey), turn);
        }
        catch
        {
            turn.Dispose();
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

    private void Seed(LambdaDbContext database, LambdaEntity lambda, string? template, DateTime now)
    {
        var demo = DemoCatalog.Find(template);

        var note = new VersionNote(Change: demo != null ? $"Started as a copy of the demo '{demo.Id}'" : "Started empty",
                                   Origin: VersionOrigins.Template);

        Append(database, lambda, TemplateCatalog.ForKey(template, lambda.PublicKey), now, note);

        if (demo?.Database == true)
        {
            database.DataStores.Add(new DataStoreEntity { LambdaId = lambda.Id, Kind = DataKinds.DatabaseId, Enabled = true, Changed = now });
        }
    }

    private LambdaVersionInfo Append(LambdaDbContext database, LambdaEntity lambda, string code, DateTime now, VersionNote note)
    {
        var version = database.Deployments.Where(d => d.LambdaId == lambda.Id)
                              .Max(d => (int?)d.Version) + 1 ?? 1;

        var specification = Tidy(note.Specification, VersionNote.MaxSpecification);

        var change = Tidy(note.Change, VersionNote.MaxChange);

        Storage.Write(lambda.Id, version, code);

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

        database.SaveChanges();

        Prune(database, lambda);

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
    internal static string? Tidy(string? text, int most)
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
    private static void CloseActivation(LambdaDbContext database, long lambdaId, DateTime now, string endedBy)
    {
        var open = database.Activations.Where(a => a.LambdaId == lambdaId && a.Ended == null)
                           .ToList();

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

    private static void PruneActivations(LambdaDbContext database, long lambdaId)
    {
        var obsolete = database.Activations.Where(a => a.LambdaId == lambdaId && a.Ended != null)
                               .OrderByDescending(a => a.Started)
                               .ThenByDescending(a => a.Id)
                               .Skip(MaxActivations)
                               .ToList();

        if (obsolete.Count > 0)
        {
            database.Activations.RemoveRange(obsolete);

            database.SaveChanges();
        }
    }

    /// <summary>
    /// Keeps the version history bounded, never touching the version that is live.
    /// </summary>
    private void Prune(LambdaDbContext database, LambdaEntity lambda)
    {
        // the base of a feature is kept like the version online: it is what
        // the feature is compared with, and what it is shown to change
        var bases = database.Features.Where(f => f.LambdaId == lambda.Id).Select(f => f.BaseVersion).ToList();

        var obsolete = database.Deployments.Where(d => d.LambdaId == lambda.Id && d.Version != lambda.ActiveVersion && !bases.Contains(d.Version))
                               .OrderByDescending(d => d.Version)
                               .Skip(Limits.Of(lambda.Tier).Versions)
                               .ToList();

        if (obsolete.Count == 0)
        {
            return;
        }

        foreach (var deployment in obsolete)
        {
            Storage.DeleteVersion(lambda.Id, deployment.Version);
        }

        database.Deployments.RemoveRange(obsolete);

        database.SaveChanges();
    }

    /// <summary>
    /// Notes that something happened, to be read long after it did.
    /// </summary>
    /// <remarks>
    /// Added to the same context as the change it describes, so it is written
    /// in the same transaction: an event recorded for a save that then failed
    /// would be a lie, and one written separately could be lost on its own.
    ///
    /// Demos are skipped. The installation seeds its own on every boot, and
    /// counting those would bury the activity the figures are meant to show.
    /// </remarks>
    private static void Record(LambdaDbContext database, LambdaEntity lambda, string kind)
    {
        if (lambda.Tier == LambdaTier.Demo)
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

    private void Remove(LambdaDbContext database, LambdaEntity lambda)
    {
        // with every preview of its features, which go with it
        Deployments.EvictAll(lambda.Id);

        // a deleted lambda takes its numbers with it rather than leaving a row
        // in the activity list that nothing can be looked up from any more
        Activity.Evict(lambda.Id);

        Record(database, lambda, LambdaEvents.Deleted);

        // the key cascades in the schema, but only where the connection has
        // foreign keys switched on - said here so it does not depend on that
        database.Activations.Where(a => a.LambdaId == lambda.Id).ExecuteDelete();

        database.Showcases.Where(s => s.LambdaId == lambda.Id).ExecuteDelete();

        database.DataStores.Where(s => s.LambdaId == lambda.Id).ExecuteDelete();

        database.Secrets.Where(s => s.LambdaId == lambda.Id).ExecuteDelete();

        database.Features.Where(f => f.LambdaId == lambda.Id).ExecuteDelete();

        database.Lambdas.Remove(lambda);

        database.SaveChanges();

        Secrets.Invalidate(lambda.Id);

        // its connections let go of before its files go
        DatabaseVault.Forget(lambda.Id);

        if (lambda.Domain != null)
        {
            Domains.Reload();
        }

        Storage.Delete(lambda.Id);
    }

    private LambdaInfo Describe(LambdaDbContext database, LambdaEntity lambda)
    {
        var latest = database.Deployments.Where(d => d.LambdaId == lambda.Id)
                             .Max(d => (int?)d.Version);

        // the two deadlines the maintenance job will act on, so the editor can
        // say when rather than leaving it to be discovered
        return new LambdaInfo(lambda.PublicKey, lambda.PrivateKey, lambda.Tier.ToString(), lambda.Created, lambda.Modified,
                              lambda.ActiveVersion, latest, lambda.Deployed, DeployedUntil(lambda), KeptUntil(lambda),
                              lambda.Domain, lambda.View.ToString());
    }

    /// <summary>
    /// When the sweep takes the lambda offline unless it is used before then.
    /// </summary>
    private DateTime? DeployedUntil(LambdaEntity lambda)
        => lambda.ActiveVersion != null && !Kept(lambda) ? Quiet(lambda) + Limits.Get().OfflineAfter : null;

    /// <summary>
    /// When the sweep removes the lambda unless it is used before then.
    /// </summary>
    private DateTime? KeptUntil(LambdaEntity lambda)
        => !Kept(lambda) ? Quiet(lambda) + Limits.Get().RemovedAfter : null;

    private static IReadOnlyList<LambdaVersionInfo> ListVersions(LambdaDbContext database, long lambdaId)
        => database.Deployments.AsNoTracking()
                   .Where(d => d.LambdaId == lambdaId)
                   .OrderByDescending(d => d.Version)
                   .Select(d => new LambdaVersionInfo(d.Version, d.Created, d.Specification, d.Change, d.Origin))
                   .ToList();

    #endregion

}
