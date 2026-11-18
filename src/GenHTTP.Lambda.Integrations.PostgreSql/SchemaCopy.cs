using System.Data;

using Npgsql;

namespace GenHTTP.Lambda.Integrations.PostgreSql;

/// <summary>
/// Copies what a lambda keeps in one schema into another - the copy a feature
/// works on.
/// </summary>
/// <remarks>
/// <para>
/// PostgreSQL has no way to copy a schema, so it is made again from what the
/// catalog says of it: its sequences, its tables with everything a table
/// declares of itself (<c>LIKE ... INCLUDING ALL</c>: defaults, constraints,
/// indexes, identities), their rows, the foreign keys between them and its
/// views. Functions, triggers and types of the lambda's own are not copied;
/// the guide says so.
/// </para>
/// <para>
/// All of it in one transaction that sees the source as it was when it began
/// (<see cref="IsolationLevel.RepeatableRead"/>), on a connection of the
/// lambda's role, while the lambda goes on writing to it. What the catalog
/// prints of a default, a key or a view is read with the source the only
/// schema on the search path, so it names the source's objects without their
/// schema, and run again with the target there instead - so it names the
/// target's.
/// </para>
/// </remarks>
internal static class SchemaCopy
{

    public static void Run(NpgsqlConnection connection, string source, string target)
    {
        using var transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);

        Path(connection, source);

        var sequences = Sequences(connection, source);
        var tables = Tables(connection, source);
        var defaults = Defaults(connection, source);
        var owned = Owned(connection, source);
        var identities = Identities(connection, source);
        var keys = Keys(connection, source);
        var views = Views(connection, source);

        Path(connection, target);

        var into = PostgreSqlNames.Quote(target);
        var from = PostgreSqlNames.Quote(source);

        foreach (var sequence in sequences)
        {
            Execute(connection, $"""
                CREATE SEQUENCE {into}.{PostgreSqlNames.Quote(sequence.Name)} AS {sequence.Type}
                INCREMENT BY {sequence.Increment} MINVALUE {sequence.Min} MAXVALUE {sequence.Max} START WITH {sequence.Start} CACHE {sequence.Cache}
                {(sequence.Cycle ? "CYCLE" : "NO CYCLE")}
                """);
        }

        foreach (var table in tables)
        {
            Execute(connection, $"CREATE TABLE {into}.{PostgreSqlNames.Quote(table.Name)} (LIKE {from}.{PostgreSqlNames.Quote(table.Name)} INCLUDING ALL)");
        }

        // a default that draws from a sequence would draw from the source's
        foreach (var (table, column, expression) in defaults)
        {
            Execute(connection, $"ALTER TABLE {into}.{PostgreSqlNames.Quote(table)} ALTER COLUMN {PostgreSqlNames.Quote(column)} SET DEFAULT {expression}");
        }

        foreach (var (sequence, table, column) in owned)
        {
            Execute(connection, $"ALTER SEQUENCE {into}.{PostgreSqlNames.Quote(sequence)} OWNED BY {into}.{PostgreSqlNames.Quote(table)}.{PostgreSqlNames.Quote(column)}");
        }

        foreach (var table in tables.Where(t => t.Columns.Length > 0))
        {
            var name = PostgreSqlNames.Quote(table.Name);

            Execute(connection, $"INSERT INTO {into}.{name} ({table.Columns}) OVERRIDING SYSTEM VALUE SELECT {table.Columns} FROM {from}.{name}");
        }

        foreach (var sequence in sequences)
        {
            Execute(connection, $"SELECT setval({PostgreSqlNames.Literal($"{into}.{PostgreSqlNames.Quote(sequence.Name)}")}, {sequence.Last}, {(sequence.Called ? "true" : "false")})");
        }

        foreach (var identity in identities)
        {
            var table = PostgreSqlNames.Literal($"{into}.{PostgreSqlNames.Quote(identity.Table)}");

            Execute(connection, $"SELECT setval(pg_get_serial_sequence({table}, {PostgreSqlNames.Literal(identity.Column)}), {identity.Last}, {(identity.Called ? "true" : "false")})");
        }

        foreach (var (table, name, definition) in keys)
        {
            Execute(connection, $"ALTER TABLE {into}.{PostgreSqlNames.Quote(table)} ADD CONSTRAINT {PostgreSqlNames.Quote(name)} {definition}");
        }

        foreach (var (name, materialized, definition) in views)
        {
            Execute(connection, $"CREATE {(materialized ? "MATERIALIZED VIEW" : "VIEW")} {into}.{PostgreSqlNames.Quote(name)} AS {definition.TrimEnd().TrimEnd(';')}");
        }

        transaction.Commit();
    }

    #region Reading the source

    /// <summary>
    /// The sequences of the schema but those an identity column keeps for
    /// itself, which come with their table.
    /// </summary>
    private static List<Sequence> Sequences(NpgsqlConnection connection, string schema)
    {
        var found = new List<Sequence>();

        foreach (var row in Rows(connection, """
                     SELECT c.relname, format_type(s.seqtypid, NULL), s.seqstart, s.seqincrement, s.seqmin, s.seqmax, s.seqcache, s.seqcycle
                     FROM pg_sequence s JOIN pg_class c ON c.oid = s.seqrelid JOIN pg_namespace n ON n.oid = c.relnamespace
                     WHERE n.nspname = @schema
                       AND NOT EXISTS (SELECT 1 FROM pg_depend d WHERE d.classid = 'pg_class'::regclass AND d.objid = c.oid AND d.deptype = 'i')
                     ORDER BY c.oid
                     """, schema))
        {
            var name = (string)row[0];

            var (last, called) = Position(connection, $"{PostgreSqlNames.Quote(schema)}.{PostgreSqlNames.Quote(name)}");

            found.Add(new Sequence(name, (string)row[1], (long)row[2], (long)row[3], (long)row[4], (long)row[5], (long)row[6], (bool)row[7], last, called));
        }

        return found;
    }

    /// <summary>
    /// The tables of the schema, in the order they were made, each with the
    /// columns a row is copied by - all but those it computes itself.
    /// </summary>
    private static List<Table> Tables(NpgsqlConnection connection, string schema)
        => [.. Rows(connection, """
               SELECT t.relname, COALESCE(string_agg(quote_ident(a.attname), ', ' ORDER BY a.attnum) FILTER (WHERE a.attgenerated = ''), '')
               FROM pg_class t
               JOIN pg_namespace n ON n.oid = t.relnamespace
               LEFT JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum > 0 AND NOT a.attisdropped
               WHERE n.nspname = @schema AND t.relkind IN ('r', 'p') AND NOT t.relispartition
               GROUP BY t.oid, t.relname
               ORDER BY t.oid
               """, schema).Select(r => new Table((string)r[0], (string)r[1]))];

    /// <summary>
    /// The defaults that draw from a sequence, as the catalog prints them.
    /// </summary>
    private static List<(string Table, string Column, string Expression)> Defaults(NpgsqlConnection connection, string schema)
        => [.. Rows(connection, """
               SELECT t.relname, a.attname, pg_get_expr(ad.adbin, ad.adrelid)
               FROM pg_attrdef ad
               JOIN pg_class t ON t.oid = ad.adrelid
               JOIN pg_namespace n ON n.oid = t.relnamespace
               JOIN pg_attribute a ON a.attrelid = ad.adrelid AND a.attnum = ad.adnum
               WHERE n.nspname = @schema AND t.relkind IN ('r', 'p') AND NOT t.relispartition
                 AND EXISTS (SELECT 1 FROM pg_depend d JOIN pg_class s ON s.oid = d.refobjid AND s.relkind = 'S'
                             WHERE d.classid = 'pg_attrdef'::regclass AND d.objid = ad.oid)
               """, schema).Select(r => ((string)r[0], (string)r[1], (string)r[2]))];

    /// <summary>
    /// The sequences a column owns, such as those of a <c>serial</c>.
    /// </summary>
    private static List<(string Sequence, string Table, string Column)> Owned(NpgsqlConnection connection, string schema)
        => [.. Rows(connection, """
               SELECT s.relname, t.relname, a.attname
               FROM pg_depend d
               JOIN pg_class s ON s.oid = d.objid AND s.relkind = 'S'
               JOIN pg_namespace n ON n.oid = s.relnamespace
               JOIN pg_class t ON t.oid = d.refobjid
               JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = d.refobjsubid
               WHERE n.nspname = @schema AND d.deptype = 'a' AND d.classid = 'pg_class'::regclass AND d.refclassid = 'pg_class'::regclass
               """, schema).Select(r => ((string)r[0], (string)r[1], (string)r[2]))];

    /// <summary>
    /// The identity columns, with where their sequence is.
    /// </summary>
    private static List<Identity> Identities(NpgsqlConnection connection, string schema)
    {
        var found = new List<Identity>();

        foreach (var row in Rows(connection, """
                     SELECT t.relname, a.attname, pg_get_serial_sequence(quote_ident(n.nspname) || '.' || quote_ident(t.relname), a.attname)
                     FROM pg_attribute a
                     JOIN pg_class t ON t.oid = a.attrelid
                     JOIN pg_namespace n ON n.oid = t.relnamespace
                     WHERE n.nspname = @schema AND t.relkind IN ('r', 'p') AND NOT t.relispartition AND a.attidentity <> '' AND a.attnum > 0 AND NOT a.attisdropped
                     """, schema))
        {
            if (row[2] is not string sequence)
            {
                continue;
            }

            var (last, called) = Position(connection, sequence);

            found.Add(new Identity((string)row[0], (string)row[1], last, called));
        }

        return found;
    }

    private static List<(string Table, string Name, string Definition)> Keys(NpgsqlConnection connection, string schema)
        => [.. Rows(connection, """
               SELECT t.relname, c.conname, pg_get_constraintdef(c.oid)
               FROM pg_constraint c JOIN pg_class t ON t.oid = c.conrelid JOIN pg_namespace n ON n.oid = t.relnamespace
               WHERE n.nspname = @schema AND c.contype = 'f' AND NOT t.relispartition
               ORDER BY c.oid
               """, schema).Select(r => ((string)r[0], (string)r[1], (string)r[2]))];

    /// <summary>
    /// The views, in the order they were made - which a view made of another
    /// one comes after.
    /// </summary>
    private static List<(string Name, bool Materialized, string Definition)> Views(NpgsqlConnection connection, string schema)
        => [.. Rows(connection, """
               SELECT c.relname, c.relkind = 'm', pg_get_viewdef(c.oid)
               FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
               WHERE n.nspname = @schema AND c.relkind IN ('v', 'm')
               ORDER BY c.oid
               """, schema).Select(r => ((string)r[0], (bool)r[1], (string)r[2]))];

    /// <summary>
    /// Where a sequence is: its last value, and whether that was handed out.
    /// </summary>
    private static (long Last, bool Called) Position(NpgsqlConnection connection, string sequence)
    {
        using var command = new NpgsqlCommand($"SELECT last_value, is_called FROM {sequence}", connection);

        using var reader = command.ExecuteReader();

        reader.Read();

        return (reader.GetInt64(0), reader.GetBoolean(1));
    }

    #endregion

    #region Helpers

    private static void Path(NpgsqlConnection connection, string schema)
        => Execute(connection, $"SET LOCAL search_path TO {PostgreSqlNames.Quote(schema)}");

    private static void Execute(NpgsqlConnection connection, string sql)
    {
        using var command = new NpgsqlCommand(sql, connection);

        command.ExecuteNonQuery();
    }

    private static List<object[]> Rows(NpgsqlConnection connection, string sql, string schema)
    {
        using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("schema", schema);

        using var reader = command.ExecuteReader();

        var rows = new List<object[]>();

        while (reader.Read())
        {
            var row = new object[reader.FieldCount];

            reader.GetValues(row);

            rows.Add(row);
        }

        return rows;
    }

    #endregion

    private sealed record Sequence(string Name, string Type, long Start, long Increment, long Min, long Max, long Cache, bool Cycle, long Last, bool Called);

    /// <param name="Columns">The columns a row is copied by, quoted and separated by commas</param>
    private sealed record Table(string Name, string Columns);

    private sealed record Identity(string Table, string Column, long Last, bool Called);

}
