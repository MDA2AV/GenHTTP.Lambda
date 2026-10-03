using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// How the runs of the server ended, kept on disk across them.
/// </summary>
public interface IRunLog
{

    /// <summary>
    /// How the run before this one ended, or nothing if this is the first.
    /// </summary>
    LastRun? Previous { get; }

    /// <summary>
    /// Says what this run is carrying now. Called on the telemetry tick.
    /// </summary>
    void Beat(long workingSet, long managed, long requests, int sockets);

    /// <summary>
    /// Notes that a stop was asked for, before doing anything about it.
    /// </summary>
    void Signalled();

    /// <summary>
    /// Notes that this run ended the way it meant to.
    /// </summary>
    void Stopped();

    /// <summary>
    /// Notes what ended it, for a run that is about to be ended by something
    /// it did not expect.
    /// </summary>
    void Faulted(Exception error);

    /// <summary>
    /// Says what the last run did, in a line, or nothing if it went quietly
    /// and properly.
    /// </summary>
    void Report(ILogger logger);

}
