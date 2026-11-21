using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Integrations.Sealed;
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
    /// than report the run before it. Awaited, since it reaches what the
    /// integrations keep: a database server is asked before the platform
    /// answers anything.
    /// </remarks>
    public async ValueTask<LambdaPlatform> BuildAsync(CancellationToken cancellation = default)
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

        LambdaPlatform? platform = null;

        try
        {
            integrations = await BuildIntegrationsAsync(new IntegrationContext(_options.DataDirectory, loggers), cancellation);

            await Migrator.MigrateAsync(integrations.Database, loggers.CreateLogger<LambdaPlatform>(), cancellation);

            platform = new LambdaPlatform(_options, loggers, _loggers == null, book, runs, integrations);

            await platform.PrepareAsync(cancellation);

            return platform;
        }
        catch (Exception e)
        {
            if (platform != null)
            {
                // its services and its integrations with them, but the
                // loggers it was handed, which are let go of below
                await platform.DisposeAsync(false);
            }
            else if (integrations != null)
            {
                await integrations.DisposeAsync();
            }

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
    /// Builds what the host chose, and the platform's own where it chose
    /// nothing, then prepares each - the database, the files, the secrets.
    /// </summary>
    /// <remarks>
    /// Building only configures, so all are built before the first reaches
    /// its infrastructure. One that fails to build or prepare stops the
    /// platform from starting, and every one built is let go of again,
    /// prepared or not.
    /// </remarks>
    private async ValueTask<PlatformIntegrations> BuildIntegrationsAsync(IntegrationContext context, CancellationToken cancellation)
    {
        var built = new List<IIntegration>();

        try
        {
            var database = Keep(built, (_database ?? SqliteDatabase.Create()).Build(context));

            var files = Keep(built, (_files ?? LocalFiles.Create()).Build(context));

            var secrets = _secrets is { } chosen ? Keep(built, chosen.Build(context)) : null;

            foreach (var integration in built)
            {
                await integration.PrepareAsync(cancellation);
            }

            // a host naming the platform's own has not chosen another
            return new PlatformIntegrations(database, files, secrets);
        }
        catch
        {
            await PlatformIntegrations.DisposeAsync(built);
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
