using System.Diagnostics;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Moves what was kept before there was a store into it, once, as the
/// platform starts: every version, feature and preview that was a file
/// holding all its files whole.
/// </summary>
/// <remarks>
/// Each is read once, its files are kept by the store of its lambda, and a
/// manifest of them is written beside it - which is what is read from then
/// on. Nothing it was is rewritten or removed: a version never changes, so
/// the file of before stays as it was, and the manifest only says the same
/// in a way that is read without holding it. A feature's file of before goes
/// once the feature is saved again, as it always went.
///
/// Before anything is served, so no request reads one of them whole
/// meanwhile, nor saves one while it is moved. One at a time, so what is held
/// is one file of before at most; one that cannot be read is said in the log
/// and read as it is, and tried again on the next start. A start after the
/// first finds a manifest beside each and moves nothing.
/// </remarks>
public sealed class StoreMigration(LambdaOptions options, IDbContextFactory<LambdaDbContext> databases, ContentStore store, ILogger<StoreMigration> logger)
{

    public async ValueTask RunAsync(CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var versions = await database.Deployments.AsNoTracking()
                                     .OrderBy(d => d.LambdaId).ThenBy(d => d.Version)
                                     .Select(d => new { d.LambdaId, d.Version })
                                     .ToListAsync(cancellation);

        var features = await database.Features.AsNoTracking()
                                     .OrderBy(f => f.LambdaId).ThenBy(f => f.Id)
                                     .Select(f => new { f.LambdaId, f.Id })
                                     .ToListAsync(cancellation);

        var watch = Stopwatch.StartNew();

        var (moved, drafts) = (0, 0);

        // said before it begins, since the first start that finds them takes a while
        if (versions.Any(v => Waiting(StorageLayout.FormerVersion(options, v.LambdaId, v.Version), StorageLayout.Version(options, v.LambdaId, v.Version)))
            || features.Any(f => Waiting(StorageLayout.FormerFeature(options, f.LambdaId, f.Id), StorageLayout.Feature(options, f.LambdaId, f.Id))))
        {
            logger.LogInformation("Moving the versions and features kept before into the stores of their lambdas");
        }

        foreach (var version in versions)
        {
            if (await MoveAsync(version.LambdaId, StorageLayout.FormerVersion(options, version.LambdaId, version.Version),
                                StorageLayout.Version(options, version.LambdaId, version.Version), cancellation))
            {
                moved++;
            }
        }

        foreach (var feature in features)
        {
            if (await MoveAsync(feature.LambdaId, StorageLayout.FormerFeature(options, feature.LambdaId, feature.Id),
                                StorageLayout.Feature(options, feature.LambdaId, feature.Id), cancellation))
            {
                drafts++;
            }

            await MoveAsync(feature.LambdaId, StorageLayout.FormerPreview(options, feature.LambdaId, feature.Id),
                            StorageLayout.Preview(options, feature.LambdaId, feature.Id), cancellation);
        }

        if (moved + drafts > 0)
        {
            logger.LogInformation("Moved {Versions} versions and {Features} features into the stores of their lambdas in {Elapsed} ms", moved, drafts, watch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Whether a file of before waits to be moved into the store.
    /// </summary>
    private static bool Waiting(string former, string manifest) => !File.Exists(manifest) && File.Exists(former);

    /// <summary>
    /// Moves one file of before into the store, unless it was or there is none.
    /// </summary>
    /// <returns>Whether it was moved now</returns>
    private async ValueTask<bool> MoveAsync(long lambdaId, string former, string manifest, CancellationToken cancellation)
    {
        if (!Waiting(former, manifest))
        {
            return false;
        }

        try
        {
            if (StorageLayout.ReadFormer(former) is not { } files)
            {
                return false;
            }

            var (kept, _) = await store.KeepAsync(lambdaId, files, cancellation);

            return await store.WriteAsync(lambdaId, manifest, kept, replace: false);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to move {File} of lambda #{LambdaId} into its store", Path.GetFileName(former), lambdaId);

            return false;
        }
    }

}
