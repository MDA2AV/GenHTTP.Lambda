namespace GenHTTP.Lambda.Integrations.Databases;

/// <summary>
/// How the code of a lambda is compiled against its database.
/// </summary>
/// <remarks>
/// A lambda is written against what every database has - a
/// <see cref="System.Data.Common.DbConnection"/> from <c>Database.GetConnection()</c>,
/// Entity Framework Core configured with <c>options.UseDatabase(connection)</c>,
/// Evolve - so the same code runs whichever provider keeps it. What a provider
/// adds here is for code written before that, against the provider of its
/// time, and is never what an agent is taught.
///
/// A client library of a database is handed to a lambda only where its
/// provider names it: one it could name would let it open connections of its
/// own, to anywhere.
/// </remarks>
/// <param name="Connection">
/// The type <c>Database.GetConnection()</c> is declared to return: a
/// <see cref="System.Data.Common.DbConnection"/>, or the provider's own
/// connection where code written before uses what only that has.
/// </param>
/// <param name="Assemblies">The names of the client libraries a lambda is compiled against, by their beginning</param>
/// <param name="Namespaces">What every file of a lambda imports of them</param>
public sealed record DatabaseBinding(Type Connection, IReadOnlyList<string> Assemblies, IReadOnlyList<string> Namespaces)
{

    /// <summary>
    /// A lambda that knows nothing of its database but what every database has.
    /// </summary>
    public static DatabaseBinding Neutral { get; } = new(typeof(System.Data.Common.DbConnection), [], []);

}
