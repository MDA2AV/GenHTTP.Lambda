using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Keeps no note about the run, for a platform that shares its process: when
/// the process starts, stops and fails is its host's to write down, and the
/// end of one is no news about a single installation in it.
/// </summary>
public sealed class NoRunLog : IRunLog
{

    public LastRun? Previous => null;

    public void Beat(long workingSet, long managed, long requests, int sockets) { }

    public void Signalled() { }

    public void Stopped() { }

    public void Faulted(Exception error) { }

    public void Report(ILogger logger) { }

}
