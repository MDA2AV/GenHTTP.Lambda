using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Diagnostics;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda;

/// <summary>
/// Where the platform logs to when the host has nothing else in mind: the
/// console, and the book the operator reads in the panel.
/// </summary>
public static class PlatformLogging
{

    /// <summary>
    /// The last few thousand lines of this run, so that an operator with the
    /// token can read them in the panel instead of needing a shell on the host.
    /// </summary>
    public static LogBook CreateBook(LambdaOptions options) => new(options.LogHistory, options.LogMemory, options.RepeatWindow);

    /// <summary>
    /// A factory that writes into the book and to the real console.
    /// </summary>
    /// <remarks>
    /// The console is taken here, before the platform puts its tee in front of
    /// it (see <see cref="ConsoleCapture"/>): records are written to it by the
    /// provider, and everything else that reaches <c>Console.Out</c> once the
    /// tee is there - the engine saying what its reactors are doing, whatever
    /// a lambda prints - goes through the tee and into the book instead.
    /// Keeping the two apart by which writer they hold, rather than by a check
    /// at the point of writing, is what stops a record being kept twice.
    /// </remarks>
    public static ILoggerFactory CreateLoggers(LambdaOptions options, LogBook book)
    {
        var console = Console.Out;

        var floor = options.Development ? LogLevel.Debug : LogLevel.Information;

        return LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new LogBookProvider(book, console, floor));

            builder.SetMinimumLevel(floor);

            // the query log of entity framework would drown out everything else
            builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

            // and so would ASP.NET Core under the Kestrel engine, with two lines per
            // request - written to the console while the request waits, and into the
            // book where, each with its own duration, nothing ever folded them
            builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

            // and the MCP module writes a line of its own for every tool call, beside
            // the one the tool writes about what it did - and one for every refusal,
            // which is an answer rather than a problem. A tool that fails is an error,
            // and still written down
            builder.AddFilter("GenHTTP.Modules.Mcp", LogLevel.Warning);
        });
    }

}
