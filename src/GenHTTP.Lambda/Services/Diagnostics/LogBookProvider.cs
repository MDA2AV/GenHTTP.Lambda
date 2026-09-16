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
        /// Whether this line is the log being read.
        /// </summary>
        /// <remarks>
        /// The panel asks for the tail every second and a half, and every one
        /// of those is a request, and every request is logged. Left in, most
        /// of what there is to read is the reading of it, and the ring fills
        /// with its own echo while what somebody opened the page for falls off
        /// the end. Matched on the path because that is what the record has:
        /// by the time a request is logged the work that served it is over and
        /// anything it might have marked has been unwound.
        /// </remarks>
        private bool Polling(string text)
            => source == "Requests" && text.Contains("/api/v1/logs", StringComparison.Ordinal);

        // what is worth keeping is decided by the filters on the factory, the
        // same ones that decide what reaches the console
        public bool IsEnabled(LogLevel level) => level != LogLevel.None;

        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> format)
        {
            if (!IsEnabled(level))
            {
                return;
            }

            var text = format(state, error);

            if (Polling(text))
            {
                return;
            }

            if (error != null)
            {
                text = text.Length > 0 ? $"{text} — {error.GetType().Name}: {error.Message}" : $"{error.GetType().Name}: {error.Message}";
            }

            // a warning about a lambda, logged while that lambda was being
            // served, belongs under it as well as in the general run
            book.Append(LogBook.NameOf(level), source, LambdaOutput.Ambient?.PublicKey, text, error?.ToString());
        }

    }

}
