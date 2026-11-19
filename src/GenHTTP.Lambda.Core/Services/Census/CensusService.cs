using GenHTTP.Lambda.Infrastructure;

namespace GenHTTP.Lambda.Services.Census;

/// <summary>
/// Counts the lambdas and measures the room they take, for the operator's
/// dashboards.
/// </summary>
/// <remarks>
/// The counts are a few queries and answered as asked. The room is a walk
/// over every file of the data directory, so the last measurement is kept and
/// renewed by <see cref="StorageCensusJob"/> - the free space beside it is
/// read on every request, since that is what runs out. A walk is taken in
/// one hop off the reactor, and one at a time: whoever asks while one runs
/// waits for it rather than walking again.
/// </remarks>
public sealed class CensusService(LambdaTally tally, StorageSurvey survey) : ICensusService
{
    private readonly SemaphoreSlim _measuring = new(1, 1);

    private StorageCensus? _measured;

    #region Functionality

    public LambdaCensus Count() => tally.Count(DateTime.UtcNow);

    public (long? Size, long? Free) Disk() => survey.Disk();

    public async ValueTask<StorageCensus> StorageAsync(CancellationToken cancellation = default)
    {
        var measured = Volatile.Read(ref _measured) ?? await MeasureOnceAsync(cancellation);

        var (size, free) = survey.Disk();

        return measured with { DiskBytes = size, FreeBytes = free };
    }

    public async ValueTask<StorageCensus> MeasureAsync(CancellationToken cancellation = default)
    {
        await _measuring.WaitAsync(cancellation);

        try
        {
            return await TakeAsync(cancellation);
        }
        finally
        {
            _measuring.Release();
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Measures unless somebody did while this waited for its turn.
    /// </summary>
    private async ValueTask<StorageCensus> MeasureOnceAsync(CancellationToken cancellation)
    {
        await _measuring.WaitAsync(cancellation);

        try
        {
            return Volatile.Read(ref _measured) ?? await TakeAsync(cancellation);
        }
        finally
        {
            _measuring.Release();
        }
    }

    private async ValueTask<StorageCensus> TakeAsync(CancellationToken cancellation)
    {
        var measured = await Offload.Run(() => survey.MeasureAsync(DateTime.UtcNow, cancellation), cancellation);

        Volatile.Write(ref _measured, measured);

        return measured;
    }

    #endregion

}
