namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// The database of a lambda, as its owner and an agent read it: its tables
/// and what they hold.
/// </summary>
/// <remarks>
/// Reading only. What a database holds is written by the lambda, through the
/// connection it is handed; somebody who wants a record changed asks for a
/// version of the app that changes it, or an agent to make one.
///
/// Every call can name a feature instead, and then reads the copy that
/// feature's preview works on.
/// </remarks>
public interface IDatabaseService
{

    /// <summary>
    /// Whether the lambda has a database, how full it is, and its tables with
    /// their columns and how many rows each holds.
    /// </summary>
    ValueTask<DatabaseOverview> GetAsync(string privateKey, string? feature = null, CancellationToken cancellation = default);

    /// <summary>
    /// A page of the rows of a table or view.
    /// </summary>
    /// <param name="order">A column to sort by; left out, the order the rows were written in</param>
    /// <param name="descending">Whether to reverse that order - newest first, where no column is named</param>
    ValueTask<DatabaseRows> ReadAsync(string privateKey, string table, int offset = 0, int limit = 50, string? order = null, bool descending = true,
                                      string? feature = null, CancellationToken cancellation = default);

}
