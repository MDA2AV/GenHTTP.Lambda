using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Storage;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// Keeps whether a lambda's source is published, and serves it packed.
/// </summary>
public sealed class SourceService(IDbContextFactory<LambdaDbContext> databases, IStorageService storage, SourceCache cache,
                                  LambdaOptions options, ILogger<SourceService> logger) : ISourceService
{
    private const string Missing = "This lambda does not exist (or has been deleted).";

    /// <summary>
    /// Which build of the server packs a project, since every release may
    /// pack it differently - a new packer is a new project.
    /// </summary>
    private static readonly string Packer = typeof(ProjectPacker).Assembly.ManifestModule.ModuleVersionId.ToString("N");

    #region Owner

    public SourceSettings? Get(string privateKey)
    {
        using var database = databases.CreateDbContext();

        var lambda = Require(database, privateKey);

        var source = database.Sources.AsNoTracking().FirstOrDefault(s => s.LambdaId == lambda.Id);

        return source == null ? null : Settings(lambda.PublicKey, source);
    }

    public SourceSettings Publish(string privateKey, SourceDraft draft)
    {
        // what is asked for is checked before anything is looked up, so a
        // request that is wrong changes nothing at all
        var license = draft.License is { } wanted
            ? SourceLicenses.Find(wanted) ?? throw LambdaException.Invalid($"'{wanted}' is not a license a source can be published under here. Pick one of: {string.Join(", ", SourceLicenses.All.Select(l => l.Id))}.")
            : null;

        var author = draft.Author == null ? null : Author(draft.Author);

        using var database = databases.CreateDbContext();

        var lambda = RequireEditable(database, privateKey);

        var source = database.Sources.FirstOrDefault(s => s.LambdaId == lambda.Id);

        var now = DateTime.UtcNow;

        if (source == null)
        {
            source = new SourceEntity
            {
                LambdaId = lambda.Id,
                Published = true,
                License = (license ?? SourceLicenses.Find(SourceLicenses.Default)!).Id,
                Author = string.IsNullOrEmpty(author) ? null : author,
                PublishedAt = now,
                Updated = now
            };

            database.Sources.Add(source);
        }
        else
        {
            if (!source.Published)
            {
                source.Published = true;
                source.PublishedAt = now;
            }

            if (license != null)
            {
                source.License = license.Id;
            }

            if (author != null)
            {
                source.Author = author.Length == 0 ? null : author;
            }

            source.Updated = now;
        }

        database.SaveChanges();

        logger.LogInformation("The source of lambda {LambdaId} is published under {License}", lambda.Id, source.License);

        return Settings(lambda.PublicKey, source);
    }

    public SourceSettings? Withdraw(string privateKey)
    {
        using var database = databases.CreateDbContext();

        var lambda = RequireEditable(database, privateKey);

        var source = database.Sources.FirstOrDefault(s => s.LambdaId == lambda.Id);

        if (source is { Published: true })
        {
            source.Published = false;
            source.Updated = DateTime.UtcNow;

            database.SaveChanges();

            logger.LogInformation("The source of lambda {LambdaId} was taken down", lambda.Id);
        }

        // nobody may read it any more, so there is nothing to keep it packed for
        cache.Remove(lambda.Id);

        return source == null ? null : Settings(lambda.PublicKey, source);
    }

    #endregion

    #region Public

    public SourceListing List(string? search, SourceOrder order, int skip, int take)
    {
        using var database = databases.CreateDbContext();

        // every published source at once: the words searched for may be in
        // what the showcase says, and the showcase is a table of its own - and
        // a row here is a few hundred bytes
        var rows = Rows(database.Sources.Where(s => s.Published)).ToList();

        var newest = Newest(database, database.Sources.Where(s => s.Published).Select(s => s.LambdaId));

        var showcases = Showcases(database, database.Sources.Where(s => s.Published).Select(s => s.LambdaId));

        var entries = rows.Select(r => Entry(r, newest, showcases)).ToList();

        var words = (search ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (words.Length > 0)
        {
            entries = [.. entries.Where(e => words.All(w => Matches(e, w)))];
        }

        IEnumerable<SourceEntry> ordered = order switch
        {
            SourceOrder.Updated => entries.OrderByDescending(e => e.Updated ?? e.PublishedAt),
            SourceOrder.Published => entries.OrderByDescending(e => e.PublishedAt),
            _ => entries.OrderByDescending(e => e.Stars).ThenByDescending(e => e.Updated ?? e.PublishedAt)
        };

        var page = ordered.Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 48)).ToList();

        // what each says it is, read again where a newer version came since
        for (var i = 0; i < page.Count; i++)
        {
            var row = rows.First(r => r.PublicKey == page[i].PublicKey);

            page[i] = page[i] with { About = About(database, row, page[i].LatestVersion) };
        }

        return new SourceListing(page, entries.Count);
    }

    public SourceProject? GetProject(string publicKey)
    {
        using var database = databases.CreateDbContext();

        var row = Rows(database.Sources.Where(s => s.Published && s.Lambda!.PublicKey == publicKey)).FirstOrDefault();

        if (row == null)
        {
            return null;
        }

        var lambda = database.Lambdas.AsNoTracking()
                             .Where(l => l.Id == row.Id)
                             .Select(l => new { l.ActiveVersion, l.Created })
                             .First();

        var versions = database.Deployments.AsNoTracking()
                               .Where(d => d.LambdaId == row.Id)
                               .OrderByDescending(d => d.Version)
                               .Select(d => new SourceVersion(d.Version, d.Created, d.Change, d.Origin))
                               .ToList();

        var only = database.Sources.Where(s => s.LambdaId == row.Id).Select(s => s.LambdaId);

        var entry = Entry(row, Newest(database, only), Showcases(database, only));

        entry = entry with { About = About(database, row, entry.LatestVersion) };

        return new SourceProject(entry, row.Author, lambda.ActiveVersion, lambda.Created, versions);
    }

    public async ValueTask<SourceArchive?> GetArchiveAsync(string publicKey, int version, CancellationToken cancellation = default)
    {
        using var database = databases.CreateDbContext();

        var row = Rows(database.Sources.Where(s => s.Published && s.Lambda!.PublicKey == publicKey)).FirstOrDefault();

        if (row == null)
        {
            return null;
        }

        var saved = database.Deployments.AsNoTracking()
                            .Where(d => d.LambdaId == row.Id && d.Version == version)
                            .Select(d => new { d.Created, d.Change })
                            .FirstOrDefault();

        if (saved == null)
        {
            return null;
        }

        var license = SourceLicenses.Find(row.License) ?? SourceLicenses.Find(SourceLicenses.Default)!;

        var holder = SourceLicenses.Holder(row.Author, publicKey);

        var address = options.PublicUrl is { } site ? $"{site}/lambda/{publicKey}/" : null;

        var page = options.PublicUrl is { } root ? $"{root}/source/{publicKey}" : null;

        // everything the project is packed with that the version does not
        // decide itself - the version never changes, the rest may
        var fingerprint = Fingerprint(Packer, publicKey, address, license.Id, holder, page);

        var file = await cache.GetAsync(row.Id, version, fingerprint, async stream =>
        {
            var code = storage.Read(row.Id, version)
                    ?? throw LambdaException.NotFound($"The code of version {version} is missing.");

            var files = LambdaSource.Parse(code);

            // the names the code reads, which it says itself - never what is
            // stored, and never a value
            var secrets = files.Where(f => f.IsCode)
                               .SelectMany(f => SecretVault.ReadBy(f.Code))
                               .Select(r => r.Name)
                               .Distinct(StringComparer.Ordinal)
                               .ToList();

            var lambda = new ExportedLambda(publicKey, version, saved.Created, saved.Change, address, null, secrets,
                                            new ExportedLicense(license, holder, page));

            // the program only: what the app keeps is data, and data is never
            // published - Publish takes no database to carry along
            ProjectPacker.Publish(lambda, files, stream);
        }, cancellation);

        return new SourceArchive(publicKey, version, file, ProjectPacker.Folder(publicKey));
    }

    public int? Star(string publicKey, bool starred)
    {
        using var database = databases.CreateDbContext();

        var id = database.Sources.Where(s => s.Published && s.Lambda!.PublicKey == publicKey)
                         .Select(s => (long?)s.LambdaId)
                         .FirstOrDefault();

        if (id == null)
        {
            return null;
        }

        var sources = database.Sources.Where(s => s.LambdaId == id && s.Published);

        // counted by the database rather than read, changed and written, so
        // two visitors at the same moment are two stars
        var changed = starred
            ? sources.ExecuteUpdate(s => s.SetProperty(x => x.Stars, x => x.Stars + 1))
            : sources.ExecuteUpdate(s => s.SetProperty(x => x.Stars, x => x.Stars > 0 ? x.Stars - 1 : 0));

        if (changed == 0)
        {
            return null;
        }

        return sources.Select(s => s.Stars).First();
    }

    public SourceStars? GetStars(string publicKey)
    {
        using var database = databases.CreateDbContext();

        return database.Sources.AsNoTracking()
                       .Where(s => s.Published && s.Lambda!.PublicKey == publicKey)
                       .Select(s => new SourceStars(s.LambdaId, s.Stars))
                       .FirstOrDefault();
    }

    public IReadOnlyList<SourceAddress> ListAddresses()
    {
        using var database = databases.CreateDbContext();

        var keys = database.Sources.AsNoTracking()
                           .Where(s => s.Published)
                           .Select(s => new { s.LambdaId, s.Lambda!.PublicKey })
                           .ToList();

        var newest = Newest(database, database.Sources.Where(s => s.Published).Select(s => s.LambdaId));

        return [.. keys.OrderBy(k => k.PublicKey, StringComparer.Ordinal)
                       .Select(k => new SourceAddress(k.PublicKey, newest.TryGetValue(k.LambdaId, out var n) ? n.Created : null))];
    }

    #endregion

    #region Helpers

    /// <summary>
    /// A published source as it is read from its row and its lambda's.
    /// </summary>
    private sealed record Row(long Id, string PublicKey, LambdaTier Tier, string? Domain, bool Online, string License, string? Author,
                              int Stars, string? About, int? AboutVersion, DateTime PublishedAt);

    private sealed record Newer(int Version, DateTime Created);

    private sealed record Shown(string Title, string Description, DateTime Updated, string ImageType);

    private static IQueryable<Row> Rows(IQueryable<SourceEntity> sources)
        => sources.AsNoTracking()
                  .Select(s => new Row(s.LambdaId, s.Lambda!.PublicKey, s.Lambda.Tier, s.Lambda.Domain, s.Lambda.ActiveVersion != null,
                                       s.License, s.Author, s.Stars, s.About, s.AboutVersion, s.PublishedAt));

    /// <summary>
    /// The newest version of each of the lambdas, and when it was saved.
    /// </summary>
    private static Dictionary<long, Newer> Newest(LambdaDbContext database, IQueryable<long> lambdas)
        => database.Deployments.AsNoTracking()
                   .Where(d => lambdas.Contains(d.LambdaId))
                   .GroupBy(d => d.LambdaId)
                   .Select(g => new { g.Key, Version = g.Max(d => d.Version), Created = g.Max(d => d.Created) })
                   .ToDictionary(g => g.Key, g => new Newer(g.Version, g.Created));

    /// <summary>
    /// What the showcase says about each of the lambdas that are on it.
    /// </summary>
    private static Dictionary<long, Shown> Showcases(LambdaDbContext database, IQueryable<long> lambdas)
        => database.Showcases.AsNoTracking()
                   .Where(s => lambdas.Contains(s.LambdaId))
                   .Select(s => new { s.LambdaId, s.Title, s.Description, s.Updated, s.ImageType })
                   .ToDictionary(s => s.LambdaId, s => new Shown(s.Title, s.Description, s.Updated, s.ImageType));

    private static SourceEntry Entry(Row row, Dictionary<long, Newer> newest, Dictionary<long, Shown> showcases)
    {
        var latest = newest.GetValueOrDefault(row.Id);

        var shown = showcases.GetValueOrDefault(row.Id);

        return new SourceEntry(row.PublicKey, shown?.Title, row.About, shown?.Description, row.License, row.Stars, row.Online,
                               row.Tier, row.Domain, latest?.Version, latest?.Created, row.PublishedAt, shown?.Updated, shown?.ImageType);
    }

    private static bool Matches(SourceEntry entry, string word)
        => entry.PublicKey.Contains(word, StringComparison.OrdinalIgnoreCase)
        || entry.Title?.Contains(word, StringComparison.OrdinalIgnoreCase) == true
        || entry.About?.Contains(word, StringComparison.OrdinalIgnoreCase) == true
        || entry.Description?.Contains(word, StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// What the newest version says the app is: the first paragraph of its
    /// product page - read from that version once, and kept.
    /// </summary>
    /// <remarks>
    /// Kept in the row, so the listing does not read every version it shows
    /// every time it is shown. Searching reads what was kept, which is at most
    /// a version behind for a source nobody looked at since.
    /// </remarks>
    private string? About(LambdaDbContext database, Row row, int? latest)
    {
        if (latest is not { } version || row.AboutVersion == version)
        {
            return row.About;
        }

        string? about;

        try
        {
            var files = LambdaSource.Parse(storage.Read(row.Id, version));

            about = ContextPages.FirstParagraph(ContextPages.Read(files, LambdaSource.ProductDoc));
        }
        catch (IOException e)
        {
            logger.LogWarning(e, "The documentation of version {Version} of lambda {LambdaId} could not be read", version, row.Id);
            return row.About;
        }

        database.Sources.Where(s => s.LambdaId == row.Id)
                      .ExecuteUpdate(s => s.SetProperty(x => x.About, about).SetProperty(x => x.AboutVersion, version));

        return about;
    }

    private static SourceSettings Settings(string publicKey, SourceEntity source)
        => new(publicKey, source.Published, source.License, source.Author, source.Stars, source.PublishedAt, source.Updated);

    /// <summary>
    /// Who holds the copyright, as the owner wrote it: trimmed, on one line,
    /// and empty for nobody.
    /// </summary>
    private static string Author(string author)
    {
        var trimmed = author.Trim();

        if (trimmed.Any(char.IsControl))
        {
            throw LambdaException.Invalid("The name in the license has to fit on one line.");
        }

        if (trimmed.Length > SourceLicenses.MaxAuthor)
        {
            throw LambdaException.Invalid($"The name in the license must not be longer than {SourceLicenses.MaxAuthor} characters.");
        }

        return trimmed;
    }

    private static string Fingerprint(params string?[] parts)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', parts.Select(p => p ?? string.Empty))));

        return Convert.ToHexStringLower(hash)[..16];
    }

    private static LambdaEntity Require(LambdaDbContext database, string privateKey)
        => database.Lambdas.AsNoTracking().FirstOrDefault(l => l.PrivateKey == privateKey)
        ?? throw LambdaException.NotFound(Missing);

    /// <summary>
    /// The lambda behind an editor key, as long as its owner may publish it.
    /// </summary>
    /// <remarks>
    /// A demo's key is announced, so holding it says nothing about being the
    /// one who decides under which license its code is given away - the
    /// installation publishes its demos itself.
    /// </remarks>
    private static LambdaEntity RequireEditable(LambdaDbContext database, string privateKey)
    {
        var lambda = Require(database, privateKey);

        return lambda.Tier == LambdaTier.Demo
             ? throw LambdaException.Forbidden(MetaService.ReadOnly(lambda.PublicKey))
             : lambda;
    }

    #endregion

}
