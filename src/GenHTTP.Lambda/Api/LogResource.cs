using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// What the server and the lambdas on it have been saying.
/// </summary>
/// <remarks>
/// Behind the token without exception, unlike the figures next door. Those are
/// aggregates and name nobody; these are whatever a stranger's code decided to
/// print, which can be anything it saw - so this answers an operator or it
/// answers nothing.
/// </remarks>
public sealed class LogResource(LogBook book, RunLog runs, LambdaOptions options)
{

    #region Functionality

    /// <summary>
    /// Everything said after the given sequence, oldest first.
    /// </summary>
    /// <param name="since">
    /// The last sequence already seen. Omitted, only the tail comes back, so
    /// arriving does not mean reading the whole ring.
    /// </param>
    /// <param name="lambda">Narrows it to one lambda's public key</param>
    /// <param name="level">The lowest level worth returning</param>
    /// <param name="limit">At most this many lines</param>
    [ResourceMethod]
    public LogResponse Get(long? since, string? lambda, string? level, int? limit, IRequest request)
    {
        AdminGate.Require(request, options);

        var wanted = limit ?? (since.HasValue ? 500 : 200);

        var (lines, cursor, missed) = book.Read(since ?? 0, Blank(lambda), Minimum(level), wanted);

        return new LogResponse(
            lines.Select(l => new LogEntry(l.Seq, l.At, l.Level, l.Source, l.Lambda, l.Text, l.Detail)).ToList(),
            cursor,
            missed,
            book.Capacity,
            book.Written,
            options.CaptureLambdaOutput,
            // a clean stop is not worth saying anything about; one that was not
            // clean is the first thing an operator wants to know
            runs.Previous is { Clean: false } last
                ? new PreviousRun(last.Started, last.LastSeen, last.Minutes, last.Signalled,
                                  last.Fault, last.WorkingSet, last.Requests, last.Sockets)
                : null
        );
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static LogLevel Minimum(string? level) => level?.Trim().ToLowerInvariant() switch
    {
        "trace" => LogLevel.Trace,
        "debug" => LogLevel.Debug,
        "warn" or "warning" => LogLevel.Warning,
        "error" => LogLevel.Error,
        "critical" => LogLevel.Critical,
        _ => LogLevel.Information
    };

    #endregion

}
