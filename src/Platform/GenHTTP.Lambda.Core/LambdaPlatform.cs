using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Integrations.Sealed;
using GenHTTP.Lambda.Integrations.Secrets;
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
using GenHTTP.Lambda.Services.Optimization;
using GenHTTP.Lambda.Services.Protection;
using GenHTTP.Lambda.Services.QuietHours;
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
/// Built once per installation and handed to each component's builder, which
/// takes what it needs from <see cref="Services"/>. Nothing here serves a
/// request on its own: the API, the MCP endpoint, the pages and the runner do,
/// and a host decides which of them it has and where.
///
/// Usually the only one in its process, which it has for itself then. A host
/// may run several, each on the runtime it hands them (see <see cref="LambdaRuntime"/>).
///
/// Disposing of it stops the background jobs and the services, gives the
/// console back, lets go of its integrations and takes back what the lambdas
/// were handed to reach it with. Their code stays loaded, as it does when a
/// lambda is taken offline, so whoever disposes of one that others outlive
/// stops handing it requests first.
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

    /// <summary>
    /// What the platform has in common with the others of its process, if
    /// there are others.
    /// </summary>
    public LambdaRuntime Runtime { get; }

    /// <summary>
    /// Whether the process is this platform's alone: the runtime is its own
    /// rather than one it was handed.
    /// </summary>
    public bool OwnsProcess { get; }

    private BackgroundScheduler Scheduler { get; }

    private bool OwnsLoggers { get; }

    /// <summary>
    /// Stops the warm-up and the seeding between two lambdas once the platform
    /// is going away.
    /// </summary>
    private CancellationTokenSource Stopping { get; } = new();

    /// <summary>
    /// What was begun as the platform started and goes on beside the requests.
    /// </summary>
    private List<Task> Beginning { get; } = [];

    private PlatformIntegrations Integrations { get; }

    #endregion

    #region Initialization

    internal LambdaPlatform(LambdaOptions options, ILoggerFactory loggers, bool ownsLoggers, LogBook book, RunLog? runs, LambdaRuntime runtime, PlatformIntegrations integrations)
    {
        Options = options;
        Loggers = loggers;
        OwnsLoggers = ownsLoggers;
        Runtime = runtime;
        OwnsProcess = runs != null;
        Integrations = integrations;

        Services = BuildServices(options, loggers, book, runs, runtime, integrations);

        // what is printed to the console goes into this platform's book from
        // here on, until it is disposed of with the services - where there is
        // nobody else it could be about
        if (OwnsProcess)
        {
            Services.GetRequiredService<ConsoleCapture>();
        }

        Scheduler = Services.GetRequiredService<BackgroundScheduler>();
    }

    /// <summary>
    /// What the platform reads through its integrations before it serves
    /// anything, once its services are there.
    /// </summary>
    internal async ValueTask PrepareAsync(CancellationToken cancellation)
    {
        var logger = Loggers.CreateLogger<LambdaPlatform>();

        // what serving reads from memory, read before anything asks for it:
        // the limits every check reads, and before the first request, which
        // is asked about against them, the domains and the blocked addresses
        await Services.GetRequiredService<LimitsService>().LoadAsync(cancellation);

        await Services.GetRequiredService<DomainRegistry>().ReloadAsync();

        await Services.GetRequiredService<AddressBlocks>().ReloadAsync();

        // what was on its way into a store when the server stopped goes, before
        // anything could be on its way again
        Services.GetRequiredService<IStorageService>();

        // the versions and features kept before there were stores, into them:
        // once, and before anything is served, so nothing reads them whole
        await Services.GetRequiredService<StoreMigration>().RunAsync(cancellation);

        // the pictures out of the records they were kept in before - read
        // from there meanwhile where that fails, and tried again next time
        try
        {
            await PictureMove.RunAsync(Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>(), Integrations.Files, logger, cancellation);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to move the pictures from the platform's records into its files");
        }

        // what the pages and the serving read from memory: the operator's
        // switches - the site's and the build agent's, with the models it
        // offers - and what every page is branded with, its logo among the files
        await Services.GetRequiredService<SettingsService>().LoadAsync(cancellation);

        await Services.GetRequiredService<AgentSettingsStore>().LoadAsync(cancellation);

        await Services.GetRequiredService<GuideService>().LoadAsync(cancellation);

        await Services.GetRequiredService<BrandingService>().LoadAsync(cancellation);
    }

    /// <summary>
    /// Starts to configure a platform.
    /// </summary>
    public static LambdaPlatformBuilder Create() => new();

    private static ServiceProvider BuildServices(LambdaOptions options, ILoggerFactory loggers, LogBook book, RunLog? runs, LambdaRuntime runtime, PlatformIntegrations integrations)
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
            services.AddSingleton<ISecretProvider, DatabaseSecrets>();
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
        services.AddSingleton<StringPool>();

        /*
         * What the process has once, handed on as it is - by instance, so the
         * container does not dispose of what is not the platform's.
         */
        services.AddSingleton(runtime.Services.GetRequiredService<GeoTable>());
        services.AddSingleton(runtime.Services.GetRequiredService<GeoPlaces>());
        services.AddSingleton(runtime.Services.GetRequiredService<CrawlerList>());
        services.AddSingleton(runtime.Services.GetRequiredService<CrawlerRanges>());
        services.AddSingleton(runtime.Services.GetRequiredService<CompiledAssemblies>());
        services.AddSingleton(runtime.Services.GetRequiredService<CompilationGate>());

        if (runs != null)
        {
            // the process is this platform's: what it prints, how it runs and
            // how the run before it ended are its own to keep
            services.AddSingleton<IRunLog>(runs);
            services.AddSingleton<ConsoleCapture>();
            services.AddSingleton<IProcessReadings, OwnProcess>();
        }
        else
        {
            services.AddSingleton<IRunLog, NoRunLog>();
            services.AddSingleton<IProcessReadings, SharedProcess>();
            services.AddSingleton(runtime.Services.GetRequiredService<ProcessThrottle>());
        }

        services.AddSingleton(loggers);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        // pooled, because a context is made for nearly every call to the API;
        // the interceptors tell the caches that something was written, and
        // the log that something asked for the records without awaiting them
        services.AddSingleton<DatabaseChanges>();
        services.AddSingleton<SynchronousRecords>();

        services.AddPooledDbContextFactory<LambdaDbContext>((provider, builder) =>
        {
            integrations.Database.ConfigureStore(builder, DatabaseStore.Platform);

            builder.AddInterceptors(provider.GetRequiredService<DatabaseChanges>(), provider.GetRequiredService<SynchronousRecords>());
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
        services.AddSingleton<ContentReads>();
        services.AddSingleton<ContentStore>();
        services.AddSingleton<StoreMigration>();
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
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ISettingsService>(provider => provider.GetRequiredService<SettingsService>());
        services.AddSingleton<LimitsService>();
        services.AddSingleton<ILimitsService>(provider => provider.GetRequiredService<LimitsService>());
        services.AddSingleton<BrandingService>();
        services.AddSingleton<IBrandingService>(provider => provider.GetRequiredService<BrandingService>());
        services.AddSingleton<GuideService>();
        services.AddSingleton<IGuideService>(provider => provider.GetRequiredService<GuideService>());
        services.AddSingleton<DemoSeeder>();
        services.AddSingleton<LambdaWarmup>();
        services.AddSingleton<AgentClient>();
        services.AddSingleton<AgentTickets>();
        services.AddSingleton<AgentSettingsStore>();
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
        services.AddSingleton<VisitorClassifier>();
        services.AddSingleton<IAnalyticsService, AnalyticsService>();

        services.AddSingleton<HourlyCounts>();
        services.AddSingleton<IQuietHoursService, QuietHoursService>();

        services.AddSingleton<IOptimizationService, OptimizationService>();

        /*
         * The concerns that need nothing but services, taken from here as the
         * chains they are part of are built (see ServiceConcern). Singletons,
         * because they keep nothing of a request's.
         */
        services.AddSingleton<CallerConcern>();
        services.AddSingleton<TelemetryConcern>();
        services.AddSingleton<QuietHoursConcern>();
        services.AddSingleton<ThrottleConcern>();
        services.AddSingleton<LambdaOutputConcern>();
        services.AddSingleton<LambdaVisitConcern>();
        services.AddSingleton<LambdaActivityConcern>();
        services.AddSingleton<AnalyticsConcern>();
        services.AddSingleton<RateLimitConcern>();

        services.AddSingleton<IBackgroundJob, MaintenanceJob>();
        services.AddSingleton<IBackgroundJob, TelemetryJob>();
        services.AddSingleton<IBackgroundJob, AnalyticsJob>();
        services.AddSingleton<IBackgroundJob, AddressBlockJob>();
        services.AddSingleton<IBackgroundJob, AgentTicketJob>();
        services.AddSingleton<IBackgroundJob, UsageReportJob>();
        services.AddSingleton<IBackgroundJob, StorageCensusJob>();
        services.AddSingleton<IBackgroundJob, QuietHoursJob>();
        services.AddSingleton<IBackgroundJob, OptimizationJob>();
        services.AddSingleton<BackgroundScheduler>();

        return services.BuildServiceProvider();
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Starts what goes on beside the requests: the background jobs, the
    /// warm-up of the lambdas kept warm, and the demos. Called once, when
    /// whatever serves the platform has been prepared.
    /// </summary>
    /// <remarks>
    /// In that order: the lambdas kept warm are the apps people use, so they
    /// come before the demos.
    /// </remarks>
    public void Start()
    {
        StartBackgroundJobs();

        WarmUp();

        SeedDemos();
    }

    /// <summary>
    /// Starts the maintenance jobs. Called by the host once it is answering.
    /// </summary>
    /// <remarks>
    /// With those of the runtime, where that is the platform's own.
    /// </remarks>
    public void StartBackgroundJobs()
    {
        if (OwnsProcess)
        {
            Runtime.Start();
        }

        Scheduler.Start();
    }

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

        Begin(seeder.SeedAsync, "The demos could not be prepared");
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

        Begin(warmup.WarmUpAsync, "The lambdas could not be warmed up");
    }

    /// <summary>
    /// Begins something that goes on beside the requests, until it is done or
    /// the platform goes away.
    /// </summary>
    private void Begin(Func<CancellationToken, ValueTask> work, string problem)
    {
        var stopping = Stopping.Token;

        // started in the background at startup, not a hop a request waits
        // for - the compiling in it goes through Offload where it happens
#pragma warning disable RS0030
        Beginning.Add(Task.Run(async () =>
        {
            try
            {
                await work(stopping);
            }
            catch (OperationCanceledException) when (stopping.IsCancellationRequested)
            {
                // the platform is going away
            }
            catch (Exception e)
            {
                Loggers.CreateLogger<LambdaPlatform>().LogWarning(e, "{Problem}", problem);
            }
        }, CancellationToken.None));
#pragma warning restore RS0030
    }

    public ValueTask DisposeAsync() => DisposeAsync(OwnsLoggers);

    /// <param name="loggers">Whether to let go of the loggers as well, which a build that fails does itself once it said so</param>
    internal async ValueTask DisposeAsync(bool loggers)
    {
        // the lambda being compiled is finished, the rest are not started:
        // the services it compiles with are disposed of next. For a while
        // only - a lambda whose startup hangs must not hold the shutdown
        // until the process is killed
        await Stopping.CancelAsync();

        try
        {
            await Task.WhenAll(Beginning).WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException)
        {
            Loggers.CreateLogger<LambdaPlatform>().LogWarning("Stopped without waiting for a lambda that is being started");
        }

        Stopping.Dispose();

        await Scheduler.DisposeAsync();

        await Services.DisposeAsync();

        // built before the container, so not the container's to dispose of
        await Integrations.DisposeAsync();

        // one that was handed in is its owner's
        if (OwnsProcess)
        {
            await Runtime.DisposeAsync();
        }

        // a factory handed in is its owner's to dispose of
        if (loggers)
        {
            Loggers.Dispose();
        }
    }

    #endregion

}
