using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.QuietHours;

/// <summary>
/// How busy the installation, or a lambda, is on a typical day, by the hour -
/// and when it is quietest.
/// </summary>
/// <remarks>
/// An indication, not a measurement: see <see cref="QuietProfile"/> for what it
/// leaves out. Whoever plans work for a quiet hour asks <see cref="IsQuiet"/>.
/// </remarks>
[Summary("How busy the installation, or a lambda, is on a typical day by the hour, and when it is quietest - an indication rather than a measurement.")]
public sealed record TypicalDay(
    [Summary("Whether enough was counted to tell its hours apart: every hour of the day counted at least once, over a day at the least, with a request an hour on average. Until then the hours are shown as far as they were counted, and the quietest stretch is not to be relied on.")]
    bool Known,
    [Summary("The requests of each hour of a typical day in UTC, from midnight on: 24 numbers, an average in which a week ago counts half as much as today; all nothing where nothing was counted")]
    IReadOnlyList<double> Hours,
    [Summary("The first hour of the quietest stretch, in UTC (0 to 23); the stretch may go past midnight")]
    int QuietFrom,
    [Summary("How many hours the quietest stretch is")]
    int QuietLength,
    [Summary("What share of a typical day's requests falls into the quietest stretch, from 0 to 1")]
    double QuietShare,
    [Summary("When counting began; absent where nothing was counted yet")]
    DateTime? Since,
    [Summary("When what was counted was last written down, which is hourly where anything was asked for")]
    DateTime? Updated
)
{

    /// <summary>
    /// A day nothing was counted of.
    /// </summary>
    public static readonly TypicalDay Unknown = new(false, [.. Enumerable.Repeat(0.0, QuietProfile.Hours)], 0, QuietProfile.Stretch, 0, null, null);

    /// <summary>
    /// Whether an hour of the day, in UTC, is one of the quietest.
    /// </summary>
    public bool IsQuiet(int hour) => ((hour - QuietFrom) % QuietProfile.Hours + QuietProfile.Hours) % QuietProfile.Hours < QuietLength;

}
