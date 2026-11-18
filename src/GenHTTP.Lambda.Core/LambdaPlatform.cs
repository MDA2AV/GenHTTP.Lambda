using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Integrations.Internal;
using GenHTTP.Lambda.Integrations.Local;
using GenHTTP.Lambda.Integrations.Secrets;
using GenHTTP.Lambda.Integrations.Sqlite;
using GenHTTP.Lambda.Services.Analytics;
using GenHTTP.Lambda.Services.Background;
using GenHTTP.Lambda.Services.Blocking;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Guide;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Census;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Git;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Integrations;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Protection;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Source;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.UsageReports;
using GenHTTP.Lambda.Services.Visits;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda;

/// <summary>
/// The platform every component is built over: its services, its database,
/// the lambdas it compiles and runs, and the jobs that look after them.
/// </summary>
/// <remarks>
/// Built once per process and handed to each component's builder, which takes
/// what it needs from <see cref="Services"/>. Nothing here serves a request on
/// its own: the API, the MCP endpoint, the pages and the runner do, and a host
/// decides which of them it has and where.
///
/// Disposing of it stops the background jobs and the services, gives the
/// console back and lets go of its integrations.
/// </remarks>
public sealed class LambdaPlatform : IAsyncDisposable
{

    #region Get-/Setters

    /// <summary>
    /// How the platform is configured.
    /// </summary>
    public LambdaOptions Options { get; }

    /// <summary>
    /// The services the components are built from, and the requests are
    /// served from (see <c>AddDependencyInjection</c>).
    /// </summary>
    public ServiceProvider Services { get; }

    /// <summary>
    /// Where everything the platform and its components log goes.
    /// </summary>
    public ILoggerFactory Loggers { get; }

    private BackgroundScheduler Scheduler { get; }

    private bool OwnsLoggers { get; }

    /// <summary>
    /// Stops the warm-up between two lambdas once the platform is going away.
    /// </summary>
    private CancellationTokenSource Stopping { get; } = new();

    private Task? Warming { get; set; }

    private PlatformIntegrations Integrations { get; }

    #endregion

    #region Initialization

    internal LambdaPlatform(LambdaOptions options, ILoggerFactory loggers, bool ownsLoggers, LogBook book, RunLog runs, PlatformIntegrations integrations)
    {
        Options = options;
        Loggers = loggers;
        OwnsLoggers = ownsLoggers;
        Integrations = integrations;

        Services = BuildServices(options, loggers, book, runs, integrations);

        // what is printed to the console goes into this platform's book from
        // here on, until it is disposed of with the services
        Services.GetRequiredService<ConsoleCapture>();

        Scheduler = Services.GetRequiredService<BackgroundScheduler>();

        // before the first request, which is asked about against them
        Services.GetRequiredService<DomainRegistry>().Reload();
        Services.GetRequiredService<AddressBlocks>().Reload();
    }

    /// <summary>
    /// Starts to configure a platform.
    /// </summary>
    public static LambdaPlatformBuilder Create() => new();

    private static ServiceProvider BuildServices(LambdaOptions options, ILoggerFactory loggers, LogBook book, RunLog runs, PlatformIntegrations integrations)
    {
        var services = new ServiceCollection();

        services.AddSingleton(options);

        /*
         * What the data is kept in, built before the container from what the
         * host chose - the database already migrated through it. The
         * secrets are kept by the platform itself unless the host chose
         * otherwise, which takes the platform's own database to do.
         */
        services.AddSingleton(integrations);
        services.AddSingleton(integrations.Database);
        services.AddSingleton(integrations.Files);

        if (integrations.Secrets is { } secrets)
        {
            services.AddSingleton(secrets);
        }
        else
        {
            services.AddSingleton<SecretCipher>();
            services.AddSingleton<ISecretProvider, InternalSecrets>();
        }

        services.AddSingleton<IIntegrationService, IntegrationService>();

        /*
         * A shared class that its own folder takes as itself is registered
         * twice, as itself and as the interface every other folder takes it
         * by, both answered with the one instance. Taking the class itself
         * from another folder is refused by ServiceBoundaryTests.
         */
        services.AddSingleton(book);
        services.AddSingleton<ILogBook>(book);
        services.AddSingleton(runs);
        services.AddSingleton<IRunLog>(runs);
        services.AddSingleton<ConsoleCapture>();
        services.AddSingleton<StringPool>();
        services.AddSingleton<GeoTable>();
        services.AddSingleton<GeoPlaces>();

        services.AddSingleton(loggers);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        // pooled, because a context is made for nearly every call to the API;
        // the interceptor tells the caches that something was written
        services.AddSingleton<DatabaseChanges>();

        services.AddPooledDbContextFactory<LambdaDbContext>((provider, builder) =>
        {
            integrations.Database.ConfigureStore(builder, DatabaseStores.Platform);

            builder.AddInterceptors(provider.GetRequiredService<DatabaseChanges>());
        });

        services.AddSingleton<ServerRegistry>();

        services.AddSingleton<LambdaAddresses>();
        services.AddSingleton<ILambdaAddresses>(provider => provider.GetRequiredService<LambdaAddresses>());
        services.AddSingleton<DomainRegistry>();
        services.AddSingleton<IDomainRegistry>(provider => provider.GetRequiredService<DomainRegistry>());
        services.AddSingleton<LambdaThrottle>();
        services.AddSingleton<LambdaRateLimiter>();
        services.AddSingleton<AddressBlocks>();
        services.AddSingleton<IAddressBlocks>(provider => provider.GetRequiredService<AddressBlocks>());

        services.AddSingleton<SecretVault>();
        services.AddSingleton<ISecretVault>(provider => provider.GetRequiredService<SecretVault>());
        services.AddSingleton<DatabaseVault>();
        services.AddSingleton<IDatabaseVault>(provider => provider.GetRequiredService<DatabaseVault>());
        services.AddSingleton<WorkspaceVault>();
        services.AddSingleton<IWorkspaceVault>(provider => provider.GetRequiredService<WorkspaceVault>());
        services.AddSingleton<IStorageService, FileSystemStorageService>();
        services.AddSingleton<IDeploymentService, DeploymentService>();
        services.AddSingleton<EditorAddresses>();
        services.AddSingleton<LambdaHistory>();
        services.AddSingleton<LambdaRemoval>();
        services.AddSingleton<LambdaLifetime>();
        services.AddSingleton<LambdaDescriber>();
        services.AddSingleton<LambdaListing>();
        services.AddSingleton<LambdaHosting>();
        services.AddSingleton<IMetaService, MetaService>();
        services.AddSingleton<IWorkspaceService, WorkspaceService>();
        services.AddSingleton<IDataService, DataService>();
        services.AddSingleton<IFeatureService, FeatureService>();
        services.AddSingleton<ISecretService, SecretService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IShowcaseService, ShowcaseService>();
        services.AddSingleton<IConnectivityService, ConnectivityService>();
        services.AddSingleton<SourceCache>();
        services.AddSingleton<StarGuard>();
        services.AddSingleton<ISourceService, SourceService>();
        services.AddSingleton<GitStore>();
        services.AddSingleton<GitHistory>();
        services.AddSingleton<GitPushes>();
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ILimitsService, LimitsService>();
        services.AddSingleton<IBrandingService, BrandingService>();
        services.AddSingleton<IGuideService, GuideService>();
        services.AddSingleton<DemoSeeder>();
        services.AddSingleton<LambdaWarmup>();
        services.AddSingleton<AgentClient>();
        services.AddSingleton<BuildAllowance>();
        services.AddSingleton<ModelGate>();
        services.AddSingleton<IBuildService, BuildService>();
        services.AddSingleton<EventReader>();
        services.AddSingleton<LambdaTally>();
        services.AddSingleton<StorageSurvey>();
        services.AddSingleton<ICensusService, CensusService>();
        services.AddSingleton<VersionFactsCache>();

        services.AddSingleton<LambdaTelemetry>();
        services.AddSingleton<ITelemetryService, TelemetryService>();

        services.AddSingleton<UsageSurvey>();
        services.AddSingleton<UsageReportSender>();
        services.AddSingleton<IUsageReportService, UsageReportService>();

        services.AddSingleton<AnalyticsStore>();
        services.AddSingleton<IAnalyticsStore>(provider => provider.GetRequiredService<AnalyticsStore>());
        services.AddSingleton<AnalyticsQueue>();
        services.AddSingleton<DailyVisitors>();
        services.AddSingleton<CrawlerList>();
        services.AddSingleton<CrawlerRanges>();
        services.AddSingleton<VisitorClassifier>();
        services.AddSingleton<IAnalyticsService, AnalyticsService>();

        /*
         * The concerns that need nothing but services, resolved per request
         * from the scope the outermost concern of the host opens. Singletons,
         * because they keep nothing of a request's: without a registration
         * the container would build one for every request.
         */
        services.AddSingleton<CallerConcern>();
        services.AddSingleton<TelemetryConcern>();
        services.AddSingleton<ThrottleConcern>();
        services.AddSingleton<LambdaOutputConcern>();
        services.AddSingleton<LambdaVisitConcern>();
        services.AddSingleton<LambdaActivityConcern>();
        services.AddSingleton<AnalyticsConcern>();
        services.AddSingleton<RateLimitConcern>();

        services.AddSingleton<IBackgroundJob, MaintenanceJob>();
        services.AddSingleton<IBackgroundJob, TelemetryJob>();
        services.AddSingleton<IBackgroundJob, GeoJob>();
        services.AddSingleton<IBackgroundJob, AnalyticsJob>();
        services.AddSingleton<IBackgroundJob, CrawlerJob>();
        services.AddSingleton<IBackgroundJob, AddressBlockJob>();
        services.AddSingleton<IBackgroundJob, UsageReportJob>();
        services.AddSingleton<IBackgroundJob, StorageCensusJob>();
        services.AddSingleton<BackgroundScheduler>();

        return services.BuildServiceProvider();
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Starts the maintenance jobs. Called by the host once it is answering.
    /// </summary>
    public void StartBackgroundJobs() => Scheduler.Start();

    /// <summary>
    /// Brings the demos online, once the server is already answering.
    /// </summary>
    /// <remarks>
    /// Not awaited: every demo has to be compiled, and that is seconds the
    /// installation would otherwise spend refusing connections. They appear
    /// shortly after startup instead.
    /// </remarks>
    public void SeedDemos()
    {
        var seeder = Services.GetRequiredService<DemoSeeder>();

        // started in the background at startup, not a hop a request waits
        // for - the compiling in it goes through Offload where it happens
#pragma warning disable RS0030
        _ = Task.Run(async () =>
        {
            try
            {
                await seeder.SeedAsync();
            }
            catch (Exception e)
            {
                Loggers.CreateLogger<LambdaPlatform>().LogWarning(e, "The demos could not be prepared");
            }
        });
#pragma warning restore RS0030
    }

    /// <summary>
    /// Compiles what the lambdas of production and the system tier have
    /// online, once the server is already answering (see <see cref="LambdaWarmup"/>).
    /// </summary>
    /// <remarks>
    /// Not awaited, for the same reason as the demos: the installation
    /// answers while it compiles, and a request to a lambda not compiled yet
    /// is served as it always was.
    /// </remarks>
    public void WarmUp()
    {
        var warmup = Services.GetRequiredService<LambdaWarmup>();

        var stopping = Stopping.Token;

        // started in the background at startup, not a hop a request waits
        // for - the compiling in it goes through Offload where it happens
#pragma warning disable RS0030
        Warming = Task.Run(async () =>
        {
            try
            {
                await warmup.WarmUpAsync(stopping);
            }
            catch (Exception e)
            {
                Loggers.CreateLogger<LambdaPlatform>().LogWarning(e, "The lambdas could not be warmed up");
            }
        });
#pragma warning restore RS0030
    }

    public async ValueTask DisposeAsync()
    {
        // the lambda being compiled is finished, the rest are not started:
        // the services it compiles with are disposed of next. For a while
        // only - a lambda whose startup hangs must not hold the shutdown
        // until the process is killed
        await Stopping.CancelAsync();

        if (Warming != null)
        {
            try
            {
                await Warming.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                Loggers.CreateLogger<LambdaPlatform>().LogWarning("Stopped without waiting for the warm-up of a lambda");
            }
        }

        Stopping.Dispose();

        await Scheduler.DisposeAsync();

        await Services.DisposeAsync();

        // built before the container, so not the container's to dispose of
        await Integrations.DisposeAsync();

        // a factory handed in is its owner's to dispose of
        if (OwnsLoggers)
        {
            Loggers.Dispose();
        }
    }

    #endregion

}

/// <summary>
/// Configures and builds a <see cref="LambdaPlatform"/>.
/// </summary>
public sealed class LambdaPlatformBuilder
{
    private LambdaOptions _options = new();

    private ILoggerFactory? _loggers;

    private LogBook? _book;

    private RunLog? _runs;

    private IIntegrationBuilder<IDatabaseProvider>? _database;

    private IIntegrationBuilder<IFileProvider>? _files;

    private IIntegrationBuilder<ISecretProvider>? _secrets;

    #region Functionality

    /// <summary>
    /// How the platform is configured.
    /// </summary>
    public LambdaPlatformBuilder Options(LambdaOptions options)
    {
        _options = options;
        return this;
    }

    /// <summary>
    /// Where the platform logs to, and the book the operator reads in the
    /// panel - which the factory has to write into.
    /// </summary>
    /// <remarks>
    /// Without, the platform makes both: a book of the configured size, and
    /// a factory writing into it and to the console (see <see cref="PlatformLogging"/>).
    /// </remarks>
    public LambdaPlatformBuilder Logging(ILoggerFactory loggers, LogBook book)
    {
        _loggers = loggers;
        _book = book;
        return this;
    }

    /// <summary>
    /// The note on disk about how this run is going, if the host keeps one
    /// already - and has reported the previous run itself. Without, the
    /// platform makes it and reports the previous run as it is built.
    /// </summary>
    public LambdaPlatformBuilder RunLog(RunLog runs)
    {
        _runs = runs;
        return this;
    }

    /// <summary>
    /// What the platform keeps its own records, its analytics and the
    /// databases of the lambdas in - SQLite files in the data directory
    /// unless chosen otherwise.
    /// </summary>
    public LambdaPlatformBuilder Database(IIntegrationBuilder<IDatabaseProvider> database)
    {
        _database = database;
        return this;
    }

    /// <summary>
    /// What the workspaces of the lambdas are kept in - directories in the
    /// data directory unless chosen otherwise.
    /// </summary>
    public LambdaPlatformBuilder Files(IIntegrationBuilder<IFileProvider> files)
    {
        _files = files;
        return this;
    }

    /// <summary>
    /// What the secrets of the lambdas are kept in - sealed in the platform's
    /// own database unless chosen otherwise.
    /// </summary>
    public LambdaPlatformBuilder Secrets(IIntegrationBuilder<ISecretProvider> secrets)
    {
        _secrets = secrets;
        return this;
    }

    /// <summary>
    /// Builds the integrations, migrates the database and builds the platform.
    /// </summary>
    /// <remarks>
    /// The note about this run is begun first, so that a start which never
    /// finishes - a migration that fails, an option that cannot be read - is
    /// written down as this run's fault, and the next one says so rather
    /// than report the run before it.
    /// </remarks>
    public LambdaPlatform Build()
    {
        var book = _book ?? PlatformLogging.CreateBook(_options);

        var loggers = _loggers ?? PlatformLogging.CreateLoggers(_options, book);

        var logger = loggers.CreateLogger("GenHTTP.Lambda");

        Directory.CreateDirectory(_options.DataDirectory);

        var runs = _runs;

        if (runs == null)
        {
            runs = new RunLog(_options.DataDirectory);

            runs.Report(logger);
        }

        PlatformIntegrations? integrations = null;

        try
        {
            integrations = BuildIntegrations(new IntegrationContext(_options.DataDirectory, loggers));

            Migrator.Migrate(integrations.Database, loggers.CreateLogger<LambdaPlatform>());

            return new LambdaPlatform(_options, loggers, _loggers == null, book, runs, integrations);
        }
        catch (Exception e)
        {
            integrations?.Dispose();

            // nothing watches the process for what ends it yet - the server
            // does that once there is a platform to serve
            runs.Faulted(e);

            logger.LogCritical(e, "The platform could not be started");

            if (_loggers == null)
            {
                loggers.Dispose();
            }

            throw;
        }
    }

    /// <summary>
    /// Builds what the host chose, and the platform's own where it chose nothing.
    /// </summary>
    /// <remarks>
    /// Each built as soon as the one before it is, so whatever was built
    /// before one that fails is let go of again.
    /// </remarks>
    private PlatformIntegrations BuildIntegrations(IntegrationContext context)
    {
        var built = new List<IIntegration>();

        try
        {
            var database = Keep(built, (_database ?? SqliteDatabase.Create()).Build(context));

            var files = Keep(built, (_files ?? LocalFiles.Create()).Build(context));

            var secrets = _secrets is { } chosen ? Keep(built, chosen.Build(context)) : null;

            // a host naming the platform's own has not chosen another
            return new PlatformIntegrations(database, files, secrets, _database is not (null or SqliteDatabaseBuilder), _files is not (null or LocalFilesBuilder));
        }
        catch
        {
            PlatformIntegrations.Dispose(built);
            throw;
        }
    }

    private static T Keep<T>(List<IIntegration> built, T integration) where T : IIntegration
    {
        built.Add(integration);
        return integration;
    }

    #endregion

}
