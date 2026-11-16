using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <summary>
/// What an installation says about itself once a day: how it is set up and
/// how much it is used, never what its lambdas are or hold.
/// </summary>
/// <remarks>
/// Counts and switches only. Nothing in here names a lambda, an owner or a
/// visitor - no key, no domain, no title, no code, no record, no address -
/// and nothing that is not about the installation as a whole, apart from the
/// size of the largest lambda's data. The address the installation is
/// reached at is the one exception, because it is what tells an installation
/// somebody uses from one that was tried once.
///
/// This is the wire shape: the receiver reads it by its field names, so a
/// field is added rather than renamed, and <see cref="Schema"/> goes up when
/// one changes its meaning.
/// </remarks>
/// <param name="Schema">The version of this shape</param>
/// <param name="Installation">A random identifier made once per installation and kept in its database, so the reports of one installation can be told from another's</param>
/// <param name="Version">The version of GenHTTP Lambda running</param>
/// <param name="Url">Where the installation is reached (<c>LAMBDA_PUBLIC_URL</c>), if it says</param>
/// <param name="Sent">When the report was made</param>
/// <param name="System">The machine it runs on</param>
/// <param name="Server">How it serves</param>
/// <param name="Configuration">What the operator switched on or off</param>
/// <param name="Lambdas">How many lambdas there are, and in which state</param>
/// <param name="Data">How much room the lambdas take</param>
/// <param name="Switches">How many lambdas use each of the platform's features</param>
/// <param name="Activity">How the lambdas were changed recently</param>
public sealed record UsageReport(
    int Schema,
    string Installation,
    string Version,
    string? Url,
    DateTime Sent,
    UsageSystem System,
    UsageServer Server,
    UsageConfiguration Configuration,
    UsageLambdas Lambdas,
    UsageData Data,
    UsageSwitches Switches,
    UsageActivity Activity
)
{

    /// <summary>
    /// The version of the shape this platform sends.
    /// </summary>
    public const int CurrentSchema = 1;

}

/// <param name="Os">The operating system: <c>Linux</c>, <c>Windows</c>, <c>macOS</c>, <c>FreeBSD</c> or <c>Other</c></param>
/// <param name="Architecture">The processor architecture, such as <c>x64</c> or <c>arm64</c></param>
/// <param name="Runtime">The version of .NET</param>
/// <param name="Container">Whether it runs in a container (Docker or Podman)</param>
/// <param name="Processors">The processors the process may use</param>
/// <param name="MemoryBytes">The memory of the machine, or the limit of the container or group the process runs in where that is lower</param>
/// <param name="DiskBytes">The size of the volume the data directory is on, where it can be read</param>
/// <param name="DiskFreeBytes">What is free on that volume</param>
public sealed record UsageSystem(string Os, string Architecture, string Runtime, bool Container, int Processors, long MemoryBytes,
                                 long? DiskBytes, long? DiskFreeBytes);

/// <param name="Engine">The engine serving: <c>Kestrel</c>, <c>Ioxide</c> or <c>Internal</c>, unknown before the server runs</param>
/// <param name="GenHttp">The version of GenHTTP</param>
/// <param name="Secure">Whether the server serves TLS itself, rather than behind a proxy</param>
/// <param name="UptimeHours">How long the server has been running</param>
/// <param name="Requests">How many requests it answered in that time</param>
/// <param name="WorkingSetBytes">How much memory the process holds</param>
public sealed record UsageServer(string? Engine, [property: JsonPropertyName("genhttp")] string? GenHttp, bool Secure, double UptimeHours, long Requests,
                                 long WorkingSetBytes);

/// <param name="Addressing">Where the lambdas answer: <c>Paths</c> or <c>Subdomains</c></param>
/// <param name="HostingDomain">Whether the lambdas have a domain of their own (<c>LAMBDA_HOSTING_URL</c>)</param>
/// <param name="BuildAgent">Whether a build agent is configured</param>
/// <param name="BuildBox">Whether the front page offers the box that has an app built</param>
/// <param name="ChangeBox">Whether the editor offers the box that has an app changed</param>
/// <param name="AdminPanel">Whether the administration panel is on</param>
/// <param name="Terms">Whether the installation has terms of use</param>
/// <param name="Contact">Whether it names a contact</param>
/// <param name="AbuseContact">Whether it names where abuse is reported</param>
/// <param name="LegalPages">Whether it has legal pages</param>
/// <param name="McpOrigins">How many origins a browser may drive the MCP endpoint from</param>
/// <param name="ClientAddresses">Whether the log keeps the addresses requests came from</param>
/// <param name="Geo">Whether the log says which country a caller is in</param>
/// <param name="GeoPlaces">Whether it says which city and network as well</param>
/// <param name="LambdaOutput">Whether what lambdas print is kept</param>
/// <param name="CrawlerLists">Whether the lists of crawlers are fetched for the analytics</param>
/// <param name="Development">Whether the server runs in development mode</param>
public sealed record UsageConfiguration(string Addressing, bool HostingDomain, bool BuildAgent, bool BuildBox, bool ChangeBox, bool AdminPanel,
                                        bool Terms, bool Contact, bool AbuseContact, bool LegalPages, int McpOrigins, bool ClientAddresses,
                                        bool Geo, bool GeoPlaces, bool LambdaOutput, bool CrawlerLists, bool Development);

/// <remarks>
/// None of the counts includes the demos, which every installation has.
/// </remarks>
/// <param name="Total">Every lambda</param>
/// <param name="Free">Of those, in the free tier</param>
/// <param name="Premium">In the premium tier</param>
/// <param name="Online">With a version online</param>
/// <param name="Blocked">Blocked by the operator</param>
/// <param name="Created30Days">Created in the last 30 days</param>
/// <param name="Active30Days">Changed or visited in the last 30 days</param>
/// <param name="SimpleView">Opening in the simple view of the editor, which is how the build agent makes them</param>
/// <param name="Versions">Their versions, all together</param>
/// <param name="Features">Their features (drafts), all together</param>
public sealed record UsageLambdas(int Total, int Free, int Premium, int Online, int Blocked, int Created30Days, int Active30Days,
                                  int SimpleView, int Versions, int Features);

/// <param name="DatabaseBytes">What the lambdas' databases take, all together</param>
/// <param name="WorkspaceBytes">What their workspaces take</param>
/// <param name="LargestBytes">What the largest lambda's database and workspace take together</param>
/// <param name="VersionBytes">What their versions take on disk</param>
/// <param name="FeatureBytes">What their features take on disk, with their copies of the data</param>
public sealed record UsageData(long DatabaseBytes, long WorkspaceBytes, long LargestBytes, long VersionBytes, long FeatureBytes);

/// <summary>
/// How many lambdas use each of the platform's features.
/// </summary>
/// <param name="Database">With a database</param>
/// <param name="Workspace">With a workspace</param>
/// <param name="Secrets">With secrets</param>
/// <param name="Analytics">Counting their visits</param>
/// <param name="Showcase">In the showcase</param>
/// <param name="OpenSource">With their source published</param>
/// <param name="Domain">With a domain of their own</param>
/// <param name="ConnectivityAllowed">Allowed by the operator to connect to other servers</param>
/// <param name="ConnectivityRequested">Waiting for the operator to decide that</param>
/// <param name="Git">Whose repository was read with git at least once</param>
public sealed record UsageSwitches(int Database, int Workspace, int Secrets, int Analytics, int Showcase, int OpenSource, int Domain,
                                   int ConnectivityAllowed, int ConnectivityRequested, int Git);

/// <param name="Days">How far back this looks</param>
/// <param name="Versions">
/// The versions saved in that time by where they came from: <c>mcp</c> (an agent over MCP), <c>api</c> (the REST API and
/// the editor), <c>git</c> (a push), <c>template</c> (a new lambda) and <c>other</c>
/// </param>
public sealed record UsageActivity(int Days, IReadOnlyDictionary<string, int> Versions);
