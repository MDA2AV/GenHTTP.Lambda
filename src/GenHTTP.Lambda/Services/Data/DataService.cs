using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Data;

/// <summary>
/// Keeps which kinds of data each lambda has, and measures what they hold.
/// </summary>
public sealed class DataService(IDbContextFactory<LambdaDbContext> databases, IMetaService meta, IWorkspaceService workspace,
                                ILogger<DataService> logger) : IDataService
{

    #region Functionality

    public async ValueTask<IReadOnlyList<DataStoreInfo>> ListAsync(string privateKey, CancellationToken cancellation = default)
    {
        var id = await RequireIdAsync(privateKey, cancellation);

        var switches = await ReadAsync(id, cancellation);

        var stores = new List<DataStoreInfo>();

        foreach (var kind in DataKinds.All)
        {
            stores.Add(await DescribeAsync(id, kind, switches[kind.Id], cancellation));
        }

        return stores;
    }

    public async ValueTask<DataStoreInfo> GetAsync(string privateKey, string kind, CancellationToken cancellation = default)
    {
        var wanted = DataKinds.Require(kind);

        var id = await RequireIdAsync(privateKey, cancellation);

        return await DescribeAsync(id, wanted, (await ReadAsync(id, cancellation))[wanted.Id], cancellation);
    }

    public async ValueTask<DataStoreInfo> EnableAsync(string privateKey, string kind, CancellationToken cancellation = default)
    {
        var wanted = DataKinds.Require(kind);

        var id = await meta.RequireEditableAsync(privateKey, cancellation);

        if (!(await ReadAsync(id, cancellation))[wanted.Id].Enabled)
        {
            await SwitchAsync(id, wanted, true, cancellation);

            logger.LogInformation("Lambda {LambdaId} switched its {Kind} on", id, wanted.Id);
        }

        return await DescribeAsync(id, wanted, (await ReadAsync(id, cancellation))[wanted.Id], cancellation);
    }

    public async ValueTask<DataStoreInfo> DisableAsync(string privateKey, string kind, CancellationToken cancellation = default)
    {
        var wanted = DataKinds.Require(kind);

        var id = await meta.RequireEditableAsync(privateKey, cancellation);

        if ((await ReadAsync(id, cancellation))[wanted.Id].Enabled)
        {
            // switched off first, so nothing new arrives while the rest goes
            await SwitchAsync(id, wanted, false, cancellation);

            await ClearAsync(id, wanted, cancellation);

            logger.LogInformation("Lambda {LambdaId} switched its {Kind} off, and what it held was deleted", id, wanted.Id);
        }

        return await DescribeAsync(id, wanted, (await ReadAsync(id, cancellation))[wanted.Id], cancellation);
    }

    #endregion

    #region Kinds

    /// <summary>
    /// What a kind of data holds, in the figures every kind can give.
    /// </summary>
    private async ValueTask<DataStoreInfo> DescribeAsync(long lambdaId, DataKind kind, DataSwitch state, CancellationToken cancellation)
    {
        switch (kind.Id)
        {
            case DataKinds.WorkspaceId:
                {
                    var listing = await workspace.ListAsync(lambdaId, cancellation);

                    return new DataStoreInfo(kind.Id, state.Enabled, kind.Default, state.Changed, listing.Files.Count, listing.UsedBytes, listing.QuotaBytes);
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
        if (kind.Id == DataKinds.WorkspaceId)
        {
            await workspace.ClearAsync(lambdaId, cancellation);
        }
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

    private async ValueTask SwitchAsync(long lambdaId, DataKind kind, bool enabled, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var row = await database.DataStores.FirstOrDefaultAsync(s => s.LambdaId == lambdaId && s.Kind == kind.Id, cancellation);

        if (row == null)
        {
            database.DataStores.Add(new DataStoreEntity { LambdaId = lambdaId, Kind = kind.Id, Enabled = enabled, Changed = DateTime.UtcNow });
        }
        else
        {
            row.Enabled = enabled;
            row.Changed = DateTime.UtcNow;
        }

        await database.SaveChangesAsync(cancellation);
    }

    #endregion

}
