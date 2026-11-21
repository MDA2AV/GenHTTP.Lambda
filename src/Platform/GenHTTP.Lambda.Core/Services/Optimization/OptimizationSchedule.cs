using GenHTTP.Lambda.Services.QuietHours;

namespace GenHTTP.Lambda.Services.Optimization;

/// <summary>
/// When a database is due to be optimized, and whether now is the hour for it.
/// </summary>
/// <remarks>
/// <para>
/// A database is optimized at most once a day, in its quiet hours: a lambda's
/// in the hours its own visitors are fewest, where enough was counted to tell
/// them, else in the installation's - which is also when the platform's own
/// are. Where the installation's are not known yet either - its first day -
/// in the three hours from 03:00 of the server's clock, which is when most
/// servers are quiet: neither never nor whenever the server happens to start.
/// </para>
/// <para>
/// Nothing is done in the first minutes after the server starts, which go to
/// serving and to compiling the lambdas that are warmed up - and a restart in
/// the middle of the quiet hours does not optimize again what was optimized
/// before it.
/// </para>
/// </remarks>
internal static class OptimizationSchedule
{

    /// <summary>
    /// How long after one optimization a database is due again: a day, less a
    /// few hours, so a quiet stretch that comes a little earlier than the day
    /// before is not missed.
    /// </summary>
    public static readonly TimeSpan Apart = TimeSpan.FromHours(20);

    /// <summary>
    /// How long after the server started nothing is optimized.
    /// </summary>
    public static readonly TimeSpan Settling = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Where nothing is known: from this hour of the server's clock.
    /// </summary>
    private const int FallbackFrom = 3;

    private const int FallbackLength = 3;

    /// <summary>
    /// Whether a database last optimized then is due again.
    /// </summary>
    public static bool IsDue(DateTime? optimized, DateTime now) => optimized is not { } last || now - last >= Apart;

    /// <summary>
    /// Whether now is in the quiet hours a database is optimized in.
    /// </summary>
    /// <param name="installation">The typical day of the installation</param>
    /// <param name="own">The typical day of the lambda whose database it is, if it is one's and it was ever visited</param>
    public static bool IsQuiet(DateTime now, TypicalDay installation, TypicalDay? own = null)
    {
        if (own is { Known: true })
        {
            return own.IsQuiet(now.Hour);
        }

        if (installation.Known)
        {
            return installation.IsQuiet(now.Hour);
        }

        var local = TimeZoneInfo.ConvertTimeFromUtc(now, TimeZoneInfo.Local).Hour;

        return local >= FallbackFrom && local < FallbackFrom + FallbackLength;
    }

    /// <summary>
    /// The hour of the day in UTC the hours used where nothing is known begin
    /// at, as the server's clock stands at a moment.
    /// </summary>
    public static int FallbackInUtc(DateTime now)
    {
        var offset = TimeZoneInfo.Local.GetUtcOffset(now);

        return (int)(((FallbackFrom - offset.TotalHours) % 24 + 24) % 24);
    }

}
