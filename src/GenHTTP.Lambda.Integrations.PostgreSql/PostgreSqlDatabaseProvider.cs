using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Security.Cryptography;

using GenHTTP.Lambda.Integrations.Databases;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace GenHTTP.Lambda.Integrations.PostgreSql;

/// <summary>
/// Keeps every database in one PostgreSQL database, a schema each.
/// </summary>
/// <remarks>
/// <para>
/// The platform's records are in the schema <c>platform</c>, its analytics in
/// <c>analytics</c>, both owned by the user the installation connects as. A
/// lambda's database is the schema <c>lambda_{id}</c>, and the copy a feature
/// works on <c>lambda_{id}_feature_{feature}</c>.
/// </para>
/// <para>
/// A lambda connects as a role of its own (<c>{prefix}_{id}</c>), which may
/// use and create in the schemas of that lambda and nothing else: it owns the
/// tables it makes there, and has no privilege on the platform's schemas or
/// any other lambda's. The platform owns the schemas, so it can drop them with
/// what is in them; the role exists for as long as the lambda's database does.
/// Its password is made anew every time the platform starts, and set as a role
/// is first used - it is never kept anywhere. What every role can see is the
/// catalog of the server: the names of schemas and tables, not what they hold.
/// That is PostgreSQL's privileges at work, not a sandbox.
/// </para>
/// <para>
/// PostgreSQL cannot hold a schema to a size. The room a lambda's data may take
/// is held by the connections it is handed instead: one opened while its
/// database is past its room only reads (<c>default_transaction_read_only</c>),
/// which code of the lambda could switch back - so it governs, as the code
/// guard does, rather than enforce. What a database takes is measured as its
/// tables, their indexes and what they keep apart, and remembered for a few
/// seconds: a lambda connects for nearly every request.
/// </para>
/// <para>
/// Everything it sends to the server is awaited, which Kestrel and GenHTTP's
/// internal engine make use of: no thread waits for the network. On the Ioxide
/// engine what it awaits resumes on the reactor of the request, through
/// Npgsql's own sockets - it works there, but does not complete on the reactor
/// as SQLite does. The platform's own records are awaited the same way,
/// through Entity Framework, and so are the secrets it seals into them.
/// </para>
/// </remarks>
internal sealed class PostgreSqlDatabaseProvider : IDatabaseProvider, IAsyncDisposable
{
    private static readonly long Patience = Stopwatch.Frequency * 5;

    private readonly NpgsqlConnectionStringBuilder _connection;

    private readonly NpgsqlDataSource _platform;

    private readonly Dictionary<DatabaseStore, NpgsqlDataSource> _stores = [];

    private readonly ConcurrentDictionary<(DataScope Scope, bool Writing), NpgsqlDataSource> _lambdas = [];

    /// <summary>
    /// The password each lambda's role was given since the platform started.
    /// </summary>
    private readonly ConcurrentDictionary<long, string> _passwords = [];

    private readonly ConcurrentDictionary<DataScope, (long Size, long At)> _measured = [];

    /// <summary>
    /// Who is making the role of a lambda: one at a time, across the
    /// statements it takes.
    /// </summary>
    private readonly ConcurrentDictionary<long, SemaphoreSlim> _turns = [];

    #region Get-/Setters

    private string RolePrefix { get; }

    private int PoolSize { get; }

    private ILogger Logger { get; }

    #endregion

    #region Initialization

    private PostgreSqlDatabaseProvider(NpgsqlConnectionStringBuilder connection, string rolePrefix, int poolSize, ILogger<PostgreSqlDatabaseProvider> logger)
    {
        _connection = connection;

        RolePrefix = rolePrefix;
        PoolSize = poolSize;
        Logger = logger;

        _platform = NpgsqlDataSource.Create(connection.ConnectionString);
    }

    /// <summary>
    /// Connects to the server and prepares what the platform keeps there:
    /// the schemas of its stores, and the schema <c>public</c> closed.
    /// </summary>
    public static async ValueTask<PostgreSqlDatabaseProvider> StartAsync(NpgsqlConnectionStringBuilder connection, string rolePrefix, int poolSize,
                                                                         ILogger<PostgreSqlDatabaseProvider> logger, CancellationToken cancellation)
    {
        var provider = new PostgreSqlDatabaseProvider(connection, rolePrefix, poolSize, logger);

        try
        {
            await provider.SecureAsync(cancellation);

            foreach (var store in Enum.GetValues<DatabaseStore>())
            {
                await provider.PrepareStoreAsync(store, cancellation);
            }

            return provider;
        }
        catch
        {
            await provider.DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// Takes from everybody the right to create tables in the schema
    /// <c>public</c>, which every role had before PostgreSQL 15: a lambda's
    /// role may create where its own schemas are and nowhere else.
    /// </summary>
    /// <remarks>
    /// The schema is not the platform's, so where the user the installation
    /// connects as may not change it, this is said once and left as it is.
    /// </remarks>
    private async ValueTask SecureAsync(CancellationToken cancellation)
    {
        try
        {
            await ExecuteAsync(_platform, "REVOKE CREATE ON SCHEMA public FROM PUBLIC", cancellation);
        }
        catch (PostgresException e)
        {
            Logger.LogWarning("Failed to revoke CREATE on the schema public from PUBLIC ({Reason}): on PostgreSQL 14 and earlier, the roles of the lambdas may then create tables there",
                              e.MessageText);
        }
    }

    /// <summary>
    /// Makes the schema of a store where it is not there, and what it is
    /// reached through: that schema, and nothing else on the search path.
    /// </summary>
    /// <remarks>
    /// Every store, as the platform starts: the platform's records are
    /// configured with it synchronously, before anything is asked of them.
    /// </remarks>
    private async ValueTask PrepareStoreAsync(DatabaseStore store, CancellationToken cancellation)
    {
        var schema = PostgreSqlNames.Schema(store);

        await ExecuteAsync(_platform, $"CREATE SCHEMA IF NOT EXISTS {PostgreSqlNames.Quote(schema)}", cancellation);

        var connection = new NpgsqlConnectionStringBuilder(_connection.ConnectionString) { SearchPath = schema };

        _stores[store] = NpgsqlDataSource.Create(connection.ConnectionString);
    }

    #endregion

    #region The platform's own

    public DatabaseDialect Dialect => DatabaseDialect.PostgreSql;

    public void ConfigureStore(DbContextOptionsBuilder options, DatabaseStore store) => options.UseNpgsql(Store(store));

    public async ValueTask<DbConnection> OpenAsync(DatabaseStore store, CancellationToken cancellation = default)
        => await Store(store).OpenConnectionAsync(cancellation);

    private NpgsqlDataSource Store(DatabaseStore store)
        => _stores.TryGetValue(store, out var source) ? source : throw new ArgumentOutOfRangeException(nameof(store), store, "The platform keeps no such store.");

    #endregion

    #region A lambda's

    public DatabaseBinding Binding => DatabaseBinding.Neutral;

    /// <remarks>
    /// Names are folded to lower case, as PostgreSQL folds a name written
    /// without quotes - so a property <c>Title</c> is the column a migration
    /// made as <c>title</c>, as it is on any database that does not mind the
    /// case of a name.
    /// </remarks>
    public void ConfigureContext(DbContextOptionsBuilder options, DbConnection connection)
    {
        options.UseNpgsql(connection, contextOwnsConnection: true);

        ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(new LowerCaseNames());
    }

    /// <summary>
    /// Makes the schema of a lambda, empty, and the role it is reached as.
    /// </summary>
    public async ValueTask CreateAsync(DataScope scope, CancellationToken cancellation = default)
    {
        var role = await PrepareAsync(scope.LambdaId, cancellation);

        await MakeSchemaAsync(PostgreSqlNames.Schema(scope), role, cancellation);

        Logger.LogInformation("Created database of {Scope}", scope);
    }

    public ValueTask<bool> ExistsAsync(DataScope scope, CancellationToken cancellation = default) => SchemaExistsAsync(PostgreSqlNames.Schema(scope), cancellation);

    public async ValueTask<DbConnection> ConnectAsync(DataScope scope, long room, CancellationToken cancellation = default)
    {
        await PrepareAsync(scope.LambdaId, cancellation);

        var writing = await MeasureAsync(scope, cancellation) < room;

        return await Lambda(scope, writing).OpenConnectionAsync(cancellation);
    }

    public async ValueTask<DbConnection?> ReadAsync(DataScope scope, TimeSpan patience, CancellationToken cancellation = default)
    {
        if (!await ExistsAsync(scope, cancellation))
        {
            return null;
        }

        var role = await PrepareAsync(scope.LambdaId, cancellation);

        var connection = new NpgsqlConnectionStringBuilder(_connection.ConnectionString)
        {
            Username = role,
            Password = _passwords[scope.LambdaId],
            SearchPath = PostgreSqlNames.Schema(scope),
            Pooling = false,
            Options = $"-c default_transaction_read_only=on -c statement_timeout={(long)patience.TotalMilliseconds}"
        };

        var opened = new NpgsqlConnection(connection.ConnectionString);

        try
        {
            await opened.OpenAsync(cancellation);
            return opened;
        }
        catch
        {
            await opened.DisposeAsync();
            throw;
        }
    }

    public async ValueTask<long> SizeOfAsync(DataScope scope, CancellationToken cancellation = default)
    {
        var size = await SizeAsync(PostgreSqlNames.Schema(scope), cancellation);

        _measured[scope] = (size, Stopwatch.GetTimestamp());

        return size;
    }

    /// <remarks>
    /// Made by the lambda's role in one transaction that sees the source as
    /// it was at its start, while the lambda goes on writing to it - see
    /// <see cref="SchemaCopy"/>.
    /// </remarks>
    public async ValueTask CopyAsync(DataScope source, DataScope target, CancellationToken cancellation = default)
    {
        await DeleteAsync(target, cancellation);

        var from = PostgreSqlNames.Schema(source);

        if (!await SchemaExistsAsync(from, cancellation))
        {
            return;
        }

        var role = await PrepareAsync(source.LambdaId, cancellation);

        var into = PostgreSqlNames.Schema(target);

        await MakeSchemaAsync(into, role, cancellation);

        await using var connection = await Lambda(source, true).OpenConnectionAsync(cancellation);

        await SchemaCopy.RunAsync(connection, from, into, cancellation);
    }

    public async ValueTask DeleteAsync(DataScope scope, CancellationToken cancellation = default)
    {
        await ReleaseAsync(scope);

        await ExecuteAsync(_platform, $"DROP SCHEMA IF EXISTS {PostgreSqlNames.Quote(PostgreSqlNames.Schema(scope))} CASCADE", cancellation);
    }

    /// <remarks>
    /// The schemas of its features are found by their names, which begin with
    /// the lambda's; its role goes with them.
    /// </remarks>
    public async ValueTask DeleteAllAsync(long lambdaId, CancellationToken cancellation = default)
    {
        foreach (var scope in _lambdas.Keys.Select(k => k.Scope).Where(s => s.LambdaId == lambdaId).Distinct().ToList())
        {
            await ReleaseAsync(scope);
        }

        var own = PostgreSqlNames.Schema(new DataScope(lambdaId));

        foreach (var schema in (await SchemasAsync(own, cancellation)).Append(own).Distinct())
        {
            await ExecuteAsync(_platform, $"DROP SCHEMA IF EXISTS {PostgreSqlNames.Quote(schema)} CASCADE", cancellation);
        }

        await ExecuteAsync(_platform, $"DROP ROLE IF EXISTS {PostgreSqlNames.Quote(PostgreSqlNames.Role(RolePrefix, lambdaId))}", cancellation);

        _passwords.TryRemove(lambdaId, out _);
    }

    /// <summary>
    /// Nothing: an exported project keeps its records in a SQLite file, and
    /// what PostgreSQL holds is taken with its own tools - <c>pg_dump</c> of
    /// the lambda's schema.
    /// </summary>
    public ValueTask<string?> ExportAsync(DataScope scope, CancellationToken cancellation = default) => ValueTask.FromResult<string?>(null);

    #endregion

    #region Looking into one

    public async ValueTask<IReadOnlyList<DatabaseTableInfo>> TablesAsync(DbConnection connection, CancellationToken cancellation = default)
    {
        var found = new List<DatabaseTableInfo>();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT c.relname, c.relkind IN ('v', 'm') FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = current_schema() AND c.relkind IN ('r', 'p', 'v', 'm', 'f')
            ORDER BY c.relkind IN ('v', 'm'), c.relname
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellation);

        while (await reader.ReadAsync(cancellation))
        {
            found.Add(new DatabaseTableInfo(reader.GetString(0), reader.GetBoolean(1)));
        }

        return found;
    }

    public async ValueTask<IReadOnlyList<DatabaseColumnInfo>> ColumnsAsync(DbConnection connection, string table, CancellationToken cancellation = default)
    {
        var columns = new List<DatabaseColumnInfo>();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT a.attname, format_type(a.atttypid, a.atttypmod), a.attnotnull,
                   EXISTS (SELECT 1 FROM pg_index i WHERE i.indrelid = c.oid AND i.indisprimary AND a.attnum = ANY (i.indkey)),
                   pg_get_expr(d.adbin, d.adrelid)
            FROM pg_attribute a
            JOIN pg_class c ON c.oid = a.attrelid
            JOIN pg_namespace n ON n.oid = c.relnamespace
            LEFT JOIN pg_attrdef d ON d.adrelid = a.attrelid AND d.adnum = a.attnum
            WHERE n.nspname = current_schema() AND c.relname = @table AND a.attnum > 0 AND NOT a.attisdropped
            ORDER BY a.attnum
            """;

        Parameter(command, "@table", table);

        await using var reader = await command.ExecuteReaderAsync(cancellation);

        while (await reader.ReadAsync(cancellation))
        {
            columns.Add(new DatabaseColumnInfo(reader.GetString(0), reader.GetString(1), reader.GetBoolean(2), reader.GetBoolean(3),
                                               reader.IsDBNull(4) ? null : reader.GetString(4)));
        }

        return columns;
    }

    /// <remarks>
    /// PostgreSQL keeps no order of its own; where a row is kept is close to
    /// when it was written for a table that is mostly added to.
    /// </remarks>
    public async ValueTask<string?> NaturalOrderAsync(DbConnection connection, string table, CancellationToken cancellation = default)
    {
        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT c.relkind = 'r' FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = current_schema() AND c.relname = @table
            """;

        Parameter(command, "@table", table);

        return await command.ExecuteScalarAsync(cancellation) is true ? "ctid" : null;
    }

    public string Quote(string name) => PostgreSqlNames.Quote(name);

    #endregion

    #region Keeping it in shape

    /// <remarks>
    /// As the user the installation connects as, which made the store's
    /// tables and owns them.
    /// </remarks>
    public async ValueTask<DatabaseOptimization> OptimizeAsync(DatabaseStore store, CancellationToken cancellation = default)
    {
        var schema = PostgreSqlNames.Schema(store);

        await using var connection = await _platform.OpenConnectionAsync(cancellation);

        var before = await SizeAsync(schema, cancellation);

        var (steps, skipped) = await VacuumAsync(connection, schema, cancellation);

        return new DatabaseOptimization(before, await SizeAsync(schema, cancellation), steps, skipped);
    }

    /// <remarks>
    /// As the role of the lambda, which owns the tables it made - whoever else
    /// may vacuum them depends on how the installation's user was set up - over
    /// a connection of its own that is not pooled, so walking every lambda
    /// leaves no connection open behind it, and that may write, since vacuuming
    /// is refused to a connection that only reads.
    /// </remarks>
    public async ValueTask<DatabaseOptimization> OptimizeAsync(DataScope scope, CancellationToken cancellation = default)
    {
        var schema = PostgreSqlNames.Schema(scope);

        if (!await SchemaExistsAsync(schema, cancellation))
        {
            return DatabaseOptimization.None;
        }

        var role = await PrepareAsync(scope.LambdaId, cancellation);

        var connection = new NpgsqlConnectionStringBuilder(_connection.ConnectionString)
        {
            Username = role,
            Password = _passwords[scope.LambdaId],
            SearchPath = schema,
            Pooling = false
        };

        await using var opened = new NpgsqlConnection(connection.ConnectionString);

        await opened.OpenAsync(cancellation);

        var before = await SizeAsync(schema, cancellation);

        var (steps, skipped) = await VacuumAsync(opened, schema, cancellation);

        return new DatabaseOptimization(before, await SizeOfAsync(scope, cancellation), steps, skipped);
    }

    /// <summary>
    /// Vacuums and analyzes every table of a schema the connected user owns,
    /// one statement each.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>VACUUM</c> marks the room of rows that were deleted or replaced as
    /// free for the table's next rows, and <c>ANALYZE</c> takes the statistics
    /// the planner reads - what autovacuum does too, when it gets round to it.
    /// Neither takes a lock that keeps the lambda from reading or writing, and
    /// a table somebody holds a lock on is skipped rather than waited for
    /// (<c>SKIP_LOCKED</c>), which PostgreSQL says as a warning: those are
    /// passed on as what was skipped.
    /// </para>
    /// <para>
    /// Not <c>VACUUM FULL</c>, which would give the room back to the disk, but
    /// rewrites each table under a lock that holds up every reader - a
    /// database's administrator does that knowingly, if at all.
    /// </para>
    /// <para>
    /// One table per statement, named: a <c>VACUUM</c> without a table goes
    /// over the whole database, and it cannot run inside a transaction, which
    /// several statements in one command would be.
    /// </para>
    /// </remarks>
    private static async ValueTask<(List<string> Steps, List<string> Skipped)> VacuumAsync(NpgsqlConnection connection, string schema, CancellationToken cancellation)
    {
        var tables = new List<string>();

        var others = 0;

        await using (var command = new NpgsqlCommand("""
                         SELECT c.relname, c.relowner = (SELECT oid FROM pg_roles WHERE rolname = current_user)
                         FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                         WHERE n.nspname = @schema AND c.relkind IN ('r', 'm')
                         ORDER BY c.relname
                         """, connection))
        {
            Parameter(command, "@schema", schema);

            await using var reader = await command.ExecuteReaderAsync(cancellation);

            while (await reader.ReadAsync(cancellation))
            {
                if (reader.GetBoolean(1))
                {
                    tables.Add(reader.GetString(0));
                }
                else
                {
                    others++;
                }
            }
        }

        var steps = new List<string>();

        // a table made by another role is not this one's to vacuum, and
        // PostgreSQL would only warn about it - said here instead
        var skipped = others == 0 ? new List<string>() : [others == 1 ? "VACUUM: 1 table is owned by another role" : $"VACUUM: {others} tables are owned by another role"];

        if (tables.Count == 0)
        {
            return (steps, skipped);
        }

        void Warned(object? sender, NpgsqlNoticeEventArgs e)
        {
            // the severity as PostgreSQL names it, whatever language it speaks
            if (e.Notice.InvariantSeverity is "WARNING")
            {
                skipped.Add(e.Notice.MessageText);
            }
        }

        connection.Notice += Warned;

        try
        {
            foreach (var table in tables)
            {
                await using var vacuum = new NpgsqlCommand($"VACUUM (ANALYZE, SKIP_LOCKED) {PostgreSqlNames.Quote(schema)}.{PostgreSqlNames.Quote(table)}", connection)
                {
                    // as long as the table takes, which is what the background
                    // is for; stopping the platform cancels it
                    CommandTimeout = 0
                };

                await vacuum.ExecuteNonQueryAsync(cancellation);
            }
        }
        finally
        {
            connection.Notice -= Warned;
        }

        steps.Add(tables.Count == 1 ? "VACUUM (ANALYZE, SKIP_LOCKED) of 1 table" : $"VACUUM (ANALYZE, SKIP_LOCKED) of {tables.Count} tables");

        return (steps, skipped);
    }

    #endregion

    #region Describing it

    public IntegrationInfo Describe() => new(IntegrationKind.Database, "PostgreSQL",
                                             [
                                                 new IntegrationDetail("Server", _connection.Port is 5432 or 0 ? _connection.Host ?? "localhost" : $"{_connection.Host}:{_connection.Port}"),
                                                 new IntegrationDetail("Database", _connection.Database ?? ""),
                                                 new IntegrationDetail("User", _connection.Username ?? ""),
                                                 new IntegrationDetail("Schemas", $"{PostgreSqlNames.Schema(DatabaseStore.Platform)}, {PostgreSqlNames.Schema(DatabaseStore.Analytics)}, lambda_{{lambda}}, lambda_{{lambda}}_feature_{{draft}}"),
                                                 new IntegrationDetail("Roles", $"{RolePrefix}_{{lambda}}"),
                                                 new IntegrationDetail("Connections", $"{PoolSize} at most per lambda and draft")
                                             ]);

    /// <remarks>
    /// As short as SQLite's: it counts against the guide's room, and what an
    /// operator needs to know beyond what changes an agent's code is in
    /// <c>docs/postgresql.md</c>.
    /// </remarks>
    public string Guide(GuideContext context)
        => "- Here: PostgreSQL. Names fold to lower case; a draft copies tables, sequences and views only; past the data's room (see limits) it only reads.";

    #endregion

    #region Roles and schemas

    /// <summary>
    /// Makes the role of a lambda where it is not there, and gives it the
    /// password of this run the first time it is used in it.
    /// </summary>
    /// <remarks>
    /// One at a time per lambda, so two requests that both find no password
    /// do not give the role two - the second waits for the first, without
    /// holding a thread while it does.
    /// </remarks>
    /// <returns>The name of the role</returns>
    private async ValueTask<string> PrepareAsync(long lambdaId, CancellationToken cancellation)
    {
        var role = PostgreSqlNames.Role(RolePrefix, lambdaId);

        if (_passwords.ContainsKey(lambdaId))
        {
            return role;
        }

        var turn = _turns.GetOrAdd(lambdaId, _ => new SemaphoreSlim(1, 1));

        await turn.WaitAsync(cancellation);

        try
        {
            if (_passwords.ContainsKey(lambdaId))
            {
                return role;
            }

            var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

            var quoted = PostgreSqlNames.Quote(role);

            // a role may not be made twice, and nothing makes one only where
            // it is missing - so this asks first, in one statement
            await ExecuteAsync(_platform, $"""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = {PostgreSqlNames.Literal(role)}) THEN
                        CREATE ROLE {quoted} LOGIN NOINHERIT NOCREATEDB NOCREATEROLE;
                    END IF;
                END
                $$
                """, cancellation);

            await ExecuteAsync(_platform, $"ALTER ROLE {quoted} WITH LOGIN PASSWORD {PostgreSqlNames.Literal(password)}", cancellation);

            _passwords[lambdaId] = password;

            return role;
        }
        finally
        {
            turn.Release();
        }
    }

    /// <summary>
    /// Makes a schema, owned by the platform, the role of the lambda using
    /// and creating in it.
    /// </summary>
    private async ValueTask MakeSchemaAsync(string schema, string role, CancellationToken cancellation)
    {
        var quoted = PostgreSqlNames.Quote(schema);

        await ExecuteAsync(_platform, $"CREATE SCHEMA IF NOT EXISTS {quoted}", cancellation);
        await ExecuteAsync(_platform, $"GRANT USAGE, CREATE ON SCHEMA {quoted} TO {PostgreSqlNames.Quote(role)}", cancellation);
    }

    private async ValueTask<bool> SchemaExistsAsync(string schema, CancellationToken cancellation)
    {
        await using var command = _platform.CreateCommand("SELECT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = @schema)");

        Parameter(command, "@schema", schema);

        return await command.ExecuteScalarAsync(cancellation) is true;
    }

    /// <summary>
    /// The schemas whose names begin with the given one and a feature's mark.
    /// </summary>
    private async ValueTask<List<string>> SchemasAsync(string own, CancellationToken cancellation)
    {
        var found = new List<string>();

        await using var command = _platform.CreateCommand("SELECT nspname FROM pg_namespace WHERE starts_with(nspname, @prefix)");

        Parameter(command, "@prefix", own + PostgreSqlNames.FeatureMark);

        await using var reader = await command.ExecuteReaderAsync(cancellation);

        while (await reader.ReadAsync(cancellation))
        {
            found.Add(reader.GetString(0));
        }

        return found;
    }

    #endregion

    #region Room

    /// <summary>
    /// What the database of a scope takes, remembered for a few seconds.
    /// </summary>
    private ValueTask<long> MeasureAsync(DataScope scope, CancellationToken cancellation)
    {
        if (_measured.TryGetValue(scope, out var known) && Stopwatch.GetTimestamp() - known.At < Patience)
        {
            return ValueTask.FromResult(known.Size);
        }

        return SizeOfAsync(scope, cancellation);
    }

    /// <summary>
    /// The room the tables of a schema take on the disk, their indexes and
    /// what they keep apart included.
    /// </summary>
    private async ValueTask<long> SizeAsync(string schema, CancellationToken cancellation)
    {
        await using var command = _platform.CreateCommand("""
            SELECT COALESCE(SUM(pg_total_relation_size(c.oid)), 0)::bigint FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = @schema AND c.relkind IN ('r', 'm', 'p')
            """);

        Parameter(command, "@schema", schema);

        return await command.ExecuteScalarAsync(cancellation) is long size ? size : 0;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// What a lambda, or a feature's copy, connects through: as its role, its
    /// schema the only one on the search path - and only reading where it is
    /// past its room.
    /// </summary>
    private NpgsqlDataSource Lambda(DataScope scope, bool writing) => _lambdas.GetOrAdd((scope, writing), key =>
    {
        var connection = new NpgsqlConnectionStringBuilder(_connection.ConnectionString)
        {
            Username = PostgreSqlNames.Role(RolePrefix, key.Scope.LambdaId),
            Password = _passwords[key.Scope.LambdaId],
            SearchPath = PostgreSqlNames.Schema(key.Scope),
            MaxPoolSize = PoolSize,
            MinPoolSize = 0,
            ConnectionIdleLifetime = 60
        };

        if (!key.Writing)
        {
            connection.Options = "-c default_transaction_read_only=on";
        }

        return NpgsqlDataSource.Create(connection.ConnectionString);
    });

    /// <summary>
    /// Closes what is pooled for a scope, so no connection outlives its schema.
    /// </summary>
    private async ValueTask ReleaseAsync(DataScope scope)
    {
        foreach (var writing in new[] { true, false })
        {
            if (_lambdas.TryRemove((scope, writing), out var source))
            {
                await source.DisposeAsync();
            }
        }

        _measured.TryRemove(scope, out _);
    }

    private static async ValueTask ExecuteAsync(NpgsqlDataSource source, string sql, CancellationToken cancellation)
    {
        await using var command = source.CreateCommand(sql);

        await command.ExecuteNonQueryAsync(cancellation);
    }

    private static void Parameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();

        parameter.ParameterName = name;
        parameter.Value = value;

        command.Parameters.Add(parameter);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var source in _lambdas.Values)
        {
            await source.DisposeAsync();
        }

        foreach (var source in _stores.Values)
        {
            await source.DisposeAsync();
        }

        await _platform.DisposeAsync();

        foreach (var turn in _turns.Values)
        {
            turn.Dispose();
        }
    }

    #endregion

}
