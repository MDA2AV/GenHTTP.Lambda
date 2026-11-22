namespace GenHTTP.Lambda.Services.Diagnostics;

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
