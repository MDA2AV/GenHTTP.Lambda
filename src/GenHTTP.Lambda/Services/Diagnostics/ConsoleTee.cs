using System.Text;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Passes the console through untouched and keeps a copy of what a lambda
/// printed.
/// </summary>
/// <remarks>
/// It only takes a copy while a lambda is being served. The server's own
/// logging reaches the book through its logger provider, and if this took
/// everything that crossed stdout then every one of those lines would be in
/// there twice - once as the record and once as the console writing it out.
/// So the rule is: no lambda, no copy. Anything else printed to stdout goes
/// where it always went and no further.
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

        var scope = LambdaOutput.Ambient;

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
            LambdaOutput.Ambient?.Feed(value, error);
        }
    }

    public override void Write(char[] buffer, int index, int count)
    {
        inner.Write(buffer, index, count);

        LambdaOutput.Ambient?.Feed(buffer.AsSpan(index, count), error);
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        inner.Write(buffer);

        LambdaOutput.Ambient?.Feed(buffer, error);
    }

    public override void Flush() => inner.Flush();

    public override Task FlushAsync() => inner.FlushAsync();

    // the console owns the streams; this only watches them
    protected override void Dispose(bool disposing) { }

}
