using System.Net;
using System.Text.Json;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;

using Microsoft.EntityFrameworkCore;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Testing;

using GenHTTP.Lambda.Services.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// Runs the entire application against a temporary directory, so tests can talk
/// to it the way the single page application does.
/// </summary>
internal sealed class LambdaFixture : IAsyncDisposable
{

    /// <summary>
    /// Stands in for the built frontend, so the tests do not depend on npm.
    /// </summary>
    internal const string SpaMarkup = "<!doctype html><title>Lambda</title><div id=\"app\"></div>";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    #region Get-/Setters

    /// <summary>
    /// The application under test.
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// The server hosting it.
    /// </summary>
    public TestHost Host { get; }

    /// <summary>
    /// The configuration it was started with.
    /// </summary>
    public LambdaOptions Options { get; }

    /// <summary>
    /// What the server logged, as the panel reads it back.
    /// </summary>
    public LogBook Book => Application.Services.GetRequiredService<LogBook>();

    private ILoggerFactory Loggers { get; }

    public IMetaService Meta => Application.Services.GetRequiredService<IMetaService>();

    public IDeploymentService Deployments => Application.Services.GetRequiredService<IDeploymentService>();

    /// <summary>
    /// Brings the examples into existence, which the application does in the
    /// background at startup and a test has to ask for so it can wait for it.
    /// </summary>
    public ValueTask SeedExamplesAsync()
        => Application.Services.GetRequiredService<ExampleSeeder>().SeedAsync();

    /// <summary>
    /// The editor key of a lambda, which only the installation itself knows
    /// for an example.
    /// </summary>
    public ValueTask<string?> PrivateKeyOfAsync(string publicKey) => Meta.GetPrivateKeyAsync(publicKey);

    /// <summary>
    /// Flags a lambda as one the installation maintains, which is what the
    /// seeder and the maintenance sweeps go by.
    /// </summary>
    public async ValueTask MarkAsExampleAsync(string publicKey)
    {
        var databases = Application.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using var database = await databases.CreateDbContextAsync();

        var entity = await database.Lambdas.FirstAsync(l => l.PublicKey == publicKey);

        entity.IsExample = true;

        await database.SaveChangesAsync();
    }

    private string Root { get; }

    #endregion

    #region Initialization

    private LambdaFixture(string root, LambdaOptions options, Application application, TestHost host, ILoggerFactory loggers)
    {
        Root = root;
        Options = options;
        Application = application;
        Host = host;
        Loggers = loggers;
    }

    /// <summary>
    /// Creates and starts an application with its own database, storage and web root.
    /// </summary>
    /// <param name="configure">Adjusts the options before the application is built</param>
    public static async Task<LambdaFixture> CreateAsync(Func<LambdaOptions, LambdaOptions>? configure = null)
    {
        var root = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        var webRoot = Path.Combine(root, "wwwroot");

        Directory.CreateDirectory(webRoot);

        await File.WriteAllTextAsync(Path.Combine(webRoot, "index.html"), SpaMarkup);

        var options = new LambdaOptions
        {
            DataDirectory = root,
            WebRoot = webRoot
        };

        if (configure != null)
        {
            options = configure(options);
        }

        var book = new LogBook(options.LogHistory);

        // the book rather than nothing, and nothing else: a test that asks what
        // the server logged has to have somewhere for it to have been logged,
        // and a console provider here would print every line of every fixture
        var loggers = LoggerFactory.Create(builder => builder.AddProvider(new LogBookProvider(book)));

        var application = Application.Create(options, loggers, book);

        var host = new TestHost(application.Handler, false, false);

        application.Configure(host.Host);

        await host.StartAsync();

        return new LambdaFixture(root, options, application, host, loggers);
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Runs a request against the application.
    /// </summary>
    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? payload = null, string? accept = null)
    {
        using var request = Host.GetRequest(path, method);

        if (payload != null)
        {
            request.Content = JsonContent.Create(payload, options: Json);
        }

        if (accept != null)
        {
            request.Headers.Add("Accept", accept);
        }

        return await Host.GetResponseAsync(request);
    }

    public Task<HttpResponseMessage> GetAsync(string path, string? accept = null) => SendAsync(HttpMethod.Get, path, accept: accept);

    /// <summary>
    /// Creates a lambda through the API, the way the creation assistant does.
    /// </summary>
    public async Task<LambdaResponse> CreateLambdaAsync(string? publicKey = null, string? template = null)
    {
        using var response = await SendAsync(HttpMethod.Post, "/api/v1/lambdas", new CreateLambdaRequest(publicKey, true, template));

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        return await response.GetContentAsync<LambdaResponse>();
    }

    /// <summary>
    /// A version made of nothing but the given snippet.
    /// </summary>
    public static VersionRequest Version(string code) => new([new LambdaFile(LambdaSource.EntryName, code)]);

    /// <summary>
    /// A question about nothing but the given snippet.
    /// </summary>
    public static CodeRequest Code(string code) => new([new LambdaFile(LambdaSource.EntryName, code)]);

    /// <summary>
    /// Stores the given code and puts it online, expecting it to build.
    /// </summary>
    public async Task<DeploymentOutcomeResponse> DeployAsync(string privateKey, string? code = null)
    {
        if (code != null)
        {
            using var saved = await SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions", LambdaFixture.Version(code));

            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);
        }

        using var response = await SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/deployment/start", new DeploymentRequest(null));

        var deployment = await response.GetContentAsync<DeploymentOutcomeResponse>();

        Assert.IsTrue(deployment.Success, string.Join("; ", deployment.Diagnostics.Select(d => d.Message)));

        return deployment;
    }

    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();

        await Application.DisposeAsync();

        Loggers.Dispose();

        Cleanup();
    }

    /// <summary>
    /// Removes the data of this fixture, except for the assemblies it compiled:
    /// those stay loaded in the process and GenHTTP reads the files behind the
    /// loaded assemblies whenever it generates invocation code.
    /// </summary>
    private void Cleanup()
    {
        try
        {
            foreach (var entry in Directory.GetFileSystemEntries(Root))
            {
                if (entry == Options.AssemblyDirectory)
                {
                    continue;
                }

                if (Directory.Exists(entry))
                {
                    Directory.Delete(entry, true);
                }
                else
                {
                    File.Delete(entry);
                }
            }
        }
        catch (Exception)
        {
            // the temp folder of the machine is cleaned up by the system
        }
    }

    #endregion

}
