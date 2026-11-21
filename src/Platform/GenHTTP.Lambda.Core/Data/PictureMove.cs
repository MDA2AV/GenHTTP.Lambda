using GenHTTP.Lambda.Integrations.Files;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Data;

/// <summary>
/// Moves the pictures the platform kept in its records - the logo of the
/// branding, the pictures of the showcase - into the files integration, and
/// empties the columns that held them.
/// </summary>
/// <remarks>
/// What a migration cannot do (V27): the bytes go to files, which only the
/// platform reaches. Run as the platform starts, once the migrations are done
/// and before the first request; a picture that is moved has an empty column,
/// so running it again moves nothing. A row at a time, since a picture may be
/// megabytes. Where moving one fails, the picture is still read from its row
/// (see the services that read them), and the next start tries again.
/// </remarks>
internal static class PictureMove
{

    public static async ValueTask RunAsync(IDbContextFactory<LambdaDbContext> databases, IFileProvider files, ILogger logger, CancellationToken cancellation = default)
    {
        var moved = 0;

        await using (var database = await databases.CreateDbContextAsync(cancellation))
        {
            var purposes = await database.BrandingPictures.AsNoTracking().Where(p => p.Content.Length > 0).Select(p => p.Purpose).ToListAsync(cancellation);

            foreach (var purpose in purposes)
            {
                var picture = await database.BrandingPictures.FirstAsync(p => p.Purpose == purpose, cancellation);

                await files.WriteAsync(FileStore.Branding, PictureNames.Branding(purpose), picture.Content, cancellation);

                picture.Content = [];

                // written to the files, so emptied in the row whoever cancels
                await database.SaveChangesAsync(CancellationToken.None);

                moved++;
            }
        }

        await using (var database = await databases.CreateDbContextAsync(cancellation))
        {
            var lambdas = await database.Showcases.AsNoTracking().Where(s => s.Image.Length > 0).Select(s => s.LambdaId).ToListAsync(cancellation);

            foreach (var lambda in lambdas)
            {
                var entry = await database.Showcases.FirstAsync(s => s.LambdaId == lambda, cancellation);

                await files.WriteAsync(FileStore.Showcase, PictureNames.Showcase(lambda), entry.Image, cancellation);

                entry.ImageBytes = entry.Image.Length;
                entry.Image = [];

                await database.SaveChangesAsync(CancellationToken.None);

                // the next picture in a context that holds none of the ones before
                database.ChangeTracker.Clear();

                moved++;
            }
        }

        if (moved > 0)
        {
            logger.LogInformation("Moved {Count} pictures from the platform's records into its files", moved);
        }
    }

}
