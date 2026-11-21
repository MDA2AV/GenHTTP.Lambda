using System.Data.Common;
using System.Globalization;

using GenHTTP.Lambda.Integrations.Databases;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// The analytics database: what was counted of every lambda whose owner
/// counts visits, a row per lambda, day and value.
/// </summary>
/// <remarks>
/// A store of its own beside the platform's (<see cref="DatabaseStore.Analytics"/>,
/// <c>analytics.db</c> on SQLite), laid out as a star - see the migration for
/// why. Spoken to with plain commands rather than Entity Framework: a writing
/// is a few hundred upserts in one transaction, and a reading a handful of sums
/// over a range of a key, neither of which a model would make clearer. Written
/// in SQL that every database the platform supports accepts, so the commands
/// are the same whatever keeps them; only the schema is written per dialect.
///
/// Awaited, as everything sent over a connection the database integration
/// opened: written by the background job, and read for an owner a lambda's
/// year at most, by its key. Migrated with the platform's records as the
/// platform starts.
/// </remarks>
public sealed class AnalyticsStore : IAnalyticsStore
{
    /// <summary>
    /// How many days are kept, today included: a year, whatever a tier shows of it.
    /// </summary>
    public const int Kept = 366;

    private readonly IDatabaseProvider _database;

    #region Initialization

    public AnalyticsStore(IDatabaseProvider database)
    {
        _database = database;
    }

    #endregion

    #region Writing

    /// <summary>
    /// Adds what a writing counted to the rows of its days, in one transaction.
    /// </summary>
    /// <remarks>
    /// A dimension with a most per day (<see cref="AnalyticsDimensions.MostOf"/>)
    /// takes a new value only while the day has room for one; past that, it
    /// is added to <see cref="AnalyticsDimensions.Other"/>. Values the day
    /// already has keep being counted under their own.
    /// </remarks>
    public async ValueTask WriteAsync(VisitTally tally, DateTime now, CancellationToken cancellation = default)
    {
        if (tally.IsEmpty)
        {
            return;
        }

        await using var connection = await OpenAsync(cancellation);

        await using var transaction = await connection.BeginTransactionAsync(cancellation);

        await using (var lambdas = Command(connection, "INSERT INTO lambdas (id, created) VALUES (@lambda, @created) ON CONFLICT (id) DO NOTHING", "@lambda", "@created"))
        {
            foreach (var lambda in tally.Lambdas)
            {
                lambdas.Parameters["@lambda"].Value = lambda;
                lambdas.Parameters["@created"].Value = now.ToString("O", CultureInfo.InvariantCulture);
                await lambdas.ExecuteNonQueryAsync(cancellation);
            }
        }

        await using (var days = Command(connection, """
                   INSERT INTO days (lambda, year, month, day, hits, views, bot_views, visitors, failed, rejected, bytes, millis)
                   VALUES (@lambda, @year, @month, @day, @hits, @views, @botViews, @visitors, @failed, @rejected, @bytes, @millis)
                   ON CONFLICT (lambda, year, month, day) DO UPDATE SET
                       hits = days.hits + excluded.hits, views = days.views + excluded.views, bot_views = days.bot_views + excluded.bot_views,
                       visitors = days.visitors + excluded.visitors, failed = days.failed + excluded.failed, rejected = days.rejected + excluded.rejected,
                       bytes = days.bytes + excluded.bytes, millis = days.millis + excluded.millis
                   """, [.. DayKey, "@hits", "@views", "@botViews", "@visitors", "@failed", "@rejected", "@bytes", "@millis"]))
        {
            foreach (var (lambda, day, totals) in tally.Days)
            {
                Day(days, lambda, day);
                days.Parameters["@hits"].Value = totals.Hits;
                days.Parameters["@views"].Value = totals.Views;
                days.Parameters["@botViews"].Value = totals.BotViews;
                days.Parameters["@visitors"].Value = totals.Visitors;
                days.Parameters["@failed"].Value = totals.Failed;
                days.Parameters["@rejected"].Value = totals.Rejected;
                days.Parameters["@bytes"].Value = totals.Bytes;
                days.Parameters["@millis"].Value = totals.Millis;
                await days.ExecuteNonQueryAsync(cancellation);
            }
        }

        foreach (var group in tally.Cells.GroupBy(c => c.Cell.Dimension))
        {
            var table = AnalyticsDimensions.TableOf(group.Key);

            var most = AnalyticsDimensions.MostOf(group.Key);

            var numbered = AnalyticsCodes.IsNumbered(group.Key);

            await using var upsert = Command(connection, $"""
                INSERT INTO {table} (lambda, year, month, day, value, hits, views, bot_views)
                VALUES (@lambda, @year, @month, @day, @value, @hits, @views, @botViews)
                ON CONFLICT (lambda, year, month, day, value) DO UPDATE SET
                    hits = {table}.hits + excluded.hits, views = {table}.views + excluded.views, bot_views = {table}.bot_views + excluded.bot_views
                """, [.. DayKey, "@value", "@hits", "@views", "@botViews"]);

            await using var known = Command(connection, $"SELECT value FROM {table} WHERE lambda = @lambda AND year = @year AND month = @month AND day = @day", DayKey);

            // the busiest first, so where a day runs out of room it is the
            // values that came up least that are gathered under other
            foreach (var day in group.GroupBy(c => (c.Cell.Lambda, c.Cell.Day)))
            {
                HashSet<string>? values = null;

                if (most != null)
                {
                    Day(known, day.Key.Lambda, day.Key.Day);
                    values = await StringsAsync(known, cancellation);
                }

                foreach (var (cell, hits, views, botViews) in day.OrderByDescending(c => c.Views).ThenByDescending(c => c.Hits))
                {
                    var value = cell.Value;

                    if (values != null && !values.Contains(value))
                    {
                        if (values.Count < most)
                        {
                            values.Add(value);
                        }
                        else
                        {
                            value = AnalyticsDimensions.Other;
                        }
                    }

                    Day(upsert, cell.Lambda, cell.Day);
                    upsert.Parameters["@value"].Value = numbered ? AnalyticsCodes.Code(group.Key, value) : value;
                    upsert.Parameters["@hits"].Value = hits;
                    upsert.Parameters["@views"].Value = views;
                    upsert.Parameters["@botViews"].Value = botViews;
                    await upsert.ExecuteNonQueryAsync(cancellation);
                }
            }
        }

        await transaction.CommitAsync(cancellation);
    }

    /// <summary>
    /// Removes everything counted of a lambda.
    /// </summary>
    public async ValueTask ForgetAsync(long lambda, CancellationToken cancellation = default)
    {
        await using var connection = await OpenAsync(cancellation);

        await using var transaction = await connection.BeginTransactionAsync(cancellation);

        foreach (var table in Tables)
        {
            await using var delete = Command(connection, $"DELETE FROM {table} WHERE lambda = @lambda", "@lambda");

            delete.Parameters["@lambda"].Value = lambda;
            await delete.ExecuteNonQueryAsync(cancellation);
        }

        await using (var row = Command(connection, "DELETE FROM lambdas WHERE id = @lambda", "@lambda"))
        {
            row.Parameters["@lambda"].Value = lambda;
            await row.ExecuteNonQueryAsync(cancellation);
        }

        await transaction.CommitAsync(cancellation);
    }

    /// <summary>
    /// Removes every day before the one given.
    /// </summary>
    /// <remarks>
    /// Lambda by lambda, so every delete is a range of the key rather than a
    /// look at every row of a table.
    /// </remarks>
    /// <returns>How many rows were removed</returns>
    public async ValueTask<int> ExpireAsync(DateOnly before, CancellationToken cancellation = default)
    {
        await using var connection = await OpenAsync(cancellation);

        var lambdas = new List<long>();

        await using (var list = Command(connection, "SELECT id FROM lambdas"))
        await using (var reader = await list.ExecuteReaderAsync(cancellation))
        {
            while (await reader.ReadAsync(cancellation))
            {
                lambdas.Add(reader.GetInt64(0));
            }
        }

        var removed = 0;

        await using var transaction = await connection.BeginTransactionAsync(cancellation);

        foreach (var table in Tables)
        {
            await using var delete = Command(connection, $"DELETE FROM {table} WHERE lambda = @lambda AND (year, month, day) < (@year, @month, @day)", DayKey);

            foreach (var lambda in lambdas)
            {
                Day(delete, lambda, before);
                removed += await delete.ExecuteNonQueryAsync(cancellation);
            }
        }

        await transaction.CommitAsync(cancellation);

        return removed;
    }

    #endregion

    #region Reading

    /// <summary>
    /// What was counted of a lambda from one day to another, both included.
    /// </summary>
    /// <param name="most">How many values of a dimension are listed at most, the rest left out</param>
    public async ValueTask<AnalyticsReport> ReadAsync(long lambda, DateOnly from, DateOnly to, int most, CancellationToken cancellation = default)
    {
        await using var connection = await OpenAsync(cancellation);

        var counted = new Dictionary<DateOnly, AnalyticsTotals>();

        await using (var days = Ranged(connection, lambda, from, to, """
                   SELECT year, month, day, hits, views, bot_views, visitors, failed, rejected, bytes, millis FROM days
                   WHERE lambda = @lambda AND (year, month, day) >= (@fromYear, @fromMonth, @fromDay) AND (year, month, day) <= (@toYear, @toMonth, @toDay)
                   """))
        await using (var reader = await days.ExecuteReaderAsync(cancellation))
        {
            while (await reader.ReadAsync(cancellation))
            {
                counted[new DateOnly(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2))] = Totals(reader, 3);
            }
        }

        var series = new List<AnalyticsDay>();

        var total = AnalyticsTotals.None;

        for (var day = from; day <= to; day = day.AddDays(1))
        {
            var totals = counted.GetValueOrDefault(day, AnalyticsTotals.None);

            series.Add(new AnalyticsDay(day, totals));

            total = total.Add(totals);
        }

        var breakdowns = new Dictionary<AnalyticsDimension, IReadOnlyList<AnalyticsValue>>();

        foreach (var dimension in AnalyticsDimensions.All)
        {
            await using var values = Ranged(connection, lambda, from, to, $"""
                SELECT value, CAST(SUM(hits) AS BIGINT) AS hits, CAST(SUM(views) AS BIGINT) AS views, CAST(SUM(bot_views) AS BIGINT) AS bot_views
                FROM {AnalyticsDimensions.TableOf(dimension)}
                WHERE lambda = @lambda AND (year, month, day) >= (@fromYear, @fromMonth, @fromDay) AND (year, month, day) <= (@toYear, @toMonth, @toDay)
                GROUP BY value ORDER BY views DESC, hits DESC, value LIMIT @most
                """);

            Add(values, "@most", most);

            await using var reader = await values.ExecuteReaderAsync(cancellation);

            var listed = new List<AnalyticsValue>();

            var numbered = AnalyticsCodes.IsNumbered(dimension);

            while (await reader.ReadAsync(cancellation))
            {
                var value = numbered ? AnalyticsCodes.Name(dimension, Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture)) : reader.GetString(0);

                listed.Add(new AnalyticsValue(value, reader.GetInt64(1), reader.GetInt64(2), reader.GetInt64(3)));
            }

            // a numbered dimension has fewer values than are listed, so all of
            // them came back, ties in the order of their numbers and a number
            // no member has beside other: summed by name, and put back in the
            // order of their names, as they were listed before
            breakdowns[dimension] = numbered
                ? [.. listed.GroupBy(v => v.Value)
                            .Select(g => new AnalyticsValue(g.Key, g.Sum(v => v.Hits), g.Sum(v => v.Views), g.Sum(v => v.BotViews)))
                            .OrderByDescending(v => v.Views).ThenByDescending(v => v.Hits).ThenBy(v => v.Value, StringComparer.Ordinal)]
                : listed;
        }

        return new AnalyticsReport(total, series, breakdowns);
    }

    /// <summary>
    /// What was counted of a lambda from one day to another, summed.
    /// </summary>
    public async ValueTask<AnalyticsTotals> SumAsync(long lambda, DateOnly from, DateOnly to, CancellationToken cancellation = default)
    {
        await using var connection = await OpenAsync(cancellation);

        await using var sum = Ranged(connection, lambda, from, to, """
            SELECT CAST(COALESCE(SUM(hits), 0) AS BIGINT), CAST(COALESCE(SUM(views), 0) AS BIGINT), CAST(COALESCE(SUM(bot_views), 0) AS BIGINT),
                   CAST(COALESCE(SUM(visitors), 0) AS BIGINT), CAST(COALESCE(SUM(failed), 0) AS BIGINT), CAST(COALESCE(SUM(rejected), 0) AS BIGINT),
                   CAST(COALESCE(SUM(bytes), 0) AS BIGINT), CAST(COALESCE(SUM(millis), 0) AS DOUBLE PRECISION) FROM days
            WHERE lambda = @lambda AND (year, month, day) >= (@fromYear, @fromMonth, @fromDay) AND (year, month, day) <= (@toYear, @toMonth, @toDay)
            """);

        await using var reader = await sum.ExecuteReaderAsync(cancellation);

        return await reader.ReadAsync(cancellation) ? Totals(reader, 0) : AnalyticsTotals.None;
    }

    /// <summary>
    /// The first day anything was counted of a lambda on, or nothing.
    /// </summary>
    public async ValueTask<DateOnly?> EarliestAsync(long lambda, CancellationToken cancellation = default)
    {
        await using var connection = await OpenAsync(cancellation);

        await using var first = Command(connection, "SELECT year, month, day FROM days WHERE lambda = @lambda ORDER BY year, month, day LIMIT 1", "@lambda");

        first.Parameters["@lambda"].Value = lambda;

        await using var reader = await first.ExecuteReaderAsync(cancellation);

        return await reader.ReadAsync(cancellation) ? new DateOnly(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)) : null;
    }

    #endregion

    #region Helpers

    private static IEnumerable<string> Tables => AnalyticsDimensions.All.Select(AnalyticsDimensions.TableOf).Prepend("days");

    private ValueTask<DbConnection> OpenAsync(CancellationToken cancellation) => _database.OpenAsync(DatabaseStore.Analytics, cancellation);

    /// <summary>
    /// The parameters that name a day of a lambda.
    /// </summary>
    private static readonly string[] DayKey = ["@lambda", "@year", "@month", "@day"];

    /// <summary>
    /// A command with the parameters given, to be set and executed as often as needed.
    /// </summary>
    private static DbCommand Command(DbConnection connection, string sql, params string[] parameters)
    {
        var command = connection.CreateCommand();

        command.CommandText = sql;

        foreach (var name in parameters)
        {
            Add(command, name, DBNull.Value);
        }

        return command;
    }

    private static DbCommand Ranged(DbConnection connection, long lambda, DateOnly from, DateOnly to, string sql)
    {
        var command = connection.CreateCommand();

        command.CommandText = sql;

        Add(command, "@lambda", lambda);
        Add(command, "@fromYear", from.Year);
        Add(command, "@fromMonth", from.Month);
        Add(command, "@fromDay", from.Day);
        Add(command, "@toYear", to.Year);
        Add(command, "@toMonth", to.Month);
        Add(command, "@toDay", to.Day);

        return command;
    }

    private static void Add(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();

        parameter.ParameterName = name;
        parameter.Value = value;

        command.Parameters.Add(parameter);
    }

    private static void Day(DbCommand command, long lambda, DateOnly day)
    {
        command.Parameters["@lambda"].Value = lambda;
        command.Parameters["@year"].Value = day.Year;
        command.Parameters["@month"].Value = day.Month;
        command.Parameters["@day"].Value = day.Day;
    }

    private static async ValueTask<HashSet<string>> StringsAsync(DbCommand command, CancellationToken cancellation)
    {
        var values = new HashSet<string>(StringComparer.Ordinal);

        await using var reader = await command.ExecuteReaderAsync(cancellation);

        while (await reader.ReadAsync(cancellation))
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }

    private static AnalyticsTotals Totals(DbDataReader reader, int at)
        => new(reader.GetInt64(at), reader.GetInt64(at + 1), reader.GetInt64(at + 2), reader.GetInt64(at + 3), reader.GetInt64(at + 4),
               reader.GetInt64(at + 5), reader.GetInt64(at + 6), reader.GetDouble(at + 7));

    #endregion

}
