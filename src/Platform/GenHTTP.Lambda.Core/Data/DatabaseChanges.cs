using System.Data.Common;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GenHTTP.Lambda.Data;

/// <summary>
/// Counts the writes to the database of the platform, so that what is held in
/// memory about it can tell whether it is still what the database says.
/// </summary>
/// <remarks>
/// Every request to a lambda has to know which lambda it is for, which version
/// that runs and how - which used to be three queries per request. Those
/// answers change only when somebody writes, and writes are rare next to
/// requests, so they are kept in memory and taken as stale once anything was
/// written since (see <see cref="Services.Meta.ResolutionCache{TKey}"/>).
///
/// Counted here, below every write, rather than by the code that writes:
/// whatever changes a lambda - a deployment, a key, a tier, a data switch, a
/// merge, the sweeps - would otherwise each have to remember to say so, and
/// the one that forgot would leave a lambda serving what it no longer is.
/// Counting every write is coarser than it needs to be, and that is the point:
/// a cache emptied once too often costs a few queries, one that was not
/// emptied serves the wrong thing.
///
/// A write is counted when it has run and again when its transaction commits,
/// so whatever was read in between - before the commit made it visible - is
/// already stale by the time it is looked at again.
/// </remarks>
public sealed class DatabaseChanges : IDbCommandInterceptor, IDbTransactionInterceptor
{
    private long _generation;

    #region Get-/Setters

    /// <summary>
    /// Moves on with every write; what was read under another number may be stale.
    /// </summary>
    public long Generation => Interlocked.Read(ref _generation);

    #endregion

    #region Commands

    public DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        Observe(command);
        return result;
    }

    public ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
                                                       CancellationToken cancellationToken = default)
    {
        Observe(command);
        return new(result);
    }

    public int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        Changed();
        return result;
    }

    public ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result,
                                                CancellationToken cancellationToken = default)
    {
        Changed();
        return new(result);
    }

    public object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        Observe(command);
        return result;
    }

    public ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result,
                                                  CancellationToken cancellationToken = default)
    {
        Observe(command);
        return new(result);
    }

    #endregion

    #region Transactions

    public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData) => Changed();

    public Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        Changed();
        return Task.CompletedTask;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Counts a command that returned rows unless it only read them.
    /// </summary>
    /// <remarks>
    /// Entity framework writes to SQLite with <c>RETURNING</c>, which makes an
    /// insert or an update a command that returns rows. Anything that does not
    /// start as a plain select is taken for a write: guessing wrong that way
    /// costs a reload.
    /// </remarks>
    private void Observe(DbCommand command)
    {
        if (!command.CommandText.AsSpan().TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            Changed();
        }
    }

    private void Changed() => Interlocked.Increment(ref _generation);

    #endregion

}
