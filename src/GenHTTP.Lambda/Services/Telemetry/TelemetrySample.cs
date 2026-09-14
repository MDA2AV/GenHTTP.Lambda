namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// One reading of the process, taken on an interval so a trend can be seen
/// rather than a single number.
/// </summary>
/// <remarks>
/// The memory figures are the point of the exercise: a managed heap that keeps
/// climbing across gen 2 collections is what a leak looks like from here, and
/// the committed and fragmented bytes tell apart a heap that is genuinely
/// holding objects from one that has simply not been given back to the system.
/// </remarks>
public sealed record TelemetrySample(
    DateTime Taken,

    // memory
    long ManagedBytes,
    long HeapCommittedBytes,
    long HeapFragmentedBytes,
    long WorkingSetBytes,
    long PrivateBytes,

    // where the resident set actually went, which the runtime cannot say
    long ResidentBytes,
    long AnonymousBytes,
    long JitBytes,
    long AssemblyBytes,
    long OtherFileBytes,
    long SwapBytes,

    // the collector
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections,
    long AllocatedBytes,
    double PausePercentage,

    // the process
    double CpuPercentage,
    int Threads,

    // what the server did since the previous sample
    long Requests,
    long Failed,
    long Upgrades,
    int InFlight,
    int OpenSockets,
    double AverageMillis,

    // connections, which are not the same thing as requests: a visitor who
    // reloads a page opens a new one, while a browser holding a tab open
    // makes many requests down a single one
    int OpenConnections,
    long AcceptedConnections,
    long Connections,
    int FileDescriptors,
    int SocketDescriptors,
    int RingDescriptors
);
