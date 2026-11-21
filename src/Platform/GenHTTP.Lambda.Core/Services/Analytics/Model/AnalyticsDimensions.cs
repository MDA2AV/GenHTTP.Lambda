namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// The names the dimensions have in the database and in what is handed out.
/// </summary>
public static class AnalyticsDimensions
{

    public static IReadOnlyList<AnalyticsDimension> All { get; } = Enum.GetValues<AnalyticsDimension>();

    /// <summary>
    /// The table of a dimension, which is also its name in an answer.
    /// </summary>
    public static string TableOf(AnalyticsDimension dimension) => dimension switch
    {
        AnalyticsDimension.Pages => "pages",
        AnalyticsDimension.Referrers => "referrers",
        AnalyticsDimension.Countries => "countries",
        AnalyticsDimension.Devices => "devices",
        AnalyticsDimension.Browsers => "browsers",
        AnalyticsDimension.Systems => "systems",
        AnalyticsDimension.Crawlers => "crawlers",
        _ => throw new ArgumentOutOfRangeException(nameof(dimension))
    };

    /// <summary>
    /// How many different values one lambda may add to a dimension in a day,
    /// or nothing where what it can hold is bounded anyway. The rest is
    /// counted under <see cref="Other"/>.
    /// </summary>
    /// <remarks>
    /// Paths and referrers come from whoever sends a request, and a scanner
    /// trying ten thousand paths should not cost ten thousand rows a day.
    /// </remarks>
    public static int? MostOf(AnalyticsDimension dimension) => dimension switch
    {
        AnalyticsDimension.Pages => 200,
        AnalyticsDimension.Referrers => 100,
        _ => null
    };

    /// <summary>
    /// What the values past <see cref="MostOf"/> are counted under.
    /// </summary>
    public const string Other = "(other)";

}
