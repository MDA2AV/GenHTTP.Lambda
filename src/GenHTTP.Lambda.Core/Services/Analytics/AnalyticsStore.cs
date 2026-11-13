using System.Globalization;
using System.Reflection;

using GenHTTP.Lambda.Configuration;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// The analytics database: what was counted of every lambda whose owner
/// counts visits, a row per lambda, day and value.
/// </summary>
/// <remarks>
/// A SQLite file of its own beside the platform's (<see cref="LambdaOptions.AnalyticsFile"/>),
/// laid out as a star - see the migration for why. Spoken to with plain
/// commands rather than Entity Framework: a writing is a few hundred upserts
/// in one transaction, and a reading a handful of sums over a range of a key,
/// neither of which a model would make clearer.
///
/// Synchronous, as SQLite is: written by the background job, and read for an
/// owner a lambda's year at most, by its key.
/// </remarks>
public sealed class AnalyticsStore : IAnalyticsStore
{
    private const string MigrationNamespace = "GenHTTP.Lambda.Services.Analytics.Migrations";

    /// <summary>
    /// How many days are kept, today included: a year, whatever a tier shows of it.
    /// </summary>
    public const int Kept = 366;

    private readonly string _connection;

    #region Initialization

    public AnalyticsStore(LambdaOptions options, ILogger<AnalyticsStore> logger)
    {
        Directory.CreateDirectory(options.DataDirectory);

        _connection = $"Data Source={options.AnalyticsFile};Default Timeout=30;Pooling=True";

        using var connection = Open();

        Execute(connection, "PRAGMA journal_mode = WAL;");

        var evolve = new EvolveDb.Evolve(connection, message => logger.LogDebug("{Message}", message))
        {
            Locations = [],
            EmbeddedResourceAssemblies = [Assembly.GetExecutingAssembly()],
            EmbeddedResourceFilters = [MigrationNamespace],
            IsEraseDisabled = true,
            MetadataTableName = "schema_versions"
        };

        evolve.Migrate();
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

        using (var lambdas = Command(connection, "INSERT OR IGNORE INTO lambdas (id, created) VALUES ($lambda, $created)", "$lambda", "$created"))
        {
            foreach (var lambda in tally.Lambdas)
            {
                lambdas.Parameters["$lambda"].Value = lambda;
                lambdas.Parameters["$created"].Value = now.ToString("O", CultureInfo.InvariantCulture);
                lambdas.ExecuteNonQuery();
            }
        }

        using (var days = Command(connection, """
                   INSERT INTO days (lambda, year, month, day, hits, views, bot_views, visitors, failed, rejected, bytes, millis)
                   VALUES ($lambda, $year, $month, $day, $hits, $views, $botViews, $visitors, $failed, $rejected, $bytes, $millis)
                   ON CONFLICT (lambda, year, month, day) DO UPDATE SET
                       hits = hits + excluded.hits, views = views + excluded.views, bot_views = bot_views + excluded.bot_views,
                       visitors = visitors + excluded.visitors, failed = failed + excluded.failed, rejected = rejected + excluded.rejected,
                       bytes = bytes + excluded.bytes, millis = millis + excluded.millis
                   """, [.. DayKey, "$hits", "$views", "$botViews", "$visitors", "$failed", "$rejected", "$bytes", "$millis"]))
        {
            foreach (var (lambda, day, totals) in tally.Days)
            {
                Day(days, lambda, day);
                days.Parameters["$hits"].Value = totals.Hits;
                days.Parameters["$views"].Value = totals.Views;
                days.Parameters["$botViews"].Value = totals.BotViews;
                days.Parameters["$visitors"].Value = totals.Visitors;
                days.Parameters["$failed"].Value = totals.Failed;
                days.Parameters["$rejected"].Value = totals.Rejected;
                days.Parameters["$bytes"].Value = totals.Bytes;
                days.Parameters["$millis"].Value = totals.Millis;
                days.ExecuteNonQuery();
            }
        }

        foreach (var group in tally.Cells.GroupBy(c => c.Cell.Dimension))
        {
            var table = AnalyticsDimensions.TableOf(group.Key);

            var most = AnalyticsDimensions.MostOf(group.Key);

            using var upsert = Command(connection, $"""
                INSERT INTO {table} (lambda, year, month, day, value, hits, views, bot_views)
                VALUES ($lambda, $year, $month, $day, $value, $hits, $views, $botViews)
                ON CONFLICT (lambda, year, month, day, value) DO UPDATE SET
                    hits = hits + excluded.hits, views = views + excluded.views, bot_views = bot_views + excluded.bot_views
                """, [.. DayKey, "$value", "$hits", "$views", "$botViews"]);

            using var known = Command(connection, $"SELECT value FROM {table} WHERE lambda = $lambda AND year = $year AND month = $month AND day = $day", DayKey);

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
                    upsert.Parameters["$value"].Value = value;
                    upsert.Parameters["$hits"].Value = hits;
                    upsert.Parameters["$views"].Value = views;
                    upsert.Parameters["$botViews"].Value = botViews;
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
            using var delete = Command(connection, $"DELETE FROM {table} WHERE lambda = $lambda", "$lambda");

            delete.Parameters["$lambda"].Value = lambda;
            delete.ExecuteNonQuery();
        }

        using (var row = Command(connection, "DELETE FROM lambdas WHERE id = $lambda", "$lambda"))
        {
            row.Parameters["$lambda"].Value = lambda;
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
            using var delete = Command(connection, $"DELETE FROM {table} WHERE lambda = $lambda AND (year, month, day) < ($year, $month, $day)", DayKey);

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
                   WHERE lambda = $lambda AND (year, month, day) >= ($fromYear, $fromMonth, $fromDay) AND (year, month, day) <= ($toYear, $toMonth, $toDay)
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
                SELECT value, SUM(hits) AS hits, SUM(views) AS views, SUM(bot_views) AS bot_views FROM {AnalyticsDimensions.TableOf(dimension)}
                WHERE lambda = $lambda AND (year, month, day) >= ($fromYear, $fromMonth, $fromDay) AND (year, month, day) <= ($toYear, $toMonth, $toDay)
                GROUP BY value ORDER BY views DESC, hits DESC, value LIMIT $most
                """);

            values.Parameters.AddWithValue("$most", most);

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
            SELECT IFNULL(SUM(hits), 0), IFNULL(SUM(views), 0), IFNULL(SUM(bot_views), 0), IFNULL(SUM(visitors), 0), IFNULL(SUM(failed), 0),
                   IFNULL(SUM(rejected), 0), IFNULL(SUM(bytes), 0), IFNULL(SUM(millis), 0) FROM days
            WHERE lambda = $lambda AND (year, month, day) >= ($fromYear, $fromMonth, $fromDay) AND (year, month, day) <= ($toYear, $toMonth, $toDay)
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

        using var first = Command(connection, "SELECT year, month, day FROM days WHERE lambda = $lambda ORDER BY year, month, day LIMIT 1", "$lambda");

        first.Parameters["$lambda"].Value = lambda;

        using var reader = first.ExecuteReader();

        return reader.Read() ? new DateOnly(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)) : null;
    }

    #endregion

    #region Helpers

    private static IEnumerable<string> Tables => AnalyticsDimensions.All.Select(AnalyticsDimensions.TableOf).Prepend("days");

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connection);

        connection.Open();

        return connection;
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();

        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// The parameters that name a day of a lambda.
    /// </summary>
    private static readonly string[] DayKey = ["$lambda", "$year", "$month", "$day"];

    /// <summary>
    /// A command with the parameters given, to be set and executed as often as needed.
    /// </summary>
    private static SqliteCommand Command(SqliteConnection connection, string sql, params string[] parameters)
    {
        var command = connection.CreateCommand();

        command.CommandText = sql;

        foreach (var name in parameters)
        {
            command.Parameters.Add(new SqliteParameter(name, null));
        }

        return command;
    }

    private static SqliteCommand Ranged(SqliteConnection connection, long lambda, DateOnly from, DateOnly to, string sql)
    {
        var command = connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.AddWithValue("$lambda", lambda);
        command.Parameters.AddWithValue("$fromYear", from.Year);
        command.Parameters.AddWithValue("$fromMonth", from.Month);
        command.Parameters.AddWithValue("$fromDay", from.Day);
        command.Parameters.AddWithValue("$toYear", to.Year);
        command.Parameters.AddWithValue("$toMonth", to.Month);
        command.Parameters.AddWithValue("$toDay", to.Day);

        return command;
    }

    private static void Day(SqliteCommand command, long lambda, DateOnly day)
    {
        command.Parameters["$lambda"].Value = lambda;
        command.Parameters["$year"].Value = day.Year;
        command.Parameters["$month"].Value = day.Month;
        command.Parameters["$day"].Value = day.Day;
    }

    private static HashSet<string> Strings(SqliteCommand command)
    {
        var values = new HashSet<string>(StringComparer.Ordinal);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }

    private static AnalyticsTotals Totals(SqliteDataReader reader, int at)
        => new(reader.GetInt64(at), reader.GetInt64(at + 1), reader.GetInt64(at + 2), reader.GetInt64(at + 3), reader.GetInt64(at + 4),
               reader.GetInt64(at + 5), reader.GetInt64(at + 6), reader.GetDouble(at + 7));

    #endregion

}
