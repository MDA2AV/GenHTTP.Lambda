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
/// Only the four primitive writes are overridden. Every other overload of
/// <see cref="TextWriter"/> is defined in terms of them, so this sees each
/// character exactly once however it was written - and because the console
/// wraps whatever it is given in a synchronizing writer, a whole
/// <c>WriteLine</c> lands here under one lock rather than interleaved with
/// another thread's.
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
            if (Installed)
            {
                return;
            }

            Installed = true;

            Console.SetOut(new ConsoleTee(Console.Out, false));
            Console.SetError(new ConsoleTee(Console.Error, true));
        }
    }

    private static readonly Lock Guard = new();

    private static bool Installed;

    public override Encoding Encoding => inner.Encoding;

    public override IFormatProvider FormatProvider => inner.FormatProvider;

    public override void Write(char value)
    {
        inner.Write(value);

        var scope = LambdaOutput.Ambient ?? LambdaOutput.Process;

        if (scope != null)
        {
            Span<char> one = [value];

            scope.Feed(one, error);
        }
    }

    public override void Write(string? value)
    {
        inner.Write(value);

        if (value != null)
        {
            (LambdaOutput.Ambient ?? LambdaOutput.Process)?.Feed(value, error);
        }
    }

    public override void Write(char[] buffer, int index, int count)
    {
        inner.Write(buffer, index, count);

        (LambdaOutput.Ambient ?? LambdaOutput.Process)?.Feed(buffer.AsSpan(index, count), error);
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        inner.Write(buffer);

        (LambdaOutput.Ambient ?? LambdaOutput.Process)?.Feed(buffer, error);
    }

    public override void Flush() => inner.Flush();

    public override Task FlushAsync() => inner.FlushAsync();

    // the console owns the streams; this only watches them
    protected override void Dispose(bool disposing) { }

}
