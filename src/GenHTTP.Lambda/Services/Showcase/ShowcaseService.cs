using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Showcase;

/// <summary>
/// Keeps the showcase entries and decides the order they are shown in.
/// </summary>
public sealed class ShowcaseService(IDbContextFactory<LambdaDbContext> databases, LambdaTelemetry telemetry,
                                    LambdaOptions options, ILogger<ShowcaseService> logger) : IShowcaseService
{
    private const string Missing = "This lambda does not exist (or has been deleted).";

    #region Owner

    public async ValueTask<ShowcaseInfo?> GetAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        return await Entries(database.Showcases.Where(s => s.LambdaId == lambda.Id))
                     .Select(e => e.Info)
                     .FirstOrDefaultAsync(cancellation);
    }

    public async ValueTask<ShowcaseInfo> SaveAsync(string privateKey, ShowcaseDraft draft, CancellationToken cancellation = default)
    {
        var title = Tidy(draft.Title);
        var description = Tidy(draft.Description);

        if (title == null)
        {
            throw LambdaException.Invalid("A showcase needs a title.");
        }

        if (title.Length > ShowcaseLimits.MaxTitle)
        {
            throw LambdaException.Invalid($"The title must not be longer than {ShowcaseLimits.MaxTitle} characters.");
        }

        if (title.Contains('\n'))
        {
            throw LambdaException.Invalid("The title has to fit on one line.");
        }

        if (description == null)
        {
            throw LambdaException.Invalid("A showcase needs a description.");
        }

        if (description.Length > ShowcaseLimits.MaxDescription)
        {
            throw LambdaException.Invalid($"The description must not be longer than {ShowcaseLimits.MaxDescription} characters.");
        }

        string? type = null;

        if (draft.Image != null)
        {
            if (draft.Image.Length > options.MaxShowcaseImageBytes)
            {
                throw LambdaException.Invalid($"The picture must not be larger than {options.MaxShowcaseImageBytes / 1024 / 1024} MB.");
            }

            type = Sniff(draft.Image)
                ?? throw LambdaException.Invalid("The picture has to be a PNG, JPEG, GIF or WebP image.");
        }

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        var entry = await database.Showcases.FirstOrDefaultAsync(s => s.LambdaId == lambda.Id, cancellation);

        var now = DateTime.UtcNow;

        if (entry == null)
        {
            if (draft.Image == null)
            {
                throw LambdaException.Invalid("A showcase needs a picture.");
            }

            database.Showcases.Add(new ShowcaseEntity
            {
                LambdaId = lambda.Id,
                Title = title,
                Description = description,
                Image = draft.Image,
                ImageType = type!,
                Created = now,
                Updated = now
            });

            logger.LogInformation("Lambda {LambdaId} was added to the showcase", lambda.Id);
        }
        else
        {
            entry.Title = title;
            entry.Description = description;
            entry.Updated = now;

            if (draft.Image != null)
            {
                entry.Image = draft.Image;
                entry.ImageType = type!;
            }
        }

        await database.SaveChangesAsync(cancellation);

        return await Entries(database.Showcases.Where(s => s.LambdaId == lambda.Id))
                     .Select(e => e.Info)
                     .FirstAsync(cancellation);
    }

    public async ValueTask RemoveAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireAsync(database, privateKey, cancellation);

        if (await database.Showcases.Where(s => s.LambdaId == lambda.Id).ExecuteDeleteAsync(cancellation) > 0)
        {
            logger.LogInformation("Lambda {LambdaId} was taken out of the showcase", lambda.Id);
        }
    }

    #endregion

    #region Public

    public async ValueTask<ShowcasePage> ListAsync(int skip, int take, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        // every online entry at once: they are ranked by figures that live
        // partly in memory, so the database cannot do the ordering - and a
        // row without its picture is a few hundred bytes
        var entries = await Entries(database.Showcases.Where(s => s.Lambda!.ActiveVersion != null))
                            .ToListAsync(cancellation);

        var now = DateTime.UtcNow;

        var ranked = entries.OrderByDescending(e => Score(e, now))
                            .ThenByDescending(e => e.Info.Updated)
                            .Select(e => e.Info)
                            .ToList();

        return new ShowcasePage([.. ranked.Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 48))], ranked.Count);
    }

    public async ValueTask<ShowcaseImage?> GetImageAsync(string publicKey, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.Showcases.AsNoTracking()
                             .Where(s => s.Lambda!.PublicKey == publicKey)
                             .Select(s => new ShowcaseImage(s.Image, s.ImageType, s.Updated))
                             .FirstOrDefaultAsync(cancellation);
    }

    #endregion

    #region Ranking

    /// <summary>
    /// How much is going on with a lambda, as one number.
    /// </summary>
    /// <remarks>
    /// A mix, because every single figure favours the wrong thing on its own.
    /// Requests over the last day say what people use, but are counted in
    /// memory and start from nothing after a restart, and a handful of busy
    /// games would sit on top for good. So the recency of the last visit and of
    /// the last change weigh in as well, and a fresh entry gets a few days in
    /// which it is seen at all. Requests count logarithmically: ten times the
    /// traffic is one step up, not ten.
    /// </remarks>
    private double Score(Entry entry, DateTime now)
    {
        var traffic = telemetry.Describe(entry.Id);

        var requests = traffic.Quarters.Sum(q => q.Requests + q.Upgrades);

        var seen = Latest(entry.LastSeen, traffic.Totals?.LastSeen);

        var score = 3.0 * Math.Log10(1 + requests);

        if (seen is { } visited)
        {
            score += 2.0 * Decay(now - visited, TimeSpan.FromDays(1));
        }

        score += 1.0 * Decay(now - entry.Modified, TimeSpan.FromDays(7));

        score += 1.5 * Decay(now - entry.Info.Created, TimeSpan.FromDays(3));

        return score;
    }

    private static double Decay(TimeSpan age, TimeSpan scale)
        => Math.Exp(-Math.Max(0, age.TotalHours) / scale.TotalHours);

    private static DateTime? Latest(DateTime? a, DateTime? b)
        => a == null ? b : b == null ? a : (a > b ? a : b);

    #endregion

    #region Helpers

    /// <summary>
    /// An entry without its picture, and what its ranking needs.
    /// </summary>
    private sealed record Entry(long Id, ShowcaseInfo Info, DateTime? LastSeen, DateTime Modified);

    private static IQueryable<Entry> Entries(IQueryable<ShowcaseEntity> showcases)
        => showcases.AsNoTracking()
                    .Select(s => new Entry(
                       s.LambdaId,
                       new ShowcaseInfo(s.Lambda!.PublicKey, s.Title, s.Description, s.ImageType, s.Image.Length,
                                        s.Lambda.ActiveVersion != null, s.Created, s.Updated),
                       s.Lambda.LastSeen,
                       s.Lambda.Modified));

    private static async ValueTask<LambdaEntity> RequireAsync(LambdaDbContext database, string privateKey, CancellationToken cancellation)
        => await database.Lambdas.AsNoTracking().FirstOrDefaultAsync(l => l.PrivateKey == privateKey, cancellation)
        ?? throw LambdaException.NotFound(Missing);

    /// <summary>
    /// Trimmed, with the line breaks of any system made one kind, and nothing
    /// where there was only space.
    /// </summary>
    private static string? Tidy(string? text)
    {
        var trimmed = text?.Replace("\r\n", "\n").Trim();

        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    /// <summary>
    /// What an image is, from its first bytes.
    /// </summary>
    /// <remarks>
    /// Never from what the uploader claims: the picture is served from this
    /// origin, and a type taken on trust is how a page of script gets served
    /// as if it were one.
    /// </remarks>
    internal static string? Sniff(ReadOnlySpan<byte> content)
    {
        if (content.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            return "image/png";
        }

        if (content.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]))
        {
            return "image/jpeg";
        }

        if (content.StartsWith("GIF87a"u8) || content.StartsWith("GIF89a"u8))
        {
            return "image/gif";
        }

        if (content.Length >= 12 && content.StartsWith("RIFF"u8) && content[8..12].SequenceEqual("WEBP"u8))
        {
            return "image/webp";
        }

        return null;
    }

    #endregion

}
