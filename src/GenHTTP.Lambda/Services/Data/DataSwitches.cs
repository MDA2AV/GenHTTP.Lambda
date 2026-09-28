using GenHTTP.Lambda.Data;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Data;

/// <summary>
/// Reads which kinds of data a lambda has, from the context of whoever asks.
/// </summary>
/// <remarks>
/// Asked on every request a lambda serves, since whether its workspace is on
/// is compiled into it - so it is a lookup by key in the context the request
/// already has open rather than a service with a connection of its own.
/// </remarks>
internal static class DataSwitches
{

    /// <summary>
    /// Whether the lambda has the kind of data: as its owner switched it, or
    /// as the kind comes by default if they never did.
    /// </summary>
    public static async ValueTask<bool> IsEnabledAsync(LambdaDbContext database, long lambdaId, DataKind kind, CancellationToken cancellation = default)
    {
        var chosen = await database.DataStores.AsNoTracking()
                                   .Where(s => s.LambdaId == lambdaId && s.Kind == kind.Id)
                                   .Select(s => (bool?)s.Enabled)
                                   .FirstOrDefaultAsync(cancellation);

        return chosen ?? kind.Default;
    }

    /// <summary>
    /// How the lambda has every kind of data there is.
    /// </summary>
    public static async ValueTask<IReadOnlyDictionary<string, DataSwitch>> ReadAsync(LambdaDbContext database, long lambdaId, CancellationToken cancellation = default)
    {
        var chosen = await database.DataStores.AsNoTracking()
                                   .Where(s => s.LambdaId == lambdaId)
                                   .ToDictionaryAsync(s => s.Kind, s => new DataSwitch(s.Enabled, s.Changed), cancellation);

        return DataKinds.All.ToDictionary(k => k.Id, k => chosen.GetValueOrDefault(k.Id) ?? new DataSwitch(k.Default, null));
    }

}
