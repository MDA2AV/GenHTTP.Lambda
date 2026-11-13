using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
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
                                IFeatureService features, ISecretVault secrets, IDatabaseVault stores, IDeploymentService deployments,
                                ILogger<DataService> logger) : IDataService
{

    #region Functionality

    public IReadOnlyList<DataStoreInfo> List(string privateKey, string? feature = null)
    {
        var id = RequireId(privateKey);

        long? featureId = feature != null ? (features.Require(privateKey, feature, false)).FeatureId : null;

        var switches = Read(id);

        var stores = new List<DataStoreInfo>();

        foreach (var kind in DataKinds.All)
        {
            stores.Add(Describe(id, kind, switches[kind.Id], featureId));
        }

        return stores;
    }

    public DataStoreInfo Get(string privateKey, string kind)
    {
        var wanted = DataKinds.Require(kind);

        var id = RequireId(privateKey);

        return Describe(id, wanted, (Read(id))[wanted.Id], null);
    }

    public DataStoreInfo Enable(string privateKey, string kind)
    {
        var wanted = DataKinds.Require(kind);

        var id = meta.RequireEditable(privateKey);

        if (!(Read(id))[wanted.Id].Enabled)
        {
            // there before it is switched on, so the first request finds it
            if (wanted.Id == DataKinds.DatabaseId)
            {
                stores.Create(id);
            }

            Switch(id, wanted, true);

            logger.LogInformation("Enabled {Kind} of lambda #{LambdaId}", wanted.Id, id);
        }

        return Describe(id, wanted, (Read(id))[wanted.Id], null);
    }

    public DataStoreInfo Disable(string privateKey, string kind)
    {
        var wanted = DataKinds.Require(kind);

        var id = meta.RequireEditable(privateKey);

        if ((Read(id))[wanted.Id].Enabled)
        {
            // switched off first, so nothing new arrives while the rest goes
            Switch(id, wanted, false);

            Clear(id, wanted);

            logger.LogInformation("Disabled and deleted {Kind} of lambda #{LambdaId}", wanted.Id, id);
        }

        return Describe(id, wanted, (Read(id))[wanted.Id], null);
    }

    #endregion

    #region Kinds

    /// <summary>
    /// What a kind of data holds, in the figures every kind can give.
    /// </summary>
    /// <param name="featureId">The feature whose copy is meant, or nothing for the lambda's own</param>
    private DataStoreInfo Describe(long lambdaId, DataKind kind, DataSwitch state, long? featureId)
    {
        switch (kind.Id)
        {
            case DataKinds.WorkspaceId:
                {
                    var listing = workspace.List(lambdaId, featureId);

                    return new DataStoreInfo(kind.Id, state.Enabled, kind.Default, state.Changed, listing.Files.Count, listing.UsedBytes, listing.QuotaBytes);
                }
            case DataKinds.SecretsId:
                {
                    var count = secrets.Count(lambdaId, featureId);

                    return new DataStoreInfo(kind.Id, state.Enabled, kind.Default, state.Changed, count, 0, 0, SecretVault.MaxSecrets);
                }
            case DataKinds.DatabaseId:
                {
                    var tier = Tier(lambdaId);

                    if (!state.Enabled)
                    {
                        return new DataStoreInfo(kind.Id, false, kind.Default, state.Changed, 0, 0, stores.RoomOf(lambdaId, featureId, tier));
                    }

                    // what it holds is counted in tables, and its room as the
                    // file takes it on the disk
                    var tables = CountTables(lambdaId, featureId);

                    return new DataStoreInfo(kind.Id, true, kind.Default, state.Changed, tables, stores.SizeOf(lambdaId, featureId), stores.RoomOf(lambdaId, featureId, tier));
                }
            default:
                return new DataStoreInfo(kind.Id, state.Enabled, kind.Default, state.Changed, 0, 0, 0);
        }
    }

    /// <summary>
    /// Deletes everything a kind of data holds.
    /// </summary>
    private void Clear(long lambdaId, DataKind kind)
    {
        if (kind.Id == DataKinds.SecretsId)
        {
            // the copies of the features with them, in one go
            secrets.Clear(lambdaId);
            return;
        }

        if (kind.Id == DataKinds.DatabaseId)
        {
            // the copies of the features with it
            stores.Clear(lambdaId);
            return;
        }

        if (kind.Id != DataKinds.WorkspaceId)
        {
            return;
        }

        workspace.Clear(lambdaId, null);

        // a copy is still the data, and switching it off leaves none of it
        using var database = databases.CreateDbContext();

        var copies = database.Features.AsNoTracking()
                             .Where(f => f.LambdaId == lambdaId)
                             .Select(f => f.Id)
                             .ToList();

        foreach (var copy in copies)
        {
            workspace.Clear(lambdaId, copy);
        }
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
    private int CountTables(long lambdaId, long? featureId)
    {
        using var connection = stores.OpenForReading(lambdaId, featureId);

        if (connection == null)
        {
            return 0;
        }

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT count(*) FROM pragma_table_list WHERE schema = 'main' AND type IN ('table', 'virtual') AND name NOT LIKE 'sqlite\\_%' ESCAPE '\\'";

        return Convert.ToInt32(command.ExecuteScalar());
    }

    #endregion

}
