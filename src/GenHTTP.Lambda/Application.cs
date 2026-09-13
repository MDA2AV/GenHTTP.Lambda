using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda.Api;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Background;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Execution;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;

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

    private Application(LambdaOptions options, ILoggerFactory loggers)
    {
        Options = options;

        Services = BuildServices(options, loggers);

        Registry = Services.GetRequiredService<ServerRegistry>();

        Scheduler = Services.GetRequiredService<BackgroundScheduler>();

        Handler = BuildHandler(Services, options, loggers);
    }

    /// <summary>
    /// Migrates the database and builds the application.
    /// </summary>
    public static Application Create(LambdaOptions options, ILoggerFactory loggers)
    {
        Migrator.Migrate(options, loggers.CreateLogger<Application>());

        return new Application(options, loggers);
    }

    private static ServiceProvider BuildServices(LambdaOptions options, ILoggerFactory loggers)
    {
        var services = new ServiceCollection();

        services.AddSingleton(options);

        services.AddSingleton(loggers);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        services.AddDbContextFactory<LambdaDbContext>(builder => builder.UseSqlite(options.ConnectionString));

        services.AddSingleton<ServerRegistry>();

        services.AddSingleton<IStorageService, FileSystemStorageService>();
        services.AddSingleton<IDeploymentService, DeploymentService>();
        services.AddSingleton<IMetaService, MetaService>();

        services.AddSingleton<SpaResources>();

        services.AddSingleton<LambdaTelemetry>();
        services.AddSingleton<TelemetryService>();
        services.AddSingleton<ITelemetryService>(p => p.GetRequiredService<TelemetryService>());

        services.AddSingleton<IBackgroundJob, MaintenanceJob>();
        services.AddSingleton<IBackgroundJob, TelemetryJob>();
        services.AddSingleton<BackgroundScheduler>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Builds the four routes of the system: the API, the deployed lambdas and
    /// the single page application, which serves both the landing page and the
    /// editor and catches every other path.
    /// </summary>
    private static IHandler BuildHandler(IServiceProvider services, LambdaOptions options, ILoggerFactory loggers)
    {
        var spa = services.GetRequiredService<SpaResources>();

        var lambdas = LambdaRoute.Create(
            services.GetRequiredService<IMetaService>(),
            services.GetRequiredService<IDeploymentService>(),
            spa,
            options,
            services.GetRequiredService<LambdaTelemetry>(),
            loggers
        );

        var layout = Layout.Create()
                           .Add("api", ApiLayout.Create())
                           .Add("lambda", lambdas);

        // before the application, so a file that is not there is reported as
        // missing rather than answered with the index page
        var assets = spa.CreateAssetHandler();

        if (assets != null)
        {
            layout.Add("assets", assets);
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
               .Add(new TelemetryConcernBuilder(Services.GetRequiredService<TelemetryService>()))
               .Defaults();

    /// <summary>
    /// Starts the maintenance jobs. Call once the server is up.
    /// </summary>
    public void StartBackgroundJobs() => Scheduler.Start();

    public async ValueTask DisposeAsync()
    {
        await Scheduler.DisposeAsync();

        await Services.DisposeAsync();
    }

    #endregion

}
