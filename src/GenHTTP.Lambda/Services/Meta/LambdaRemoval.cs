using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Telemetry;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Removes a lambda and everything that hangs on it: its rows, its files,
/// its database, and what the caches remember of it.
/// </summary>
/// <remarks>
/// One place for it, because a lambda goes away two ways - its owner deletes
/// it, or the sweep finds it abandoned - and both have to leave nothing behind.
/// </remarks>
public sealed class LambdaRemoval(IDeploymentService deployments, LambdaTelemetry activity, LambdaHistory history, SecretVault secrets,
                                  DatabaseVault databaseVault, DomainRegistry domains, IStorageService storage)
{

    public void Remove(LambdaDbContext database, LambdaEntity lambda)
    {
        // with every preview of its features, which go with it
        deployments.EvictAll(lambda.Id);

        // a deleted lambda takes its numbers with it rather than leaving a row
        // in the activity list that nothing can be looked up from any more
        activity.Evict(lambda.Id);

        history.Record(database, lambda, LambdaEvents.Deleted);

        // the key cascades in the schema, but only where the connection has
        // foreign keys switched on - said here so it does not depend on that
        database.Activations.Where(a => a.LambdaId == lambda.Id).ExecuteDelete();

        database.Showcases.Where(s => s.LambdaId == lambda.Id).ExecuteDelete();

        database.DataStores.Where(s => s.LambdaId == lambda.Id).ExecuteDelete();

        database.Secrets.Where(s => s.LambdaId == lambda.Id).ExecuteDelete();

        database.Features.Where(f => f.LambdaId == lambda.Id).ExecuteDelete();

        database.Lambdas.Remove(lambda);

        database.SaveChanges();

        secrets.Invalidate(lambda.Id);

        // its connections let go of before its files go
        databaseVault.Forget(lambda.Id);

        if (lambda.Domain != null)
        {
            domains.Reload();
        }

        storage.Delete(lambda.Id);
    }

}
