namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// The database of a lambda, or a feature's copy of it, as its owner sees it.
/// </summary>
/// <param name="Enabled">Whether the lambda has its database switched on</param>
/// <param name="UsedBytes">The room it takes on disk, its journal included</param>
/// <param name="QuotaBytes">How large it may grow, which the tier of the lambda decides</param>
/// <param name="Tables">What it holds, tables first and then views, by name</param>
/// <param name="Used">Whether the code calls <c>Database.GetConnection()</c> - the version online and the newest one, or the feature's files</param>
public sealed record DatabaseOverview(bool Enabled, long UsedBytes, long QuotaBytes, IReadOnlyList<DatabaseTable> Tables, bool Used);
