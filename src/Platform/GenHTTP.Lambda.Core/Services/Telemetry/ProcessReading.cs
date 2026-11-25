namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// How the process looked at one moment.
/// </summary>
/// <param name="ManagedBytes">What the managed heap holds</param>
/// <param name="HeapCommittedBytes">What the managed heap has committed</param>
/// <param name="HeapFragmentedBytes">Of that, what lies free between objects</param>
/// <param name="WorkingSetBytes">The working set</param>
/// <param name="PrivateBytes">The private memory</param>
/// <param name="Memory">The resident memory, by what holds it</param>
/// <param name="Gen0Collections">Generation 0 collections since the start</param>
/// <param name="Gen1Collections">Generation 1 collections since the start</param>
/// <param name="Gen2Collections">Generation 2 collections since the start</param>
/// <param name="AllocatedBytes">Everything allocated since the start</param>
/// <param name="PausePercentage">The share of time spent paused for collections, in percent</param>
/// <param name="CpuPercentage">The share of every core used since the previous reading, in percent</param>
/// <param name="Threads">How many threads there are</param>
/// <param name="Connections">The connections, as the kernel counts them</param>
/// <param name="Accepted">Connections accepted since the previous reading</param>
/// <param name="Descriptors">The open file descriptors</param>
public readonly record struct ProcessReading(
    long ManagedBytes, long HeapCommittedBytes, long HeapFragmentedBytes, long WorkingSetBytes, long PrivateBytes,
    ProcessProbe.MemoryBreakdown Memory,
    int Gen0Collections, int Gen1Collections, int Gen2Collections, long AllocatedBytes, double PausePercentage,
    double CpuPercentage, int Threads,
    ProcessProbe.ConnectionCounters Connections, long Accepted,
    ProcessProbe.DescriptorCounts Descriptors
);
