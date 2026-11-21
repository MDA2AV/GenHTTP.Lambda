namespace GenHTTP.Lambda.Services.Census;

/// <summary>
/// The installation counted for whoever runs it: its lambdas, and the room
/// they take.
/// </summary>
/// <remarks>
/// For the operator's dashboards only. Unlike the usage report, which says
/// the same kind of thing to the project, this names lambdas - by their
/// public key - and stays behind the admin token.
/// </remarks>
public interface ICensusService
{

    /// <summary>
    /// The lambdas counted as of now: a few counts on the platform's database.
    /// </summary>
    ValueTask<LambdaCensus> CountAsync(CancellationToken cancellation = default);

    /// <summary>
    /// How large the volume of the data directory is and how much of it is
    /// free, read as of now - nothing where the system does not say.
    /// </summary>
    (long? Size, long? Free) Disk();

    /// <summary>
    /// The room the installation has, read as of now, and what takes it, as
    /// last measured - measured now where it never was.
    /// </summary>
    ValueTask<StorageCensus> StorageAsync(CancellationToken cancellation = default);

    /// <summary>
    /// Measures what takes the room again, off the thread that asked.
    /// </summary>
    ValueTask<StorageCensus> MeasureAsync(CancellationToken cancellation = default);

}
