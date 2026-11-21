using System.Data.Common;

using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// What the database class generated into a lambda calls, once the lambda is
/// loaded: one function that opens a connection to its database.
/// </summary>
/// <remarks>
/// Both forms, since the lambda keeps both: <c>Database.GetConnectionAsync()</c>,
/// which is what agents are taught, and <c>Database.GetConnection()</c>, which
/// code written before calls - and which waits where the database does not
/// answer at once.
/// </remarks>
/// <param name="Connect">The plain call: <c>Database.GetConnection()</c></param>
/// <param name="ConnectAsync">The awaited one: <c>Database.GetConnectionAsync()</c></param>
public sealed record DatabaseConnector(Func<DbConnection> Connect, Func<CancellationToken, ValueTask<DbConnection>> ConnectAsync);
