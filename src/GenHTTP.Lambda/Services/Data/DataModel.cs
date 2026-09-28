namespace GenHTTP.Lambda.Services.Data;

/// <summary>
/// One kind of data of one lambda: whether it is switched on, and how much of
/// it there is.
/// </summary>
/// <remarks>
/// The figures are the ones every kind can give - how many things it holds,
/// the room they take and how much room there is - so a kind added later is
/// listed the same way without the editor learning about it first.
/// </remarks>
/// <param name="Kind">Which kind of data, one of <see cref="DataKinds" /></param>
/// <param name="Enabled">Whether the lambda has it</param>
/// <param name="Default">Whether a lambda has it until its owner decides</param>
/// <param name="Changed">When the owner last switched it, or nothing if it is as it was by default</param>
/// <param name="Items">What it holds: files for the workspace</param>
/// <param name="UsedBytes">The room that takes</param>
/// <param name="QuotaBytes">The room the lambda's tier gives it</param>
public sealed record DataStoreInfo(string Kind, bool Enabled, bool Default, DateTime? Changed, int Items, long UsedBytes, long QuotaBytes);

/// <summary>
/// Whether a lambda has a kind of data, as the owner left it.
/// </summary>
/// <param name="Changed">When the owner last switched it, or nothing if it is as it was by default</param>
public sealed record DataSwitch(bool Enabled, DateTime? Changed);
