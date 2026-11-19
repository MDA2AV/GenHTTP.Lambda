using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Data;

/// <summary>
/// Keeps which kinds of data each lambda has, and measures what they hold.
/// </summary>
public sealed class DataService(IDbContextFactory<LambdaDbContext> databases, IMetaService meta, IWorkspaceService workspace,
                                IFeatureService features, ISecretVault secrets, IDatabaseVault stores, IDatabaseProvider provider, IDeploymentService deployments,
                                ILogger<DataService> logger) : IDataService
{

    #region Functionality

    public async ValueTask<IReadOnlyList<DataStoreInfo>> ListAsync(string privateKey, string? feature = null, CancellationToken cancellation = default)
    {
        var id = RequireId(privateKey);

        long? featureId = feature != null ? (features.Require(privateKey, feature, false)).FeatureId : null;

        var switches = Read(id);

        var stores = new List<DataStoreInfo>();

        foreach (var kind in DataKinds.All)
        {
            stores.Add(await DescribeAsync(id, kind, switches[kind.Id], featureId, cancellation));
        }

        return stores;
    }

    public async ValueTask<DataStoreInfo> GetAsync(string privateKey, string kind, CancellationToken cancellation = default)
    {
        var wanted = DataKinds.Require(kind);

        var id = RequireId(privateKey);

        return await DescribeAsync(id, wanted, (Read(id))[wanted.Id], null, cancellation);
    }

    public async ValueTask<DataStoreInfo> EnableAsync(string privateKey, string kind, CancellationToken cancellation = default)
    {
        var wanted = DataKinds.Require(kind);

        var id = meta.RequireEditable(privateKey);

        if (!(Read(id))[wanted.Id].Enabled)
        {
            // there before it is switched on, so the first request finds it
            if (wanted.Id == DataKinds.DatabaseId)
            {
                await stores.CreateAsync(id, cancellation);
            }

            Switch(id, wanted, true);

            logger.LogInformation("Enabled {Kind} of lambda #{LambdaId}", wanted.Id, id);
        }

        return await DescribeAsync(id, wanted, (Read(id))[wanted.Id], null, cancellation);
    }

    public async ValueTask<DataStoreInfo> DisableAsync(string privateKey, string kind, CancellationToken cancellation = default)
    {
        var wanted = DataKinds.Require(kind);

        var id = meta.RequireEditable(privateKey);

        if ((Read(id))[wanted.Id].Enabled)
        {
            // switched off first, so nothing new arrives while the rest goes
            Switch(id, wanted, false);

            await ClearAsync(id, wanted, cancellation);

            logger.LogInformation("Disabled and deleted {Kind} of lambda #{LambdaId}", wanted.Id, id);
        }

        return await DescribeAsync(id, wanted, (Read(id))[wanted.Id], null, cancellation);
    }

    #endregion

    #region Kinds

    /// <summary>
    /// What a kind of data holds, in the figures every kind can give.
    /// </summary>
    /// <param name="featureId">The feature whose copy is meant, or nothing for the lambda's own</param>
    private async ValueTask<DataStoreInfo> DescribeAsync(long lambdaId, DataKind kind, DataSwitch state, long? featureId, CancellationToken cancellation)
    {
        switch (kind.Id)
        {
            case DataKinds.WorkspaceId:
                {
                    var listing = await workspace.ListAsync(lambdaId, featureId, cancellation);

                    return new DataStoreInfo(kind.Id, state.Enabled, kind.Default, state.Changed, listing.Files.Count, listing.UsedBytes, listing.QuotaBytes);
                }
            case DataKinds.SecretsId:
                {
                    var count = await secrets.CountAsync(lambdaId, featureId, cancellation);

                    return new DataStoreInfo(kind.Id, state.Enabled, kind.Default, state.Changed, count, 0, 0, SecretVault.MaxSecrets);
                }
            case DataKinds.DatabaseId:
                {
                    var tier = Tier(lambdaId);

                    var room = await stores.RoomOfAsync(lambdaId, featureId, tier, cancellation);

                    if (!state.Enabled)
                    {
                        return new DataStoreInfo(kind.Id, false, kind.Default, state.Changed, 0, 0, room);
                    }

                    // what it holds is counted in tables, and its room as the
                    // database takes it
                    var tables = await CountTablesAsync(lambdaId, featureId, cancellation);

                    return new DataStoreInfo(kind.Id, true, kind.Default, state.Changed, tables, await stores.SizeOfAsync(lambdaId, featureId, cancellation), room);
                }
            default:
                return new DataStoreInfo(kind.Id, state.Enabled, kind.Default, state.Changed, 0, 0, 0);
        }
    }

    /// <summary>
    /// Deletes everything a kind of data holds.
    /// </summary>
    private async ValueTask ClearAsync(long lambdaId, DataKind kind, CancellationToken cancellation)
    {
        if (kind.Id == DataKinds.SecretsId)
        {
            // the copies of the features with them, in one go
            await secrets.ClearAsync(lambdaId, cancellation);
            return;
        }

        if (kind.Id == DataKinds.DatabaseId)
        {
            // the copies of the features with it
            await stores.ClearAsync(lambdaId, cancellation);
            return;
        }

        if (kind.Id != DataKinds.WorkspaceId)
        {
            return;
        }

        await workspace.ClearAsync(lambdaId, null, cancellation);

        // a copy is still the data, and switching it off leaves none of it
        foreach (var copy in Copies(lambdaId))
        {
            await workspace.ClearAsync(lambdaId, copy, cancellation);
        }
    }

    /// <summary>
    /// The features of a lambda, each of which has a copy of its data.
    /// </summary>
    private List<long> Copies(long lambdaId)
    {
        using var database = databases.CreateDbContext();

        return database.Features.AsNoTracking()
                       .Where(f => f.LambdaId == lambdaId)
                       .Select(f => f.Id)
                       .ToList();
    }

    #endregion

    #region Helpers

    private long RequireId(string privateKey)
        => meta.GetId(privateKey) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

    private IReadOnlyDictionary<string, DataSwitch> Read(long lambdaId)
    {
        using var database = databases.CreateDbContext();

        return DataSwitches.Read(database, lambdaId);
    }

    private void Switch(long lambdaId, DataKind kind, bool enabled)
    {
        using var database = databases.CreateDbContext();

        var row = database.DataStores.FirstOrDefault(s => s.LambdaId == lambdaId && s.Kind == kind.Id);

        if (row == null)
        {
            database.DataStores.Add(new DataStoreEntity { LambdaId = lambdaId, Kind = kind.Id, Enabled = enabled, Changed = DateTime.UtcNow });
        }
        else
        {
            row.Enabled = enabled;
            row.Changed = DateTime.UtcNow;
        }

        database.SaveChanges();

        // a lambda reads whether it has secrets and a database as it uses them
        secrets.Invalidate(lambdaId);
        stores.Invalidate(lambdaId);

        // and it prepares its database while it starts - migrating it, most
        // often - so a database that came or went is started with again, the
        // previews of its features included, on the next request each gets
        if (kind.Id == DataKinds.DatabaseId)
        {
            deployments.EvictAll(lambdaId);
        }
    }

    private LambdaTier Tier(long lambdaId)
    {
        using var database = databases.CreateDbContext();

        return database.Lambdas.AsNoTracking().Where(l => l.Id == lambdaId).Select(l => l.Tier).FirstOrDefault();
    }

    /// <summary>
    /// How many tables the app keeps in its database - Evolve's history of
    /// its migrations among them, since it is a table the app made.
    /// </summary>
    private async ValueTask<int> CountTablesAsync(long lambdaId, long? featureId, CancellationToken cancellation)
    {
        await using var connection = await stores.OpenForReadingAsync(lambdaId, featureId, cancellation);

        if (connection == null)
        {
            return 0;
        }

        return (await provider.TablesAsync(connection, cancellation)).Count(t => !t.View);
    }

    #endregion

}
