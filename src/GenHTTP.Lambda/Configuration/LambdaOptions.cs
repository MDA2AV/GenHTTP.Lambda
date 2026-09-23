using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Lambda.Configuration;

/// <summary>
/// All knobs of the system, read once from the environment on startup.
/// </summary>
public sealed record LambdaOptions
{

    /// <summary>
    /// The port the root web server listens on.
    /// </summary>
    public ushort Port { get; init; } = 8080;

    /// <summary>
    /// Enables the development mode of the server (more verbose error pages).
    /// </summary>
    public bool Development { get; init; }

    /// <summary>
    /// The engine the root web server is hosted with.
    /// </summary>
    public LambdaEngine Engine { get; init; } = LambdaEngine.Ioxide;

    /// <summary>
    /// Which HTTP versions a port answers on.
    /// </summary>
    /// <remarks>
    /// This was HTTP/1.1 only for a while: the ioxide engine's HTTP/2 driver
    /// stopped writing once it had filled the window the client advertised,
    /// so Firefox, which advertises a hundred and twenty eight kilobytes,
    /// never finished loading the editor. Fixed in ioxide since.
    ///
    /// If a browser hangs on a large response again, check this first: set
    /// LAMBDA_HTTP_PROTOCOLS=Http1 to fall back without a release.
    /// </remarks>
    public HttpProtocols Protocols { get; init; } = HttpProtocols.Http1AndHttp2;

    /// <summary>
    /// Where the build agent listens, or nothing to do without one.
    /// </summary>
    /// <remarks>
    /// Empty by default and deliberately: the agent spends a Claude
    /// subscription on whatever a stranger types, so an installation has to
    /// opt into that rather than inherit it. Clearing this is also the kill
    /// switch - the text box stops being offered the moment it is unset.
    /// </remarks>
    public string? AgentUrl { get; init; }

    /// <summary>The secret the server sends the agent, so only it can ask.</summary>
    public string? AgentToken { get; init; }

    /// <summary>
    /// The password for the second model the build page offers.
    /// </summary>
    /// <remarks>
    /// Unset, the second model is not offered and asking for it is refused, so
    /// an installation that has not chosen a password does not quietly have an
    /// unguarded one. There is deliberately no default: a password with a
    /// default is a password everybody knows.
    /// </remarks>
    public string? AgentFablePassword { get; init; }

    /// <summary>How many builds one address may ask for in a day.</summary>
    /// <remarks>
    /// Ten rather than a handful, because a build is rarely the end of it: the
    /// page invites somebody to change what they just made, and a conversation
    /// that runs out after three turns is a conversation that stopped being
    /// one. The counting is in memory, so a restart forgives everybody.
    /// </remarks>
    public int AgentBuildsPerDay { get; init; } = 10;

    /// <summary>
    /// The directory holding the database, the stored code and the lambda workspaces.
    /// </summary>
    public string DataDirectory { get; init; } = Path.Combine(AppContext.BaseDirectory, "data");

    /// <summary>
    /// The directory holding the compiled single page application (index.html and assets).
    /// </summary>
    public string WebRoot { get; init; } = Path.Combine(AppContext.BaseDirectory, "wwwroot");

    /// <summary>
    /// How long a free tier lambda may go unused before it is taken offline.
    /// </summary>
    /// <remarks>
    /// Unused, not undeployed. This used to be the age of the deployment: a
    /// lambda went offline a day after it was put online however many people
    /// were using it, which meant anything built to be visited rather than
    /// demonstrated stopped working overnight. What it measures now is the
    /// later of when somebody last edited it and when somebody last asked it
    /// for something, so a lambda stays up for as long as it is wanted and
    /// goes quiet only once nobody is coming.
    /// </remarks>
    public TimeSpan DeploymentLifetime { get; init; } = TimeSpan.FromDays(30);

    /// <summary>
    /// How long an unused free tier lambda is kept before it is removed.
    /// </summary>
    /// <remarks>
    /// Measured the same way and from the same moment, but further out, and
    /// that gap is the point: the sweep removes before it retires, so a
    /// retention equal to the lifetime deletes a lambda at the very moment it
    /// would have gone quietly offline and the offline step never happens at
    /// all. Two months of grace, in which it is down but its key, its code and
    /// its versions are all still there to be deployed again.
    /// </remarks>
    public TimeSpan Retention { get; init; } = TimeSpan.FromDays(90);

    /// <summary>
    /// How often the maintenance job looks for expired lambdas.
    /// </summary>
    public TimeSpan MaintenanceInterval { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// The maximum size of a code snippet that will be accepted.
    /// </summary>
    /// <remarks>
    /// Counted across every C# file of a lambda together, and not against
    /// its assets, which have their own budget. It was half this and that
    /// was the tightest of the three limits by a distance: a lambda may
    /// ship twelve files and two megabytes of things to serve, and then be
    /// refused for the code that serves them. What this actually guards is
    /// the time the compiler spends, and that is not close to mattering at
    /// either number.
    /// </remarks>
    public int MaxCodeLength { get; init; } = 128 * 1024;

    /// <summary>
    /// How many bytes of assets a lambda may ship beside its code.
    /// </summary>
    /// <remarks>
    /// Counted apart from the code because it is not code: a stylesheet is
    /// never compiled, and charging a page of markup against the budget for
    /// the program that serves it is the wrong ceiling for both.
    /// </remarks>
    public int MaxAssetBytes { get; init; } = 2 * 1024 * 1024;

    /// <summary>
    /// The number of versions kept per lambda (older ones are pruned).
    /// </summary>
    public int MaxVersions { get; init; } = 50;

    /// <summary>
    /// Requests per second a single client may send to the lambda routes.
    /// </summary>
    public int RateLimit { get; init; } = 5000;

    /// <summary>
    /// How many lambda requests may be executed at the same time.
    /// </summary>
    public int MaxConcurrency { get; init; } = 64;

    /// <summary>
    /// The time a single lambda invocation may take before it is aborted.
    /// </summary>
    public TimeSpan ExecutionTimeout { get; init; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The token the administration panel has to present. Empty leaves the
    /// panel switched off.
    /// </summary>
    /// <remarks>
    /// It reads the code of lambdas it does not own and can take them down, so
    /// it is not something to leave open on an installation anyone can reach.
    /// Nothing is served at all until a token is configured - a panel that is
    /// off cannot be misconfigured into being open.
    /// </remarks>
    public string? AdminToken { get; init; }

    /// <summary>
    /// Whether the per lambda activity is served to anyone who asks.
    /// </summary>
    /// <remarks>
    /// Public while the platform is small and the numbers are interesting to
    /// look at. Turning it off leaves the figures being collected and the
    /// owner of a lambda still seeing its own; only the overview of everyone
    /// else's goes away.
    /// </remarks>
    public bool PublicActivity { get; init; } = true;

    /// <summary>
    /// How often a telemetry reading is taken.
    /// </summary>
    public TimeSpan TelemetryInterval { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How many readings are kept. At the default interval this is a day.
    /// </summary>
    public int TelemetrySamples { get; init; } = 2880;

    /// <summary>
    /// How many log lines are kept for the panel to read back.
    /// </summary>
    /// <remarks>
    /// Held in memory and lost on a restart. Docker keeps the whole run on
    /// stdout regardless; this is only the part an operator can reach without
    /// a shell on the host - so the depth to ask for is however far back
    /// somebody wants to scroll while the server is up, and the cost of it is
    /// <see cref="LogMemory"/>.
    /// </remarks>
    public int LogHistory { get; init; } = 4000;

    /// <summary>
    /// How much memory the text of those lines may take, in megabytes.
    /// </summary>
    /// <remarks>
    /// Zero derives it from the depth, at half a kilobyte of characters per
    /// line, which is generous against real ones. Whichever runs out first
    /// decides: a ring asked to be deep but given little memory holds short
    /// lines to its depth and long ones to its budget.
    /// </remarks>
    public int LogMemory { get; init; }

    /// <summary>
    /// Whether what a lambda prints while serving a request is kept.
    /// </summary>
    /// <remarks>
    /// On, because a lambda that cannot be watched can only be guessed at, and
    /// the author of one has no other way to see a print. Off, the console of
    /// a lambda still reaches stdout and simply is not gathered - which is the
    /// setting for an installation where what strangers print is not something
    /// the operator wants held in memory at all.
    /// </remarks>
    public bool CaptureLambdaOutput { get; init; } = true;

    /// <summary>
    /// How many lines one request may contribute before the rest is dropped.
    /// </summary>
    public int MaxOutputLines { get; init; } = 200;

    /// <summary>
    /// How long identical lines are gathered into one rather than each being
    /// written. Zero writes every one.
    /// </summary>
    /// <remarks>
    /// The same request, from the same caller, answered the same way, is one
    /// fact and a rate. Ten seconds is short enough that a burst is still
    /// visible as it happens and long enough that a scanner knocking twice a
    /// second becomes one line rather than twenty.
    /// </remarks>
    public TimeSpan RepeatWindow { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Whether log lines say which country the caller's address belongs to.
    /// </summary>
    /// <remarks>
    /// Read from the delegation files the regional registries publish, which
    /// are fetched on an interval and cached on the data volume. No third
    /// party is asked anything: doing this by lookup would mean handing
    /// somebody else the address of every visitor.
    ///
    /// It answers where a range is registered, which is not always where the
    /// person using it is. Off, and nothing is downloaded at all.
    /// </remarks>
    public bool Geo { get; init; } = true;

    /// <summary>
    /// Whether the city and network databases are fetched as well.
    /// </summary>
    /// <remarks>
    /// The registries say which country delegated a range and are exact about
    /// it. This is the other kind of answer: a guess by a third party, from
    /// measurement, that names a town and an internet provider. Right about
    /// most consumer connections and wrong about most infrastructure.
    ///
    /// It costs about a hundred and thirty megabytes on the data volume,
    /// refetched monthly, and nothing in memory - the files are mapped rather
    /// than read. Still nobody is asked about a visitor: the database comes
    /// here and the lookups happen here.
    ///
    /// Data by DB-IP under CC BY 4.0, which the panel credits.
    /// </remarks>
    public bool GeoPlaces { get; init; } = true;

    /// <summary>
    /// How often the registry files are asked for again.
    /// </summary>
    /// <remarks>
    /// They are published daily and change slowly, so weekly is frequent
    /// enough to stay useful and rare enough to be a good neighbour to
    /// somebody else's public FTP.
    /// </remarks>
    public TimeSpan GeoRefresh { get; init; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Whether the address a request came from is recorded against its lines.
    /// </summary>
    /// <remarks>
    /// On, because without it a path being hit five hundred times a minute is
    /// a mystery rather than a question. It is personal data and it is held in
    /// memory for as long as the ring is deep, so an installation that would
    /// rather not keep it turns this off and still gets everything else - the
    /// lines simply have no address on them.
    ///
    /// Only ever served behind the administration token, which is the same bar
    /// as reading the code of somebody else's lambda. It is deliberately not
    /// part of the figures next door: those are aggregates and name nobody,
    /// and that is what makes them safe to serve to anyone.
    /// </remarks>
    public bool LogClientAddress { get; init; } = true;

    /// <summary>
    /// The port the server offers TLS on. Zero leaves the secure endpoint off.
    /// </summary>
    public ushort SecurePort { get; init; }

    /// <summary>
    /// The certificate the TLS endpoint is secured with, either a PEM chain or
    /// a PKCS#12 archive.
    /// </summary>
    public string? CertificatePath { get; init; }

    /// <summary>
    /// The private key belonging to the certificate. Set for a PEM pair, left
    /// empty when the archive carries its own key.
    /// </summary>
    public string? CertificateKeyPath { get; init; }

    /// <summary>
    /// The password of the PKCS#12 archive, if it has one.
    /// </summary>
    public string? CertificatePassword { get; init; }

    /// <summary>
    /// Hosts a browser may drive the MCP endpoint from.
    /// </summary>
    /// <remarks>
    /// Only browsers send an Origin, and only a browser can be made to send a
    /// request somebody else wrote. An agent speaking HTTP sends none and is
    /// never checked against this.
    /// </remarks>
    public IReadOnlyList<string> McpOrigins { get; init; } = [];

    /// <summary>
    /// The address the site is meant to be found at, such as <c>https://genhttp.dev</c>.
    /// </summary>
    /// <remarks>
    /// Pages name it as their canonical address and the sitemap lists pages
    /// under it. Left empty, neither is written: a canonical pointing at the
    /// wrong host is worse than none, and an installation cannot tell which of
    /// the names it answers to is the one that counts.
    /// </remarks>
    public string? PublicUrl { get; init; }

    /// <summary>
    /// A directory holding further certificates, one subdirectory per name,
    /// each with a <c>fullchain.pem</c> and a <c>privkey.pem</c> beside it.
    /// </summary>
    /// <remarks>
    /// The server answers to more than one hostname and a certificate names
    /// the hosts it is good for, so the one to present depends on the host the
    /// client asked for. The certificate above stays the default, used when a
    /// client sends no name or a name nothing here covers.
    /// </remarks>
    public string? CertificateDirectory { get; init; }

    #region Derived

    public string DatabaseFile => Path.Combine(DataDirectory, "lambda.db");

    public string ConnectionString => $"Data Source={DatabaseFile};Default Timeout=30;Pooling=True";

    public string CodeDirectory => Path.Combine(DataDirectory, "code");

    public string WorkspaceDirectory => Path.Combine(DataDirectory, "workspaces");

    public string AssemblyDirectory => Path.Combine(DataDirectory, "assemblies");

    public string AssetDirectory => Path.Combine(DataDirectory, "assets");

    /// <summary>
    /// Whether the server should offer a TLS endpoint next to the plain one.
    /// </summary>
    public bool Secure => SecurePort > 0 && !string.IsNullOrWhiteSpace(CertificatePath);

    /// <summary>
    /// Whether the administration panel is available at all.
    /// </summary>
    public bool Administrable => !string.IsNullOrWhiteSpace(AdminToken);

    #endregion

    #region Functionality

    /// <summary>
    /// Reads the configuration from the environment, falling back to the defaults.
    /// </summary>
    public static LambdaOptions FromEnvironment()
    {
        var defaults = new LambdaOptions();

        return new LambdaOptions
        {
            Port = (ushort)ReadInt("LAMBDA_PORT", defaults.Port),
            Development = ReadBool("LAMBDA_DEVELOPMENT", defaults.Development),
            Engine = ReadEngine("LAMBDA_ENGINE", defaults.Engine),
            Protocols = ReadProtocols("LAMBDA_HTTP_PROTOCOLS", defaults.Protocols),
            AgentUrl = ReadOptional("LAMBDA_AGENT_URL"),
            AgentToken = ReadOptional("LAMBDA_AGENT_TOKEN"),
            AgentFablePassword = ReadOptional("LAMBDA_AGENT_FABLE_PASSWORD"),
            AgentBuildsPerDay = ReadInt("LAMBDA_AGENT_BUILDS_PER_DAY", defaults.AgentBuildsPerDay),
            DataDirectory = Path.GetFullPath(ReadString("LAMBDA_DATA_DIRECTORY", defaults.DataDirectory)),
            WebRoot = Path.GetFullPath(ReadString("LAMBDA_WEB_ROOT", defaults.WebRoot)),
            DeploymentLifetime = ReadSpan("LAMBDA_DEPLOYMENT_LIFETIME_HOURS", defaults.DeploymentLifetime),
            Retention = ReadSpan("LAMBDA_RETENTION_HOURS", defaults.Retention),
            MaintenanceInterval = ReadSpan("LAMBDA_MAINTENANCE_INTERVAL_HOURS", defaults.MaintenanceInterval),
            MaxCodeLength = ReadInt("LAMBDA_MAX_CODE_LENGTH", defaults.MaxCodeLength),
            MaxAssetBytes = ReadInt("LAMBDA_MAX_ASSET_BYTES", defaults.MaxAssetBytes),
            MaxVersions = ReadInt("LAMBDA_MAX_VERSIONS", defaults.MaxVersions),
            RateLimit = ReadInt("LAMBDA_RATE_LIMIT", defaults.RateLimit),
            MaxConcurrency = ReadInt("LAMBDA_MAX_CONCURRENCY", defaults.MaxConcurrency),
            ExecutionTimeout = TimeSpan.FromSeconds(ReadInt("LAMBDA_EXECUTION_TIMEOUT_SECONDS", (int)defaults.ExecutionTimeout.TotalSeconds)),
            AdminToken = ReadOptional("LAMBDA_ADMIN_TOKEN"),
            PublicActivity = ReadBool("LAMBDA_PUBLIC_ACTIVITY", defaults.PublicActivity),
            TelemetryInterval = TimeSpan.FromSeconds(ReadInt("LAMBDA_TELEMETRY_INTERVAL_SECONDS", (int)defaults.TelemetryInterval.TotalSeconds)),
            TelemetrySamples = ReadInt("LAMBDA_TELEMETRY_SAMPLES", defaults.TelemetrySamples),
            LogHistory = ReadInt("LAMBDA_LOG_HISTORY", defaults.LogHistory),
            LogMemory = ReadInt("LAMBDA_LOG_MEMORY_MB", defaults.LogMemory),
            CaptureLambdaOutput = ReadBool("LAMBDA_LOG_LAMBDA_OUTPUT", defaults.CaptureLambdaOutput),
            MaxOutputLines = ReadInt("LAMBDA_LOG_MAX_LINES_PER_REQUEST", defaults.MaxOutputLines),
            LogClientAddress = ReadBool("LAMBDA_LOG_CLIENT_ADDRESS", defaults.LogClientAddress),
            RepeatWindow = TimeSpan.FromSeconds(ReadInt("LAMBDA_LOG_REPEAT_WINDOW_SECONDS", (int)defaults.RepeatWindow.TotalSeconds)),
            Geo = ReadBool("LAMBDA_LOG_GEO", defaults.Geo),
            GeoPlaces = ReadBool("LAMBDA_LOG_GEO_PLACES", defaults.GeoPlaces),
            GeoRefresh = TimeSpan.FromHours(ReadInt("LAMBDA_LOG_GEO_REFRESH_HOURS", (int)defaults.GeoRefresh.TotalHours)),
            SecurePort = (ushort)ReadInt("LAMBDA_TLS_PORT", defaults.SecurePort),
            CertificatePath = ReadOptional("LAMBDA_CERTIFICATE"),
            CertificateKeyPath = ReadOptional("LAMBDA_CERTIFICATE_KEY"),
            CertificatePassword = ReadOptional("LAMBDA_CERTIFICATE_PASSWORD"),
            CertificateDirectory = ReadOptional("LAMBDA_CERTIFICATE_DIRECTORY"),
            McpOrigins = (ReadOptional("LAMBDA_MCP_ORIGINS") ?? "")
                         .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            PublicUrl = ReadOptional("LAMBDA_PUBLIC_URL")?.TrimEnd('/')
        };
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

    private static int ReadInt(string key, int fallback)
        => int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

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
