using GenHTTP.Lambda.Services.Visits;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// The visits of one writing, summed into the rows they are written as.
/// </summary>
/// <remarks>
/// A visit becomes one row of totals for its lambda and day and a row in each
/// dimension it says something about, and a writing adds to those rows rather
/// than to a row per visit: thousands of visits to the same page in five
/// minutes are one row to write. What a visit is counted as - the host it was
/// referred from, the device it came on - is worked out here, once, off the
/// requests.
/// </remarks>
public sealed class VisitTally(DailyVisitors visitors, VisitorClassifier classifier)
{
    private readonly Dictionary<(long Lambda, DateOnly Day), Totals> _days = [];

    private readonly Dictionary<Cell, Counts> _cells = [];

    #region Get-/Setters

    /// <summary>
    /// The totals of each lambda and day.
    /// </summary>
    public IEnumerable<(long Lambda, DateOnly Day, AnalyticsTotals Totals)> Days
        => _days.Select(d => (d.Key.Lambda, d.Key.Day,
                              new AnalyticsTotals(d.Value.Hits, d.Value.Views, d.Value.Visitors, d.Value.Failed, d.Value.Rejected,
                                                  d.Value.Bytes, d.Value.Millis)));

    /// <summary>
    /// What each value of a dimension came to, per lambda and day.
    /// </summary>
    public IEnumerable<(Cell Cell, long Hits, long Views)> Cells => _cells.Select(c => (c.Key, c.Value.Hits, c.Value.Views));

    /// <summary>
    /// The lambdas anything was counted for.
    /// </summary>
    public IEnumerable<long> Lambdas => _days.Keys.Select(k => k.Lambda).Distinct();

    public bool IsEmpty => _days.Count == 0;

    #endregion

    #region Functionality

    /// <summary>
    /// Counts a visit.
    /// </summary>
    public void Add(LambdaVisit visit)
    {
        var lambda = visit.Lambda.Id;

        var day = DateOnly.FromDateTime(visit.At);

        if (!_days.TryGetValue((lambda, day), out var totals))
        {
            _days[(lambda, day)] = totals = new Totals();
        }

        var view = visit.Page ? 1 : 0;

        totals.Hits++;
        totals.Views += view;
        totals.Bytes += visit.Bytes;

        // an upgraded connection returned at the handshake; what follows is a
        // socket, and its time says nothing about how fast the lambda answers
        if (visit.Status != 101)
        {
            totals.Millis += visit.Elapsed.TotalMilliseconds;
        }

        switch (visit.Status)
        {
            case >= 500:
                totals.Failed++;
                break;
            case >= 400:
                totals.Rejected++;
                break;
        }

        var agent = classifier.Classify(visit.Agent, visit.Hints, visit.Client);

        // a visitor is a person, counted with the first thing they asked for
        // that day, whatever it was - a crawler opens pages, but is nobody
        if (agent.Browser != null && visitors.IsNew(lambda, day, visit.Client, visit.Agent) == true)
        {
            totals.Visitors++;
        }

        // what was not there is not a page of the lambda: a scanner's guesses
        // are in the totals, and the paths stay the lambda's own
        if (visit.Status < 400)
        {
            Count(lambda, day, AnalyticsDimension.Pages, visit.Path, view);
        }

        Count(lambda, day, AnalyticsDimension.Countries, visit.Country ?? string.Empty, view);

        if (ReferrerOf(visit.Referrer, visit.Domain) is { } referrer)
        {
            Count(lambda, day, AnalyticsDimension.Referrers, referrer, view);
        }

        Count(lambda, day, AnalyticsDimension.Devices, agent.Device, view);

        if (agent.Browser != null)
        {
            Count(lambda, day, AnalyticsDimension.Browsers, agent.Browser, view);
        }

        if (agent.System != null)
        {
            Count(lambda, day, AnalyticsDimension.Systems, agent.System, view);
        }

        if (agent.Crawler != null)
        {
            Count(lambda, day, AnalyticsDimension.Crawlers, agent.Crawler, view);
        }
    }

    /// <summary>
    /// The site a visitor came from, by its host, or nothing where they came
    /// from the lambda itself or said nothing.
    /// </summary>
    /// <remarks>
    /// Only the host is kept. Most browsers send no more than that to another
    /// site anyway - a search engine is the address of the search engine and
    /// not what was searched for - and where they do send the whole address,
    /// it may carry what was typed into a form or a token of somebody's.
    /// The leading www is left off, so a site is one row whichever way it was
    /// written.
    /// </remarks>
    public static string? ReferrerOf(string? referrer, string? domain)
    {
        if (string.IsNullOrWhiteSpace(referrer) || !Uri.TryCreate(referrer, UriKind.Absolute, out var address) || address.Host.Length == 0)
        {
            return null;
        }

        var host = Bare(address.Host);

        if (domain != null && host == Bare(domain))
        {
            return null;
        }

        return host.Length <= 100 ? host : host[..100];
    }

    private static string Bare(string host)
    {
        host = host.ToLowerInvariant().TrimEnd('.');

        return host.StartsWith("www.", StringComparison.Ordinal) ? host[4..] : host;
    }

    private void Count(long lambda, DateOnly day, AnalyticsDimension dimension, string value, int view)
    {
        var cell = new Cell(lambda, day, dimension, value);

        if (!_cells.TryGetValue(cell, out var counts))
        {
            _cells[cell] = counts = new Counts();
        }

        counts.Hits++;
        counts.Views += view;
    }

    #endregion

    #region Rows

    /// <summary>
    /// One value of a dimension on a day of a lambda.
    /// </summary>
    public readonly record struct Cell(long Lambda, DateOnly Day, AnalyticsDimension Dimension, string Value);

    private sealed class Totals
    {
        internal long Hits;
        internal long Views;
        internal long Visitors;
        internal long Failed;
        internal long Rejected;
        internal long Bytes;
        internal double Millis;
    }

    private sealed class Counts
    {
        internal long Hits;
        internal long Views;
    }

    #endregion

}
