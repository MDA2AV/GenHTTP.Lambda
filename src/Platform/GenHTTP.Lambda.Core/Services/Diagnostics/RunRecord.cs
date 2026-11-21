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

/// <summary>
/// How the run before this one ended, for whoever is asking why the server
/// restarted.
/// </summary>
/// <param name="Clean">
/// Whether it was asked to stop and finished doing so. False is the
/// interesting case: the process went away between one heartbeat and the next
/// without being asked and without saying why.
/// </param>
/// <param name="Signalled">
/// Whether a stop was asked for at all. Signalled but not clean means it was
/// told to go and died partway; neither means nothing asked it to.
/// </param>
public sealed record LastRun(
    DateTime Started,
    DateTime LastSeen,
    double Minutes,
    bool Clean,
    bool Signalled,
    string? Fault,
    long WorkingSet,
    long Requests,
    int Sockets
);
