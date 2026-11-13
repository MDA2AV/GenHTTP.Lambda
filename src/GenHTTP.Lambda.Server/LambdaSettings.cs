using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Hosting;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Server;

/// <summary>
/// The configuration of an installation as its environment gives it: the
/// <c>LAMBDA_*</c> variables, read once on startup.
/// </summary>
/// <remarks>
/// The only place below the hosts that reads the environment. Everything
/// else is handed what it needs by the builder it is made with, so a
/// component runs the same in a test, in a host that is configured some
/// other way, and here.
/// </remarks>
/// <param name="Platform">What the platform and its components are built with</param>
/// <param name="Server">How the host serves it</param>
public sealed record LambdaSettings(LambdaOptions Platform, ServerSettings Server)
{

    #region Functionality

    /// <summary>
    /// Reads the configuration from the environment, falling back to the defaults.
    /// </summary>
    /// <param name="addressing">
    /// How the lambdas answer where the environment does not say: an
    /// installation given <c>LAMBDA_HOSTING_URL</c> has them answer at its
    /// subdomains whatever this is, and <c>LAMBDA_ADDRESSING</c> decides
    /// over both.
    /// </param>
    public static LambdaSettings FromEnvironment(LambdaAddressing addressing = LambdaAddressing.Subdomains)
    {
        var defaults = new LambdaOptions();
        var server = new ServerSettings();

        var hosting = ReadOptional("LAMBDA_HOSTING_URL")?.TrimEnd('/');

        var addressed = hosting != null ? LambdaAddressing.Subdomains : addressing;

        var platform = new LambdaOptions
        {
            Port = (ushort)ReadInt("LAMBDA_PORT", defaults.Port),
            Development = ReadBool("LAMBDA_DEVELOPMENT", defaults.Development),
            AgentUrl = ReadOptional("LAMBDA_AGENT_URL"),
            AgentToken = ReadOptional("LAMBDA_AGENT_TOKEN"),
            AgentFablePassword = ReadOptional("LAMBDA_AGENT_FABLE_PASSWORD"),
            AgentBuildsPerDay = ReadInt("LAMBDA_AGENT_BUILDS_PER_DAY", defaults.AgentBuildsPerDay),
            DataDirectory = Path.GetFullPath(ReadString("LAMBDA_DATA_DIRECTORY", defaults.DataDirectory)),
            DeploymentLifetime = ReadSpan("LAMBDA_DEPLOYMENT_LIFETIME_HOURS", defaults.DeploymentLifetime),
            Retention = ReadSpan("LAMBDA_RETENTION_HOURS", defaults.Retention),
            MaintenanceInterval = ReadSpan("LAMBDA_MAINTENANCE_INTERVAL_HOURS", defaults.MaintenanceInterval),
            BuildBytes = Combined(defaults.BuildBytes, ("LAMBDA_MAX_CODE_LENGTH", LambdaOptions.RetiredDefaults["code-characters"].Free),
                                  ("LAMBDA_MAX_ASSET_BYTES", LambdaOptions.RetiredDefaults["asset-bytes"].Free)),
            PremiumBuildBytes = Combined(defaults.PremiumBuildBytes, ("LAMBDA_PREMIUM_MAX_CODE_LENGTH", LambdaOptions.RetiredDefaults["code-characters"].Premium),
                                         ("LAMBDA_PREMIUM_MAX_ASSET_BYTES", LambdaOptions.RetiredDefaults["asset-bytes"].Premium)),
            DataBytes = Combined(defaults.DataBytes, ("LAMBDA_WORKSPACE_BYTES", LambdaOptions.RetiredDefaults["workspace-bytes"].Free),
                                 ("LAMBDA_DATABASE_BYTES", LambdaOptions.RetiredDefaults["database-bytes"].Free)),
            PremiumDataBytes = Combined(defaults.PremiumDataBytes, ("LAMBDA_PREMIUM_WORKSPACE_BYTES", LambdaOptions.RetiredDefaults["workspace-bytes"].Premium),
                                        ("LAMBDA_PREMIUM_DATABASE_BYTES", LambdaOptions.RetiredDefaults["database-bytes"].Premium)),
            MaxShowcaseImageBytes = ReadInt("LAMBDA_MAX_SHOWCASE_IMAGE_BYTES", defaults.MaxShowcaseImageBytes),
            MaxVersions = ReadInt("LAMBDA_MAX_VERSIONS", defaults.MaxVersions),
            MaxFeatures = ReadInt("LAMBDA_MAX_FEATURES", defaults.MaxFeatures),
            SourceCacheBytes = ReadLong("LAMBDA_SOURCE_CACHE_BYTES", defaults.SourceCacheBytes),
            GitMaxPushBytes = ReadLong("LAMBDA_GIT_MAX_PUSH_BYTES", defaults.GitMaxPushBytes),
            RateLimit = ReadInt("LAMBDA_RATE_LIMIT", defaults.RateLimit),
            MaxConcurrency = ReadInt("LAMBDA_MAX_CONCURRENCY", defaults.MaxConcurrency),
            ExecutionTimeout = TimeSpan.FromSeconds(ReadInt("LAMBDA_EXECUTION_TIMEOUT_SECONDS", (int)defaults.ExecutionTimeout.TotalSeconds)),
            AdminToken = ReadOptional("LAMBDA_ADMIN_TOKEN"),
            TelemetryInterval = TimeSpan.FromSeconds(ReadInt("LAMBDA_TELEMETRY_INTERVAL_SECONDS", (int)defaults.TelemetryInterval.TotalSeconds)),
            TelemetrySamples = ReadInt("LAMBDA_TELEMETRY_SAMPLES", defaults.TelemetrySamples),
            AnalyticsInterval = TimeSpan.FromSeconds(ReadInt("LAMBDA_ANALYTICS_INTERVAL_SECONDS", (int)defaults.AnalyticsInterval.TotalSeconds)),
            CrawlerLists = ReadBool("LAMBDA_ANALYTICS_CRAWLER_LISTS", defaults.CrawlerLists),
            CrawlerRefresh = TimeSpan.FromHours(ReadInt("LAMBDA_ANALYTICS_CRAWLER_REFRESH_HOURS", (int)defaults.CrawlerRefresh.TotalHours)),
            LogHistory = ReadInt("LAMBDA_LOG_HISTORY", defaults.LogHistory),
            LogMemory = ReadInt("LAMBDA_LOG_MEMORY_MB", defaults.LogMemory),
            CaptureLambdaOutput = ReadBool("LAMBDA_LOG_LAMBDA_OUTPUT", defaults.CaptureLambdaOutput),
            MaxOutputLines = ReadInt("LAMBDA_LOG_MAX_LINES_PER_REQUEST", defaults.MaxOutputLines),
            LogClientAddress = ReadBool("LAMBDA_LOG_CLIENT_ADDRESS", defaults.LogClientAddress),
            UnblockableNetworks = ReadNetworks("LAMBDA_UNBLOCKABLE_NETWORKS", defaults.UnblockableNetworks),
            RepeatWindow = TimeSpan.FromSeconds(ReadInt("LAMBDA_LOG_REPEAT_WINDOW_SECONDS", (int)defaults.RepeatWindow.TotalSeconds)),
            Geo = ReadBool("LAMBDA_LOG_GEO", defaults.Geo),
            GeoPlaces = ReadBool("LAMBDA_LOG_GEO_PLACES", defaults.GeoPlaces),
            GeoRefresh = TimeSpan.FromHours(ReadInt("LAMBDA_LOG_GEO_REFRESH_HOURS", (int)defaults.GeoRefresh.TotalHours)),
            SecretsKey = ReadOptional("LAMBDA_SECRETS_KEY"),
            McpOrigins = (ReadOptional("LAMBDA_MCP_ORIGINS") ?? "")
                         .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            Contact = ReadOptional("LAMBDA_CONTACT"),
            AbuseContact = ReadOptional("LAMBDA_ABUSE_CONTACT"),
            PublicUrl = ReadOptional("LAMBDA_PUBLIC_URL")?.TrimEnd('/'),
            HostingUrl = hosting,
            Addressing = Enum.TryParse<LambdaAddressing>(ReadOptional("LAMBDA_ADDRESSING"), true, out var chosen) ? chosen : addressed
        };

        var serving = new ServerSettings
        {
            Port = platform.Port,
            Engine = ReadEngine("LAMBDA_ENGINE", server.Engine),
            Protocols = ReadProtocols("LAMBDA_HTTP_PROTOCOLS", server.Protocols),
            ReceiveQueueEntries = ReadInt("LAMBDA_RECEIVE_QUEUE_ENTRIES", server.ReceiveQueueEntries),
            SecurePort = (ushort)ReadInt("LAMBDA_TLS_PORT", server.SecurePort),
            CertificatePath = ReadOptional("LAMBDA_CERTIFICATE"),
            CertificateKeyPath = ReadOptional("LAMBDA_CERTIFICATE_KEY"),
            CertificatePassword = ReadOptional("LAMBDA_CERTIFICATE_PASSWORD"),
            CertificateDirectory = ReadOptional("LAMBDA_CERTIFICATE_DIRECTORY"),
            AcmeDirectory = ReadOptional("LAMBDA_ACME_DIRECTORY")
        };

        return new LambdaSettings(platform, serving);
    }

    /// <summary>
    /// Says what about the configuration the operator should know: what is
    /// read from where it no longer belongs, and what was left out.
    /// </summary>
    public void Report(ILogger logger)
    {
        // the limits of the tiers moved to the administration panel; the variables
        // still give their defaults for this release, and are ignored after it
        foreach (var variable in MovedToPanel())
        {
            logger.LogWarning("Read {Variable} from the environment, which is deprecated: the limits of the tiers are set in the administration panel, and a value saved there wins", variable);
        }

        // an entry that names no origin is left out rather than keep the server from
        // starting, and said here so the operator finds out
        foreach (var entry in Platform.McpOrigins.Where(e => McpOrigins.Of(e) == null))
        {
            logger.LogWarning("Ignored {Entry} in LAMBDA_MCP_ORIGINS, which names neither a host nor an origin such as https://genhttp.dev", entry);
        }

        // an installation that names where it is found, and not where its lambdas
        // are, hands its visitors addresses below localhost
        if (Platform.Addressing == LambdaAddressing.Subdomains && Platform.PublicUrl != null && Platform.HostingUrl == null)
        {
            logger.LogWarning("LAMBDA_PUBLIC_URL is set and LAMBDA_HOSTING_URL is not, so the lambdas answer below localhost: set it to the address their subdomains are below, such as https://genhttp.run");
        }

        // fine on a laptop, and what a container is started with first - but
        // the addresses handed out elsewhere would lead nowhere
        if (Platform.Addressing == LambdaAddressing.Paths && Platform.PublicUrl == null)
        {
            logger.LogInformation("LAMBDA_PUBLIC_URL is not set, so the addresses of the lambdas read http://localhost:{Port}/lambda/: set it to where the installation is reached, such as https://lambda.example.com", Platform.Port);
        }
    }

    /// <summary>
    /// The environment variables that set a limit of the tiers, which the
    /// administration panel sets now.
    /// </summary>
    public static IReadOnlyList<string> LimitVariables { get; } =
    [
        "LAMBDA_MAX_CODE_LENGTH", "LAMBDA_PREMIUM_MAX_CODE_LENGTH", "LAMBDA_MAX_ASSET_BYTES", "LAMBDA_PREMIUM_MAX_ASSET_BYTES",
        "LAMBDA_WORKSPACE_BYTES", "LAMBDA_PREMIUM_WORKSPACE_BYTES", "LAMBDA_DATABASE_BYTES", "LAMBDA_PREMIUM_DATABASE_BYTES",
        "LAMBDA_DEPLOYMENT_LIFETIME_HOURS", "LAMBDA_RETENTION_HOURS", "LAMBDA_MAX_VERSIONS", "LAMBDA_MAX_FEATURES",
        "LAMBDA_MAX_SHOWCASE_IMAGE_BYTES", "LAMBDA_RATE_LIMIT", "LAMBDA_AGENT_BUILDS_PER_DAY"
    ];

    /// <summary>
    /// What the variables of the four allowances that became two set: the
    /// characters of code and the bytes of assets, which a version's build
    /// allowance counts together now, and the room of the workspace and of
    /// the database, which share the data allowance.
    /// </summary>
    /// <remarks>
    /// Only where one of them is set, as the sum of both - the one that is
    /// not set counting as its old default - so an installation that raised
    /// one keeps what it raised until the panel says otherwise.
    /// </remarks>
    private static long Combined(long fallback, (string Name, long Before) first, (string Name, long Before) second)
    {
        var one = ReadOptional(first.Name) is { } a && long.TryParse(a, out var x) ? x : (long?)null;
        var other = ReadOptional(second.Name) is { } b && long.TryParse(b, out var y) ? y : (long?)null;

        return one == null && other == null ? fallback : (one ?? first.Before) + (other ?? second.Before);
    }

    /// <summary>
    /// Which of <see cref="LimitVariables"/> are set in this environment.
    /// </summary>
    public static IReadOnlyList<string> MovedToPanel()
        => [.. LimitVariables.Where(v => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(v)))];

    private static string ReadString(string key, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);

        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string? ReadOptional(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// A list of networks, separated by commas: <c>10.0.0.0/8, 2001:db8::/32</c>,
    /// a single address for itself, or <c>none</c> for none at all.
    /// </summary>
    private static IReadOnlyList<IPNetwork> ReadNetworks(string key, IReadOnlyList<IPNetwork> fallback)
    {
        var value = ReadOptional(key);

        if (value == null)
        {
            return fallback;
        }

        if (value.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var networks = new List<IPNetwork>();

        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (IPNetwork.TryParse(entry, out var network))
            {
                networks.Add(network);
            }
            else if (IPAddress.TryParse(entry, out var address))
            {
                networks.Add(new IPNetwork(address, address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128));
            }
            else
            {
                throw new InvalidOperationException($"{key}: '{entry}' is neither a network nor an address.");
            }
        }

        return networks;
    }

    private static int ReadInt(string key, int fallback)
        => int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

    private static long ReadLong(string key, long fallback)
        => long.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

    private static bool ReadBool(string key, bool fallback)
        => bool.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

    private static LambdaEngine ReadEngine(string key, LambdaEngine fallback)
        => Enum.TryParse<LambdaEngine>(Environment.GetEnvironmentVariable(key), true, out var value) ? value : fallback;

    private static HttpProtocols ReadProtocols(string key, HttpProtocols fallback)
        => Enum.TryParse<HttpProtocols>(Environment.GetEnvironmentVariable(key), true, out var value) ? value : fallback;

    private static TimeSpan ReadSpan(string key, TimeSpan fallback)
        => double.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? TimeSpan.FromHours(value) : fallback;

    #endregion

}
