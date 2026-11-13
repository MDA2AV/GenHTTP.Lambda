using System.Net;
using System.Runtime.InteropServices;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda.Services.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Server;

/// <summary>
/// Runs a platform on an engine until the process is told to stop.
/// </summary>
/// <example>
/// <code>
/// return await LambdaServer.Create(platform)
///                          .Handler(Runner.Create(platform).Site(site))
///                          .RunAsync();
/// </code>
/// </example>
public static class LambdaServer
{

    /// <summary>
    /// A server for the given platform.
    /// </summary>
    public static LambdaServerBuilder Create(LambdaPlatform platform) => new(platform);

}

/// <summary>
/// Configures how a platform is served: the engine, the ports and the
/// certificates, and what answers there.
/// </summary>
public sealed class LambdaServerBuilder(LambdaPlatform platform)
{
    private IHandlerBuilder? _handler;

    private ServerSettings _settings = new();

    private LambdaSettings? _environment;

    #region Functionality

    /// <summary>
    /// What answers every request - usually the runner around the site.
    /// </summary>
    public LambdaServerBuilder Handler(IHandlerBuilder handler)
    {
        _handler = handler;
        return this;
    }

    /// <summary>
    /// The engine, the ports and the certificates.
    /// </summary>
    public LambdaServerBuilder Settings(ServerSettings settings)
    {
        _settings = settings;
        return this;
    }

    /// <summary>
    /// The engine, the ports and the certificates as the environment gave
    /// them - and what the operator should know about how it gave them,
    /// which is said once the server starts.
    /// </summary>
    public LambdaServerBuilder Settings(LambdaSettings settings)
    {
        _settings = settings.Server;
        _environment = settings;
        return this;
    }

    /// <summary>
    /// What answers every request: the handler, behind the answers to ACME
    /// challenges where certificates are issued while the server runs - for
    /// every host it receives, a lambda's own domain included.
    /// </summary>
    public IHandler Build()
    {
        var handler = _handler ?? throw new BuilderMissingPropertyException("Handler");

        return _settings.AcmeDirectory is { } acme
             ? Concerns.Chain([new AcmeChallengeConcernBuilder(acme)], handler.Build())
             : handler.Build();
    }

    /// <summary>
    /// Starts the server, the background jobs and the demos, and serves
    /// until the process is asked to stop.
    /// </summary>
    /// <returns>The exit code of the process</returns>
    public async Task<int> RunAsync()
    {
        var root = Build();

        var logger = platform.Loggers.CreateLogger("GenHTTP.Lambda");

        var runs = platform.Services.GetRequiredService<IRunLog>();

        Watch(runs, logger);

        runs.Report(logger);

        _environment?.Report(logger);

        var settings = _settings;

        var host = CreateHost(settings).Handler(root)
                                      .Platform(platform);

        // the certificate is loaded up front so a bad one stops the server here rather
        // than on the first handshake, and it stays alive to serve renewals afterwards
        using var certificates = settings.Secure ? new CertificateLoader(settings, platform.Options.HostingUrl, platform.Loggers.CreateLogger<CertificateLoader>()) : null;

        host.Bind(IPAddress.Any, settings.Port, settings.Protocols);

        if (certificates != null)
        {
            host.Bind(IPAddress.Any, settings.SecurePort, certificates, httpProtocols: settings.Protocols);
        }

        await host.StartAsync();

        platform.StartBackgroundJobs();

        // after the server is up: the demos have to be compiled, and doing it first
        // would be seconds spent refusing connections
        platform.SeedDemos();

        if (certificates != null)
        {
            logger.LogInformation("GenHTTP Lambda is listening on port {Port} and TLS port {SecurePort} ({Engine}), storing data in '{Directory}'",
                                  settings.Port, settings.SecurePort, settings.Engine, platform.Options.DataDirectory);
        }
        else
        {
            logger.LogInformation("GenHTTP Lambda is listening on port {Port} ({Engine}), storing data in '{Directory}'",
                                  settings.Port, settings.Engine, platform.Options.DataDirectory);
        }

        await WaitForShutdownAsync(runs);

        logger.LogInformation("Shutting down");

        await host.StopAsync();

        // stamped last: a note without this is a run that did not get to finish, and
        // telling the two apart is the whole point of keeping it
        runs.Stopped();

        return 0;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// An unhandled exception on a thread the runtime does not own ends the
    /// process, and the only trace of it is whatever reached stderr on the way
    /// down - which, in a container that is immediately restarted, is easy to
    /// lose. Written into the note first, so the next run repeats it.
    /// </summary>
    private static void Watch(IRunLog runs, ILogger logger)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception error)
            {
                runs.Faulted(error);
                logger.LogCritical(error, "Unhandled exception, the process is going down");
            }
        };

        // these do not end anything, but a task nobody awaited failing is worth
        // knowing about and is otherwise completely silent
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            logger.LogWarning(e.Exception, "A task failed with nobody waiting on it");
            e.SetObserved();
        };
    }

    private static IServerHost CreateHost(ServerSettings settings) => settings.Engine switch
    {
        LambdaEngine.Kestrel => GenHTTP.Engine.Kestrel.Host.Create(),
        _ => GenHTTP.Engine.Ioxide.Host.Create(options: new GenHTTP.Engine.Ioxide.EngineOptions
        {
            Tcp = new GenHTTP.Engine.Ioxide.TcpTransportOptions { RecvQueueEntries = settings.ReceiveQueueEntries }
        })
    };

    /// <remarks>
    /// Must be awaited rather than returned: the registrations have to stay
    /// alive (and rooted) until the signal actually arrives, and disposing
    /// them while the process keeps running both loses the signal and races
    /// the runtime's handler.
    /// </remarks>
    private static async Task WaitForShutdownAsync(IRunLog runs)
    {
        var shutdown = new TaskCompletionSource();

        using var term = PosixSignalRegistration.Create(PosixSignal.SIGTERM, Handle);
        using var interrupt = PosixSignalRegistration.Create(PosixSignal.SIGINT, Handle);

        AppDomain.CurrentDomain.ProcessExit += (_, _) => shutdown.TrySetResult();

        await shutdown.Task;

        void Handle(PosixSignalContext context)
        {
            context.Cancel = true;

            // as the signal arrives rather than once the stopping is done, so that
            // a run told to stop and then killed partway can be told from one that
            // nothing asked to stop at all
            runs.Signalled();

            shutdown.TrySetResult();
        }
    }

    #endregion

}
