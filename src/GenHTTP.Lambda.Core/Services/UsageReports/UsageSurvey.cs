using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <summary>
/// Gathers what a usage report says: the machine, the server, the
/// configuration, and counts over the lambdas.
/// </summary>
/// <remarks>
/// Reads the platform's database once per report and measures a few folders
/// of the data directory, which on a large installation is a walk over many
/// files - so it runs in the background job, never while a request waits.
/// Every count leaves the demos out: each installation has the same ones.
/// </remarks>
public sealed class UsageSurvey(IDbContextFactory<LambdaDbContext> databases, IDatabaseVault databaseVault, IWorkspaceVault workspaces,
                                ITelemetryService telemetry, ISettingsService settings, ServerRegistry server, LambdaOptions options)
{

    /// <summary>
    /// The key of the row in the settings the installation's identifier is kept in.
    /// </summary>
    private const string InstallationKey = "installation";

    /// <summary>
    /// How far back the counts of what is recent look.
    /// </summary>
    private const int RecentDays = 30;

    private readonly Lock _lock = new();

    #region Get-/Setters

    /// <summary>
    /// The version of GenHTTP Lambda running, without the build metadata the
    /// SDK appends to it (the commit it was built from).
    /// </summary>
    public static string PlatformVersion { get; } = ReadVersion();

    #endregion

    #region Functionality

    /// <summary>
    /// Makes the report as of now.
    /// </summary>
    public UsageReport Take()
    {
        var now = DateTime.UtcNow;

        using var database = databases.CreateDbContext();

        var lambdas = database.Lambdas.AsNoTracking()
                              .Where(l => l.Tier != LambdaTier.Demo)
                              .Select(l => new Row(l.Id, l.Tier, l.ActiveVersion, l.Blocked, l.Created, l.Modified, l.LastSeen, l.View,
                                                   l.Domain, l.AnalyticsSince, l.Connectivity))
                              .ToList();

        var ids = lambdas.Select(l => l.Id).ToHashSet();

        var stores = database.DataStores.AsNoTracking()
                             .Where(s => s.Enabled && s.Lambda!.Tier != LambdaTier.Demo)
                             .Select(s => new { s.LambdaId, s.Kind })
                             .ToList();

        var withDatabase = stores.Where(s => s.Kind == DataKinds.DatabaseId).Select(s => s.LambdaId).ToHashSet();
        var withWorkspace = stores.Where(s => s.Kind == DataKinds.WorkspaceId).Select(s => s.LambdaId).ToHashSet();
        var withSecrets = stores.Count(s => s.Kind == DataKinds.SecretsId);

        var versions = database.Deployments.Count(d => d.Lambda!.Tier != LambdaTier.Demo);

        var features = database.Features.Count(f => f.Lambda!.Tier != LambdaTier.Demo);

        var showcased = database.Showcases.Count(s => s.Lambda!.Tier != LambdaTier.Demo);

        var published = database.Sources.Count(s => s.Published && s.Lambda!.Tier != LambdaTier.Demo);

        var recent = now.AddDays(-RecentDays);

        var origins = database.Deployments.AsNoTracking()
                              .Where(d => d.Created >= recent && d.Lambda!.Tier != LambdaTier.Demo)
                              .GroupBy(d => d.Origin)
                              .Select(g => new { Origin = g.Key, Count = g.Count() })
                              .ToList();

        return new UsageReport(
            UsageReport.CurrentSchema,
            Installation(database),
            PlatformVersion,
            options.PublicUrl,
            now,
            DescribeSystem(),
            DescribeServer(now),
            DescribeConfiguration(),
            new UsageLambdas(
                lambdas.Count,
                lambdas.Count(l => l.Tier == LambdaTier.Free),
                lambdas.Count(l => l.Tier == LambdaTier.Premium),
                lambdas.Count(l => l.ActiveVersion != null),
                lambdas.Count(l => l.Blocked != null),
                lambdas.Count(l => l.Created >= recent),
                lambdas.Count(l => l.Modified >= recent || l.LastSeen >= recent),
                lambdas.Count(l => l.View == EditorView.Simple),
                versions,
                features
            ),
            Measure(withDatabase, withWorkspace),
            new UsageSwitches(
                withDatabase.Count,
                withWorkspace.Count,
                withSecrets,
                lambdas.Count(l => l.AnalyticsSince != null),
                showcased,
                published,
                lambdas.Count(l => l.Domain != null),
                lambdas.Count(l => l.Connectivity == ConnectivityStatus.Allowed),
                lambdas.Count(l => l.Connectivity == ConnectivityStatus.Requested),
                CountRepositories(ids)
            ),
            new UsageActivity(RecentDays, CountOrigins(origins.Select(o => (o.Origin, o.Count))))
        );
    }

    #endregion

    #region Helpers

    private sealed record Row(long Id, LambdaTier Tier, int? ActiveVersion, DateTime? Blocked, DateTime Created, DateTime Modified,
                              DateTime? LastSeen, EditorView View, string? Domain, DateTime? AnalyticsSince, ConnectivityStatus? Connectivity);

    /// <summary>
    /// The installation's identifier, made the first time a report is.
    /// </summary>
    /// <remarks>
    /// Random rather than derived from anything about the machine, so it
    /// says nothing but "the same installation as yesterday". Kept in the
    /// database, so it moves with a backup and an installation restored on
    /// another server stays the one it was.
    /// </remarks>
    private string Installation(LambdaDbContext database)
    {
        lock (_lock)
        {
            var stored = database.Settings.Find(InstallationKey);

            if (stored != null)
            {
                return stored.Value;
            }

            var made = Guid.NewGuid().ToString("n");

            database.Settings.Add(new SettingEntity { Key = InstallationKey, Value = made });
            database.SaveChanges();

            return made;
        }
    }

    private UsageSystem DescribeSystem()
    {
        var (size, free) = Disk(options.DataDirectory);

        return new UsageSystem(
            OperatingSystemName(),
            RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(),
            Environment.Version.ToString(),
            InContainer(),
            Environment.ProcessorCount,
            Memory(),
            size,
            free
        );
    }

    /// <summary>
    /// The memory the installation has: the machine's, or the limit of the
    /// control group it runs in where that is lower - a container's, usually.
    /// </summary>
    /// <remarks>
    /// Not what the runtime says it may use: in a container with a limit the
    /// garbage collector holds itself to three quarters of it, which would
    /// report a machine of 2 GB as one of 1.5. Read from the kernel where it
    /// can be, which is Linux; elsewhere the runtime's figure is what there is.
    /// </remarks>
    private static long Memory()
    {
        var runtime = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;

        if (!OperatingSystem.IsLinux())
        {
            return runtime;
        }

        try
        {
            var total = File.ReadLines("/proc/meminfo")
                            .Where(l => l.StartsWith("MemTotal:", StringComparison.Ordinal))
                            .Select(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                            .Select(f => f.Length >= 2 && long.TryParse(f[1], out var kb) ? kb * 1024 : 0)
                            .FirstOrDefault();

            if (total <= 0)
            {
                return runtime;
            }

            return GroupLimit() is { } limit && limit < total ? limit : total;
        }
        catch (Exception)
        {
            return runtime;
        }
    }

    /// <summary>
    /// The memory limit of the control group the process is in, if it has one.
    /// </summary>
    /// <remarks>
    /// Version 2 names the group in <c>/proc/self/cgroup</c> as <c>0::/path</c>,
    /// which is <c>/</c> inside a container of its own; version 1 keeps the
    /// limit at a fixed place, and an unlimited one is a number larger than
    /// any machine.
    /// </remarks>
    private static long? GroupLimit()
    {
        var group = File.ReadLines("/proc/self/cgroup").FirstOrDefault(l => l.StartsWith("0::", StringComparison.Ordinal))?[3..];

        string[] candidates = group != null
            ? [Path.Combine("/sys/fs/cgroup", group.TrimStart('/'), "memory.max")]
            : ["/sys/fs/cgroup/memory/memory.limit_in_bytes"];

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate) && long.TryParse(File.ReadAllText(candidate).Trim(), out var limit) && limit > 0)
            {
                return limit;
            }
        }

        return null;
    }

    private UsageServer DescribeServer(DateTime now)
    {
        var running = server.Instance;

        using var process = Process.GetCurrentProcess();

        return new UsageServer(
            running?.ServerEngine.ToString(),
            running?.Version,
            running?.EndPoints.Any(e => e.Secure) ?? false,
            Math.Round((now - telemetry.Started).TotalHours, 1),
            telemetry.TotalRequests,
            process.WorkingSet64
        );
    }

    private UsageConfiguration DescribeConfiguration()
    {
        var site = settings.Get();

        return new UsageConfiguration(
            options.Addressing.ToString(),
            options.HostingUrl != null,
            !string.IsNullOrWhiteSpace(options.AgentUrl),
            site.BuildBox,
            site.ChangeBox,
            options.Administrable,
            !string.IsNullOrWhiteSpace(options.Terms),
            !string.IsNullOrWhiteSpace(options.Contact),
            !string.IsNullOrWhiteSpace(options.AbuseContact),
            options.LegalPages,
            options.McpOrigins.Count,
            options.LogClientAddress,
            options.Geo,
            options.GeoPlaces,
            options.CaptureLambdaOutput,
            options.CrawlerLists,
            options.Development
        );
    }

    /// <summary>
    /// What the data of the lambdas takes: their databases and workspaces as
    /// their allowance counts them, their versions and features as files.
    /// </summary>
    /// <remarks>
    /// Only the lambdas that have a database or a workspace are asked about
    /// it - asking about the others would make their folders.
    /// </remarks>
    private UsageData Measure(HashSet<long> withDatabase, HashSet<long> withWorkspace)
    {
        long databaseBytes = 0, workspaceBytes = 0, largest = 0;

        foreach (var lambda in withDatabase.Union(withWorkspace))
        {
            var database = withDatabase.Contains(lambda) ? databaseVault.SizeOf(lambda, null) : 0;
            var workspace = withWorkspace.Contains(lambda) ? workspaces.SizeOf(lambda, null) : 0;

            databaseBytes += database;
            workspaceBytes += workspace;

            largest = Math.Max(largest, database + workspace);
        }

        return new UsageData(databaseBytes, workspaceBytes, largest, SizeOf(options.CodeDirectory), SizeOf(options.FeatureDirectory));
    }

    /// <summary>
    /// How many of the lambdas have had their repository read, which leaves
    /// a folder of commits behind (see <c>GitStore</c>).
    /// </summary>
    private int CountRepositories(HashSet<long> lambdas)
    {
        if (!Directory.Exists(options.GitDirectory))
        {
            return 0;
        }

        return Directory.EnumerateDirectories(options.GitDirectory)
                        .Count(d => long.TryParse(Path.GetFileName(d), out var id) && lambdas.Contains(id));
    }

    private static Dictionary<string, int> CountOrigins(IEnumerable<(string? Origin, int Count)> origins)
    {
        var counted = new Dictionary<string, int> { ["mcp"] = 0, ["api"] = 0, ["git"] = 0, ["template"] = 0, ["other"] = 0 };

        foreach (var (origin, count) in origins)
        {
            var name = origin switch
            {
                VersionOrigins.Agent => "mcp",
                VersionOrigins.Api => "api",
                VersionOrigins.Git => "git",
                VersionOrigins.Template => "template",
                _ => "other"
            };

            counted[name] += count;
        }

        return counted;
    }

    private static string OperatingSystemName()
    {
        if (OperatingSystem.IsLinux()) return "Linux";
        if (OperatingSystem.IsWindows()) return "Windows";
        if (OperatingSystem.IsMacOS()) return "macOS";
        if (OperatingSystem.IsFreeBSD()) return "FreeBSD";

        return "Other";
    }

    /// <summary>
    /// Whether the process runs in a container, by the file Docker and
    /// Podman put at its root.
    /// </summary>
    /// <remarks>
    /// Not by <c>DOTNET_RUNNING_IN_CONTAINER</c>: nothing below the hosts
    /// reads the environment, and only Microsoft's images set it.
    /// </remarks>
    private static bool InContainer()
    {
        try
        {
            return OperatingSystem.IsLinux() && (File.Exists("/.dockerenv") || File.Exists("/run/.containerenv"));
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static (long? Size, long? Free) Disk(string directory)
    {
        try
        {
            var drive = new DriveInfo(Path.GetFullPath(directory));

            return (drive.TotalSize, drive.AvailableFreeSpace);
        }
        catch (Exception)
        {
            // a volume some sandboxes will not describe; the figure is left
            // out rather than the report
            return (null, null);
        }
    }

    /// <summary>
    /// What the files below a folder take, counting each one as long as it is.
    /// </summary>
    private static long SizeOf(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return 0;
        }

        long total = 0;

        var walk = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };

        foreach (var file in new DirectoryInfo(directory).EnumerateFiles("*", walk))
        {
            try
            {
                total += file.Length;
            }
            catch (IOException)
            {
                // removed while it was counted
            }
        }

        return total;
    }

    private static string ReadVersion()
    {
        var assembly = typeof(UsageSurvey).Assembly;

        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                   ?? assembly.GetName().Version?.ToString()
                   ?? "0.0.0";

        var metadata = version.IndexOf('+');

        return metadata > 0 ? version[..metadata] : version;
    }

    #endregion

}
