using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Background;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// Keeps what tells crawlers from people current: the list of crawlers, and
/// the addresses the operators that publish them crawl from.
/// </summary>
/// <remarks>
/// New crawlers appear every month and the operators move their addresses
/// more often than that, so both are fetched again daily by default rather
/// than shipped once. What was fetched is kept on the data volume and read on
/// the way up, like the country table (see <c>GeoJob</c>): a server that comes
/// back while the sources are unreachable knows what it knew yesterday. One
/// that has never reached them has the list of its release and confirms no
/// crawler - every claim is counted as unconfirmed, which is what it is.
/// </remarks>
public sealed class CrawlerJob(CrawlerList crawlers, CrawlerRanges ranges, LambdaOptions options, ILogger<CrawlerJob> logger) : IBackgroundJob
{

    /// <summary>
    /// Where the list of crawlers is published.
    /// </summary>
    private const string ListSource = "https://raw.githubusercontent.com/monperrus/crawler-user-agents/master/crawler-user-agents.json";

    private const string ListFile = "crawler-user-agents.json";

    private bool _loaded;

    public string Name => "Crawlers";

    public TimeSpan Interval => options.CrawlerRefresh;

    public async ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        if (!options.CrawlerLists)
        {
            return;
        }

        var directory = Path.Combine(options.DataDirectory, "crawlers");

        Directory.CreateDirectory(directory);

        // whatever is on disk first, so a restart confirms crawlers within
        // milliseconds rather than after a dozen downloads
        if (!_loaded)
        {
            _loaded = true;

            LoadList(directory);

            foreach (var claimed in CrawlerRanges.Operators)
            {
                LoadRanges(directory, claimed);
            }
        }

        if (await FetchAsync(ListSource, Path.Combine(directory, ListFile), cancellation) && LoadList(directory))
        {
            logger.LogInformation("Refreshed the list of {Count:N0} crawlers", crawlers.Count);
        }

        foreach (var claimed in CrawlerRanges.Operators)
        {
            var fetched = false;

            for (var i = 0; i < claimed.Lists.Count; i++)
            {
                fetched |= await FetchAsync(claimed.Lists[i], FileOf(directory, claimed, i), cancellation);
            }

            if (fetched && LoadRanges(directory, claimed))
            {
                logger.LogInformation("Refreshed the addresses of the crawlers of {Operator}", claimed.Name);
            }
        }
    }

    /// <summary>
    /// Fetches a source where the copy on disk is older than the interval.
    /// </summary>
    /// <returns>Whether a new copy was written</returns>
    private async ValueTask<bool> FetchAsync(string source, string file, CancellationToken cancellation)
    {
        // nothing to do if the copy is younger than the interval, which is
        // what makes a restart cheap
        if (File.Exists(file) && DateTime.UtcNow - File.GetLastWriteTimeUtc(file) < options.CrawlerRefresh)
        {
            return false;
        }

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };

            client.DefaultRequestHeaders.UserAgent.ParseAdd("GenHTTP-Lambda (+https://genhttp.dev)");

            var body = await client.GetByteArrayAsync(source, cancellation);

            // written beside and moved, so a download cut off halfway leaves
            // yesterday's copy rather than half of today's
            var scratch = file + ".writing";

            File.WriteAllBytes(scratch, body);

            File.Move(scratch, file, true);

            return true;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // one source unreachable leaves what is known of it, which is not
            // worth a failed job
            logger.LogDebug(e, "Failed to refresh {Source}", source);
            return false;
        }
    }

    private bool LoadList(string directory)
    {
        var file = Path.Combine(directory, ListFile);

        if (!File.Exists(file))
        {
            return false;
        }

        using var stream = File.OpenRead(file);

        if (crawlers.Load(stream))
        {
            return true;
        }

        logger.LogWarning("The list of crawlers in {File} does not read, the one before stays", file);
        return false;
    }

    private bool LoadRanges(string directory, CrawlerOperator claimed)
    {
        var files = Enumerable.Range(0, claimed.Lists.Count).Select(i => FileOf(directory, claimed, i)).Where(File.Exists).ToList();

        if (files.Count == 0)
        {
            return false;
        }

        var streams = files.Select(f => (Stream)File.OpenRead(f)).ToList();

        try
        {
            return ranges.Load(claimed.Name, streams);
        }
        finally
        {
            streams.ForEach(s => s.Dispose());
        }
    }

    private static string FileOf(string directory, CrawlerOperator claimed, int index)
        => Path.Combine(directory, $"{claimed.Name.ToLowerInvariant()}-{index}.json");

}
