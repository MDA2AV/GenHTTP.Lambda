using System.Diagnostics;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Reads the process of an installation that has it for itself.
/// </summary>
public sealed class OwnProcess : IProcessReadings
{
    private readonly Process _process = Process.GetCurrentProcess();

    private readonly Lock _lock = new();

    // what the previous reading saw, so a reading describes the interval
    private TimeSpan _lastCpu;
    private DateTime _lastTaken;
    private long _lastAccepted;

    #region Initialization

    public OwnProcess()
    {
        _lastCpu = _process.TotalProcessorTime;
        _lastTaken = DateTime.UtcNow;
    }

    #endregion

    #region Get-/Setters

    public bool Shared => false;

    public int OpenConnections => ProcessProbe.Connections().Established;

    #endregion

    #region Functionality

    public ProcessReading Take()
    {
        var now = DateTime.UtcNow;

        var info = GC.GetGCMemoryInfo();

        _process.Refresh();

        var memory = ProcessProbe.Memory();

        var connections = ProcessProbe.Connections();

        var descriptors = ProcessProbe.Descriptors();

        var cpu = _process.TotalProcessorTime;

        lock (_lock)
        {
            var wall = (now - _lastTaken).TotalSeconds;

            // over the interval, across every core the process was given
            var busy = (cpu - _lastCpu).TotalSeconds;

            var reading = new ProcessReading(
                GC.GetTotalMemory(false),
                info.TotalCommittedBytes,
                info.FragmentedBytes,
                _process.WorkingSet64,
                _process.PrivateMemorySize64,
                memory,
                GC.CollectionCount(0),
                GC.CollectionCount(1),
                GC.CollectionCount(2),
                GC.GetTotalAllocatedBytes(),
                info.PauseTimePercentage,
                wall > 0 ? Math.Round(busy / wall / Environment.ProcessorCount * 100, 2) : 0,
                _process.Threads.Count,
                connections,
                // the first reading has nothing to subtract from, and reporting
                // every connection since boot as though they arrived in one
                // interval would put a spike at the left of every graph
                _lastAccepted > 0 ? Math.Max(0, connections.Accepted - _lastAccepted) : 0,
                descriptors
            );

            _lastAccepted = connections.Accepted;
            _lastCpu = cpu;
            _lastTaken = now;

            return reading;
        }
    }

    #endregion

}
