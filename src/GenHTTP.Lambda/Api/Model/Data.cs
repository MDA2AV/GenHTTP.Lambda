namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// One kind of data of a lambda, as its owner sees it.
/// </summary>
/// <remarks>
/// Data belongs to the lambda rather than to a version: every version reads
/// and writes the same, deploying or rolling back leaves it alone, and it goes
/// only with the lambda - or when its owner switches the kind off, which
/// deletes what it held. The figures are the ones every kind can give, so a
/// kind added later is listed the same way.
/// </remarks>
/// <param name="Kind">Which kind of data: <c>workspace</c>, the files the lambda reads and writes</param>
/// <param name="Enabled">Whether the lambda has it</param>
/// <param name="Default">Whether a lambda has it until its owner decides otherwise</param>
/// <param name="Changed">When the owner last switched it; absent while it is as it came</param>
/// <param name="Items">What it holds: files, for the workspace</param>
/// <param name="UsedBytes">The room that takes, as the quota counts it</param>
/// <param name="QuotaBytes">The room the tier of the lambda gives it</param>
public sealed record DataStoreResponse(string Kind, bool Enabled, bool Default, DateTime? Changed, int Items, long UsedBytes, long QuotaBytes);
