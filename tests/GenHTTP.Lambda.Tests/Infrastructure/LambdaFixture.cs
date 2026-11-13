using System.Net;
using System.Net.WebSockets;
using System.Text.Json;

using GenHTTP.Lambda.Administration;
using GenHTTP.Lambda.Api;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Cloud;
using GenHTTP.Lambda.Community;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Testing;

using GenHTTP.Lambda.Editing;
using GenHTTP.Lambda.Execution;
using GenHTTP.Lambda.Mcp;
using GenHTTP.Lambda.Server;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Sources;

using GenHTTP.Modules.Layouting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// Runs the entire application against a temporary directory, so tests can talk
/// to it the way the single page application does.
/// </summary>
/// <remarks>
/// Put together from the components the way the cloud host puts them
/// together (see its <c>Program</c>), on a test host instead of an engine.
/// </remarks>
internal sealed class LambdaFixture : IAsyncDisposable
{

    /// <summary>
    /// Stands in for the built frontend, so the tests do not depend on npm.
    /// </summary>
    internal const string SpaMarkup = "<!doctype html><title>Lambda</title><div id=\"app\"></div>";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    #region Get-/Setters

    /// <summary>
    /// The platform under test.
    /// </summary>
    public LambdaPlatform Platform { get; }

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
    public LogBook Book => Platform.Services.GetRequiredService<LogBook>();

    private ILoggerFactory Loggers { get; }

    public IMetaService Meta => Platform.Services.GetRequiredService<IMetaService>();

    public IDeploymentService Deployments => Platform.Services.GetRequiredService<IDeploymentService>();

    /// <summary>
    /// What a lambda may have in its tier, as the operator set it.
    /// </summary>
    public ILimitsService Limits => Platform.Services.GetRequiredService<ILimitsService>();

    /// <summary>
    /// Brings the demos into existence, which the application does in the
    /// background at startup and a test has to ask for so it can wait for it.
    /// </summary>
    public ValueTask SeedDemosAsync()
        => Platform.Services.GetRequiredService<DemoSeeder>().SeedAsync();

    /// <summary>
    /// Puts a lambda into the demo tier, the way only the seeder would.
    /// </summary>
    public async ValueTask MakeDemoAsync(string publicKey)
    {
        var databases = Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using var database = await databases.CreateDbContextAsync();

        var entity = await database.Lambdas.FirstAsync(l => l.PublicKey == publicKey);

        entity.Tier = LambdaTier.Demo;

        await database.SaveChangesAsync();
    }

    private string Root { get; }

    #endregion

    #region Initialization

    private LambdaFixture(string root, LambdaOptions options, LambdaPlatform platform, TestHost host, ILoggerFactory loggers)
    {
        Root = root;
        Options = options;
        Platform = platform;
        Host = host;
        Loggers = loggers;
    }

    /// <summary>
    /// Creates and starts an application with its own database, storage and web root.
    /// </summary>
    /// <param name="configure">Adjusts the options before the application is built</param>
    /// <param name="server">How it is served, where a test needs that - the answers to ACME challenges, for one</param>
    /// <param name="compiled">
    /// Whether the API is compiled into delegates as the server starts. Off
    /// by default: calling it through reflection is what makes a fixture take
    /// milliseconds rather than a second.
    /// </param>
    public static Task<LambdaFixture> CreateAsync(Func<LambdaOptions, LambdaOptions>? configure = null, ServerSettings? server = null,
                                                  bool compiled = false)
        => CreateAsync(Edition.Cloud, configure, server, compiled);

    /// <summary>
    /// Creates and starts the community edition: put together the way its
    /// host does, with the lambdas below /lambda/ and none of what only the
    /// cloud says about itself - no terms, no contact, no legal pages.
    /// </summary>
    public static Task<LambdaFixture> CreateCommunityAsync(Func<LambdaOptions, LambdaOptions>? configure = null)
        => CreateAsync(Edition.Community, configure, null, false);

    private static async Task<LambdaFixture> CreateAsync(Edition edition, Func<LambdaOptions, LambdaOptions>? configure, ServerSettings? server,
                                                         bool compiled)
    {
        var root = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        var options = edition == Edition.Cloud
                    ? CloudInstallation.Of(new LambdaOptions { DataDirectory = root })
                    : new LambdaOptions { DataDirectory = root, Addressing = LambdaAddressing.Paths };

        if (configure != null)
        {
            options = configure(options);
        }

        // a stand-in for each component's bundle, where a test did not write
        // what it needs of a build there already (see BundleOf)
        foreach (var bundle in Bundles)
        {
            var index = Path.Combine(BundleOf(options, bundle), "index.html");

            if (!File.Exists(index))
            {
                await File.WriteAllTextAsync(index, SpaMarkup);
            }
        }

        var book = new LogBook(options.LogHistory);

        // the book rather than nothing, and nothing else: a test that asks what
        // the server logged has to have somewhere for it to have been logged,
        // and a console provider here would print every line of every fixture
        var loggers = LoggerFactory.Create(builder => builder.AddProvider(new LogBookProvider(book)));

        var platform = LambdaPlatform.Create()
                                     .Options(options)
                                     .Logging(loggers, book)
                                     .Build();

        var site = Layout.Create()
                         .Add(PlatformRoutes.Api, RestApi.Create(platform).Compiled(compiled))
                         .Add(PlatformRoutes.Mcp, McpServer.Create(platform))
                         .Add(PlatformRoutes.Editor, Editor.Create(platform).Bundle(BundleOf(options, "editor")))
                         .Add(PlatformRoutes.Admin, AdminPanel.Create().Bundle(BundleOf(options, "admin")))
                         .Add(SourcePages.Create(platform).Bundle(BundleOf(options, "sources")))
                         .Add(edition == Edition.Cloud
                                  ? Website.Create(platform).Bundle(BundleOf(options, "site"))
                                  : LandingPage.Create(platform).Bundle(BundleOf(options, "site")));

        var handler = LambdaServer.Create(platform)
                                  .Settings(server ?? new ServerSettings())
                                  .Handler(Runner.Create(platform).Site(site))
                                  .Build();

        var host = new TestHost(handler, false, false);

        host.Host.Platform(platform);

        await host.StartAsync();

        return new LambdaFixture(root, options, platform, host, loggers);
    }

    #endregion

    private enum Edition { Cloud, Community }

    #region Functionality

    /// <summary>
    /// A request for an address, the way a browser asks for it: a path of the
    /// platform, or a full address - <c>http://quiz.localhost/api</c> - whose
    /// host is sent as such, to the server under test.
    /// </summary>
    /// <remarks>
    /// The lambdas answer at subdomains of the hosting domain, which in a
    /// fixture is localhost as on a laptop: <c>http://quiz.localhost:8080/</c>.
    /// The server tells hosts apart by the Host header, so that is what a full
    /// address sets - and the request goes to the port of the fixture.
    /// </remarks>
    public HttpRequestMessage Request(string address, HttpMethod? method = null)
    {
        if (!address.StartsWith("http://", StringComparison.Ordinal) && !address.StartsWith("https://", StringComparison.Ordinal))
        {
            return Host.GetRequest(address, method ?? HttpMethod.Get);
        }

        var url = new Uri(address);

        var request = Host.GetRequest(url.PathAndQuery, method ?? HttpMethod.Get);

        request.Headers.Host = url.Host;

        return request;
    }

    /// <summary>
    /// Opens a websocket at an address, the way <see cref="Request"/> sends a
    /// request there: to the port of the fixture, with the host of the address.
    /// </summary>
    public async Task ConnectAsync(ClientWebSocket client, string address, CancellationToken cancellation)
    {
        using var probe = Request(address);

        if (probe.Headers.Host is { } host)
        {
            client.Options.SetRequestHeader("Host", host);
        }

        await client.ConnectAsync(new UriBuilder(probe.RequestUri!) { Scheme = "ws" }.Uri, cancellation);
    }

    /// <summary>
    /// Runs a request against the application.
    /// </summary>
    /// <param name="address">A path of the platform, or the full address of a lambda (see <see cref="Request"/>)</param>
    /// <param name="host">The Host header to send, to reach a lambda at a domain of its own</param>
    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string address, object? payload = null, string? accept = null,
                                                     string? host = null)
    {
        using var request = Request(address, method);

        if (host != null)
        {
            request.Headers.Host = host;
        }

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

    public Task<HttpResponseMessage> GetAsync(string address, string? accept = null, string? host = null)
        => SendAsync(HttpMethod.Get, address, accept: accept, host: host);

    /// <summary>
    /// The administration token of an installation made with <see cref="WithPanel"/>.
    /// </summary>
    /// <summary>
    /// The components with a bundle of their own.
    /// </summary>
    private static readonly string[] Bundles = ["site", "editor", "admin", "sources"];

    /// <summary>
    /// Where the fixture serves a component's bundle from: "site" for the
    /// cloud's pages, "editor", "admin" or "sources" - for a test to put what
    /// it needs of a build there before the application starts.
    /// </summary>
    public static string BundleOf(LambdaOptions options, string component)
        => Directory.CreateDirectory(Path.Combine(options.DataDirectory, "bundles", component)).FullName;

    public const string OperatorToken = "a-token-nobody-would-guess";

    /// <summary>
    /// Configures an installation with an administration panel, which is what
    /// the server's own figures and log are served to.
    /// </summary>
    public static LambdaOptions WithPanel(LambdaOptions options) => options with { AdminToken = OperatorToken };

    /// <summary>
    /// Reads what only the operator may read, with the token of <see cref="WithPanel"/>.
    /// </summary>
    public async Task<HttpResponseMessage> GetAsOperatorAsync(string path)
    {
        using var request = Host.GetRequest(path, HttpMethod.Get);

        request.Headers.Add("X-Admin-Token", OperatorToken);

        return await Host.GetResponseAsync(request);
    }

    /// <summary>
    /// Moves a lambda to a tier, which only an administrator can do.
    /// </summary>
    public void ChangeTier(string privateKey, LambdaTier tier) => Meta.ChangeTier(privateKey, tier);

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

        await Platform.DisposeAsync();

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
