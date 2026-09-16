using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Files everything the server logs into the book as well as onto the console.
/// </summary>
/// <remarks>
/// A provider beside the console one rather than a reader of it: the record
/// still has its level, its category and its exception here, and reconstructing
/// those from a formatted console line would be guesswork.
/// </remarks>
public sealed class LogBookProvider(LogBook book) : ILoggerProvider
{

    public ILogger CreateLogger(string categoryName) => new BookLogger(book, Shorten(categoryName));

    public void Dispose() { }

    /// <summary>
    /// The last part of a category, which is the part that identifies it.
    /// </summary>
    private static string Shorten(string category)
    {
        var cut = category.LastIndexOf('.');

        return cut >= 0 && cut < category.Length - 1 ? category[(cut + 1)..] : category;
    }

    private sealed class BookLogger(LogBook book, string source) : ILogger
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
            book.Append(LogBook.NameOf(level), source, LambdaOutput.Ambient?.PublicKey, text, error?.ToString(),
                        caller?.Client, caller?.Agent);
        }

    }

}
