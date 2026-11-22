namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Which lambda, if any, the current thread of work belongs to.
/// </summary>
/// <remarks>
/// Lambdas run in this process and share its console, so a print from one is
/// indistinguishable from a print from anything else by the time it reaches
/// stdout. This is what tells them apart: the concern that serves a lambda
/// marks the request, the marking flows down every await into the code of the
/// user, and the writer below reads it back to decide whose line it is.
/// </remarks>
public static class LambdaOutput
{

    private static readonly AsyncLocal<OutputScope?> Current = new();

    private static readonly Lock Gate = new();

    private static OutputScope[] _process = [];

    /// <summary>
    /// The lambda being served here, or nothing.
    /// </summary>
    public static OutputScope? Ambient => Current.Value;

    /// <summary>
    /// Where console output belongs when no lambda is being served: the log
    /// of every application running in this process.
    /// </summary>
    /// <remarks>
    /// The engine prints its own lines - which reactor is listening on what,
    /// which connection handler faulted and why - straight to the console
    /// rather than through a logger, and they were being dropped: the tee only
    /// copied a write while a lambda was the one making it. Those lines are
    /// exactly what an operator opens a log to read, so they land here
    /// instead, under no lambda at all.
    ///
    /// Every application, not the last one to start: nothing on the console
    /// says which of them a line is about. A server runs one; tests run many
    /// side by side, and each keeps what it was running for.
    ///
    /// Read on every write to the console, so without a lock: a write sees
    /// the scopes as they were before or after a change, never half of one.
    /// </remarks>
    public static ReadOnlySpan<OutputScope> Process => Volatile.Read(ref _process);

    /// <summary>
    /// Files the process's own console output under the given scope as well,
    /// until it is detached again (see <see cref="ConsoleCapture"/>).
    /// </summary>
    public static void Attach(OutputScope scope)
    {
        lock (Gate)
        {
            Volatile.Write(ref _process, [.. _process, scope]);
        }
    }

    public static void Detach(OutputScope scope)
    {
        lock (Gate)
        {
            Volatile.Write(ref _process, [.. _process.Where(s => s != scope)]);
        }

        scope.Settle();
    }

    /// <summary>
    /// Marks this request as belonging to a lambda until the scope is closed.
    /// </summary>
    public static IDisposable Enter(OutputScope scope)
    {
        var previous = Current.Value;

        Current.Value = scope;

        return new Leave(previous, scope);
    }

    private sealed class Leave(OutputScope? previous, OutputScope scope) : IDisposable
    {
        public void Dispose()
        {
            // whatever was written without a newline to end it is still worth
            // seeing, so it is closed out here rather than waiting for a line
            // that is not coming
            scope.Settle();

            Current.Value = previous;
        }
    }

}
