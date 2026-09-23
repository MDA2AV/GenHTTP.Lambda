using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Files everything the server logs into the book as well as onto the console.
/// </summary>
/// <remarks>
/// It writes the console line as well, rather than sitting beside a provider
/// that does.
///
/// That is not tidiness. The tee over the console has to copy what the engine
/// prints - which is how anyone ever sees a reactor faulting - without copying
/// the console logger's own output, or every record would be in the book
/// twice. Which of those happens depends on whether that logger captured
/// Console.Out before or after the tee replaced it, and arranging to be on the
/// right side of that turned out to deadlock. So the console logger is gone
/// and this writes the line itself, to a writer taken before the tee existed:
/// a record goes to the book as a record and to the console as a line, and
/// neither can become the other.
/// </remarks>
public sealed class LogBookProvider(LogBook book, TextWriter? console = null, LogLevel minimum = LogLevel.Information) : ILoggerProvider
{

    public ILogger CreateLogger(string categoryName) => new BookLogger(book, Shorten(categoryName), console, minimum);

    public void Dispose() { }

    /// <summary>
    /// The last part of a category, which is the part that identifies it.
    /// </summary>
    private static string Shorten(string category)
    {
        var cut = category.LastIndexOf('.');

        return cut >= 0 && cut < category.Length - 1 ? category[(cut + 1)..] : category;
    }

    private sealed class BookLogger(LogBook book, string source, TextWriter? console, LogLevel minimum) : ILogger
    {

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        /// <summary>
        /// Whether the book is already going to have this line, better.
        /// </summary>
        /// <remarks>
        /// The engine writes one per request with the method, the path and the
        /// status. CallerConcern writes its own with the address it came from
        /// as well, which is the part anybody actually needs, so the engine's
        /// would only be the same line without the useful field on it. Both
        /// still reach stdout - this only decides what the ring keeps.
        /// </remarks>
        private bool Duplicated() => source == "Requests";

        // what is worth keeping is decided by the filters on the factory, the
        // same ones that decide what reaches the console
        public bool IsEnabled(LogLevel level) => level != LogLevel.None;

        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> format)
        {
            if (!IsEnabled(level))
            {
                return;
            }

            if (Duplicated())
            {
                return;
            }

            var text = format(state, error);

            if (error != null)
            {
                text = text.Length > 0 ? $"{text} — {error.GetType().Name}: {error.Message}" : $"{error.GetType().Name}: {error.Message}";
            }

            var caller = Caller.Ambient;

            // a warning about a lambda, logged while that lambda was being
            // served, belongs under it as well as in the general run - and
            // carries whoever was being answered at the time
            var scope = LambdaOutput.Ambient;

            book.Append(LogBook.NameOf(level), source, scope?.PublicKey, text, error?.ToString(),
                        caller?.Client, caller?.Agent, caller?.Country, caller?.Place, text, scope?.LambdaId);

            if (console == null || level < minimum)
            {
                return;
            }

            try
            {
                // the same shape the console logger wrote, so whatever reads
                // these logs does not have to learn a new one
                console.WriteLine($"{DateTime.Now:HH:mm:ss} {LogBook.NameOf(level)}: {source}[{id.Id}] {text}");

                if (error != null)
                {
                    console.WriteLine(error.ToString());
                }
            }
            catch (Exception)
            {
                // a console that has gone away is not a reason to stop serving
            }
        }

    }

}
