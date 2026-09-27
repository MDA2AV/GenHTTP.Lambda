using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta.Model;

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
public sealed class DemoSeeder(IMetaService meta, IDbContextFactory<LambdaDbContext> databases, ILogger<DemoSeeder> logger)
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

            await meta.SaveAsync(demo.Key, wanted, new VersionNote(Specification: demo.Description, Change: change, Origin: VersionOrigins.System), cancellation);

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
