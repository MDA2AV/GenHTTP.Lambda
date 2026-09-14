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
    /// The directory holding the database, the stored code and the lambda workspaces.
    /// </summary>
    public string DataDirectory { get; init; } = Path.Combine(AppContext.BaseDirectory, "data");

    /// <summary>
    /// The directory holding the compiled single page application (index.html and assets).
    /// </summary>
    public string WebRoot { get; init; } = Path.Combine(AppContext.BaseDirectory, "wwwroot");

    /// <summary>
    /// How long a deployment of a free tier lambda stays active.
    /// </summary>
    public TimeSpan DeploymentLifetime { get; init; } = TimeSpan.FromDays(1);

    /// <summary>
    /// How long an untouched free tier lambda is kept before it is removed.
    /// </summary>
    public TimeSpan Retention { get; init; } = TimeSpan.FromDays(30);

    /// <summary>
    /// How often the maintenance job looks for expired lambdas.
    /// </summary>
    public TimeSpan MaintenanceInterval { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// The maximum size of a code snippet that will be accepted.
    /// </summary>
    public int MaxCodeLength { get; init; } = 64 * 1024;

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
            SecurePort = (ushort)ReadInt("LAMBDA_TLS_PORT", defaults.SecurePort),
            CertificatePath = ReadOptional("LAMBDA_CERTIFICATE"),
            CertificateKeyPath = ReadOptional("LAMBDA_CERTIFICATE_KEY"),
            CertificatePassword = ReadOptional("LAMBDA_CERTIFICATE_PASSWORD"),
            CertificateDirectory = ReadOptional("LAMBDA_CERTIFICATE_DIRECTORY"),
            McpOrigins = (ReadOptional("LAMBDA_MCP_ORIGINS") ?? "")
                         .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
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

    private static TimeSpan ReadSpan(string key, TimeSpan fallback)
        => double.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? TimeSpan.FromHours(value) : fallback;

    #endregion

}
