namespace GenHTTP.Lambda.Configuration;

/// <summary>
/// How what a process has once is configured (see <see cref="LambdaRuntime"/>),
/// for a host that runs several platforms in one.
/// </summary>
/// <remarks>
/// A platform on its own makes its runtime from its <see cref="LambdaOptions"/>
/// (<see cref="Of"/>) and needs none of this. One that is handed a runtime
/// leaves what is named here to it: its own <see cref="LambdaOptions.GeoRefresh"/>,
/// <see cref="LambdaOptions.CrawlerLists"/> and <see cref="LambdaOptions.CrawlerRefresh"/>
/// are not read, while <see cref="LambdaOptions.Geo"/> still decides whether
/// its log and its analytics name the country of a caller.
/// </remarks>
public sealed record RuntimeOptions
{

    /// <summary>
    /// The directory holding what the process fetches and compiles: the
    /// registry files, the lists of crawlers and the assemblies of the lambdas.
    /// </summary>
    /// <remarks>
    /// Nothing in it is anybody's data, so it needs no backup - and it is no
    /// platform's, which is the point: what is compiled into the process
    /// outlives the platform it was compiled for.
    /// </remarks>
    public string DataDirectory { get; init; } = Path.Combine(AppContext.BaseDirectory, "runtime");

    /// <summary>
    /// Whether the delegation files of the registries are fetched, see <see cref="LambdaOptions.Geo"/>.
    /// </summary>
    public bool Geo { get; init; } = true;

    /// <summary>
    /// Whether the city and network databases are fetched as well, see <see cref="LambdaOptions.GeoPlaces"/>.
    /// </summary>
    public bool GeoPlaces { get; init; } = true;

    /// <summary>
    /// How often the registry files are asked for again.
    /// </summary>
    public TimeSpan GeoRefresh { get; init; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Whether what tells crawlers from people is fetched, see <see cref="LambdaOptions.CrawlerLists"/>.
    /// </summary>
    public bool CrawlerLists { get; init; } = true;

    /// <summary>
    /// How often those lists are asked for again.
    /// </summary>
    public TimeSpan CrawlerRefresh { get; init; } = TimeSpan.FromDays(1);

    /// <summary>
    /// How many requests to lambdas may run at the same time, those of every
    /// platform together. Each is held to its own <see cref="LambdaOptions.MaxConcurrency"/> as well.
    /// </summary>
    public int MaxConcurrency { get; init; } = 256;

    #region Functionality

    /// <summary>
    /// What a platform on its own runs on: its own options and its own data directory.
    /// </summary>
    public static RuntimeOptions Of(LambdaOptions options) => new()
    {
        DataDirectory = options.DataDirectory,
        Geo = options.Geo,
        GeoPlaces = options.GeoPlaces,
        GeoRefresh = options.GeoRefresh,
        CrawlerLists = options.CrawlerLists,
        CrawlerRefresh = options.CrawlerRefresh,
        MaxConcurrency = options.MaxConcurrency
    };

    #endregion

}
