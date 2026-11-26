using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Agents;
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
                                IAgentJournal journal, ILogger<DataService> logger) : IDataService
{

    #region Functionality

    public async ValueTask<IReadOnlyList<DataStoreInfo>> ListAsync(string privateKey, string? feature = null, CancellationToken cancellation = default)
    {
        var id = await RequireIdAsync(privateKey, cancellation);

        long? featureId = feature != null ? (await features.RequireAsync(privateKey, feature, false, cancellation)).FeatureId : null;

        var switches = await ReadAsync(id, cancellation);

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

        var id = await RequireIdAsync(privateKey, cancellation);

        return await DescribeAsync(id, wanted, (await ReadAsync(id, cancellation))[wanted.Id], null, cancellation);
    }

    public async ValueTask<DataStoreInfo> EnableAsync(string privateKey, string kind, VersionAuthor? author = null, CancellationToken cancellation = default)
    {
        var wanted = DataKinds.Require(kind);

        var id = await meta.RequireEditableAsync(privateKey, cancellation);

        if (!(await ReadAsync(id, cancellation))[wanted.Id].Enabled)
        {
            // there before it is switched on, so the first request finds it
            if (wanted.Id == DataKinds.DatabaseId)
            {
                await stores.CreateAsync(id, cancellation);
            }

            // and switched on once it is there, whoever cancels
            await SwitchAsync(id, wanted, true);

            logger.LogInformation("Enabled {Kind} of lambda #{LambdaId}", wanted.Id, id);

            await journal.RecordAsync(author, new AgentEffect(AgentEffectKind.Enabled, id, Name: wanted.Id), CancellationToken.None);
        }

        return await DescribeAsync(id, wanted, (await ReadAsync(id, CancellationToken.None))[wanted.Id], null, CancellationToken.None);
    }

    public async ValueTask<DataStoreInfo> DisableAsync(string privateKey, string kind, CancellationToken cancellation = default)
    {
        var wanted = DataKinds.Require(kind);

        var id = await meta.RequireEditableAsync(privateKey, cancellation);

        if ((await ReadAsync(id, cancellation))[wanted.Id].Enabled)
        {
            // switched off first, so nothing new arrives while the rest goes -
            // and the rest goes whoever cancels, or it would stay behind unseen
            await SwitchAsync(id, wanted, false);

            await ClearAsync(id, wanted, CancellationToken.None);

            logger.LogInformation("Disabled and deleted {Kind} of lambda #{LambdaId}", wanted.Id, id);
        }

        return await DescribeAsync(id, wanted, (await ReadAsync(id, CancellationToken.None))[wanted.Id], null, CancellationToken.None);
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
                    var tier = await TierAsync(lambdaId, cancellation);

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
        foreach (var copy in await CopiesAsync(lambdaId, cancellation))
        {
            await workspace.ClearAsync(lambdaId, copy, cancellation);
        }
    }

    /// <summary>
    /// The features of a lambda, each of which has a copy of its data.
    /// </summary>
    private async ValueTask<List<long>> CopiesAsync(long lambdaId, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.Features.AsNoTracking()
                             .Where(f => f.LambdaId == lambdaId)
                             .Select(f => f.Id)
                             .ToListAsync(cancellation);
    }

    #endregion

    #region Helpers

    private async ValueTask<long> RequireIdAsync(string privateKey, CancellationToken cancellation)
        => await meta.GetIdAsync(privateKey, cancellation) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

    private async ValueTask<IReadOnlyDictionary<string, DataSwitch>> ReadAsync(long lambdaId, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await DataSwitches.ReadAsync(database, lambdaId, cancellation);
    }

    /// <remarks>
    /// Without a cancellation token: it follows what was begun - a database
    /// made, data about to be deleted - and is written whoever leaves.
    /// </remarks>
    private async ValueTask SwitchAsync(long lambdaId, DataKind kind, bool enabled)
    {
        await using var database = await databases.CreateDbContextAsync();

        var row = await database.DataStores.FirstOrDefaultAsync(s => s.LambdaId == lambdaId && s.Kind == kind.Id);

        if (row == null)
        {
            database.DataStores.Add(new DataStoreEntity { LambdaId = lambdaId, Kind = kind.Id, Enabled = enabled, Changed = DateTime.UtcNow });
        }
        else
        {
            row.Enabled = enabled;
            row.Changed = DateTime.UtcNow;
        }

        await database.SaveChangesAsync();

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

    private async ValueTask<LambdaTier> TierAsync(long lambdaId, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.Lambdas.AsNoTracking().Where(l => l.Id == lambdaId).Select(l => l.Tier).FirstOrDefaultAsync(cancellation);
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
