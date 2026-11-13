using System.Diagnostics;

using Microsoft.Data.Sqlite;

using SQLitePCL;

namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// What a connection to the database of a lambda may do, enforced by SQLite
/// itself on every statement it prepares.
/// </summary>
/// <remarks>
/// The code guard keeps a lambda from making a connection of its own, so the
/// one it is handed is the only one it has - and SQL is a way out of that one:
/// <c>ATTACH '/data/lambda.db'</c> opens any file the server can read, and
/// <c>VACUUM INTO</c> writes one wherever it likes. An authorizer is asked by
/// SQLite about every statement while it is prepared, and refuses those. It
/// also refuses the pragmas that would point SQLite at directories of its
/// choosing, and the one that would lift the limit on the database's size.
///
/// Applied every time the connection opens, not only the first: a connection
/// the lambda closes and opens again takes one from the pool, and one that
/// is new there has nothing applied yet. Nothing here survives a statement
/// the lambda could send, because the authorizer is what decides which
/// statements there are.
///
/// SQLite's defensive mode comes on with it, which keeps SQL from writing the
/// schema or the file behind SQLite's back - the ways a database corrupts
/// itself on purpose. It is not a sandbox either: it is what SQLite offers
/// for running SQL it does not trust, and that is all it is used for here.
/// </remarks>
internal static class ConnectionGuard
{

    /// <summary>
    /// SQLITE_DBCONFIG_DEFENSIVE, which SQLitePCLRaw has no name for.
    /// </summary>
    private const int Defensive = 1010;

    /// <summary>
    /// The pragmas a lambda may not set: where SQLite writes files other than
    /// the database.
    /// </summary>
    private static readonly HashSet<string> Refused = new(StringComparer.OrdinalIgnoreCase)
    {
        "temp_store_directory", "data_store_directory"
    };

    /// <summary>
    /// Pragmas a lambda may read and not set.
    /// </summary>
    private static readonly HashSet<string> ReadOnly = new(StringComparer.OrdinalIgnoreCase)
    {
        "max_page_count"
    };

    /// <summary>
    /// SQL functions a lambda may not call.
    /// </summary>
    /// <remarks>
    /// Loading an extension is off in SQLite unless it is switched on, which a
    /// lambda cannot do - refused here all the same. The tokenizer function of
    /// FTS3 takes a pointer.
    /// </remarks>
    private static bool IsRefusedFunction(string name)
        => name.Equals("load_extension", StringComparison.OrdinalIgnoreCase)
        || name.Equals("fts3_tokenizer", StringComparison.OrdinalIgnoreCase);

    // kept in a field, so the delegate SQLite calls back into is never collected
    private static readonly delegate_authorizer Authorizer = Authorize;

    #region Functionality

    /// <summary>
    /// Keeps the connection to what a lambda may do with it, every time it
    /// opens.
    /// </summary>
    /// <param name="pages">How many pages the database may grow to, which is its quota</param>
    public static void Watch(SqliteConnection connection, long pages)
    {
        connection.StateChange += (_, change) =>
        {
            if (change.CurrentState == System.Data.ConnectionState.Open)
            {
                Apply(connection, pages);
            }
        };
    }

    /// <summary>
    /// Keeps a connection to reading what is there, for as long as the given
    /// time allows: what the platform opens to show a database to its owner.
    /// </summary>
    /// <remarks>
    /// The schema of the database is the lambda's, and a view is SQL the
    /// lambda wrote, run when it is read - so a view that never ends would
    /// hold the request that shows it. SQLite asks the progress handler every
    /// few thousand steps, and the answer stops the statement once its time
    /// is up.
    /// </remarks>
    public static void ApplyReading(SqliteConnection connection, TimeSpan patience)
    {
        Apply(connection, null);

        // functions a view calls run in the reader's name, so only the ones
        // SQLite marks harmless are run at all
        Execute(connection, "PRAGMA trusted_schema = OFF");

        var until = Stopwatch.GetTimestamp() + (long)(patience.TotalSeconds * Stopwatch.Frequency);

        raw.sqlite3_progress_handler(connection.Handle!, 10_000, static state => Stopwatch.GetTimestamp() > (long)state ? 1 : 0, until);
    }

    private static void Apply(SqliteConnection connection, long? pages)
    {
        var handle = connection.Handle!;

        // what follows is the platform's own doing, so the authorizer a pooled
        // connection still has from the last time is taken off while it runs
        raw.sqlite3_set_authorizer(handle, (delegate_authorizer)null!, null);

        if (pages is { } limit)
        {
            Execute(connection, $"PRAGMA max_page_count = {limit}");
        }

        raw.sqlite3_db_config(handle, Defensive, 1, out _);

        raw.sqlite3_set_authorizer(handle, Authorizer, null);
    }

    private static int Authorize(object user, int action, utf8z first, utf8z second, utf8z database, utf8z trigger)
    {
        switch (action)
        {
            case raw.SQLITE_ATTACH:
                // VACUUM attaches a temporary database without a name, which
                // is the one thing attaching is left for
                return string.IsNullOrEmpty(first.utf8_to_string()) ? raw.SQLITE_OK : raw.SQLITE_DENY;

            case raw.SQLITE_PRAGMA:
                {
                    var name = first.utf8_to_string() ?? string.Empty;

                    if (Refused.Contains(name))
                    {
                        return raw.SQLITE_DENY;
                    }

                    // read, the value is null; set, it is what it is set to
                    return ReadOnly.Contains(name) && second.utf8_to_string() != null ? raw.SQLITE_DENY : raw.SQLITE_OK;
                }

            case raw.SQLITE_FUNCTION:
                return IsRefusedFunction(second.utf8_to_string() ?? string.Empty) ? raw.SQLITE_DENY : raw.SQLITE_OK;

            default:
                return raw.SQLITE_OK;
        }
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();

        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    #endregion

}
