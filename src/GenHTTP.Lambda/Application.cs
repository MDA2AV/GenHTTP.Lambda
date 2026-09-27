using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Mcp;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Background;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Execution;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Protection;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Webservices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda;

/// <summary>
/// Wires the application together: the services, the handler tree and the
/// background jobs. Kept separate from <c>Program</c> so tests can run the
/// whole thing against a temporary directory.
/// </summary>
public sealed class Application : IAsyncDisposable
{

    #region Get-/Setters

    /// <summary>
    /// The root handler of the application.
    /// </summary>
    public IHandler Handler { get; }

    /// <summary>
    /// The services the API resources are resolved from.
    /// </summary>
    public ServiceProvider Services { get; }

    private LambdaOptions Options { get; }

    private ServerRegistry Registry { get; }

    private BackgroundScheduler Scheduler { get; }

    #endregion

    #region Initialization

    private Application(LambdaOptions options, ILoggerFactory loggers, LogBook book, RunLog runs)
    {
        Options = options;

        Services = BuildServices(options, loggers, book, runs);

        Registry = Services.GetRequiredService<ServerRegistry>();

        Scheduler = Services.GetRequiredService<BackgroundScheduler>();

        // before the first request, which is asked about against it
        Services.GetRequiredService<DomainRegistry>().ReloadAsync().AsTask().GetAwaiter().GetResult();

        Handler = BuildHandler(Services, options);
    }

    /// <summary>
    /// Migrates the database and builds the application.
    /// </summary>
    public static Application Create(LambdaOptions options, ILoggerFactory loggers, LogBook? book = null, RunLog? runs = null)
    {
        Migrator.Migrate(options, loggers.CreateLogger<Application>());

        return new Application(options, loggers, book ?? new LogBook(options.LogHistory, options.LogMemory, options.RepeatWindow),
                               runs ?? new RunLog(options.DataDirectory));
    }

    private static ServiceProvider BuildServices(LambdaOptions options, ILoggerFactory loggers, LogBook book, RunLog runs)
    {
        var services = new ServiceCollection();

        services.AddSingleton(options);

        services.AddSingleton(book);
        services.AddSingleton(runs);
        services.AddSingleton<StringPool>();
        services.AddSingleton<GeoTable>();
        services.AddSingleton<GeoPlaces>();

        services.AddSingleton(loggers);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        services.AddDbContextFactory<LambdaDbContext>(builder => builder.UseSqlite(options.ConnectionString));

        services.AddSingleton<ServerRegistry>();

        services.AddSingleton<DomainRegistry>();
        services.AddSingleton<LambdaThrottle>();
        services.AddSingleton<LambdaRateLimiter>();

        services.AddSingleton<IStorageService, FileSystemStorageService>();
        services.AddSingleton<IDeploymentService, DeploymentService>();
        services.AddSingleton<IMetaService, MetaService>();
        services.AddSingleton<IWorkspaceService, WorkspaceService>();
        services.AddSingleton<IShowcaseService, ShowcaseService>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<DemoSeeder>();
        services.AddSingleton<McpTools>();
        services.AddSingleton<BuildService>();
        services.AddSingleton<EventReader>();

        services.AddSingleton<SiteMeta>();
        services.AddSingleton<SpaResources>();

        services.AddSingleton<LambdaTelemetry>();
        services.AddSingleton<ITelemetryService, TelemetryService>();

        services.AddSingleton<IBackgroundJob, MaintenanceJob>();
        services.AddSingleton<IBackgroundJob, TelemetryJob>();
        services.AddSingleton<IBackgroundJob, GeoJob>();
        services.AddSingleton<BackgroundScheduler>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Builds the routes of the system: a lambda's own domain goes straight to
    /// the lambda, and everything else to the platform.
    /// </summary>
    private static IHandler BuildHandler(IServiceProvider services, LambdaOptions options)
    {
        var meta = services.GetRequiredService<IMetaService>();

        var domains = LambdaRoute.Create(services, new DomainLocator(meta));

        var router = new DomainRouter(services.GetRequiredService<DomainRegistry>(), domains, BuildPlatform(services, options));

        // ahead of the router, so a challenge for a lambda's own domain is
        // answered here rather than handed to the lambda
        return options.AcmeDirectory is { } acme
             ? Concerns.Chain([new AcmeChallengeConcernBuilder(acme)], router)
             : router;
    }

    /// <summary>
    /// The platform itself: the API, the deployed lambdas below their keys, the
    /// frontend's own assets and the single page application, which serves both
    /// the landing page and the editor and catches every other path.
    /// </summary>
    private static IHandler BuildPlatform(IServiceProvider services, LambdaOptions options)
    {
        var spa = services.GetRequiredService<SpaResources>();

        var lambdas = LambdaRoute.Create(services, new KeyLocator(services.GetRequiredService<IMetaService>(), spa));

        var layout = Layout.Create()
                           .Add("api", ApiLayout.Create(options))
                           // one path, for agents rather than for browsers
                           .Add("mcp", new McpHandlerBuilder(services.GetRequiredService<McpTools>(), options.McpOrigins))
                           .Add("lambda", lambdas);

        // Ahead of the application, so a miss here is a 404 rather than the
        // index page: a named route answers for itself and never falls through
        // to the handler that catches everything else.
        if (spa.HasAssets)
        {
            layout = layout.Add("assets", spa.CreateAssetHandler());
        }

        return layout.Add(spa.CreateHandler())
                     .Build();
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Applies the application to a server host, ready to be started.
    /// </summary>
    public IServerHost Configure(IServerHost host)
        => host.Handler(Handler)
               .Logging(Services.GetRequiredService<ILoggerFactory>())
               .Development(Options.Development)
               .AddDependencyInjection(Services)
               .Add(Registry.Capture())
               .Add(new TelemetryConcernBuilder(Services.GetRequiredService<ITelemetryService>()))
               /*
                * The upgrade to the secure endpoint covers a lambda's own
                * domain as well: it redirects to the host that was asked for,
                * so http://shop.example.com goes to https://shop.example.com.
                * The certificate for such a domain is put beside the others
                * by the operator (see CertificateLoader).
                */
               .Defaults()
               /*
                * Last, which puts it outside everything else, because a
                * concern added later wraps the ones added before it.
                *
                * It has to be outside the defaults specifically. Those carry
                * the upgrade from plain HTTP to HTTPS, which answers the
                * request itself and never calls through - so from anywhere
                * inside them, every visitor who typed the bare domain, every
                * scanner knocking on port 80 and the container's own health
                * check are simply not there. They were being logged by the
                * engine and then dropped from the book as duplicates of a
                * line that was never written.
                *
                * Outermost also means the mark is in place before anything
                * below can log under it, and that what is timed is the whole
                * answer rather than the part after the throttle.
                */
               .Add(new CallerConcernBuilder(Services.GetRequiredService<LogBook>(),
                                             Services.GetRequiredService<StringPool>(),
                                             Services.GetRequiredService<GeoTable>(),
                                             Services.GetRequiredService<GeoPlaces>(),
                                             Services.GetRequiredService<DomainRegistry>(),
                                             Options));

    /// <summary>
    /// Starts the maintenance jobs. Call once the server is up.
    /// </summary>
    public void StartBackgroundJobs() => Scheduler.Start();

    /// <summary>
    /// Brings the demos online, once the server is already answering.
    /// </summary>
    /// <remarks>
    /// Deliberately not awaited by the caller: every demo has to be
    /// compiled, and that is seconds the installation would otherwise spend
    /// refusing connections. They appear shortly after startup instead.
    /// </remarks>
    public void SeedDemos()
    {
        var seeder = Services.GetRequiredService<DemoSeeder>();

        _ = Task.Run(async () =>
        {
            try
            {
                await seeder.SeedAsync();
            }
            catch (Exception e)
            {
                Services.GetRequiredService<ILoggerFactory>().CreateLogger<Application>().LogWarning(e, "The demos could not be prepared");
            }
        });
    }

    public async ValueTask DisposeAsync()
    {
        await Scheduler.DisposeAsync();

        await Services.DisposeAsync();
    }

    #endregion

}
