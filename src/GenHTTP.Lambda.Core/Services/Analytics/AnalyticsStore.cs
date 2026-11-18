using System.Data.Common;
using System.Globalization;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Integrations.Databases;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// The analytics database: what was counted of every lambda whose owner
/// counts visits, a row per lambda, day and value.
/// </summary>
/// <remarks>
/// A store of its own beside the platform's (<see cref="DatabaseStores.Analytics"/>,
/// <c>analytics.db</c> on SQLite), laid out as a star - see the migration for
/// why. Spoken to with plain commands rather than Entity Framework: a writing
/// is a few hundred upserts in one transaction, and a reading a handful of sums
/// over a range of a key, neither of which a model would make clearer. Written
/// in SQL that every database the platform supports accepts, so the commands
/// are the same whatever keeps them; only the schema is written per dialect.
///
/// Synchronous, as the platform's services are: written by the background
/// job, and read for an owner a lambda's year at most, by its key.
/// </remarks>
public sealed class AnalyticsStore : IAnalyticsStore
{
    /// <summary>
    /// How many days are kept, today included: a year, whatever a tier shows of it.
    /// </summary>
    public const int Kept = 366;

    private readonly IDatabaseProvider _database;

    #region Initialization

    public AnalyticsStore(IDatabaseProvider database, ILogger<AnalyticsStore> logger)
    {
        _database = database;

        Migrator.Migrate(database, DatabaseStores.Analytics, logger);
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
    public void Write(VisitTally tally, DateTime now)
    {
        if (tally.IsEmpty)
        {
            return;
        }

        using var connection = Open();

        using var transaction = connection.BeginTransaction();

        using (var lambdas = Command(connection, "INSERT INTO lambdas (id, created) VALUES (@lambda, @created) ON CONFLICT (id) DO NOTHING", "@lambda", "@created"))
        {
            foreach (var lambda in tally.Lambdas)
            {
                lambdas.Parameters["@lambda"].Value = lambda;
                lambdas.Parameters["@created"].Value = now.ToString("O", CultureInfo.InvariantCulture);
                lambdas.ExecuteNonQuery();
            }
        }

        using (var days = Command(connection, """
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
                days.ExecuteNonQuery();
            }
        }

        foreach (var group in tally.Cells.GroupBy(c => c.Cell.Dimension))
        {
            var table = AnalyticsDimensions.TableOf(group.Key);

            var most = AnalyticsDimensions.MostOf(group.Key);

            using var upsert = Command(connection, $"""
                INSERT INTO {table} (lambda, year, month, day, value, hits, views, bot_views)
                VALUES (@lambda, @year, @month, @day, @value, @hits, @views, @botViews)
                ON CONFLICT (lambda, year, month, day, value) DO UPDATE SET
                    hits = {table}.hits + excluded.hits, views = {table}.views + excluded.views, bot_views = {table}.bot_views + excluded.bot_views
                """, [.. DayKey, "@value", "@hits", "@views", "@botViews"]);

            using var known = Command(connection, $"SELECT value FROM {table} WHERE lambda = @lambda AND year = @year AND month = @month AND day = @day", DayKey);

            // the busiest first, so where a day runs out of room it is the
            // values that came up least that are gathered under other
            foreach (var day in group.GroupBy(c => (c.Cell.Lambda, c.Cell.Day)))
            {
                HashSet<string>? values = null;

                if (most != null)
                {
                    Day(known, day.Key.Lambda, day.Key.Day);
                    values = Strings(known);
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
                    upsert.Parameters["@value"].Value = value;
                    upsert.Parameters["@hits"].Value = hits;
                    upsert.Parameters["@views"].Value = views;
                    upsert.Parameters["@botViews"].Value = botViews;
                    upsert.ExecuteNonQuery();
                }
            }
        }

        transaction.Commit();
    }

    /// <summary>
    /// Removes everything counted of a lambda.
    /// </summary>
    public void Forget(long lambda)
    {
        using var connection = Open();

        using var transaction = connection.BeginTransaction();

        foreach (var table in Tables)
        {
            using var delete = Command(connection, $"DELETE FROM {table} WHERE lambda = @lambda", "@lambda");

            delete.Parameters["@lambda"].Value = lambda;
            delete.ExecuteNonQuery();
        }

        using (var row = Command(connection, "DELETE FROM lambdas WHERE id = @lambda", "@lambda"))
        {
            row.Parameters["@lambda"].Value = lambda;
            row.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    /// <summary>
    /// Removes every day before the one given.
    /// </summary>
    /// <remarks>
    /// Lambda by lambda, so every delete is a range of the key rather than a
    /// look at every row of a table.
    /// </remarks>
    /// <returns>How many rows were removed</returns>
    public int Expire(DateOnly before)
    {
        using var connection = Open();

        var lambdas = new List<long>();

        using (var list = Command(connection, "SELECT id FROM lambdas"))
        using (var reader = list.ExecuteReader())
        {
            while (reader.Read())
            {
                lambdas.Add(reader.GetInt64(0));
            }
        }

        var removed = 0;

        using var transaction = connection.BeginTransaction();

        foreach (var table in Tables)
        {
            using var delete = Command(connection, $"DELETE FROM {table} WHERE lambda = @lambda AND (year, month, day) < (@year, @month, @day)", DayKey);

            foreach (var lambda in lambdas)
            {
                Day(delete, lambda, before);
                removed += delete.ExecuteNonQuery();
            }
        }

        transaction.Commit();

        return removed;
    }

    #endregion

    #region Reading

    /// <summary>
    /// What was counted of a lambda from one day to another, both included.
    /// </summary>
    /// <param name="most">How many values of a dimension are listed at most, the rest left out</param>
    public AnalyticsReport Read(long lambda, DateOnly from, DateOnly to, int most)
    {
        using var connection = Open();

        var counted = new Dictionary<DateOnly, AnalyticsTotals>();

        using (var days = Ranged(connection, lambda, from, to, """
                   SELECT year, month, day, hits, views, bot_views, visitors, failed, rejected, bytes, millis FROM days
                   WHERE lambda = @lambda AND (year, month, day) >= (@fromYear, @fromMonth, @fromDay) AND (year, month, day) <= (@toYear, @toMonth, @toDay)
                   """))
        using (var reader = days.ExecuteReader())
        {
            while (reader.Read())
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
            using var values = Ranged(connection, lambda, from, to, $"""
                SELECT value, CAST(SUM(hits) AS BIGINT) AS hits, CAST(SUM(views) AS BIGINT) AS views, CAST(SUM(bot_views) AS BIGINT) AS bot_views
                FROM {AnalyticsDimensions.TableOf(dimension)}
                WHERE lambda = @lambda AND (year, month, day) >= (@fromYear, @fromMonth, @fromDay) AND (year, month, day) <= (@toYear, @toMonth, @toDay)
                GROUP BY value ORDER BY views DESC, hits DESC, value LIMIT @most
                """);

            Add(values, "@most", most);

            using var reader = values.ExecuteReader();

            var listed = new List<AnalyticsValue>();

            while (reader.Read())
            {
                listed.Add(new AnalyticsValue(reader.GetString(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt64(3)));
            }

            breakdowns[dimension] = listed;
        }

        return new AnalyticsReport(total, series, breakdowns);
    }

    /// <summary>
    /// What was counted of a lambda from one day to another, summed.
    /// </summary>
    public AnalyticsTotals Sum(long lambda, DateOnly from, DateOnly to)
    {
        using var connection = Open();

        using var sum = Ranged(connection, lambda, from, to, """
            SELECT CAST(COALESCE(SUM(hits), 0) AS BIGINT), CAST(COALESCE(SUM(views), 0) AS BIGINT), CAST(COALESCE(SUM(bot_views), 0) AS BIGINT),
                   CAST(COALESCE(SUM(visitors), 0) AS BIGINT), CAST(COALESCE(SUM(failed), 0) AS BIGINT), CAST(COALESCE(SUM(rejected), 0) AS BIGINT),
                   CAST(COALESCE(SUM(bytes), 0) AS BIGINT), CAST(COALESCE(SUM(millis), 0) AS DOUBLE PRECISION) FROM days
            WHERE lambda = @lambda AND (year, month, day) >= (@fromYear, @fromMonth, @fromDay) AND (year, month, day) <= (@toYear, @toMonth, @toDay)
            """);

        using var reader = sum.ExecuteReader();

        return reader.Read() ? Totals(reader, 0) : AnalyticsTotals.None;
    }

    /// <summary>
    /// The first day anything was counted of a lambda on, or nothing.
    /// </summary>
    public DateOnly? Earliest(long lambda)
    {
        using var connection = Open();

        using var first = Command(connection, "SELECT year, month, day FROM days WHERE lambda = @lambda ORDER BY year, month, day LIMIT 1", "@lambda");

        first.Parameters["@lambda"].Value = lambda;

        using var reader = first.ExecuteReader();

        return reader.Read() ? new DateOnly(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)) : null;
    }

    #endregion

    #region Helpers

    private static IEnumerable<string> Tables => AnalyticsDimensions.All.Select(AnalyticsDimensions.TableOf).Prepend("days");

    private DbConnection Open() => _database.Open(DatabaseStores.Analytics);

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

    private static HashSet<string> Strings(DbCommand command)
    {
        var values = new HashSet<string>(StringComparer.Ordinal);

        using var reader = command.ExecuteReader();

        while (reader.Read())
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
