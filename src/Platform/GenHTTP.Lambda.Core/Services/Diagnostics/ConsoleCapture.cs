namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Keeps what is printed to the console in the book of an application, for
/// as long as the application runs.
/// </summary>
/// <remarks>
/// Lambdas and the engine both print to this console, so the console is where
/// they are told apart: a write while a lambda is being served is filed under
/// that lambda, and everything else under the process (see
/// <see cref="ConsoleTee"/>).
///
/// The console is one per process and so is the tee over it, installed once
/// whoever asks first. What belongs to the application is the book the
/// process's own lines go to, attached here and detached when the application
/// is disposed of - so tests, which run many applications side by side, each
/// keep theirs, rather than the last one started taking all of them.
///
/// Not conditional. LAMBDA_LOG_LAMBDA_OUTPUT decides whether a stranger's
/// print is gathered, which is a question about holding somebody else's output
/// in memory; what the engine says about its own reactors is the server
/// talking about itself and is the first thing wanted when connections start
/// misbehaving. The switch is applied where a lambda is marked, not here.
///
/// Built after the logger factory, deliberately: the provider writing to the
/// console holds the writer it was given, and the tee replaces the one
/// everything else will find.
/// </remarks>
public sealed class ConsoleCapture : IDisposable
{

    #region Get-/Setters

    private OutputScope Scope { get; }

    #endregion

    #region Initialization

    public ConsoleCapture(LogBook book)
    {
        // uncapped, unlike a request's: this is the server talking about
        // itself, and the ring and the folding are what bound it
        Scope = new OutputScope(null, book, int.MaxValue);

        ConsoleTee.Install();

        LambdaOutput.Attach(Scope);
    }

    #endregion

    #region Functionality

    public void Dispose() => LambdaOutput.Detach(Scope);

    #endregion

}
