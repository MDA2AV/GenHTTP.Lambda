using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Lambda.Configuration;

/// <summary>
/// All knobs of the system, read once from the environment on startup.
/// </summary>
/// <remarks>
/// The limits of the tiers are here only as the defaults of what the operator
/// sets in the administration panel (see <see cref="GenHTTP.Lambda.Services.Settings.LimitsService"/>):
/// they are the product's promises, not the server's configuration. Their
/// environment variables still set those defaults for one release, with a
/// warning on startup (<see cref="MovedToPanel"/>), and are removed after.
/// </remarks>
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
    /// How large a version of a lambda outside the premium tier may be: its
    /// code and its resources together.
    /// </summary>
    /// <remarks>
    /// Mind what it costs: every version keeps its own copy of all of it, as
    /// base64 in JSON for what is not text, so a version saved at the limit
    /// takes nearly half as much again on disk, <see cref="MaxVersions"/> of
    /// them are kept, and a version is read and written whole, in memory.
    /// </remarks>
    public long BuildBytes { get; init; } = 32L * 1024 * 1024;

    /// <summary>
    /// How large a version of a premium lambda may be.
    /// </summary>
    /// <remarks>
    /// Room for pictures, recordings and data rather than a stylesheet and a
    /// logo. Never less than what every other lambda may have, however the
    /// two are set.
    /// </remarks>
    public long PremiumBuildBytes { get; init; } = 128L * 1024 * 1024;

    /// <summary>
    /// How much room the data of a lambda outside the premium tier may take:
    /// its database and its workspace together.
    /// </summary>
    /// <remarks>
    /// One allowance for both, so a lambda that keeps everything in its
    /// database has all of it there, and one that keeps files has all of it
    /// for them. Each feature's copy of the data is held to it on its own.
    /// </remarks>
    public long DataBytes { get; init; } = 512L * 1024 * 1024;

    /// <summary>
    /// How much room the data of a premium lambda may take.
    /// </summary>
    public long PremiumDataBytes { get; init; } = 4096L * 1024 * 1024;

    /// <summary>
    /// How large the picture promoting a lambda in the showcase may be.
    /// </summary>
    /// <remarks>
    /// Room for a short animated recording, which is what shows a lambda best,
    /// and not for a video: every visitor of the showcase page downloads it.
    /// </remarks>
    public int MaxShowcaseImageBytes { get; init; } = 3 * 1024 * 1024;

    /// <summary>
    /// The number of versions kept per lambda (older ones are pruned).
    /// </summary>
    public int MaxVersions { get; init; } = 50;

    /// <summary>
    /// How many features a lambda may have open at once.
    /// </summary>
    /// <remarks>
    /// Each one holds a copy of the lambda's files, of its workspace and of its
    /// database, so a lambda at the limit of either takes that room again for
    /// every feature it has open.
    /// </remarks>
    public int MaxFeatures { get; init; } = 10;

    /// <summary>
    /// How much room the published sources may take on disk, packed.
    /// </summary>
    /// <remarks>
    /// A published source is packed into a project once per version and kept,
    /// so that it is not packed again for every visitor. Anybody may ask for
    /// any version of it, though, and a premium lambda may ship a hundred
    /// megabytes of resources fifty times over - so the cache is held to this,
    /// and the version read least recently is packed again should somebody
    /// ask for it once more.
    /// </remarks>
    public long SourceCacheBytes { get; init; } = 2048L * 1024 * 1024;

    /// <summary>
    /// How large a single <c>git push</c> to a lambda may be.
    /// </summary>
    /// <remarks>
    /// A push is read into memory and unpacked there before anything is
    /// decided about it, which is the git module's way and what makes it
    /// atomic - so this protects the process, like a buffer, rather than
    /// being a promise of a tier: what a version may hold is checked after
    /// it was unpacked, against the tier of the lambda. Room for the first
    /// push of a premium lambda that ships all the resources it may.
    /// </remarks>
    public long GitMaxPushBytes { get; init; } = 256L * 1024 * 1024;

    /// <summary>
    /// Requests per second a single client may send to the lambda routes.
    /// </summary>
    /// <remarks>
    /// Room for a page that loads a few dozen assets at once and a client that
    /// polls, and well short of one visitor occupying the server. It was five
    /// thousand, which no person comes near and which stopped nothing.
    /// </remarks>
    public int RateLimit { get; init; } = 250;

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
    /// The terms of use of this installation, which creating a lambda
    /// accepts. Without any, there is nothing to accept.
    /// </summary>
    /// <remarks>
    /// Shown to agents in <c>platform_guide</c> and to people on the page
    /// that creates a lambda. Whoever runs an installation for themselves
    /// has nobody to agree terms with, so none are assumed.
    /// </remarks>
    public string? Terms { get; init; }

    /// <summary>
    /// How often a telemetry reading is taken.
    /// </summary>
    public TimeSpan TelemetryInterval { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How many readings are kept. At the default interval this is a day.
    /// </summary>
    public int TelemetrySamples { get; init; } = 2880;

    /// <summary>
    /// How often the visits counted for the owners' analytics are written
    /// down. Visits not yet written are lost when the server stops.
    /// </summary>
    public TimeSpan AnalyticsInterval { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Whether the list of crawlers and the addresses Google, Bing, OpenAI,
    /// Apple and Perplexity crawl from are fetched, to tell crawlers from people
    /// and confirm the crawlers that say whose they are. Off, the list of the
    /// release is used and no crawler is confirmed.
    /// </summary>
    public bool CrawlerLists { get; init; } = true;

    /// <summary>
    /// How often they are fetched again.
    /// </summary>
    public TimeSpan CrawlerRefresh { get; init; } = TimeSpan.FromDays(1);

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
    /// The networks whose addresses are never recorded against a lambda and
    /// never blocked.
    /// </summary>
    /// <remarks>
    /// The installation's own: loopback, the private ranges, link local and
    /// shared address space. The build agent reaches the MCP from a
    /// container on one of them, a fresh address each build, and a proxy in
    /// front of the server would be one - blocking either would refuse
    /// everybody behind it. An installation reached from a network of its
    /// own (behind a proxy, inside a company) cannot block anybody; one whose
    /// proxy has a public address adds it here.
    /// </remarks>
    public IReadOnlyList<IPNetwork> UnblockableNetworks { get; init; } = DefaultUnblockableNetworks;

    /// <summary>
    /// The networks that are the installation's own by default (see <see cref="UnblockableNetworks"/>).
    /// </summary>
    public static readonly IReadOnlyList<IPNetwork> DefaultUnblockableNetworks =
    [
        IPNetwork.Parse("127.0.0.0/8"),
        IPNetwork.Parse("10.0.0.0/8"),
        IPNetwork.Parse("172.16.0.0/12"),
        IPNetwork.Parse("192.168.0.0/16"),
        IPNetwork.Parse("169.254.0.0/16"),
        IPNetwork.Parse("100.64.0.0/10"),
        IPNetwork.Parse("::1/128"),
        IPNetwork.Parse("fc00::/7"),
        IPNetwork.Parse("fe80::/10")
    ];

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
    /// the names it answers to is the one that counts. Set, it is also what
    /// tells the platform's own names from a host nobody here has, which is
    /// then not answered with the site (see
    /// <see cref="Services.Hosting.LambdaAddresses.IsPlatform"/>).
    /// </remarks>
    public string? PublicUrl { get; init; }

    /// <summary>
    /// The address the lambdas answer below, each at a subdomain named after
    /// its public key, such as <c>https://genhttp.run</c> for
    /// <c>https://quiz.genhttp.run/</c>.
    /// </summary>
    /// <remarks>
    /// A domain of its own rather than a path below the platform's, so that
    /// what somebody built runs on a site of its own: it cannot read what the
    /// platform keeps in a browser - the admin token, for one - nor what
    /// another lambda keeps there, and it has the root of its host to itself.
    /// Needs a wildcard record in the DNS and a wildcard certificate, which is
    /// presented to every name no other certificate is listed for (see
    /// <see cref="GenHTTP.Lambda.Infrastructure.CertificateLoader"/>).
    ///
    /// Left empty, it is this server on localhost - <c>http://quiz.localhost:8080/</c> -
    /// which browsers resolve without a record, so a server run on a laptop
    /// serves its lambdas as it is.
    /// </remarks>
    public string? HostingUrl { get; init; }

    /// <summary>
    /// Whether the lambdas answer at subdomains of <see cref="HostingUrl"/>
    /// or below <c>/lambda/</c> on the platform's own host.
    /// </summary>
    /// <remarks>
    /// Below a path, a lambda's address is made from <see cref="PublicUrl"/>
    /// - or this server on localhost without one - and the hosting URL is
    /// not used. A domain of a lambda's own is served either way.
    /// </remarks>
    public LambdaAddressing Addressing { get; init; } = LambdaAddressing.Subdomains;

    /// <summary>
    /// The installation's half of the key the secrets of every lambda are
    /// sealed with: 32 bytes in base64, or a passphrase of at least 32
    /// characters.
    /// </summary>
    /// <remarks>
    /// Left empty, a random key is made on first use and kept in
    /// <see cref="SecretsKeyFile"/>. Either way it is not in the database, which
    /// is the point: a copy of the database opens no secret without it. Moving
    /// to another server, or restoring a backup, takes the database and this
    /// key - set it here on the new server, or copy the key file along.
    /// Losing it loses every secret; the lambdas and everything else stay.
    /// </remarks>
    public string? SecretsKey { get; init; }

    #region Derived

    public string DatabaseFile => Path.Combine(DataDirectory, "lambda.db");

    public string ConnectionString => $"Data Source={DatabaseFile};Default Timeout=30;Pooling=True";

    /// <summary>
    /// The database the owners' analytics are kept in, apart from the platform's.
    /// </summary>
    public string AnalyticsFile => Path.Combine(DataDirectory, "analytics.db");

    public string CodeDirectory => Path.Combine(DataDirectory, "code");

    public string WorkspaceDirectory => Path.Combine(DataDirectory, "workspaces");

    /// <summary>
    /// Where the databases of the lambdas are kept, a folder each - the
    /// database, and the journal SQLite keeps beside it.
    /// </summary>
    public string DatabaseDirectory => Path.Combine(DataDirectory, "databases");

    public string AssemblyDirectory => Path.Combine(DataDirectory, "assemblies");

    /// <summary>
    /// Where the resources of what is online are written, one folder per
    /// lambda - rewritten from the version every time one goes online.
    /// </summary>
    public string ResourceDirectory => Path.Combine(DataDirectory, "resources");

    /// <summary>
    /// Where features keep their files, their copy of the data and what their
    /// preview serves, one folder per lambda and feature.
    /// </summary>
    public string FeatureDirectory => Path.Combine(DataDirectory, "features");

    /// <summary>
    /// Where the published sources are kept packed, one folder per lambda and
    /// a project per version - a cache, made again from the versions whenever
    /// something in it is missing.
    /// </summary>
    public string SourceDirectory => Path.Combine(DataDirectory, "sources");

    /// <summary>
    /// Where the commits a lambda is read as with git are kept, one folder per
    /// lambda: the commits, the files they hold, and which of them is which
    /// version and feature.
    /// </summary>
    public string GitDirectory => Path.Combine(DataDirectory, "git");

    /// <summary>
    /// Where the key the secrets are sealed with is kept, when
    /// <see cref="SecretsKey"/> does not give one.
    /// </summary>
    public string SecretsKeyFile => Path.Combine(DataDirectory, "secrets.key");

    /// <summary>
    /// Whether the administration panel is available at all.
    /// </summary>
    public bool Administrable => !string.IsNullOrWhiteSpace(AdminToken);

    #endregion

    #region Functionality

    /// <summary>
    /// What the four allowances that became two were by default, in the free
    /// tier and the premium one - what a value set for only one of a pair is
    /// added to.
    /// </summary>
    public static IReadOnlyDictionary<string, (long Free, long Premium)> RetiredDefaults { get; } = new Dictionary<string, (long, long)>
    {
        ["code-characters"] = (1024 * 1024, 10 * 1024 * 1024),
        ["asset-bytes"] = (32L * 1024 * 1024, 128L * 1024 * 1024),
        ["workspace-bytes"] = (256L * 1024 * 1024, 2048L * 1024 * 1024),
        ["database-bytes"] = (256L * 1024 * 1024, 2048L * 1024 * 1024)
    };

    #endregion

}
