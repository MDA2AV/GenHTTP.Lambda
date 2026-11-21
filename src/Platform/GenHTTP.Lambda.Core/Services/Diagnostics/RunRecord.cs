namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// What one run of this process was doing, last time anybody looked.
/// </summary>
/// <remarks>
/// Written to disk rather than kept in memory, because the question it answers
/// is about a process that is no longer there to ask. A run that ends properly
/// stamps <see cref="Stopped"/> on its way out; one that does not leaves the
/// last heartbeat it managed, and the next start reads that and says so.
/// </remarks>
/// <param name="Heartbeat">When this was last written, which for a run that
/// vanished is as close to the time of death as anything gets</param>
/// <param name="Signalled">When a stop was asked for, if one was</param>
/// <param name="Stopped">When the shutdown finished. Absent means it did not.</param>
/// <param name="Fault">The unhandled exception that ended it, if there was one</param>
public sealed record RunRecord(
    DateTime Started,
    int Pid,
    DateTime Heartbeat,
    long WorkingSet,
    long Managed,
    long Requests,
    int Sockets,
    DateTime? Signalled = null,
    DateTime? Stopped = null,
    string? Fault = null
);
