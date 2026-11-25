using System.Collections.Concurrent;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Analytics;
using GenHTTP.Lambda.Services.Background;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Protection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda;

/// <summary>
/// What a process has once, however many platforms run in it: where addresses
/// are and who crawls, the turn to compile, the assemblies compiled so far,
/// the slots requests run in, and what the components keep that is the same
/// for every platform.
/// </summary>
/// <remarks>
/// A platform on its own makes one and is none the wiser. A host that runs
/// several - an installation per customer, say - makes the runtime itself and
/// hands it to each of them (<see cref="LambdaPlatformBuilder.Runtime"/>), so
/// the tables and lists are in memory once, fetched once, and one lambda is
/// compiled at a time rather than one per platform.
///
/// The process is the host's then. A platform it hands a runtime keeps no
/// lines of the console but those of its lambdas, reads nothing of the
/// process and writes no note about the run: all of that is about everything
/// in there, and no business of a single installation's operator.
///
/// Nothing in it is anybody's data, and it knows none of its platforms. Built
/// before the first of them, started once (<see cref="Start"/>) and disposed
/// of after the last.
/// </remarks>
public sealed class LambdaRuntime : IAsyncDisposable
{
    private readonly ConcurrentDictionary<(Type Type, string Key), Lazy<object>> _kept = [];

    private int _started;

    #region Get-/Setters

    /// <summary>
    /// How the runtime is configured.
    /// </summary>
    public RuntimeOptions Options { get; }

    /// <summary>
    /// The services the platforms are handed, which are the runtime's to dispose of.
    /// </summary>
    internal ServiceProvider Services { get; }

    private BackgroundScheduler Scheduler { get; }

    #endregion

    #region Initialization

    internal LambdaRuntime(RuntimeOptions options, ILoggerFactory loggers)
    {
        Options = options;

        Services = BuildServices(options, loggers);

        // what lambdas print is told from what anybody else does by the tee,
        // which is the process's as the console is
        ConsoleTee.Install();

        Scheduler = Services.GetRequiredService<BackgroundScheduler>();
    }

    /// <summary>
    /// Starts to configure a runtime.
    /// </summary>
    public static LambdaRuntimeBuilder Create() => new();

    private static ServiceProvider BuildServices(RuntimeOptions options, ILoggerFactory loggers)
    {
        var services = new ServiceCollection();

        services.AddSingleton(options);

        services.AddSingleton(loggers);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        services.AddSingleton<GeoTable>();
        services.AddSingleton<GeoPlaces>();

        services.AddSingleton<CrawlerList>();
        services.AddSingleton<CrawlerRanges>();

        services.AddSingleton<CompiledAssemblies>();
        services.AddSingleton<CompilationGate>();
        services.AddSingleton<PreparationLane>();

        services.AddSingleton<ProcessThrottle>();

        services.AddSingleton<IBackgroundJob, GeoJob>();
        services.AddSingleton<IBackgroundJob, CrawlerJob>();
        services.AddSingleton<BackgroundScheduler>();

        return services.BuildServiceProvider();
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Starts to fetch what is fetched and kept current in the background.
    /// Called once the process serves; called again, it does nothing.
    /// </summary>
    public void Start()
    {
        if (Interlocked.Exchange(ref _started, 1) == 0)
        {
            Scheduler.Start();
        }
    }

    /// <summary>
    /// What a component keeps that is the same for every platform of the
    /// process, made by whoever asks first.
    /// </summary>
    /// <remarks>
    /// For what is read from the process's own files and costs enough to be
    /// worth having once - the prerendered pages of a bundle, for one. Never
    /// for anything that is a platform's.
    /// </remarks>
    /// <param name="key">What it is, among others of its type: the file it was read from, say</param>
    public T Shared<T>(string key, Func<T> make) where T : class
        => (T)_kept.GetOrAdd((typeof(T), key), _ => new Lazy<object>(() => make())).Value;

    #endregion

    #region Lifecycle

    public async ValueTask DisposeAsync()
    {
        await Scheduler.DisposeAsync();

        await Services.DisposeAsync();
    }

    #endregion

}
