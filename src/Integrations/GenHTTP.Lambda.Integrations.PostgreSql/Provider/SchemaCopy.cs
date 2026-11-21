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

    public static async ValueTask RunAsync(NpgsqlConnection connection, string source, string target, CancellationToken cancellation)
    {
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellation);

        await PathAsync(connection, source, cancellation);

        var sequences = await SequencesAsync(connection, source, cancellation);
        var tables = await TablesAsync(connection, source, cancellation);
        var defaults = await DefaultsAsync(connection, source, cancellation);
        var owned = await OwnedAsync(connection, source, cancellation);
        var identities = await IdentitiesAsync(connection, source, cancellation);
        var keys = await KeysAsync(connection, source, cancellation);
        var views = await ViewsAsync(connection, source, cancellation);

        await PathAsync(connection, target, cancellation);

        var into = PostgreSqlNames.Quote(target);
        var from = PostgreSqlNames.Quote(source);

        foreach (var sequence in sequences)
        {
            await ExecuteAsync(connection, $"""
                CREATE SEQUENCE {into}.{PostgreSqlNames.Quote(sequence.Name)} AS {sequence.Type}
                INCREMENT BY {sequence.Increment} MINVALUE {sequence.Min} MAXVALUE {sequence.Max} START WITH {sequence.Start} CACHE {sequence.Cache}
                {(sequence.Cycle ? "CYCLE" : "NO CYCLE")}
                """, cancellation);
        }

        foreach (var table in tables)
        {
            await ExecuteAsync(connection, $"CREATE TABLE {into}.{PostgreSqlNames.Quote(table.Name)} (LIKE {from}.{PostgreSqlNames.Quote(table.Name)} INCLUDING ALL)", cancellation);
        }

        // a default that draws from a sequence would draw from the source's
        foreach (var (table, column, expression) in defaults)
        {
            await ExecuteAsync(connection, $"ALTER TABLE {into}.{PostgreSqlNames.Quote(table)} ALTER COLUMN {PostgreSqlNames.Quote(column)} SET DEFAULT {expression}", cancellation);
        }

        foreach (var (sequence, table, column) in owned)
        {
            await ExecuteAsync(connection, $"ALTER SEQUENCE {into}.{PostgreSqlNames.Quote(sequence)} OWNED BY {into}.{PostgreSqlNames.Quote(table)}.{PostgreSqlNames.Quote(column)}", cancellation);
        }

        foreach (var table in tables.Where(t => t.Columns.Length > 0))
        {
            var name = PostgreSqlNames.Quote(table.Name);

            await ExecuteAsync(connection, $"INSERT INTO {into}.{name} ({table.Columns}) OVERRIDING SYSTEM VALUE SELECT {table.Columns} FROM {from}.{name}", cancellation);
        }

        foreach (var sequence in sequences)
        {
            await ExecuteAsync(connection, $"SELECT setval({PostgreSqlNames.Literal($"{into}.{PostgreSqlNames.Quote(sequence.Name)}")}, {sequence.Last}, {(sequence.Called ? "true" : "false")})", cancellation);
        }

        foreach (var identity in identities)
        {
            var table = PostgreSqlNames.Literal($"{into}.{PostgreSqlNames.Quote(identity.Table)}");

            await ExecuteAsync(connection, $"SELECT setval(pg_get_serial_sequence({table}, {PostgreSqlNames.Literal(identity.Column)}), {identity.Last}, {(identity.Called ? "true" : "false")})", cancellation);
        }

        foreach (var (table, name, definition) in keys)
        {
            await ExecuteAsync(connection, $"ALTER TABLE {into}.{PostgreSqlNames.Quote(table)} ADD CONSTRAINT {PostgreSqlNames.Quote(name)} {definition}", cancellation);
        }

        foreach (var (name, materialized, definition) in views)
        {
            await ExecuteAsync(connection, $"CREATE {(materialized ? "MATERIALIZED VIEW" : "VIEW")} {into}.{PostgreSqlNames.Quote(name)} AS {definition.TrimEnd().TrimEnd(';')}", cancellation);
        }

        await transaction.CommitAsync(cancellation);
    }

    #region Reading the source

    /// <summary>
    /// The sequences of the schema but those an identity column keeps for
    /// itself, which come with their table.
    /// </summary>
    private static async ValueTask<List<Sequence>> SequencesAsync(NpgsqlConnection connection, string schema, CancellationToken cancellation)
    {
        var found = new List<Sequence>();

        foreach (var row in await RowsAsync(connection, """
                     SELECT c.relname, format_type(s.seqtypid, NULL), s.seqstart, s.seqincrement, s.seqmin, s.seqmax, s.seqcache, s.seqcycle
                     FROM pg_sequence s JOIN pg_class c ON c.oid = s.seqrelid JOIN pg_namespace n ON n.oid = c.relnamespace
                     WHERE n.nspname = @schema
                       AND NOT EXISTS (SELECT 1 FROM pg_depend d WHERE d.classid = 'pg_class'::regclass AND d.objid = c.oid AND d.deptype = 'i')
                     ORDER BY c.oid
                     """, schema, cancellation))
        {
            var name = (string)row[0];

            var (last, called) = await PositionAsync(connection, $"{PostgreSqlNames.Quote(schema)}.{PostgreSqlNames.Quote(name)}", cancellation);

            found.Add(new Sequence(name, (string)row[1], (long)row[2], (long)row[3], (long)row[4], (long)row[5], (long)row[6], (bool)row[7], last, called));
        }

        return found;
    }

    /// <summary>
    /// The tables of the schema, in the order they were made, each with the
    /// columns a row is copied by - all but those it computes itself.
    /// </summary>
    private static async ValueTask<List<Table>> TablesAsync(NpgsqlConnection connection, string schema, CancellationToken cancellation)
        => [.. (await RowsAsync(connection, """
               SELECT t.relname, COALESCE(string_agg(quote_ident(a.attname), ', ' ORDER BY a.attnum) FILTER (WHERE a.attgenerated = ''), '')
               FROM pg_class t
               JOIN pg_namespace n ON n.oid = t.relnamespace
               LEFT JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum > 0 AND NOT a.attisdropped
               WHERE n.nspname = @schema AND t.relkind IN ('r', 'p') AND NOT t.relispartition
               GROUP BY t.oid, t.relname
               ORDER BY t.oid
               """, schema, cancellation)).Select(r => new Table((string)r[0], (string)r[1]))];

    /// <summary>
    /// The defaults that draw from a sequence, as the catalog prints them.
    /// </summary>
    private static async ValueTask<List<(string Table, string Column, string Expression)>> DefaultsAsync(NpgsqlConnection connection, string schema, CancellationToken cancellation)
        => [.. (await RowsAsync(connection, """
               SELECT t.relname, a.attname, pg_get_expr(ad.adbin, ad.adrelid)
               FROM pg_attrdef ad
               JOIN pg_class t ON t.oid = ad.adrelid
               JOIN pg_namespace n ON n.oid = t.relnamespace
               JOIN pg_attribute a ON a.attrelid = ad.adrelid AND a.attnum = ad.adnum
               WHERE n.nspname = @schema AND t.relkind IN ('r', 'p') AND NOT t.relispartition
                 AND EXISTS (SELECT 1 FROM pg_depend d JOIN pg_class s ON s.oid = d.refobjid AND s.relkind = 'S'
                             WHERE d.classid = 'pg_attrdef'::regclass AND d.objid = ad.oid)
               """, schema, cancellation)).Select(r => ((string)r[0], (string)r[1], (string)r[2]))];

    /// <summary>
    /// The sequences a column owns, such as those of a <c>serial</c>.
    /// </summary>
    private static async ValueTask<List<(string Sequence, string Table, string Column)>> OwnedAsync(NpgsqlConnection connection, string schema, CancellationToken cancellation)
        => [.. (await RowsAsync(connection, """
               SELECT s.relname, t.relname, a.attname
               FROM pg_depend d
               JOIN pg_class s ON s.oid = d.objid AND s.relkind = 'S'
               JOIN pg_namespace n ON n.oid = s.relnamespace
               JOIN pg_class t ON t.oid = d.refobjid
               JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = d.refobjsubid
               WHERE n.nspname = @schema AND d.deptype = 'a' AND d.classid = 'pg_class'::regclass AND d.refclassid = 'pg_class'::regclass
               """, schema, cancellation)).Select(r => ((string)r[0], (string)r[1], (string)r[2]))];

    /// <summary>
    /// The identity columns, with where their sequence is.
    /// </summary>
    private static async ValueTask<List<Identity>> IdentitiesAsync(NpgsqlConnection connection, string schema, CancellationToken cancellation)
    {
        var found = new List<Identity>();

        foreach (var row in await RowsAsync(connection, """
                     SELECT t.relname, a.attname, pg_get_serial_sequence(quote_ident(n.nspname) || '.' || quote_ident(t.relname), a.attname)
                     FROM pg_attribute a
                     JOIN pg_class t ON t.oid = a.attrelid
                     JOIN pg_namespace n ON n.oid = t.relnamespace
                     WHERE n.nspname = @schema AND t.relkind IN ('r', 'p') AND NOT t.relispartition AND a.attidentity <> '' AND a.attnum > 0 AND NOT a.attisdropped
                     """, schema, cancellation))
        {
            if (row[2] is not string sequence)
            {
                continue;
            }

            var (last, called) = await PositionAsync(connection, sequence, cancellation);

            found.Add(new Identity((string)row[0], (string)row[1], last, called));
        }

        return found;
    }

    private static async ValueTask<List<(string Table, string Name, string Definition)>> KeysAsync(NpgsqlConnection connection, string schema, CancellationToken cancellation)
        => [.. (await RowsAsync(connection, """
               SELECT t.relname, c.conname, pg_get_constraintdef(c.oid)
               FROM pg_constraint c JOIN pg_class t ON t.oid = c.conrelid JOIN pg_namespace n ON n.oid = t.relnamespace
               WHERE n.nspname = @schema AND c.contype = 'f' AND NOT t.relispartition
               ORDER BY c.oid
               """, schema, cancellation)).Select(r => ((string)r[0], (string)r[1], (string)r[2]))];

    /// <summary>
    /// The views, in the order they were made - which a view made of another
    /// one comes after.
    /// </summary>
    private static async ValueTask<List<(string Name, bool Materialized, string Definition)>> ViewsAsync(NpgsqlConnection connection, string schema, CancellationToken cancellation)
        => [.. (await RowsAsync(connection, """
               SELECT c.relname, c.relkind = 'm', pg_get_viewdef(c.oid)
               FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
               WHERE n.nspname = @schema AND c.relkind IN ('v', 'm')
               ORDER BY c.oid
               """, schema, cancellation)).Select(r => ((string)r[0], (bool)r[1], (string)r[2]))];

    /// <summary>
    /// Where a sequence is: its last value, and whether that was handed out.
    /// </summary>
    private static async ValueTask<(long Last, bool Called)> PositionAsync(NpgsqlConnection connection, string sequence, CancellationToken cancellation)
    {
        await using var command = new NpgsqlCommand($"SELECT last_value, is_called FROM {sequence}", connection);

        await using var reader = await command.ExecuteReaderAsync(cancellation);

        await reader.ReadAsync(cancellation);

        return (reader.GetInt64(0), reader.GetBoolean(1));
    }

    #endregion

    #region Helpers

    private static ValueTask PathAsync(NpgsqlConnection connection, string schema, CancellationToken cancellation)
        => ExecuteAsync(connection, $"SET LOCAL search_path TO {PostgreSqlNames.Quote(schema)}", cancellation);

    private static async ValueTask ExecuteAsync(NpgsqlConnection connection, string sql, CancellationToken cancellation)
    {
        await using var command = new NpgsqlCommand(sql, connection);

        await command.ExecuteNonQueryAsync(cancellation);
    }

    private static async ValueTask<List<object[]>> RowsAsync(NpgsqlConnection connection, string sql, string schema, CancellationToken cancellation)
    {
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("schema", schema);

        await using var reader = await command.ExecuteReaderAsync(cancellation);

        var rows = new List<object[]>();

        while (await reader.ReadAsync(cancellation))
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
