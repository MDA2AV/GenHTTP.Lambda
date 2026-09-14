using System.Net;
using System.Runtime.InteropServices;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;

using Microsoft.Extensions.Logging;

var options = LambdaOptions.FromEnvironment();

using var loggers = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(console =>
    {
        console.SingleLine = true;
        console.TimestampFormat = "HH:mm:ss ";
    });

    builder.SetMinimumLevel(options.Development ? LogLevel.Debug : LogLevel.Information);

    // the query log of entity framework would drown out everything else
    builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
});

var logger = loggers.CreateLogger("GenHTTP.Lambda");

await using var application = Application.Create(options, loggers);

var host = application.Configure(CreateHost(options.Engine));

// the certificate is loaded up front so a bad one stops the server here rather
// than on the first handshake, and it stays alive to serve renewals afterwards
using var certificates = options.Secure ? new CertificateLoader(options, loggers.CreateLogger<CertificateLoader>()) : null;

if (certificates != null)
{
    host.Bind(IPAddress.Any, options.Port);
    host.Bind(IPAddress.Any, options.SecurePort, certificates);
}
else
{
    host.Port(options.Port);
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

await WaitForShutdownAsync();

logger.LogInformation("Shutting down");

await host.StopAsync();

return 0;

static IServerHost CreateHost(LambdaEngine engine) => engine switch
{
    LambdaEngine.Kestrel => GenHTTP.Engine.Kestrel.Host.Create(),
    _ => GenHTTP.Engine.Ioxide.Host.Create()
};

// must be awaited rather than returned: the registrations have to stay alive
// (and rooted) until the signal actually arrives, and disposing them while the
// process keeps running both loses the signal and races the runtime's handler
static async Task WaitForShutdownAsync()
{
    var shutdown = new TaskCompletionSource();

    using var term = PosixSignalRegistration.Create(PosixSignal.SIGTERM, Handle);
    using var interrupt = PosixSignalRegistration.Create(PosixSignal.SIGINT, Handle);

    AppDomain.CurrentDomain.ProcessExit += (_, _) => shutdown.TrySetResult();

    await shutdown.Task;

    void Handle(PosixSignalContext context)
    {
        context.Cancel = true;
        shutdown.TrySetResult();
    }
}
