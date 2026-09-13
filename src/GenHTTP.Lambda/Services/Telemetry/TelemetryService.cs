using System.Diagnostics;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Keeps the counters the concern feeds and a window of readings taken from
/// the runtime, so the memory of a long running server can be looked at as a
/// curve rather than a number.
/// </summary>
/// <remarks>
/// The readings live in memory and go away with the process. That is the right
/// trade for what they are for - a restart ends the run they were measuring
/// anyway - and it keeps the database out of the path of something that writes
/// every few seconds.
/// </remarks>
public sealed class TelemetryService : ITelemetryService
{
    private readonly LambdaOptions _options;

    private readonly Process _process = Process.GetCurrentProcess();

    private readonly Lock _lock = new();

    private readonly Queue<TelemetrySample> _samples = new();

    private long _requests;
    private long _failed;
    private long _upgrades;
    private long _elapsedTicks;
    private int _inFlight;
    private int _openSockets;

    // the deltas of the previous reading, so a sample describes the interval
    private long _lastRequests;
    private long _lastFailed;
    private long _lastUpgrades;
    private long _lastElapsedTicks;
    private TimeSpan _lastCpu;
    private DateTime _lastTaken;

    #region Initialization

    public TelemetryService(LambdaOptions options)
    {
        _options = options;

        _lastCpu = _process.TotalProcessorTime;
        _lastTaken = DateTime.UtcNow;

        Started = DateTime.UtcNow;
    }

    #endregion

    #region Get-/Setters

    /// <summary>
    /// When the service was created, which is as good as when the server came up.
    /// </summary>
    public DateTime Started { get; }

    /// <summary>
    /// Every request the server has answered since it started.
    /// </summary>
    public long TotalRequests => Interlocked.Read(ref _requests);

    /// <summary>
    /// Every request that was answered with a server error.
    /// </summary>
    public long TotalFailed => Interlocked.Read(ref _failed);

    /// <summary>
    /// Every connection that was upgraded rather than answered.
    /// </summary>
    public long TotalUpgrades => Interlocked.Read(ref _upgrades);

    /// <summary>
    /// Connections that were upgraded and have not closed again.
    /// </summary>
    public int OpenSockets => Volatile.Read(ref _openSockets);

    #endregion

    #region Functionality

    public void Record(TimeSpan elapsed, int status)
    {
        Interlocked.Increment(ref _requests);
        Interlocked.Add(ref _elapsedTicks, elapsed.Ticks);

        if (status >= 500)
        {
            Interlocked.Increment(ref _failed);
        }
    }

    public void Upgraded()
    {
        Interlocked.Increment(ref _upgrades);
        Interlocked.Increment(ref _openSockets);
    }

    /// <summary>
    /// Called when an upgraded connection is done with.
    /// </summary>
    public void Closed() => Interlocked.Decrement(ref _openSockets);

    public IDisposable Track()
    {
        Interlocked.Increment(ref _inFlight);

        return new Tracker(this);
    }

    public TelemetrySample Sample()
    {
        var now = DateTime.UtcNow;

        var info = GC.GetGCMemoryInfo();

        _process.Refresh();

        var cpu = _process.TotalProcessorTime;

        var requests = Interlocked.Read(ref _requests);
        var failed = Interlocked.Read(ref _failed);
        var upgrades = Interlocked.Read(ref _upgrades);
        var ticks = Interlocked.Read(ref _elapsedTicks);

        TelemetrySample sample;

        lock (_lock)
        {
            var wall = (now - _lastTaken).TotalSeconds;

            // over the interval, across every core the process was given
            var busy = (cpu - _lastCpu).TotalSeconds;

            var handled = requests - _lastRequests;

            sample = new TelemetrySample(
                now,
                GC.GetTotalMemory(false),
                info.TotalCommittedBytes,
                info.FragmentedBytes,
                _process.WorkingSet64,
                _process.PrivateMemorySize64,
                GC.CollectionCount(0),
                GC.CollectionCount(1),
                GC.CollectionCount(2),
                GC.GetTotalAllocatedBytes(false),
                info.PauseTimePercentage,
                wall > 0 ? Math.Round(busy / wall / Environment.ProcessorCount * 100, 2) : 0,
                _process.Threads.Count,
                handled,
                failed - _lastFailed,
                upgrades - _lastUpgrades,
                Volatile.Read(ref _inFlight),
                Volatile.Read(ref _openSockets),
                handled > 0 ? Math.Round(TimeSpan.FromTicks(ticks - _lastElapsedTicks).TotalMilliseconds / handled, 2) : 0
            );

            _lastRequests = requests;
            _lastFailed = failed;
            _lastUpgrades = upgrades;
            _lastElapsedTicks = ticks;
            _lastCpu = cpu;
            _lastTaken = now;

            _samples.Enqueue(sample);

            while (_samples.Count > _options.TelemetrySamples)
            {
                _samples.Dequeue();
            }
        }

        return sample;
    }

    public IReadOnlyList<TelemetrySample> Series(TimeSpan window)
    {
        var since = DateTime.UtcNow - window;

        lock (_lock)
        {
            return [.. _samples.Where(s => s.Taken >= since)];
        }
    }

    private sealed class Tracker(TelemetryService owner) : IDisposable
    {
        private int _done;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _done, 1) == 0)
            {
                Interlocked.Decrement(ref owner._inFlight);
            }
        }
    }

    #endregion

}
