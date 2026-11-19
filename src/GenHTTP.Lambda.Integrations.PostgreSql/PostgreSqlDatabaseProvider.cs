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
/// </remarks>
internal sealed class PostgreSqlDatabaseProvider : IDatabaseProvider, IDisposable
{
    private static readonly long Patience = Stopwatch.Frequency * 5;

    private readonly NpgsqlConnectionStringBuilder _connection;

    private readonly NpgsqlDataSource _platform;

    private readonly ConcurrentDictionary<string, NpgsqlDataSource> _stores = new(StringComparer.Ordinal);

    private readonly ConcurrentDictionary<(DataScope Scope, bool Writing), NpgsqlDataSource> _lambdas = [];

    /// <summary>
    /// The password each lambda's role was given since the platform started.
    /// </summary>
    private readonly ConcurrentDictionary<long, string> _passwords = [];

    private readonly ConcurrentDictionary<DataScope, (long Size, long At)> _measured = [];

    private readonly ConcurrentDictionary<long, Lock> _turns = [];

    #region Get-/Setters

    private string RolePrefix { get; }

    private int PoolSize { get; }

    private ILogger Logger { get; }

    #endregion

    #region Initialization

    public PostgreSqlDatabaseProvider(NpgsqlConnectionStringBuilder connection, string rolePrefix, int poolSize, ILogger<PostgreSqlDatabaseProvider> logger)
    {
        _connection = connection;

        RolePrefix = rolePrefix;
        PoolSize = poolSize;
        Logger = logger;

        _platform = NpgsqlDataSource.Create(connection.ConnectionString);

        Secure();
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
    private void Secure()
    {
        try
        {
            Execute(_platform, "REVOKE CREATE ON SCHEMA public FROM PUBLIC");
        }
        catch (PostgresException e)
        {
            Logger.LogWarning("Failed to revoke CREATE on the schema public from PUBLIC ({Reason}): on PostgreSQL 14 and earlier, the roles of the lambdas may then create tables there",
                              e.MessageText);
        }
    }

    #endregion

    #region The platform's own

    public string Dialect => "postgresql";

    public void ConfigureStore(DbContextOptionsBuilder options, string store) => options.UseNpgsql(Store(store));

    public DbConnection Open(string store) => Store(store).OpenConnection();

    /// <summary>
    /// What a store is reached through: its schema, made where it is not
    /// there, and nothing else on the search path.
    /// </summary>
    private NpgsqlDataSource Store(string store) => _stores.GetOrAdd(store, name =>
    {
        Execute(_platform, $"CREATE SCHEMA IF NOT EXISTS {PostgreSqlNames.Quote(name)}");

        var connection = new NpgsqlConnectionStringBuilder(_connection.ConnectionString) { SearchPath = name };

        return NpgsqlDataSource.Create(connection.ConnectionString);
    });

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
    public void Create(DataScope scope)
    {
        var role = Prepare(scope.LambdaId);

        MakeSchema(PostgreSqlNames.Schema(scope), role);

        Logger.LogInformation("Created database of {Scope}", scope);
    }

    public bool Exists(DataScope scope) => SchemaExists(PostgreSqlNames.Schema(scope));

    public DbConnection Connect(DataScope scope, long room)
    {
        Prepare(scope.LambdaId);

        var writing = Measure(scope) < room;

        return Lambda(scope, writing).OpenConnection();
    }

    public DbConnection? Read(DataScope scope, TimeSpan patience)
    {
        if (!Exists(scope))
        {
            return null;
        }

        var role = Prepare(scope.LambdaId);

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
            opened.Open();
            return opened;
        }
        catch
        {
            opened.Dispose();
            throw;
        }
    }

    public long SizeOf(DataScope scope)
    {
        var size = Size(PostgreSqlNames.Schema(scope));

        _measured[scope] = (size, Stopwatch.GetTimestamp());

        return size;
    }

    /// <remarks>
    /// Made by the lambda's role in one transaction that sees the source as
    /// it was at its start, while the lambda goes on writing to it - see
    /// <see cref="SchemaCopy"/>.
    /// </remarks>
    public void Copy(DataScope source, DataScope target)
    {
        Delete(target);

        var from = PostgreSqlNames.Schema(source);

        if (!SchemaExists(from))
        {
            return;
        }

        var role = Prepare(source.LambdaId);

        var into = PostgreSqlNames.Schema(target);

        MakeSchema(into, role);

        using var connection = Lambda(source, true).OpenConnection();

        SchemaCopy.Run(connection, from, into);
    }

    public void Delete(DataScope scope)
    {
        Release(scope);

        Execute(_platform, $"DROP SCHEMA IF EXISTS {PostgreSqlNames.Quote(PostgreSqlNames.Schema(scope))} CASCADE");
    }

    /// <remarks>
    /// The schemas of its features are found by their names, which begin with
    /// the lambda's; its role goes with them.
    /// </remarks>
    public void DeleteAll(long lambdaId)
    {
        foreach (var scope in _lambdas.Keys.Select(k => k.Scope).Where(s => s.LambdaId == lambdaId).Distinct().ToList())
        {
            Release(scope);
        }

        var own = PostgreSqlNames.Schema(new DataScope(lambdaId));

        foreach (var schema in Schemas(own).Append(own).Distinct())
        {
            Execute(_platform, $"DROP SCHEMA IF EXISTS {PostgreSqlNames.Quote(schema)} CASCADE");
        }

        Execute(_platform, $"DROP ROLE IF EXISTS {PostgreSqlNames.Quote(PostgreSqlNames.Role(RolePrefix, lambdaId))}");

        _passwords.TryRemove(lambdaId, out _);
    }

    /// <summary>
    /// Nothing: an exported project keeps its records in a SQLite file, and
    /// what PostgreSQL holds is taken with its own tools - <c>pg_dump</c> of
    /// the lambda's schema.
    /// </summary>
    public string? Export(DataScope scope) => null;

    #endregion

    #region Looking into one

    public IReadOnlyList<DatabaseTableInfo> Tables(DbConnection connection)
    {
        var found = new List<DatabaseTableInfo>();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT c.relname, c.relkind IN ('v', 'm') FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = current_schema() AND c.relkind IN ('r', 'p', 'v', 'm', 'f')
            ORDER BY c.relkind IN ('v', 'm'), c.relname
            """;

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            found.Add(new DatabaseTableInfo(reader.GetString(0), reader.GetBoolean(1)));
        }

        return found;
    }

    public IReadOnlyList<DatabaseColumnInfo> Columns(DbConnection connection, string table)
    {
        var columns = new List<DatabaseColumnInfo>();

        using var command = connection.CreateCommand();

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

        using var reader = command.ExecuteReader();

        while (reader.Read())
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
    public string? NaturalOrder(DbConnection connection, string table)
    {
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT c.relkind = 'r' FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = current_schema() AND c.relname = @table
            """;

        Parameter(command, "@table", table);

        return command.ExecuteScalar() is true ? "ctid" : null;
    }

    public string Quote(string name) => PostgreSqlNames.Quote(name);

    #endregion

    #region Describing it

    public IntegrationInfo Describe() => new(IntegrationKinds.Database, "PostgreSQL",
                                             [
                                                 new IntegrationDetail("Server", _connection.Port is 5432 or 0 ? _connection.Host ?? "localhost" : $"{_connection.Host}:{_connection.Port}"),
                                                 new IntegrationDetail("Database", _connection.Database ?? ""),
                                                 new IntegrationDetail("User", _connection.Username ?? ""),
                                                 new IntegrationDetail("Schemas", "platform, analytics, lambda_{lambda}, lambda_{lambda}_feature_{draft}"),
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
    /// <returns>The name of the role</returns>
    private string Prepare(long lambdaId)
    {
        var role = PostgreSqlNames.Role(RolePrefix, lambdaId);

        if (_passwords.ContainsKey(lambdaId))
        {
            return role;
        }

        lock (_turns.GetOrAdd(lambdaId, _ => new Lock()))
        {
            if (_passwords.ContainsKey(lambdaId))
            {
                return role;
            }

            var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

            var quoted = PostgreSqlNames.Quote(role);

            // a role may not be made twice, and nothing makes one only where
            // it is missing - so this asks first, in one statement
            Execute(_platform, $"""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = {PostgreSqlNames.Literal(role)}) THEN
                        CREATE ROLE {quoted} LOGIN NOINHERIT NOCREATEDB NOCREATEROLE;
                    END IF;
                END
                $$
                """);

            Execute(_platform, $"ALTER ROLE {quoted} WITH LOGIN PASSWORD {PostgreSqlNames.Literal(password)}");

            _passwords[lambdaId] = password;

            return role;
        }
    }

    /// <summary>
    /// Makes a schema, owned by the platform, the role of the lambda using
    /// and creating in it.
    /// </summary>
    private void MakeSchema(string schema, string role)
    {
        var quoted = PostgreSqlNames.Quote(schema);

        Execute(_platform, $"CREATE SCHEMA IF NOT EXISTS {quoted}");
        Execute(_platform, $"GRANT USAGE, CREATE ON SCHEMA {quoted} TO {PostgreSqlNames.Quote(role)}");
    }

    private bool SchemaExists(string schema)
    {
        using var connection = _platform.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = @schema)";

        Parameter(command, "@schema", schema);

        return command.ExecuteScalar() is true;
    }

    /// <summary>
    /// The schemas whose names begin with the given one and a feature's mark.
    /// </summary>
    private List<string> Schemas(string own)
    {
        var found = new List<string>();

        using var connection = _platform.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT nspname FROM pg_namespace WHERE starts_with(nspname, @prefix)";

        Parameter(command, "@prefix", own + PostgreSqlNames.FeatureMark);

        using var reader = command.ExecuteReader();

        while (reader.Read())
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
    private long Measure(DataScope scope)
    {
        if (_measured.TryGetValue(scope, out var known) && Stopwatch.GetTimestamp() - known.At < Patience)
        {
            return known.Size;
        }

        return SizeOf(scope);
    }

    /// <summary>
    /// The room the tables of a schema take on the disk, their indexes and
    /// what they keep apart included.
    /// </summary>
    private long Size(string schema)
    {
        using var connection = _platform.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT COALESCE(SUM(pg_total_relation_size(c.oid)), 0)::bigint FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = @schema AND c.relkind IN ('r', 'm', 'p')
            """;

        Parameter(command, "@schema", schema);

        return command.ExecuteScalar() is long size ? size : 0;
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
    private void Release(DataScope scope)
    {
        foreach (var writing in new[] { true, false })
        {
            if (_lambdas.TryRemove((scope, writing), out var source))
            {
                source.Dispose();
            }
        }

        _measured.TryRemove(scope, out _);
    }

    private static void Execute(NpgsqlDataSource source, string sql)
    {
        using var command = source.CreateCommand(sql);

        command.ExecuteNonQuery();
    }

    private static void Parameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();

        parameter.ParameterName = name;
        parameter.Value = value;

        command.Parameters.Add(parameter);
    }

    public void Dispose()
    {
        foreach (var source in _lambdas.Values)
        {
            source.Dispose();
        }

        foreach (var source in _stores.Values)
        {
            source.Dispose();
        }

        _platform.Dispose();
    }

    #endregion

}
