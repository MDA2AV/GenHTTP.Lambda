using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Keeps the examples in existence and online.
/// </summary>
/// <remarks>
/// This runs after the server is already answering rather than before, because
/// every example has to be compiled and that is seconds the installation would
/// otherwise spend refusing connections. An example is briefly absent after a
/// restart, which is a better failure than a server that is not up yet.
///
/// It is written to be run again at any time: an example whose template has
/// changed is given the new code and redeployed, and one that is already
/// current is left alone.
/// </remarks>
public sealed class ExampleSeeder(IMetaService meta, IDbContextFactory<LambdaDbContext> databases, ILogger<ExampleSeeder> logger)
{

    /// <summary>
    /// Brings every example up to date with the template it comes from.
    /// </summary>
    public async ValueTask SeedAsync(CancellationToken cancellation = default)
    {
        var seeded = 0;

        foreach (var example in ExampleCatalog.All)
        {
            try
            {
                if (await SeedAsync(example, cancellation))
                {
                    seeded++;
                }
            }
            catch (Exception e)
            {
                // one example that will not compile is not a reason for the
                // installation to be without the other five
                logger.LogWarning(e, "The example '{Example}' could not be prepared", example.Id);
            }
        }

        if (seeded > 0)
        {
            logger.LogInformation("Prepared {Count} example(s)", seeded);
        }
    }

    /// <summary>
    /// Creates or refreshes one example, and returns whether anything changed.
    /// </summary>
    private async ValueTask<bool> SeedAsync(LambdaExample example, CancellationToken cancellation)
    {
        var privateKey = await meta.GetPrivateKeyAsync(example.PublicKey, cancellation);

        if (privateKey == null)
        {
            var created = await meta.CreateAsync(example.PublicKey, example.Id, cancellation);

            await MarkAsExampleAsync(created.PublicKey, cancellation);

            await meta.DeployAsync(created.PrivateKey, null, cancellation);

            logger.LogInformation("Created the example '{Example}' at '{Key}'", example.Id, example.PublicKey);

            return true;
        }

        // the flag is set every time rather than only on creation: an example
        // that existed before this flag did would otherwise stay sweepable
        await MarkAsExampleAsync(example.PublicKey, cancellation);

        var wanted = TemplateCatalog.ForKey(example.Id, example.PublicKey);

        var current = await CurrentCodeAsync(privateKey, cancellation);

        var lambda = await meta.GetAsync(privateKey, cancellation);

        if (current == wanted && lambda?.ActiveVersion != null)
        {
            return false;
        }

        if (current != wanted)
        {
            await meta.SaveAsync(privateKey, wanted, cancellation);

            logger.LogInformation("The example '{Example}' was behind its template and has been updated", example.Id);
        }

        await meta.DeployAsync(privateKey, null, cancellation);

        return true;
    }

    /// <summary>
    /// The code of the newest version, or null where there is none.
    /// </summary>
    private async ValueTask<string?> CurrentCodeAsync(string privateKey, CancellationToken cancellation)
    {
        var lambda = await meta.GetAsync(privateKey, cancellation);

        if (lambda?.LatestVersion is not { } version)
        {
            return null;
        }

        try
        {
            return (await meta.GetVersionAsync(privateKey, version, cancellation)).Code;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async ValueTask MarkAsExampleAsync(string publicKey, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var entity = await database.Lambdas.FirstOrDefaultAsync(l => l.PublicKey == publicKey, cancellation);

        if (entity is null or { IsExample: true })
        {
            return;
        }

        entity.IsExample = true;

        await database.SaveChangesAsync(cancellation);
    }

}
