namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// How busy a lambda, or the installation as a whole, is on a typical day,
/// by the hour.
/// </summary>
/// <remarks>
/// Written hourly from counters kept in memory (see <c>QuietHoursService</c>),
/// and only for what was visited in that hour; what is read is decayed to
/// the moment it is read. A row of a lambda goes with the lambda.
/// </remarks>
public sealed class QuietHoursEntity
{

    /// <summary>
    /// The lambda, or <see cref="Installation"/> for every request the server
    /// answered.
    /// </summary>
    public long LambdaId { get; set; }

    /// <summary>
    /// The requests of each hour of the day in UTC, from midnight on: 24
    /// numbers, as their weighted average was when the row was last written.
    /// </summary>
    public required string Hours { get; set; }

    /// <summary>
    /// Which hours of the day were counted at all, a bit each from midnight
    /// on - kept on the installation's row, the lambdas' being counted
    /// whenever the server is.
    /// </summary>
    public int Covered { get; set; }

    /// <summary>
    /// When counting began.
    /// </summary>
    public DateTime Since { get; set; }

    /// <summary>
    /// When the row was last written, which is what <see cref="Hours"/> was
    /// decayed to.
    /// </summary>
    public DateTime Updated { get; set; }

    /// <summary>
    /// The row that is not a lambda's but the whole installation's.
    /// </summary>
    public const long Installation = 0;

}
