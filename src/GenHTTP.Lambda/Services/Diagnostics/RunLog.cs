using System.Diagnostics;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Keeps a note on disk of how this run is doing, so that the next one can say
/// how it ended.
/// </summary>
/// <remarks>
/// "Did it just crash" is a question the server cannot answer about itself:
/// whatever it would have said went down with it. So it leaves a note instead,
/// refreshed on a heartbeat, and the run after reads it. A note whose last
/// heartbeat is recent and which has no stop stamped on it is a process that
/// went away between one beat and the next - and the memory and load it was
/// carrying at that beat are usually the whole of the answer.
///
/// One small file, replaced atomically, so a kill halfway through a write
/// leaves the previous note rather than half of this one.
/// </remarks>
public sealed class RunLog
{

    #region Get-/Setters

    /// <summary>
    /// How the run before this one ended, or nothing if this is the first.
    /// </summary>
    public LastRun? Previous { get; private set; }

    private string Path { get; }

    private string Scratch { get; }

    private Lock Gate { get; } = new();

    private RunRecord Current { get; set; }

    #endregion

    #region Initialization

    public RunLog(string directory)
    {
        Path = System.IO.Path.Combine(directory, "run.json");
        Scratch = Path + ".writing";

        Previous = ReadPrevious();

        Current = new RunRecord(DateTime.UtcNow, Environment.ProcessId, DateTime.UtcNow, 0, 0, 0, 0);

        Write();
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Says what this run is carrying now. Called on the telemetry tick.
    /// </summary>
    public void Beat(long workingSet, long managed, long requests, int sockets)
    {
        lock (Gate)
        {
            Current = Current with
            {
                Heartbeat = DateTime.UtcNow,
                WorkingSet = workingSet,
                Managed = managed,
                Requests = requests,
                Sockets = sockets
            };
        }

        Write();
    }

    /// <summary>
    /// Notes that a stop was asked for, before doing anything about it.
    /// </summary>
    /// <remarks>
    /// Stamped as the signal arrives rather than once the shutdown is done, so
    /// that a run which is told to stop and then dies partway through can be
    /// told from one that was never asked at all.
    /// </remarks>
    public void Signalled()
    {
        lock (Gate)
        {
            Current = Current with { Signalled = DateTime.UtcNow };
        }

        Write();
    }

    /// <summary>
    /// Notes that this run ended the way it meant to.
    /// </summary>
    public void Stopped()
    {
        lock (Gate)
        {
            Current = Current with { Stopped = DateTime.UtcNow, Heartbeat = DateTime.UtcNow };
        }

        Write();
    }

    /// <summary>
    /// Notes what ended it, for a run that is about to be ended by something
    /// it did not expect.
    /// </summary>
    public void Faulted(Exception error)
    {
        lock (Gate)
        {
            Current = Current with { Fault = Describe(error), Heartbeat = DateTime.UtcNow };
        }

        Write();
    }

    /// <summary>
    /// Says what the last run did, in a line, or nothing if it went quietly
    /// and properly.
    /// </summary>
    public void Report(ILogger logger)
    {
        if (Previous is not { } last)
        {
            return;
        }

        if (last.Clean)
        {
            logger.LogInformation("The previous run stopped cleanly after {Minutes:N0} minutes", last.Minutes);

            return;
        }

        logger.LogWarning(
            "The previous run ended without stopping: {Minutes:N0} minutes in, last seen {LastSeen:u} holding {Memory:N0} MB " +
            "with {Sockets:N0} socket(s) open after {Requests:N0} request(s){Asked}{Fault}",
            last.Minutes, last.LastSeen, last.WorkingSet / 1024 / 1024, last.Sockets, last.Requests,
            last.Signalled ? ", having been asked to stop" : ", nothing having asked it to",
            last.Fault == null ? "" : $" — {last.Fault}");
    }

    private LastRun? ReadPrevious()
    {
        try
        {
            if (!File.Exists(Path))
            {
                return null;
            }

            var record = JsonSerializer.Deserialize<RunRecord>(File.ReadAllText(Path));

            if (record == null)
            {
                return null;
            }

            return new LastRun(
                record.Started,
                record.Stopped ?? record.Heartbeat,
                ((record.Stopped ?? record.Heartbeat) - record.Started).TotalMinutes,
                record.Stopped != null,
                record.Signalled != null,
                record.Fault,
                record.WorkingSet,
                record.Requests,
                record.Sockets
            );
        }
        catch (Exception)
        {
            // an unreadable note is worth exactly as much as no note, and this
            // runs before anything else on the way up: it does not get to stop
            // the server from starting
            return null;
        }
    }

    private void Write()
    {
        try
        {
            RunRecord snapshot;

            lock (Gate)
            {
                snapshot = Current;
            }

            File.WriteAllText(Scratch, JsonSerializer.Serialize(snapshot));

            // replaces in one step, so a kill during the write above leaves
            // the note from the beat before rather than a truncated one
            File.Move(Scratch, Path, true);
        }
        catch (Exception)
        {
            // best effort by nature - the disk being full is not a reason to
            // bring down a server that is otherwise fine
        }
    }

    private static string Describe(Exception error)
    {
        var text = $"{error.GetType().Name}: {error.Message}";

        return text.Length <= 400 ? text : string.Concat(text.AsSpan(0, 400), " …");
    }

    #endregion

}
