using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Background;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Keeps the country table current, and keeps a copy where a restart can find
/// it.
/// </summary>
/// <remarks>
/// The registries publish these once a day and the contents change slowly, so
/// this asks weekly by default. What it fetches is written to the data volume
/// and read from there on the way up: a server that comes back while the
/// registries are unreachable still knows what it knew yesterday, and one
/// that has never reached them simply has no countries to show rather than a
/// startup that fails.
/// </remarks>
public sealed class GeoJob(GeoTable table, GeoPlaces places, LambdaOptions options, ILogger<GeoJob> logger) : IBackgroundJob
{

    /// <summary>
    /// The five regional registries, each publishing the ranges it delegated.
    /// </summary>
    private static readonly string[] Sources =
    [
        "https://ftp.ripe.net/pub/stats/ripencc/delegated-ripencc-extended-latest",
        "https://ftp.arin.net/pub/stats/arin/delegated-arin-extended-latest",
        "https://ftp.apnic.net/stats/apnic/delegated-apnic-extended-latest",
        "https://ftp.lacnic.net/pub/stats/lacnic/delegated-lacnic-extended-latest",
        "https://ftp.afrinic.net/stats/afrinic/delegated-afrinic-extended-latest"
    ];

    public string Name => "Geography";

    public TimeSpan Interval => options.GeoRefresh;

    public async ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        if (!options.Geo)
        {
            return;
        }

        var directory = Path.Combine(options.DataDirectory, "geo");

        Directory.CreateDirectory(directory);

        // whatever is already on disk first, so the table is useful within
        // milliseconds of a restart rather than after five downloads
        if (table.Ranges == 0 && Rebuild(directory))
        {
            logger.LogInformation("Loaded {Ranges:N0} allocated ranges from the cached registry files", table.Ranges);
        }

        var fetched = 0;

        foreach (var source in Sources)
        {
            if (cancellation.IsCancellationRequested)
            {
                return;
            }

            var name = Path.Combine(directory, Path.GetFileName(new Uri(source).LocalPath));

            // nothing to do if the copy on disk is younger than the interval,
            // which is what makes a restart cheap
            if (File.Exists(name) && DateTime.UtcNow - File.GetLastWriteTimeUtc(name) < options.GeoRefresh)
            {
                continue;
            }

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };

                var body = await client.GetStringAsync(source, cancellation);

                // written beside and moved, so a download cut off halfway
                // leaves yesterday's copy rather than half of today's
                var scratch = name + ".writing";

                await File.WriteAllTextAsync(scratch, body, cancellation);

                File.Move(scratch, name, true);

                fetched++;
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                // one registry being unreachable is four fifths of a table,
                // which is better than none and not worth a failed job
                logger.LogDebug(e, "Could not refresh {Source}", source);
            }
        }

        if (fetched > 0 && Rebuild(directory))
        {
            logger.LogInformation("Refreshed {Ranges:N0} allocated ranges from {Count} registry file(s)", table.Ranges, fetched);
        }

        if (options.GeoPlaces)
        {
            await PlacesAsync(directory, cancellation);
        }
    }

    /// <summary>
    /// Keeps the city and network databases current.
    /// </summary>
    /// <remarks>
    /// Published monthly under a name that carries the month, so the file to
    /// ask for is derived from the date and last month's is accepted when this
    /// month's is not up yet. The old one is left where it is until the new
    /// one has arrived whole.
    /// </remarks>
    private async ValueTask PlacesAsync(string directory, CancellationToken cancellation)
    {
        var opened = false;

        foreach (var kind in new[] { "city", "asn" })
        {
            var target = Path.Combine(directory, $"dbip-{kind}.mmdb");

            if (File.Exists(target) && DateTime.UtcNow - File.GetLastWriteTimeUtc(target) < TimeSpan.FromDays(28))
            {
                opened = true;
                continue;
            }

            foreach (var month in new[] { DateTime.UtcNow, DateTime.UtcNow.AddMonths(-1) })
            {
                var source = $"https://download.db-ip.com/free/dbip-{kind}-lite-{month:yyyy-MM}.mmdb.gz";

                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(20) };

                    await using var response = await client.GetStreamAsync(source, cancellation);
                    await using var expand = new System.IO.Compression.GZipStream(response, System.IO.Compression.CompressionMode.Decompress);

                    var scratch = target + ".writing";

                    await using (var file = File.Create(scratch))
                    {
                        await expand.CopyToAsync(file, cancellation);
                    }

                    File.Move(scratch, target, true);

                    opened = true;

                    logger.LogInformation("Fetched the {Kind} database for {Month:yyyy-MM}", kind, month);

                    break;
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    logger.LogDebug(e, "Could not fetch {Source}", source);
                }
            }
        }

        if (opened)
        {
            places.Open(Path.Combine(directory, "dbip-city.mmdb"), Path.Combine(directory, "dbip-asn.mmdb"));
        }
    }

    private bool Rebuild(string directory)
    {
        try
        {
            var files = Directory.GetFiles(directory, "delegated-*-latest");

            if (files.Length == 0)
            {
                return false;
            }

            table.Load(files.SelectMany(File.ReadLines));

            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "The country table could not be built");

            return false;
        }
    }

}
