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

    public static void Run(IDbContextFactory<LambdaDbContext> databases, IFileProvider files, ILogger logger)
    {
        var moved = 0;

        using (var database = databases.CreateDbContext())
        {
            var purposes = database.BrandingPictures.AsNoTracking().Where(p => p.Content.Length > 0).Select(p => p.Purpose).ToList();

            foreach (var purpose in purposes)
            {
                var picture = database.BrandingPictures.First(p => p.Purpose == purpose);

                files.Write(FileStores.Branding, PictureNames.Branding(purpose), picture.Content);

                picture.Content = [];

                database.SaveChanges();

                moved++;
            }
        }

        using (var database = databases.CreateDbContext())
        {
            var lambdas = database.Showcases.AsNoTracking().Where(s => s.Image.Length > 0).Select(s => s.LambdaId).ToList();

            foreach (var lambda in lambdas)
            {
                var entry = database.Showcases.First(s => s.LambdaId == lambda);

                files.Write(FileStores.Showcase, PictureNames.Showcase(lambda), entry.Image);

                entry.ImageBytes = entry.Image.Length;
                entry.Image = [];

                database.SaveChanges();

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
