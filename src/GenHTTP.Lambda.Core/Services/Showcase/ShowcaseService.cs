using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Showcase;

/// <summary>
/// Keeps the showcase entries and decides the order they are shown in.
/// </summary>
/// <remarks>
/// An entry is a row of the platform's records, and its picture a file of
/// the files integration (<c>showcase/{lambda}</c>) - no picture is kept in a
/// database. The row says what the picture is and how large.
/// </remarks>
public sealed class ShowcaseService(IDbContextFactory<LambdaDbContext> databases, IFileProvider files, ITelemetryService telemetry,
                                    ILimitsService limits, ILogger<ShowcaseService> logger) : IShowcaseService
{
    private const string Missing = "This lambda does not exist (or has been deleted).";

    #region Owner

    public ShowcaseInfo? Get(string privateKey)
    {
        using var database = databases.CreateDbContext();

        var lambda = Require(database, privateKey);

        return Entries(database.Showcases.Where(s => s.LambdaId == lambda.Id))
                     .Select(e => e.Info)
                     .FirstOrDefault();
    }

    public ShowcaseInfo Save(string privateKey, ShowcaseDraft draft)
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
            var allowed = limits.Get().ShowcaseImageBytes;

            if (draft.Image.Length > allowed)
            {
                throw LambdaException.Invalid($"The picture must not be larger than {allowed / 1024 / 1024} MB.");
            }

            type = ImageTypes.Sniff(draft.Image)
                ?? throw LambdaException.Invalid("The picture has to be a PNG, JPEG, GIF or WebP image.");
        }

        using var database = databases.CreateDbContext();

        var lambda = RequireEditable(database, privateKey);

        if (!limits.PolicyOf(lambda.Tier).Showcased)
        {
            throw LambdaException.Forbidden($"'{lambda.PublicKey}' is in the tier '{limits.PolicyOf(lambda.Tier).Name}', the installation's own, which is never listed in the showcase.");
        }

        var entry = database.Showcases.FirstOrDefault(s => s.LambdaId == lambda.Id);

        var now = DateTime.UtcNow;

        if (entry == null && draft.Image == null)
        {
            throw LambdaException.Invalid("A showcase needs a picture.");
        }

        // the picture first: a row is never there without the file it names
        if (draft.Image != null)
        {
            files.Write(FileStores.Showcase, PictureOf(lambda.Id), draft.Image);
        }

        if (entry == null)
        {
            database.Showcases.Add(new ShowcaseEntity
            {
                LambdaId = lambda.Id,
                Title = title,
                Description = description,
                ImageBytes = draft.Image!.Length,
                ImageType = type!,
                Created = now,
                Updated = now
            });

            logger.LogInformation("Added lambda #{LambdaId} to showcase", lambda.Id);
        }
        else
        {
            entry.Title = title;
            entry.Description = description;
            entry.Updated = now;

            if (draft.Image != null)
            {
                entry.Image = [];
                entry.ImageBytes = draft.Image.Length;
                entry.ImageType = type!;
            }
        }

        database.SaveChanges();

        return Entries(database.Showcases.Where(s => s.LambdaId == lambda.Id))
                     .Select(e => e.Info)
                     .First();
    }

    public void Remove(string privateKey)
    {
        using var database = databases.CreateDbContext();

        var lambda = RequireEditable(database, privateKey);

        if (database.Showcases.Where(s => s.LambdaId == lambda.Id).ExecuteDelete() > 0)
        {
            logger.LogInformation("Removed lambda #{LambdaId} from showcase", lambda.Id);
        }

        files.Delete(FileStores.Showcase, PictureOf(lambda.Id));
    }

    public void Forget(long lambdaId)
    {
        using var database = databases.CreateDbContext();

        database.Showcases.Where(s => s.LambdaId == lambdaId).ExecuteDelete();

        files.Delete(FileStores.Showcase, PictureOf(lambdaId));
    }

    #endregion

    #region Public

    public ShowcasePage List(int skip, int take)
    {
        using var database = databases.CreateDbContext();

        // every online entry at once: they are ranked by figures that live
        // partly in memory, so the database cannot do the ordering - and a
        // row without its picture is a few hundred bytes. Not of a lambda
        // moved to a tier that is never listed: kept, and back once it is
        // moved out again
        var listed = limits.TiersWhere(p => p.Showcased);

        var entries = Entries(database.Showcases.Where(s => s.Lambda!.ActiveVersion != null && listed.Contains(s.Lambda.Tier)))
                            .ToList();

        var now = DateTime.UtcNow;

        var ranked = entries.OrderByDescending(e => Score(e, now))
                            .ThenByDescending(e => e.Info.Updated)
                            .Select(e => e.Info)
                            .ToList();

        return new ShowcasePage([.. ranked.Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 48))], ranked.Count);
    }

    public ShowcaseImage? GetImage(string publicKey)
    {
        using var database = databases.CreateDbContext();

        var entry = database.Showcases.AsNoTracking()
                            .Where(s => s.Lambda!.PublicKey == publicKey)
                            .Select(s => new { s.LambdaId, s.ImageType, s.Updated })
                            .FirstOrDefault();

        if (entry == null)
        {
            return null;
        }

        // the row's own bytes where the picture was not moved out of it yet
        var content = files.Read(FileStores.Showcase, PictureOf(entry.LambdaId))
                   ?? database.Showcases.AsNoTracking().Where(s => s.LambdaId == entry.LambdaId).Select(s => s.Image).FirstOrDefault();

        return content is { Length: > 0 } ? new ShowcaseImage(content, entry.ImageType, entry.Updated) : null;
    }

    /// <summary>
    /// What the picture of a lambda's entry is called in the store of the showcase.
    /// </summary>
    internal static string PictureOf(long lambdaId) => lambdaId.ToString(System.Globalization.CultureInfo.InvariantCulture);

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
        var traffic = telemetry.DescribeLambda(entry.Id);

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
                       new ShowcaseInfo(s.Lambda!.PublicKey, s.Title, s.Description, s.ImageType, s.ImageBytes,
                                        s.Lambda.ActiveVersion != null, s.Created, s.Updated,
                                        s.Lambda.Tier, s.Lambda.Domain),
                       s.Lambda.LastSeen,
                       s.Lambda.Modified));

    private static LambdaEntity Require(LambdaDbContext database, string privateKey)
        => database.Lambdas.AsNoTracking().FirstOrDefault(l => l.PrivateKey == privateKey)
        ?? throw LambdaException.NotFound(Missing);

    /// <summary>
    /// The lambda behind an editor key, as long as its owner may list it.
    /// </summary>
    /// <remarks>
    /// A demo's key is announced, so holding it says nothing about being the
    /// one who decides what the showcase says about it.
    /// </remarks>
    private LambdaEntity RequireEditable(LambdaDbContext database, string privateKey)
    {
        var lambda = Require(database, privateKey);

        LambdaGuard.EnsureUnblocked(lambda);

        return limits.PolicyOf(lambda.Tier).ReadOnly
             ? throw LambdaException.Forbidden(LambdaGuard.ReadOnly(lambda.PublicKey))
             : lambda;
    }

    /// <summary>
    /// Trimmed, with the line breaks of any system made one kind, and nothing
    /// where there was only space.
    /// </summary>
    private static string? Tidy(string? text)
    {
        var trimmed = text?.Replace("\r\n", "\n").Trim();

        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    #endregion

}
