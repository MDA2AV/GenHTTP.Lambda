using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Administration;
using GenHTTP.Lambda.Api;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Community;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Editing;
using GenHTTP.Lambda.Execution;
using GenHTTP.Lambda.Mcp;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Sources;

using GenHTTP.Modules.Layouting;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// Several installations of the community edition in one process and behind
/// one server, the way a host that runs an installation per customer puts them
/// together: on a runtime they share, each at a host of its own.
/// </summary>
internal sealed class SeveralPlatforms : IAsyncDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly ConcurrentDictionary<string, Installation> _installations = new(StringComparer.Ordinal);

    #region Get-/Setters

    public LambdaRuntime Runtime { get; }

    private TestHost Host { get; }

    private Hosts Router { get; }

    private string Root { get; }

    #endregion

    #region Initialization

    private SeveralPlatforms(string root, LambdaRuntime runtime)
    {
        Root = root;
        Runtime = runtime;

        Router = new Hosts(_installations);
        Host = new TestHost(Router, false, false);
    }

    /// <summary>
    /// Starts a server with nothing to serve yet.
    /// </summary>
    public static async Task<SeveralPlatforms> CreateAsync(Func<RuntimeOptions, RuntimeOptions>? configure = null)
    {
        var root = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        // fetches nothing: the tests put what they need into the tables
        var options = new RuntimeOptions { DataDirectory = Path.Combine(root, "runtime"), Geo = false, GeoPlaces = false, CrawlerLists = false };

        var several = new SeveralPlatforms(root, LambdaRuntime.Create().Options(configure?.Invoke(options) ?? options).Build());

        foreach (var bundle in new[] { "community", "editor", "admin", "sources" })
        {
            await File.WriteAllTextAsync(Path.Combine(Directory.CreateDirectory(several.BundleOf(bundle)).FullName, "index.html"), LambdaFixture.SpaMarkup);
        }

        await several.Host.StartAsync();

        return several;
    }

    #endregion

    #region Lifecycle of an installation

    /// <summary>
    /// Builds an installation and has it served, while the server is running.
    /// </summary>
    public async Task<LambdaPlatform> StartAsync(string name, Func<LambdaOptions, LambdaOptions>? configure = null)
    {
        var options = new LambdaOptions
        {
            DataDirectory = DataOf(name),
            Addressing = LambdaAddressing.Paths,
            PublicUrl = $"http://{name}.localhost:{Host.Port}",
            AdminToken = TokenOf(name),
            Profile = InstallationProfile.Enterprise
        };

        options = configure?.Invoke(options) ?? options;

        var book = new LogBook(options.LogHistory);

        var loggers = LoggerFactory.Create(builder => builder.AddProvider(new LogBookProvider(book)));

        var platform = await LambdaPlatform.Create()
                                           .Options(options)
                                           .Logging(loggers, book)
                                           .Runtime(Runtime)
                                           .BuildAsync();

        var site = Layout.Create()
                         .Add(PlatformRoutes.Api, RestApi.Create(platform).Compiled(false))
                         .Add(PlatformRoutes.Mcp, McpServer.Create(platform))
                         .Add(PlatformRoutes.Editor, Editor.Create(platform).Bundle(BundleOf("editor")))
                         .Add(PlatformRoutes.Admin, AdminPanel.Create(platform).Bundle(BundleOf("admin")).Add(OperatorMcp.Create(platform)))
                         .Add(SourcePages.Create(platform).Bundle(BundleOf("sources")))
                         .Add(LandingPage.Create(platform).Bundle(BundleOf("community")));

        var handler = PlatformHandler.Create(platform)
                                     .Handler(Runner.Create(platform).Site(site))
                                     .Build();

        await handler.PrepareAsync(Router.Server);

        _installations[name] = new Installation(platform, handler, loggers);

        return platform;
    }

    /// <summary>
    /// Takes an installation away while the server, and the others, go on.
    /// </summary>
    public async Task StopAsync(string name)
    {
        if (_installations.TryRemove(name, out var installation))
        {
            await installation.Platform.DisposeAsync();

            installation.Loggers.Dispose();
        }
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Where an installation keeps its data.
    /// </summary>
    public string DataOf(string name) => Path.Combine(Root, name);

    public static string TokenOf(string name) => $"the-token-of-{name}";

    public LambdaPlatform this[string name] => _installations[name].Platform;

    /// <summary>
    /// What an installation logged, as its panel reads it back.
    /// </summary>
    public IReadOnlyList<LogLine> LogOf(string name)
        => _installations[name].Platform.Services.GetRequiredService<LogBook>().Read(0, null, LogLevel.Trace, 5000).Lines;

    /// <param name="operatorOf">Whose operator asks, if one does</param>
    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string name, string path, object? payload = null, string? operatorOf = null)
    {
        using var request = Host.GetRequest(path, method);

        request.Headers.Host = $"{name}.localhost";

        if (operatorOf != null)
        {
            request.Headers.Add("X-Admin-Token", TokenOf(operatorOf));
        }

        if (payload != null)
        {
            request.Content = JsonContent.Create(payload, options: Json);
        }

        return await Host.GetResponseAsync(request);
    }

    public Task<HttpResponseMessage> GetAsync(string name, string path, string? operatorOf = null) => SendAsync(HttpMethod.Get, name, path, null, operatorOf);

    /// <summary>
    /// What a lambda of an installation answers.
    /// </summary>
    public async Task<string> AskAsync(string name, string key)
    {
        using var response = await GetAsync(name, $"/lambda/{key}/");

        return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : $"{(int)response.StatusCode}";
    }

    /// <summary>
    /// Creates a lambda and puts it online, as an agent would.
    /// </summary>
    /// <returns>Its editor key</returns>
    public async Task<string> DeployAsync(string name, string key, params LambdaFile[] files)
    {
        using var created = await SendAsync(HttpMethod.Post, name, "/api/v1/lambdas", new CreateLambdaRequest(key, true));

        Assert.AreEqual(System.Net.HttpStatusCode.Created, created.StatusCode, await created.Content.ReadAsStringAsync());

        var lambda = await created.GetContentAsync<LambdaResponse>();

        using var saved = await SendAsync(HttpMethod.Post, name, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new VersionRequest(files));

        Assert.AreEqual(System.Net.HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        using var started = await SendAsync(HttpMethod.Post, name, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/start", new DeploymentRequest(null));

        var outcome = await started.GetContentAsync<DeploymentOutcomeResponse>();

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));

        return lambda.PrivateKey;
    }

    public Task<string> DeployAsync(string name, string key, string code) => DeployAsync(name, key, new LambdaFile(LambdaSource.EntryName, code));

    private string BundleOf(string component) => Path.Combine(Root, "bundles", component);

    #endregion

    #region Lifecycle

    public async ValueTask DisposeAsync()
    {
        foreach (var name in _installations.Keys)
        {
            await StopAsync(name);
        }

        await Host.DisposeAsync();

        await Runtime.DisposeAsync();

        // the data of every installation is removed, as a host removes that of
        // a customer who left; the assemblies stay, see CompiledAssemblies
        foreach (var directory in Directory.GetDirectories(Root).Where(d => Path.GetFileName(d) != "runtime"))
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch (IOException)
            {
                // the temp folder of the machine is cleaned up by the system
            }
        }
    }

    #endregion

    private sealed record Installation(LambdaPlatform Platform, IHandler Handler, ILoggerFactory Loggers);

    /// <summary>
    /// Hands a request to the installation its host names.
    /// </summary>
    private sealed class Hosts(ConcurrentDictionary<string, Installation> installations) : IHandler
    {
        private IServer? _server;

        public IServer Server => _server ?? throw new InvalidOperationException("The server is not running.");

        public ValueTask PrepareAsync(IServer server)
        {
            _server = server;

            return ValueTask.CompletedTask;
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var name = request.Header.Headers.GetEntry("Host")?.Split('.')[0] ?? "";

            return installations.TryGetValue(name, out var installation)
                ? installation.Handler.HandleAsync(request)
                : new(request.Respond().Status(ResponseStatus.NotFound).Build());
        }
    }

}
