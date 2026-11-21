using System.Collections.Concurrent;
using System.Data.Common;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Data;

/// <summary>
/// Says so when the platform's records are asked for without being awaited.
/// </summary>
/// <remarks>
/// The platform awaits its records, one path rather than two: over SQLite the
/// awaited forms complete on the thread that asks, and over PostgreSQL they
/// wait for the server without holding a thread. The compiler refuses Entity
/// Framework's synchronous methods (see BannedSymbols.txt), but not a query
/// enumerated as a sequence - <c>ToList()</c>, <c>foreach</c>, a spread over
/// it - which binds to LINQ's own methods and runs synchronously all the
/// same, unnoticed over SQLite. This is where one shows up: a warning in the
/// log, once per statement, and a count the tests fail on.
///
/// It refuses nothing: such a query still answers, as it did before.
/// </remarks>
public sealed class SynchronousRecords(ILogger<SynchronousRecords> logger) : DbCommandInterceptor
{
    /// <summary>
    /// How many statements are warned about, so a query in a loop does not
    /// fill the log.
    /// </summary>
    private const int Remembered = 100;

    private readonly ConcurrentDictionary<string, byte> _warned = new(StringComparer.Ordinal);

    private long _count;

    private string? _first;

    #region Get-/Setters

    /// <summary>
    /// How often the records were asked for synchronously since the platform started.
    /// </summary>
    public long Count => Interlocked.Read(ref _count);

    /// <summary>
    /// The first statement that was, if any.
    /// </summary>
    public string? First => Volatile.Read(ref _first);

    #endregion

    #region Commands

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Notice(command);
        return result;
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Notice(command);
        return result;
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        Notice(command);
        return result;
    }

    #endregion

    #region Helpers

    /// <remarks>
    /// The statement as Entity Framework writes it, with its values as
    /// parameters: what is logged is the query, never what it was asked with.
    /// </remarks>
    private void Notice(DbCommand command)
    {
        Interlocked.Increment(ref _count);

        var statement = command.CommandText;

        Interlocked.CompareExchange(ref _first, statement, null);

        if (_warned.Count < Remembered && _warned.TryAdd(statement, 0))
        {
            logger.LogWarning("Queried the platform's records synchronously statement {Statement}", statement);
        }
    }

    #endregion

}
