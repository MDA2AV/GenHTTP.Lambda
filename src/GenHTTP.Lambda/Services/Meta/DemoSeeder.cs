using System.Security.Cryptography;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Secrets;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Keeps the demos in existence, online and read only.
/// </summary>
/// <remarks>
/// This runs after the server is already answering rather than before, because
/// every demo has to be compiled and that is seconds the installation would
/// otherwise spend refusing connections. A demo is briefly absent after a
/// restart, which is a better failure than a server that is not up yet.
///
/// It is written to be run again at any time: a demo whose template has
/// changed is given the new code and redeployed, and one that is already
/// current is left alone.
///
/// The rows are written here directly rather than through the meta service,
/// because a demo is the one kind of lambda whose editor key is chosen - it
/// is its public key, announced - and whose tier nobody else may set.
/// </remarks>
public sealed class DemoSeeder(IMetaService meta, IDbContextFactory<LambdaDbContext> databases, SecretVault secrets, DatabaseVault stores,
                               ILogger<DemoSeeder> logger)
{

    /// <summary>
    /// Brings every demo up to date with the template it comes from.
    /// </summary>
    public async ValueTask SeedAsync(CancellationToken cancellation = default)
    {
        var seeded = 0;

        foreach (var demo in DemoCatalog.All)
        {
            try
            {
                if (await SeedAsync(demo, cancellation))
                {
                    seeded++;
                }
            }
            catch (Exception e)
            {
                // one demo that will not compile is not a reason for the
                // installation to be without the others
                logger.LogWarning(e, "The demo '{Demo}' could not be prepared", demo.Id);
            }
        }

        if (seeded > 0)
        {
            logger.LogInformation("Prepared {Count} demo(s)", seeded);
        }

        await RetireAsync(cancellation);
    }

    /// <summary>
    /// Removes lambdas that were demos and are not any more.
    /// </summary>
    /// <remarks>
    /// A demo is exempt from both maintenance sweeps, so one dropped from the
    /// catalogue would otherwise sit on its key for ever, answering with
    /// something nobody maintains. The examples this tier replaced leave the
    /// same way. Only the tier is gone by: nothing else is ever put into it.
    /// </remarks>
    private async ValueTask RetireAsync(CancellationToken cancellation)
    {
        var wanted = DemoCatalog.All.Select(e => e.Key).ToHashSet(StringComparer.Ordinal);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var retired = await database.Lambdas.Where(l => l.Tier == LambdaTier.Demo)
                                    .ToListAsync(cancellation);

        foreach (var lambda in retired.Where(l => !wanted.Contains(l.PublicKey)))
        {
            try
            {
                // out of the tier first, which is what makes it removable at all
                lambda.Tier = LambdaTier.Free;

                await database.SaveChangesAsync(cancellation);

                await meta.DeleteAsync(lambda.PrivateKey, cancellation);

                logger.LogInformation("Retired the demo at '{Key}', which is no longer one", lambda.PublicKey);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "The retired demo at '{Key}' could not be removed", lambda.PublicKey);
            }
        }
    }

    /// <summary>
    /// Creates or refreshes one demo, and returns whether anything changed.
    /// </summary>
    private async ValueTask<bool> SeedAsync(LambdaDemo demo, CancellationToken cancellation)
    {
        if (!await ClaimAsync(demo, cancellation))
        {
            return false;
        }

        await ProvideSecretsAsync(demo, cancellation);

        await ProvideDatabaseAsync(demo, cancellation);

        await PublishSourceAsync(demo, cancellation);

        var wanted = TemplateCatalog.ForKey(demo.Id, demo.Key, demo: true);

        var lambda = await meta.GetAsync(demo.Key, cancellation);

        var current = await CurrentCodeAsync(demo.Key, lambda, cancellation);

        if (current == wanted && lambda?.ActiveVersion != null)
        {
            return false;
        }

        if (current != wanted)
        {
            var change = current == null ? "Set up as a demo" : "Brought up to date with its template";

            await meta.SaveAsync(demo.Key, wanted, new VersionNote(Specification: demo.Description, Change: change, Origin: VersionOrigins.System), cancellation: cancellation);

            if (current != null)
            {
                logger.LogInformation("The demo '{Demo}' was behind its template and has been updated", demo.Id);
            }
        }

        var result = await meta.DeployAsync(demo.Key, null, VersionOrigins.System, cancellation);

        if (!result.Success)
        {
            throw new InvalidOperationException($"The demo '{demo.Id}' does not deploy: {string.Join(" ", result.Diagnostics.Select(d => d.Message))}");
        }

        return true;
    }

    /// <summary>
    /// Makes sure the row of a demo exists, with its announced key and in its
    /// tier, and says whether it may be seeded.
    /// </summary>
    /// <remarks>
    /// A lambda that already sits at the key and is not a demo belongs to
    /// somebody - claimed before the prefix was kept for demos - and is left
    /// alone rather than taken over. The demo is missing until it is gone.
    /// </remarks>
    private async ValueTask<bool> ClaimAsync(LambdaDemo demo, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var entity = await database.Lambdas.FirstOrDefaultAsync(l => l.PublicKey == demo.Key, cancellation);

        if (entity == null)
        {
            var now = DateTime.UtcNow;

            database.Lambdas.Add(new LambdaEntity
            {
                PublicKey = demo.Key,
                PrivateKey = demo.Key,
                Tier = LambdaTier.Demo,
                Created = now,
                Modified = now
            });

            await database.SaveChangesAsync(cancellation);

            logger.LogInformation("Created the demo '{Demo}'", demo.Id);

            return true;
        }

        if (entity.Tier != LambdaTier.Demo)
        {
            logger.LogWarning("The key of the demo '{Demo}' belongs to another lambda, so the demo is not set up", demo.Id);

            return false;
        }

        if (entity.PrivateKey != demo.Key)
        {
            entity.PrivateKey = demo.Key;

            await database.SaveChangesAsync(cancellation);
        }

        return true;
    }

    /// <summary>
    /// Switches the secrets of a demo on and gives it the ones it reads,
    /// each a random value made here and never seen by anybody.
    /// </summary>
    /// <remarks>
    /// Written past the tier on purpose, like the rest of the seeding: nobody
    /// else may change a demo. A secret that is there already is kept, so
    /// what the demo sealed with it stays readable across restarts.
    /// </remarks>
    private async ValueTask ProvideSecretsAsync(LambdaDemo demo, CancellationToken cancellation)
    {
        if (demo.Secrets is not { Count: > 0 } wanted)
        {
            return;
        }

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var id = await database.Lambdas.Where(l => l.PublicKey == demo.Key).Select(l => l.Id).FirstAsync(cancellation);

        var store = await database.DataStores.FirstOrDefaultAsync(s => s.LambdaId == id && s.Kind == DataKinds.SecretsId, cancellation);

        if (store == null)
        {
            database.DataStores.Add(new DataStoreEntity { LambdaId = id, Kind = DataKinds.SecretsId, Enabled = true, Changed = DateTime.UtcNow });
        }
        else if (!store.Enabled)
        {
            store.Enabled = true;
            store.Changed = DateTime.UtcNow;
        }

        await database.SaveChangesAsync(cancellation);

        secrets.Invalidate(id);

        var present = (await secrets.ListAsync(id, null, cancellation)).Select(s => s.Name).ToHashSet(StringComparer.Ordinal);

        foreach (var name in wanted.Where(n => !present.Contains(n)))
        {
            await secrets.StoreAsync(id, null, name, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)), cancellation);

            logger.LogInformation("Gave the demo '{Demo}' the secret {Name}", demo.Id, name);
        }
    }

    /// <summary>
    /// Switches the database of a demo on, and makes it where it is not there -
    /// the first start, or a demo that kept its records elsewhere before.
    /// </summary>
    /// <remarks>
    /// Past the tier like the secrets. What a demo's visitors wrote into its
    /// database stays across restarts and new versions; its code migrates it.
    /// </remarks>
    private async ValueTask ProvideDatabaseAsync(LambdaDemo demo, CancellationToken cancellation)
    {
        if (!demo.Database)
        {
            return;
        }

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var id = await database.Lambdas.Where(l => l.PublicKey == demo.Key).Select(l => l.Id).FirstAsync(cancellation);

        var store = await database.DataStores.FirstOrDefaultAsync(s => s.LambdaId == id && s.Kind == DataKinds.DatabaseId, cancellation);

        if (store is { Enabled: true } && stores.Exists(id))
        {
            return;
        }

        await stores.CreateAsync(id, cancellation);

        if (store == null)
        {
            database.DataStores.Add(new DataStoreEntity { LambdaId = id, Kind = DataKinds.DatabaseId, Enabled = true, Changed = DateTime.UtcNow });
        }
        else
        {
            store.Enabled = true;
            store.Changed = DateTime.UtcNow;
        }

        await database.SaveChangesAsync(cancellation);

        stores.Invalidate(id);

        logger.LogInformation("Gave the demo '{Demo}' a database", demo.Id);
    }

    /// <summary>
    /// Publishes the source of a demo, under the license the catalogue gives it.
    /// </summary>
    /// <remarks>
    /// A demo is there to be read and built on, which is exactly what a
    /// published source is for - so the installation publishes its own, and
    /// the listing of sources is never empty. Written here rather than through
    /// the source service, which refuses a demo like everything else that
    /// would change one; once published it is left as it is.
    /// </remarks>
    private async ValueTask PublishSourceAsync(LambdaDemo demo, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var id = await database.Lambdas.Where(l => l.PublicKey == demo.Key).Select(l => l.Id).FirstAsync(cancellation);

        if (await database.Sources.AnyAsync(s => s.LambdaId == id, cancellation))
        {
            return;
        }

        var now = DateTime.UtcNow;

        database.Sources.Add(new SourceEntity { LambdaId = id, Published = true, License = demo.License, PublishedAt = now, Updated = now });

        await database.SaveChangesAsync(cancellation);

        logger.LogInformation("Published the source of the demo '{Demo}' under {License}", demo.Id, demo.License);
    }

    /// <summary>
    /// The code of the newest version, or null where there is none.
    /// </summary>
    private async ValueTask<string?> CurrentCodeAsync(string key, LambdaInfo? lambda, CancellationToken cancellation)
    {
        if (lambda?.LatestVersion is not { } version)
        {
            return null;
        }

        try
        {
            return (await meta.GetVersionAsync(key, version, cancellation)).Code;
        }
        catch (Exception)
        {
            return null;
        }
    }

}
