using System.Data.Common;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Integrations.Databases;

/// <summary>
/// The relational database of an installation: what the platform keeps its
/// own records in, and what every lambda that keeps records has one of.
/// </summary>
/// <remarks>
/// <para>
/// The platform's own records are in named stores - <see cref="DatabaseStores.Platform"/>
/// and <see cref="DatabaseStores.Analytics"/> - whose schema the platform
/// migrates itself, in the <see cref="Dialect"/> the provider says it speaks.
/// A lambda's database, and the copy a feature works on, is a database of its
/// own however the provider makes one: a file, a schema, a database on a
/// server. Whatever it is, a lambda reaches only its own.
/// </para>
/// <para>
/// Whether a lambda has a database, how large it may grow and who may read
/// it is decided by the platform before it asks; what is asked here is only
/// ever done. Synchronous, as the platform's services are: a call to a
/// database on another machine waits on the network, which the operator who
/// chooses one accepts.
/// </para>
/// </remarks>
public interface IDatabaseProvider : IIntegration
{

    #region The platform's own

    /// <summary>
    /// The SQL the provider speaks, which decides the migrations the
    /// platform's own stores are made with: <c>sqlite</c> or <c>postgresql</c>.
    /// </summary>
    /// <remarks>
    /// A provider of a dialect the platform has no migrations for is refused
    /// as the platform starts.
    /// </remarks>
    string Dialect { get; }

    /// <summary>
    /// Configures a context of Entity Framework for one of the platform's
    /// stores, as it is pooled for the life of the platform.
    /// </summary>
    void ConfigureStore(DbContextOptionsBuilder options, string store);

    /// <summary>
    /// Opens a connection to one of the platform's stores - made where it is
    /// not there yet - for its migrations and for plain commands.
    /// </summary>
    /// <remarks>
    /// The caller disposes of it. Commands on it are written in the
    /// <see cref="Dialect"/>, with parameters named <c>@name</c>.
    /// </remarks>
    DbConnection Open(string store);

    #endregion

    #region A lambda's

    /// <summary>
    /// How the code of a lambda is compiled against its database.
    /// </summary>
    DatabaseBinding Binding { get; }

    /// <summary>
    /// Configures a context of Entity Framework a lambda makes on the
    /// connection it was handed - what <c>options.UseDatabase(connection)</c>
    /// does in its code - with the context owning the connection.
    /// </summary>
    void ConfigureContext(DbContextOptionsBuilder options, DbConnection connection);

    /// <summary>
    /// Makes the database of a lambda, empty, where there is none.
    /// </summary>
    void Create(DataScope scope);

    /// <summary>
    /// Whether the database is there.
    /// </summary>
    bool Exists(DataScope scope);

    /// <summary>
    /// Opens a connection a running lambda reads and writes its database
    /// with, and that reaches nothing else.
    /// </summary>
    /// <remarks>
    /// Opened for every request that asks, so the provider pools them. The
    /// lambda disposes of it.
    /// </remarks>
    /// <param name="room">
    /// How large the database may grow, in bytes. A write past it is refused,
    /// or - where the database cannot refuse one - a connection opened while
    /// it is past it only reads.
    /// </param>
    DbConnection Connect(DataScope scope, long room);

    /// <summary>
    /// Opens a connection that only reads, for the owner and the agents to
    /// look into a database - or nothing where there is none.
    /// </summary>
    /// <param name="patience">How long a statement may run before it is stopped</param>
    DbConnection? Read(DataScope scope, TimeSpan patience);

    /// <summary>
    /// The room the database takes, in bytes, which counts against the data
    /// of its lambda.
    /// </summary>
    long SizeOf(DataScope scope);

    /// <summary>
    /// Replaces the database of one scope with a copy of another's, as it
    /// was at one moment while it may be written to - or removes it where
    /// the other has none.
    /// </summary>
    /// <remarks>
    /// A whole database, so the platform asks for it away from the thread
    /// a request is served on.
    /// </remarks>
    void Copy(DataScope source, DataScope target);

    /// <summary>
    /// Deletes a database, letting go of the connections to it first. A
    /// database made later in its place is a new one.
    /// </summary>
    void Delete(DataScope scope);

    /// <summary>
    /// Deletes the database of a lambda and every copy of its features.
    /// </summary>
    void DeleteAll(long lambdaId);

    /// <summary>
    /// Writes what a lambda's database holds into a SQLite file of its own,
    /// which is what an exported project keeps its records in - or nothing
    /// where it has none, or the provider cannot.
    /// </summary>
    /// <returns>Where the file was written, for the caller to delete</returns>
    string? Export(DataScope scope);

    #endregion

    #region Looking into one

    /// <summary>
    /// The tables and views a database holds that its lambda made - not
    /// what the database keeps for itself - views last.
    /// </summary>
    IReadOnlyList<DatabaseTableInfo> Tables(DbConnection connection);

    /// <summary>
    /// The columns of a table or view, in their order - none where there is
    /// no table of that name.
    /// </summary>
    IReadOnlyList<DatabaseColumnInfo> Columns(DbConnection connection, string table);

    /// <summary>
    /// What a table's rows are ordered by where nobody chose a column -
    /// the order they were written in, where the database keeps one - or
    /// nothing.
    /// </summary>
    string? NaturalOrder(DbConnection connection, string table);

    /// <summary>
    /// A name as the dialect quotes it in a statement.
    /// </summary>
    string Quote(string name);

    #endregion

}

/// <summary>
/// The stores the platform keeps its own records in.
/// </summary>
public static class DatabaseStores
{

    /// <summary>
    /// The lambdas, their versions, features, settings and secrets.
    /// </summary>
    public const string Platform = "platform";

    /// <summary>
    /// What was counted of the visits to lambdas, kept apart so writing a
    /// batch of counts never holds up the platform's own records.
    /// </summary>
    public const string Analytics = "analytics";

}

/// <summary>
/// A table or a view of a lambda's database.
/// </summary>
public sealed record DatabaseTableInfo(string Name, bool View);

/// <summary>
/// A column of a table, as it was declared.
/// </summary>
/// <param name="Type">The type it was declared with, as the database names it</param>
/// <param name="Default">Its default, as SQL, if it has one</param>
public sealed record DatabaseColumnInfo(string Name, string Type, bool NotNull, bool PrimaryKey, string? Default);
