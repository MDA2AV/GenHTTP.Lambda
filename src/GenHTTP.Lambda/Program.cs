using System.Net;
using System.Runtime.InteropServices;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Diagnostics;

using Microsoft.Extensions.Logging;

var options = LambdaOptions.FromEnvironment();

/*
 * The last few thousand lines of this run, so that an operator with the token
 * can read them in the panel instead of needing a shell on the host.
 *
 * It is built here rather than in the container because the logger factory
 * below has to write into it, and that is made before there is a container to
 * take it from. Everything still goes to stdout exactly as it did.
 */
var book = new LogBook(options.LogHistory, options.LogMemory, options.RepeatWindow);

/*
 * A note on disk about how this run is going, so that the next one can say how
 * this one ended. The server cannot answer "did it just crash" about itself -
 * whatever it would have said went down with it.
 */
var runs = new RunLog(options.DataDirectory);

/*
 * The real console, taken before anything is put in front of it.
 *
 * Records are written here by the provider below. Everything else that reaches
 * Console.Out afterwards - which is the engine saying what its reactors are
 * doing, and whatever a lambda prints - goes through the tee and into the book
 * instead. Keeping the two apart by which writer they hold, rather than by a
 * check at the point of writing, is what stops a record being kept twice.
 */
var console = Console.Out;

var floor = options.Development ? LogLevel.Debug : LogLevel.Information;

using var loggers = LoggerFactory.Create(builder =>
{
    builder.AddProvider(new LogBookProvider(book, console, floor));

    builder.SetMinimumLevel(floor);

    // the query log of entity framework would drown out everything else
    builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
});

var logger = loggers.CreateLogger("GenHTTP.Lambda");

/*
 * Lambdas and the engine both print to this console, so the console is where
 * they are told apart: a write while a lambda is being served is filed under
 * that lambda, and everything else under the process.
 *
 * Not conditional. LAMBDA_LOG_LAMBDA_OUTPUT decides whether a stranger's
 * print is gathered, which is a question about holding somebody else's output
 * in memory; what the engine says about its own reactors is the server
 * talking about itself and is the first thing wanted when connections start
 * misbehaving. The switch is applied where a lambda is marked, not here.
 *
 * After the logger factory, deliberately. The provider above holds the writer
 * it was given; this replaces the one everything else will find.
 */
LambdaOutput.Adopt(book);

ConsoleTee.Install();

runs.Report(logger);

/*
 * An unhandled exception on a thread the runtime does not own ends the
 * process, and the only trace of it is whatever reached stderr on the way
 * down - which, in a container that is immediately restarted, is easy to lose.
 * Written into the note first, so the next run repeats it.
 */
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

await using var application = Application.Create(options, loggers, book, runs);

var host = application.Configure(CreateHost(options.Engine));

// the certificate is loaded up front so a bad one stops the server here rather
// than on the first handshake, and it stays alive to serve renewals afterwards
using var certificates = options.Secure ? new CertificateLoader(options, loggers.CreateLogger<CertificateLoader>()) : null;

if (certificates != null)
{
    host.Bind(IPAddress.Any, options.Port, options.Protocols);
    host.Bind(IPAddress.Any, options.SecurePort, certificates, httpProtocols: options.Protocols);
}
else
{
    host.Bind(IPAddress.Any, options.Port, options.Protocols);
}

await host.StartAsync();

application.StartBackgroundJobs();

// after the server is up: the examples have to be compiled, and doing it first
// would be seconds spent refusing connections
application.SeedExamples();

if (certificates != null)
{
    logger.LogInformation("GenHTTP Lambda is listening on port {Port} and TLS port {SecurePort} ({Engine}), storing data in '{Directory}'", options.Port, options.SecurePort, options.Engine, options.DataDirectory);
}
else
{
    logger.LogInformation("GenHTTP Lambda is listening on port {Port} ({Engine}), storing data in '{Directory}'", options.Port, options.Engine, options.DataDirectory);
}

await WaitForShutdownAsync(runs);

logger.LogInformation("Shutting down");

await host.StopAsync();

// stamped last: a note without this is a run that did not get to finish, and
// telling the two apart is the whole point of keeping it
runs.Stopped();

return 0;

static IServerHost CreateHost(LambdaEngine engine) => engine switch
{
    LambdaEngine.Kestrel => GenHTTP.Engine.Kestrel.Host.Create(),
    _ => GenHTTP.Engine.Ioxide.Host.Create()
};

// must be awaited rather than returned: the registrations have to stay alive
// (and rooted) until the signal actually arrives, and disposing them while the
// process keeps running both loses the signal and races the runtime's handler
static async Task WaitForShutdownAsync(RunLog runs)
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
