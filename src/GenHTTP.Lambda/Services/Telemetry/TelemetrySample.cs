using GenHTTP.Api.Content.Services;

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
[Summary("One reading of the server process.")]
[Remarks("The breakdown of the resident memory and the descriptors are read from `/proc` on Linux, and are zero elsewhere.")]
public sealed record TelemetrySample(
    [Summary("When it was taken")]
    DateTime Taken,

    // memory

    [Summary("What the managed heap holds")]
    long ManagedBytes,
    [Summary("What the managed heap has committed")]
    long HeapCommittedBytes,
    [Summary("Of that, what lies free between objects")]
    long HeapFragmentedBytes,
    [Summary("The working set of the process")]
    long WorkingSetBytes,
    [Summary("The private memory of the process")]
    long PrivateBytes,

    // where the resident set actually went, which the runtime cannot say

    [Summary("Everything the process has in physical memory")]
    long ResidentBytes,
    [Summary("Of that, anonymous memory: heaps and stacks")]
    long AnonymousBytes,
    [Summary("Of that, code the JIT compiled")]
    long JitBytes,
    [Summary("Of that, assemblies mapped from disk")]
    long AssemblyBytes,
    [Summary("Of that, other files mapped from disk")]
    long OtherFileBytes,
    [Summary("What was swapped out")]
    long SwapBytes,

    // the collector

    [Summary("Generation 0 collections since the start")]
    int Gen0Collections,
    [Summary("Generation 1 collections since the start")]
    int Gen1Collections,
    [Summary("Generation 2 collections since the start")]
    int Gen2Collections,
    [Summary("Everything allocated since the start")]
    long AllocatedBytes,
    [Summary("The share of time spent paused for collections, in percent")]
    double PausePercentage,

    // the process

    [Summary("The share of every core the process used since the previous reading, in percent")]
    double CpuPercentage,
    [Summary("Threads of the process")]
    int Threads,

    // what the server did since the previous sample

    [Summary("Requests answered since the previous reading")]
    long Requests,
    [Summary("Of those, answered with a server error")]
    long Failed,
    [Summary("Websocket upgrades since the previous reading")]
    long Upgrades,
    [Summary("Requests being answered as it was taken")]
    int InFlight,
    [Summary("Open connections as it was taken")]
    int OpenSockets,
    [Summary("The mean time to answer since the previous reading, in milliseconds")]
    double AverageMillis,

    // connections, which are not the same thing as requests: a visitor who
    // reloads a page opens a new one, while a browser holding a tab open
    // makes many requests down a single one

    [Summary("Connections established as it was taken")]
    int OpenConnections,
    [Summary("Connections accepted since the start")]
    long AcceptedConnections,
    [Summary("Connections accepted since the previous reading")]
    long Connections,
    [Summary("File descriptors the process holds")]
    int FileDescriptors,
    [Summary("Of those, sockets")]
    int SocketDescriptors,
    [Summary("Of those, io_uring rings")]
    int RingDescriptors
);
