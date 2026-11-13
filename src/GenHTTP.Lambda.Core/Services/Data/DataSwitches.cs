using GenHTTP.Lambda.Data;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Data;

/// <summary>
/// Reads which kinds of data a lambda has, from the context of whoever asks.
/// </summary>
/// <remarks>
/// Asked whenever a lambda is looked up to be served, since whether its
/// workspace is on is compiled into it - so it is a lookup by key in the
/// context the caller already has open rather than a service with a
/// connection of its own.
/// </remarks>
internal static class DataSwitches
{

    /// <summary>
    /// Whether the lambda has the kind of data: as its owner switched it, or
    /// as the kind comes by default if they never did.
    /// </summary>
    public static bool IsEnabled(LambdaDbContext database, long lambdaId, DataKind kind)
    {
        var chosen = database.DataStores.AsNoTracking()
                             .Where(s => s.LambdaId == lambdaId && s.Kind == kind.Id)
                             .Select(s => (bool?)s.Enabled)
                             .FirstOrDefault();

        return chosen ?? kind.Default;
    }

    /// <summary>
    /// How the lambda has every kind of data there is.
    /// </summary>
    public static IReadOnlyDictionary<string, DataSwitch> Read(LambdaDbContext database, long lambdaId)
    {
        var chosen = database.DataStores.AsNoTracking()
                             .Where(s => s.LambdaId == lambdaId)
                             .ToDictionary(s => s.Kind, s => new DataSwitch(s.Enabled, s.Changed));

        return DataKinds.All.ToDictionary(k => k.Id, k => chosen.GetValueOrDefault(k.Id) ?? new DataSwitch(k.Default, null));
    }

}
