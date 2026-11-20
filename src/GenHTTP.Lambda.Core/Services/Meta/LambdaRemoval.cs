using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Analytics;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Removes a lambda and everything that hangs on it: its rows, its files,
/// its database, and what the caches remember of it.
/// </summary>
/// <remarks>
/// One place for it, because a lambda goes away two ways - its owner deletes
/// it, or the sweep finds it abandoned - and both have to leave nothing behind.
/// So it is seen through once begun, whoever asked and however they fare: a
/// lambda whose row went and whose data stayed behind would hold its data for
/// nobody, and nothing here takes a token to be cancelled with.
/// </remarks>
public sealed class LambdaRemoval(IDeploymentService deployments, ITelemetryService telemetry, LambdaHistory history, ISecretVault secrets,
                                  IDatabaseVault databaseVault, IWorkspaceVault workspaces, IShowcaseService showcases, IDomainRegistry domains, IStorageService storage,
                                  IAnalyticsStore analytics)
{

    public async ValueTask RemoveAsync(LambdaDbContext database, LambdaEntity lambda)
    {
        // with every preview of its features, which go with it
        deployments.EvictAll(lambda.Id);

        // a deleted lambda takes its numbers with it rather than leaving a row
        // in the activity list that nothing can be looked up from any more
        telemetry.ForgetLambda(lambda.Id);

        await history.RecordAsync(database, lambda, LambdaEvents.Deleted);

        // the key cascades in the schema, but only where the connection has
        // foreign keys switched on - said here so it does not depend on that
        await database.Activations.Where(a => a.LambdaId == lambda.Id).ExecuteDeleteAsync();

        // its entry and its picture, wherever the files are kept
        await showcases.ForgetAsync(lambda.Id);

        await database.DataStores.Where(s => s.LambdaId == lambda.Id).ExecuteDeleteAsync();

        // wherever they are kept, before the lambda they belong to goes
        await secrets.ClearAsync(lambda.Id);

        await database.Features.Where(f => f.LambdaId == lambda.Id).ExecuteDeleteAsync();

        database.Lambdas.Remove(lambda);

        await database.SaveChangesAsync();

        secrets.Invalidate(lambda.Id);

        // its data, wherever it is kept, with the copies of its features
        await databaseVault.ClearAsync(lambda.Id);

        await workspaces.DeleteAllAsync(lambda.Id);

        if (lambda.Domain != null)
        {
            await domains.ReloadAsync();
        }

        storage.Delete(lambda.Id);

        // what its owner counted of its visitors goes with it
        await analytics.ForgetAsync(lambda.Id);
    }

}
