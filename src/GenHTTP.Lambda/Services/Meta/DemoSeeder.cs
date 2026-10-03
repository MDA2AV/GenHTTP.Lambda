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
                logger.LogWarning(e, "Failed to prepare demo {Demo}", demo.Id);
            }
        }

        if (seeded > 0)
        {
            logger.LogInformation("Prepared {Count} demo(s)", seeded);
        }

        Retire();
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
    private void Retire()
    {
        var wanted = DemoCatalog.All.Select(e => e.Key).ToHashSet(StringComparer.Ordinal);

        using var database = databases.CreateDbContext();

        var retired = database.Lambdas.Where(l => l.Tier == LambdaTier.Demo)
                              .ToList();

        foreach (var lambda in retired.Where(l => !wanted.Contains(l.PublicKey)))
        {
            try
            {
                // out of the tier first, which is what makes it removable at all
                lambda.Tier = LambdaTier.Free;

                database.SaveChanges();

                meta.Delete(lambda.PrivateKey);

                logger.LogInformation("Retired demo {Lambda}", lambda.PublicKey);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to remove retired demo {Lambda}", lambda.PublicKey);
            }
        }
    }

    /// <summary>
    /// Creates or refreshes one demo, and returns whether anything changed.
    /// </summary>
    private async ValueTask<bool> SeedAsync(LambdaDemo demo, CancellationToken cancellation)
    {
        if (!Claim(demo))
        {
            return false;
        }

        ProvideSecrets(demo);

        ProvideDatabase(demo);

        PublishSource(demo);

        var wanted = TemplateCatalog.ForKey(demo.Id, demo.Key, demo: true);

        var lambda = meta.Get(demo.Key);

        var current = CurrentCode(demo.Key, lambda);

        if (current == wanted && lambda?.ActiveVersion != null)
        {
            return false;
        }

        if (current != wanted)
        {
            var change = current == null ? "Set up as a demo" : "Brought up to date with its template";

            meta.Save(demo.Key, wanted, new VersionNote(Specification: demo.Description, Change: change, Origin: VersionOrigins.System));

            if (current != null)
            {
                logger.LogInformation("Updated demo {Demo} from its template", demo.Id);
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
    private bool Claim(LambdaDemo demo)
    {
        using var database = databases.CreateDbContext();

        var entity = database.Lambdas.FirstOrDefault(l => l.PublicKey == demo.Key);

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

            database.SaveChanges();

            logger.LogInformation("Created demo {Demo}", demo.Id);

            return true;
        }

        if (entity.Tier != LambdaTier.Demo)
        {
            logger.LogWarning("Skipped demo {Demo}, its key belongs to another lambda", demo.Id);

            return false;
        }

        if (entity.PrivateKey != demo.Key)
        {
            entity.PrivateKey = demo.Key;

            database.SaveChanges();
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
    private void ProvideSecrets(LambdaDemo demo)
    {
        if (demo.Secrets is not { Count: > 0 } wanted)
        {
            return;
        }

        using var database = databases.CreateDbContext();

        var id = database.Lambdas.Where(l => l.PublicKey == demo.Key).Select(l => l.Id).First();

        var store = database.DataStores.FirstOrDefault(s => s.LambdaId == id && s.Kind == DataKinds.SecretsId);

        if (store == null)
        {
            database.DataStores.Add(new DataStoreEntity { LambdaId = id, Kind = DataKinds.SecretsId, Enabled = true, Changed = DateTime.UtcNow });
        }
        else if (!store.Enabled)
        {
            store.Enabled = true;
            store.Changed = DateTime.UtcNow;
        }

        database.SaveChanges();

        secrets.Invalidate(id);

        var present = (secrets.List(id, null)).Select(s => s.Name).ToHashSet(StringComparer.Ordinal);

        foreach (var name in wanted.Where(n => !present.Contains(n)))
        {
            secrets.Store(id, null, name, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

            logger.LogInformation("Set secret {Name} of demo {Demo}", name, demo.Id);
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
    private void ProvideDatabase(LambdaDemo demo)
    {
        if (!demo.Database)
        {
            return;
        }

        using var database = databases.CreateDbContext();

        var id = database.Lambdas.Where(l => l.PublicKey == demo.Key).Select(l => l.Id).First();

        var store = database.DataStores.FirstOrDefault(s => s.LambdaId == id && s.Kind == DataKinds.DatabaseId);

        if (store is { Enabled: true } && stores.Exists(id))
        {
            return;
        }

        stores.Create(id);

        if (store == null)
        {
            database.DataStores.Add(new DataStoreEntity { LambdaId = id, Kind = DataKinds.DatabaseId, Enabled = true, Changed = DateTime.UtcNow });
        }
        else
        {
            store.Enabled = true;
            store.Changed = DateTime.UtcNow;
        }

        database.SaveChanges();

        stores.Invalidate(id);

        logger.LogInformation("Created database of demo {Demo}", demo.Id);
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
    private void PublishSource(LambdaDemo demo)
    {
        using var database = databases.CreateDbContext();

        var id = database.Lambdas.Where(l => l.PublicKey == demo.Key).Select(l => l.Id).First();

        if (database.Sources.Any(s => s.LambdaId == id))
        {
            return;
        }

        var now = DateTime.UtcNow;

        database.Sources.Add(new SourceEntity { LambdaId = id, Published = true, License = demo.License, PublishedAt = now, Updated = now });

        database.SaveChanges();

        logger.LogInformation("Published source of demo {Demo} license {License}", demo.Id, demo.License);
    }

    /// <summary>
    /// The code of the newest version, or null where there is none.
    /// </summary>
    private string? CurrentCode(string key, LambdaInfo? lambda)
    {
        if (lambda?.LatestVersion is not { } version)
        {
            return null;
        }

        try
        {
            return (meta.GetVersion(key, version)).Code;
        }
        catch (Exception)
        {
            return null;
        }
    }

}
