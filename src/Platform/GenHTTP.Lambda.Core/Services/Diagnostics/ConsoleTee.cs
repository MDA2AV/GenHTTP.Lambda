using System.Text;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Passes the console through untouched and keeps a copy of what a lambda
/// printed.
/// </summary>
/// <remarks>
/// A write while a lambda is being served is filed under that lambda;
/// everything else is filed under the process. That second half matters more
/// than it looks: the engine prints what its reactors are doing straight to
/// the console rather than through a logger, and while the rule here was "no
/// lambda, no copy" those lines existed only on stdout.
///
/// What it must not do is take the console logger's own output, or every
/// record would be in the book twice - once as the record and once as the
/// console writing it out. That is handled by when this is installed rather
/// than by a check: the logger holds the writer it was given when it was
/// built, so installing afterwards leaves it writing to the real console
/// while everything that reaches Console.Out later comes through here.
///
/// Passed through where a platform has the process for itself, that is. Where
/// several share it the console is their host's, whose log is read by others
/// than a lambda's: what one prints is in the log of its installation, and
/// there only.
///
/// Only the four primitive writes are overridden. Every other overload of
/// <see cref="TextWriter"/> is defined in terms of them, so this sees each
/// character exactly once however it was written - and because the console
/// wraps whatever it is given in a synchronizing writer, a whole
/// <c>WriteLine</c> lands here under one lock rather than interleaved with
/// another thread's.
///
/// That lock is the one of <c>Console.Out</c>, and the console takes it again
/// underneath every write to its stream - from this tee's writer and from the
/// logger's alike. So a print here holds it and then wants the real console's
/// writer, and the logger must want them in the same order: it takes
/// <c>Console.Out</c> before writing (see <see cref="LogBookProvider"/>).
/// Holding the real console's writer first and getting to the stream's lock
/// second, it waited for a print that was waiting for it, and the process
/// stopped.
/// </remarks>
public sealed class ConsoleTee(TextWriter inner, bool error) : TextWriter
{

    /// <summary>
    /// Puts a tee over the console, once.
    /// </summary>
    /// <remarks>
    /// Process wide, which is why it is guarded: there is one console, and
    /// wrapping it twice would file every lambda line twice with it. Nothing
    /// about it is specific to an installation - it reads whose request is
    /// running and writes into the book that scope was given - so a process
    /// hosting more than one application still only needs the one.
    /// </remarks>
    public static void Install()
    {
        lock (Guard)
        {
            if (_installed)
            {
                return;
            }

            _installed = true;

            _console = Console.Out;

            Console.SetOut(new ConsoleTee(_console, false));
            Console.SetError(new ConsoleTee(Console.Error, true));
        }
    }

    private static readonly Lock Guard = new();

    private static bool _installed;

    private static TextWriter? _console;

    /// <summary>
    /// The console itself, whether or not there is a tee over it: what a
    /// logger writes to, whose records are in the book already.
    /// </summary>
    public static TextWriter Underlying
    {
        get
        {
            lock (Guard)
            {
                return _console ?? Console.Out;
            }
        }
    }

    public override Encoding Encoding => inner.Encoding;

    public override IFormatProvider FormatProvider => inner.FormatProvider;

    public override void Write(char value)
    {
        if (Passed)
        {
            inner.Write(value);
        }

        Span<char> one = [value];

        Feed(one);
    }

    public override void Write(string? value)
    {
        if (Passed)
        {
            inner.Write(value);
        }

        if (value != null)
        {
            Feed(value);
        }
    }

    public override void Write(char[] buffer, int index, int count)
    {
        if (Passed)
        {
            inner.Write(buffer, index, count);
        }

        Feed(buffer.AsSpan(index, count));
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        if (Passed)
        {
            inner.Write(buffer);
        }

        Feed(buffer);
    }

    /// <summary>
    /// Whether what is written goes on to the console: what nobody's lambda
    /// prints does, and what one prints where a platform has the process for
    /// itself.
    /// </summary>
    private static bool Passed => LambdaOutput.Ambient == null || !LambdaOutput.Process.IsEmpty;

    /// <summary>
    /// Files what was written under the lambda being served, or else under
    /// every application that keeps the process's output.
    /// </summary>
    private void Feed(ReadOnlySpan<char> text)
    {
        if (LambdaOutput.Ambient is { } lambda)
        {
            lambda.Feed(text, error);
            return;
        }

        foreach (var scope in LambdaOutput.Process)
        {
            scope.Feed(text, error);
        }
    }

    public override void Flush() => inner.Flush();

    public override Task FlushAsync() => inner.FlushAsync();

    // the console owns the streams; this only watches them
    protected override void Dispose(bool disposing) { }

}
