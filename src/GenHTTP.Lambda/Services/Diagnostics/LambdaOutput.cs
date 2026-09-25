using System.Text;

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

    /// <summary>
    /// The lambda being served here, or nothing.
    /// </summary>
    public static OutputScope? Ambient => Current.Value;

    /// <summary>
    /// Where console output belongs when no lambda is being served.
    /// </summary>
    /// <remarks>
    /// The engine prints its own lines - which reactor is listening on what,
    /// which connection handler faulted and why - straight to the console
    /// rather than through a logger, and they were being dropped: the tee only
    /// copied a write while a lambda was the one making it. Those lines are
    /// exactly what an operator opens a log to read, so they land here
    /// instead, under no lambda at all.
    /// </remarks>
    public static OutputScope? Process { get; private set; }

    /// <summary>
    /// Says where the process's own console output should go.
    /// </summary>
    public static void Adopt(LogBook book)
    {
        // uncapped, unlike a request's: this is the server talking about
        // itself, and the ring and the folding are what bound it
        Process = new OutputScope(null, book, int.MaxValue);
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

/// <summary>
/// One request's worth of console output, gathered into lines.
/// </summary>
/// <remarks>
/// Console writes arrive in whatever pieces the caller chose - a character, a
/// word, a whole paragraph - so they are collected here until a newline ends
/// one. The collecting is per request rather than per process because two
/// lambdas printing at once would otherwise splice into each other.
/// </remarks>
public sealed class OutputScope(string? publicKey, LogBook book, int most, long? lambdaId = null)
{

    #region Get-/Setters

    /// <summary>
    /// The lambda every line gathered here is filed under, or nothing for the
    /// process's own output.
    /// </summary>
    public string? PublicKey { get; } = publicKey;

    /// <summary>
    /// The identity of that lambda, which is what its owner's view reads by.
    /// </summary>
    public long? LambdaId { get; } = lambdaId;

    /// <summary>
    /// How many lines one request may contribute before the rest is counted
    /// and dropped.
    /// </summary>
    /// <remarks>
    /// Without this a single lambda in a print loop owns the whole ring and
    /// nothing else on the installation can be read while it runs.
    /// </remarks>
    private int Most { get; } = most;

    private StringBuilder Pending { get; } = new();

    private Lock Gate { get; } = new();

    private int Said { get; set; }

    private bool Stream { get; set; }

    #endregion

    #region Functionality

    /// <summary>
    /// Takes a piece of whatever was written.
    /// </summary>
    public void Feed(ReadOnlySpan<char> text, bool error)
    {
        lock (Gate)
        {
            // stdout and stderr share the buffer, so a switch between them ends
            // the line rather than letting the two run together
            if (Stream != error)
            {
                Settle(locked: true);
                Stream = error;
            }

            foreach (var c in text)
            {
                if (c == '\n')
                {
                    Emit();
                }
                else if (c != '\r')
                {
                    // past the cap the rest of the line is dropped, but the
                    // line is still closed by its newline
                    if (Pending.Length < LogBook.MaxText)
                    {
                        Pending.Append(c);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Writes out anything left over without a newline to end it.
    /// </summary>
    public void Settle() => Settle(locked: false);

    private void Settle(bool locked)
    {
        if (locked)
        {
            if (Pending.Length > 0)
            {
                Emit();
            }

            return;
        }

        lock (Gate)
        {
            if (Pending.Length > 0)
            {
                Emit();
            }
        }
    }

    private void Emit()
    {
        var text = Pending.ToString();

        Pending.Clear();

        if (Said >= Most)
        {
            return;
        }

        Said++;

        var caller = Caller.Ambient;

        if (Said == Most)
        {
            book.Append("warn", "stdout", PublicKey,
                        $"… this request printed more than {Most} lines; the rest was dropped.",
                        null, caller?.Client, caller?.Agent, caller?.Country, caller?.Place, lambdaId: LambdaId, domain: caller?.Domain);

            return;
        }

        book.Append(Stream ? "error" : "info", Stream ? "stderr" : "stdout", PublicKey, text,
                    null, caller?.Client, caller?.Agent, caller?.Country, caller?.Place,
                    // a print is what it says, so the same print twice is the
                    // same line - which is what folds a reactor faulting over
                    // and over into one line and a count
                    text, LambdaId, caller?.Domain);
    }

    #endregion

}
