using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Census;

/// <summary>
/// Measures what the data directory holds: by part, and by the lambda each
/// file belongs to.
/// </summary>
/// <remarks>
/// One walk over every file, attributing each by the folder it is in - the
/// parts the platform keeps per lambda are named after its id - so it reads
/// each file's length once and opens none. That is still a walk over many
/// files on a large installation, so it runs off the thread that asked and
/// its result is kept (see <see cref="CensusService"/>).
///
/// The data of a lambda - its database and its workspace - is measured as
/// its allowance measures it, so the panel and the limits agree on whether a
/// lambda is beyond what its tier allows.
/// </remarks>
public sealed class StorageSurvey(IDbContextFactory<LambdaDbContext> databases, IDatabaseVault databaseVault, IWorkspaceVault workspaces,
                                  ILimitsService limits, LambdaOptions options)
{

    /// <summary>
    /// How many of the largest lambdas are named.
    /// </summary>
    private const int Largest = 10;

    /// <summary>
    /// How many of the lambdas beyond their allowance are named.
    /// </summary>
    private const int MostOver = 50;

    #region Functionality

    /// <summary>
    /// How large the volume of the data directory is, and how much of it is
    /// free - cheap, so read whenever it is asked for.
    /// </summary>
    public (long? Size, long? Free) Disk()
    {
        try
        {
            var drive = new DriveInfo(Path.GetFullPath(options.DataDirectory));

            return (drive.TotalSize, drive.AvailableFreeSpace);
        }
        catch (Exception)
        {
            // a volume some sandboxes will not describe; the figures are left
            // out rather than the page
            return (null, null);
        }
    }

    public async ValueTask<StorageCensus> MeasureAsync(DateTime now, CancellationToken cancellation = default)
    {
        var parts = new Dictionary<string, (long Bytes, long Files)>(StringComparer.Ordinal);
        var owned = new Dictionary<long, Owned>();

        // which part of the data directory a folder at its top is, and
        // whether the folders in it are named after the lambdas
        var layout = new Dictionary<string, (string Part, bool PerLambda)>(StringComparer.Ordinal)
        {
            [Name(options.CodeDirectory)] = ("versions", true),
            [Name(options.FeatureDirectory)] = ("drafts", true),
            [Name(options.DatabaseDirectory)] = ("databases", true),
            [Name(options.WorkspaceDirectory)] = ("workspaces", true),
            [Name(options.GitDirectory)] = ("git", true),
            [Name(options.SourceDirectory)] = ("sources", true),
            [Name(options.AssemblyDirectory)] = ("compiled", true),
            [Name(options.ResourceDirectory)] = ("resources", true),
            // the platform's pictures, where the local files integration keeps them
            ["branding"] = ("pictures", false),
            ["showcase"] = ("pictures", false),
            // the lists the log and the analytics read, downloaded by their jobs
            ["geo"] = ("locations", false),
            ["crawlers"] = ("crawlers", false),
        };

        var root = new DirectoryInfo(options.DataDirectory);

        if (root.Exists)
        {
            foreach (var file in root.EnumerateFiles())
            {
                Add(parts, Top(file.Name), Length(file));
            }

            foreach (var folder in root.EnumerateDirectories())
            {
                if (!layout.TryGetValue(folder.Name, out var known))
                {
                    Add(parts, "other", Walk(folder));
                    continue;
                }

                foreach (var file in folder.EnumerateFiles())
                {
                    Add(parts, known.Part, Length(file));
                }

                foreach (var inner in folder.EnumerateDirectories())
                {
                    var size = Walk(inner);

                    Add(parts, known.Part, size);

                    if (known.PerLambda && long.TryParse(inner.Name, out var id))
                    {
                        var counted = owned.GetValueOrDefault(id) ?? new Owned();

                        switch (known.Part)
                        {
                            case "versions":
                                counted.Versions += size.Bytes;
                                break;
                            case "drafts":
                                counted.Drafts += size.Bytes;
                                break;
                            case "databases":
                                counted.Database = true;
                                break;
                            case "workspaces":
                                counted.Workspace = true;
                                break;
                            default:
                                counted.Other += size.Bytes;
                                break;
                        }

                        owned[id] = counted;
                    }
                }
            }
        }

        var lambdas = await DescribeAsync(owned, cancellation);

        return new StorageCensus(
            options.DataDirectory,
            null,
            null,
            now,
            [.. parts.Select(p => new StoragePart(p.Key, p.Value.Bytes, p.Value.Files)).OrderByDescending(p => p.Bytes)],
            lambdas.Count,
            [.. lambdas.OrderByDescending(l => l.TotalBytes).Take(Largest)],
            [.. lambdas.Where(l => l.DataBytes > l.AllowedBytes).OrderByDescending(l => l.DataBytes - l.AllowedBytes).Take(MostOver)]
        );
    }

    #endregion

    #region Helpers

    private sealed class Owned
    {
        internal bool Database;
        internal bool Workspace;
        internal long Versions;
        internal long Drafts;
        internal long Other;
    }

    /// <summary>
    /// What each lambda takes, by its public key and its tier as they are now.
    /// </summary>
    /// <remarks>
    /// Its database and its workspace are asked about where the walk found
    /// them, and measured as its allowance measures them - asking about the
    /// others would make their folders. A database kept elsewhere than the
    /// data directory is asked about where it is switched on. A folder of a
    /// lambda that is gone is left out of the lambdas but counted in its part.
    /// </remarks>
    private async ValueTask<List<LambdaStorage>> DescribeAsync(Dictionary<long, Owned> owned, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambdas = await database.Lambdas.AsNoTracking()
                                    .Select(l => new { l.Id, l.PublicKey, l.Tier })
                                    .ToListAsync(cancellation);

        var keeping = await database.DataStores.AsNoTracking()
                                    .Where(s => s.Kind == DataKinds.DatabaseId && s.Enabled)
                                    .Select(s => s.LambdaId)
                                    .ToHashSetAsync(cancellation);

        var described = new List<LambdaStorage>(owned.Count);

        foreach (var lambda in lambdas)
        {
            var found = owned.GetValueOrDefault(lambda.Id);

            if (found == null && !keeping.Contains(lambda.Id))
            {
                continue;
            }

            found ??= new Owned();

            var data = (found.Database || keeping.Contains(lambda.Id) ? await databaseVault.SizeOfAsync(lambda.Id, null, cancellation) : 0)
                     + (found.Workspace ? await workspaces.SizeOfAsync(lambda.Id, null, cancellation) : 0);

            described.Add(new LambdaStorage(lambda.PublicKey, lambda.Tier, data, limits.PolicyOf(lambda.Tier).MaxDataBytes,
                                            found.Versions, found.Drafts, found.Other));
        }

        return described;
    }

    /// <summary>
    /// The part a file at the top of the data directory belongs to.
    /// </summary>
    private string Top(string file)
    {
        // the database keeps a journal and shared memory beside it
        if (file.StartsWith(Path.GetFileName(options.DatabaseFile), StringComparison.Ordinal))
        {
            return "platform";
        }

        if (file.StartsWith(Path.GetFileName(options.AnalyticsFile), StringComparison.Ordinal))
        {
            return "analytics";
        }

        return "other";
    }

    private static string Name(string directory) => Path.GetFileName(Path.TrimEndingDirectorySeparator(directory));

    private static void Add(Dictionary<string, (long Bytes, long Files)> parts, string part, (long Bytes, long Files) size)
    {
        var current = parts.GetValueOrDefault(part);

        parts[part] = (current.Bytes + size.Bytes, current.Files + size.Files);
    }

    private static void Add(Dictionary<string, (long Bytes, long Files)> parts, string part, long bytes)
        => Add(parts, part, (bytes, 1));

    /// <summary>
    /// What the files below a folder come to, each counted as long as it is.
    /// </summary>
    private static (long Bytes, long Files) Walk(DirectoryInfo folder)
    {
        long bytes = 0, files = 0;

        var walk = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };

        try
        {
            foreach (var file in folder.EnumerateFiles("*", walk))
            {
                bytes += Length(file);
                files++;
            }
        }
        catch (DirectoryNotFoundException)
        {
            // removed while it was walked
        }

        return (bytes, files);
    }

    private static long Length(FileInfo file)
    {
        try
        {
            return file.Length;
        }
        catch (IOException)
        {
            // removed while it was counted
            return 0;
        }
    }

    #endregion

}
