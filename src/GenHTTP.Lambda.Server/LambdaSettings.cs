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
            DataDirectory = Path.GetFullPath(ReadString("LAMBDA_DATA_DIRECTORY", defaults.DataDirectory)),
            MaintenanceInterval = ReadSpan("LAMBDA_MAINTENANCE_INTERVAL_HOURS", defaults.MaintenanceInterval),
            SourceCacheBytes = ReadLong("LAMBDA_SOURCE_CACHE_BYTES", defaults.SourceCacheBytes),
            GitMaxPushBytes = ReadLong("LAMBDA_GIT_MAX_PUSH_BYTES", defaults.GitMaxPushBytes),
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
    /// Says what about the configuration the operator should know: what was
    /// left out, and what does not fit together.
    /// </summary>
    public void Report(ILogger logger)
    {
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
