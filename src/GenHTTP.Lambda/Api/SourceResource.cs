using System.IO.Compression;
using System.Text;

using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Source;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The published sources, for anybody to read and download.
/// </summary>
/// <remarks>
/// Public and read only, like the showcase: a source is named by its public
/// key, and nothing here takes or hands out an editor key. A lambda whose
/// source is not published is not here at all - asked for, it is not found,
/// the same as one that does not exist.
///
/// What is read is a version packed into a project, the export without the
/// data: the files come out of the zip it was packed into once, never out of
/// the lambda's database, workspace or secrets.
/// </remarks>
public sealed class SourceResource(ISourceService sources, StarGuard stars, ILogger<SourceResource> logger)
{

    public const int PageSize = 24;

    /// <summary>
    /// How long a file may be to be shown as text rather than fetched as it is.
    /// </summary>
    private const int MaxText = 1024 * 1024;

    /// <summary>
    /// Pictures served as themselves, so a page of the documentation shows the
    /// ones beside it. Everything else is text or bytes to download.
    /// </summary>
    private static readonly Dictionary<string, string> Pictures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".avif"] = "image/avif",
        [".ico"] = "image/x-icon",
        [".svg"] = "image/svg+xml"
    };

    #region Listing

    /// <summary>
    /// One page of the published sources.
    /// </summary>
    /// <param name="search">Words the key, the title or what it is about have to contain</param>
    /// <param name="order">stars (the default), updated or published</param>
    /// <param name="skip">How many to leave out, from the start</param>
    /// <param name="take">How many to answer with, at most 48</param>
    [ResourceMethod]
    public Page<SourceEntryResponse> List(string? search, string? order, int skip = 0, int take = PageSize)
    {
        var from = Math.Max(0, skip);

        var ordered = order?.ToLowerInvariant() switch
        {
            "updated" => SourceOrder.Updated,
            "published" or "new" => SourceOrder.Published,
            _ => SourceOrder.Stars
        };

        var page = sources.List(search, ordered, from, take);

        return Page<SourceEntryResponse>.Of([.. page.Entries.Select(SourceEntryResponse.Of)], from, page.Total);
    }

    /// <summary>
    /// A published source: what it is, under which license, where it runs,
    /// and every version it went through.
    /// </summary>
    /// <remarks>
    /// Answered with a ticket to star it with, which is the one thing here that
    /// changes anything.
    /// </remarks>
    [ResourceMethod(":publicKey")]
    public SourceProjectResponse Get(string publicKey)
    {
        var project = sources.GetProject(publicKey) ?? throw NotPublished(publicKey);

        var entry = SourceEntryResponse.Of(project.Entry);

        var versions = project.Versions.Select(v => new SourceVersionResponse(v.Version, v.Created, v.Change, v.Origin,
                                                                              v.Version == project.ActiveVersion && entry.Online,
                                                                              ZipPath(publicKey, v.Version)));

        return new SourceProjectResponse(entry, project.Author, SourceLicenses.Holder(project.Author, publicKey), project.ActiveVersion,
                                         project.Created, [.. versions], stars.Issue(publicKey, DateTime.UtcNow));
    }

    #endregion

    #region Versions

    /// <summary>
    /// The files of a version, as the project it is packed into has them.
    /// </summary>
    /// <remarks>
    /// Packed on the first request for the version and kept, so the first
    /// answer may take a moment for a large lambda and every later one does
    /// not.
    /// </remarks>
    [ResourceMethod(":publicKey/versions/:version")]
    public async ValueTask<SourceTreeResponse> Tree(string publicKey, int version)
    {
        var archive = await RequireAsync(publicKey, version);

        using var zip = ZipFile.OpenRead(archive.File);

        var files = Entries(zip, archive.Root).Select(e => new SourceFileResponse(e.Path, e.Entry.Length, SourceKinds.Of(e.Path)))
                                              .OrderBy(f => f.Path, StringComparer.Ordinal)
                                              .ToList();

        return new SourceTreeResponse(publicKey, version, archive.Root, new FileInfo(archive.File).Length, files, ZipPath(publicKey, version));
    }

    /// <summary>
    /// One file of a version: its text, while it is text and no longer than a
    /// megabyte, and where to fetch it as it is either way.
    /// </summary>
    /// <param name="path">Its path below the project's folder, encoded as one segment ("docs%2Fproduct.md")</param>
    [ResourceMethod(":publicKey/versions/:version/files/:path")]
    public async ValueTask<SourceFileContentResponse> File(string publicKey, int version, string path)
    {
        var archive = await RequireAsync(publicKey, version);

        using var zip = ZipFile.OpenRead(archive.File);

        var entry = Find(zip, archive, path);

        var raw = $"/api/v1/sources/{publicKey}/versions/{version}/raw/{Uri.EscapeDataString(path)}";

        string? text = null;

        if (entry.Length <= MaxText && !Pictures.ContainsKey(Path.GetExtension(path)))
        {
            text = ReadText(entry);
        }

        return new SourceFileContentResponse(path, entry.Length, SourceKinds.Of(path), text != null || (entry.Length > MaxText && LooksLikeText(entry)),
                                             text, raw);
    }

    /// <summary>
    /// One file of a version as it is: a picture as a picture, anything else
    /// as text or bytes - never as something a browser would run.
    /// </summary>
    /// <remarks>
    /// Served from the origin of the site, so a page or a script a lambda
    /// ships is sent as text here, and whatever is sent is sandboxed and never
    /// sniffed. That is what lets the documentation show the pictures beside
    /// it without letting a published source run anything on this site.
    /// </remarks>
    /// <param name="path">Its path below the project's folder, encoded as one segment</param>
    /// <param name="download">Anything but empty: sent to be saved rather than shown</param>
    [ResourceMethod(":publicKey/versions/:version/raw/:path")]
    public async ValueTask<IResponse> Raw(string publicKey, int version, string path, string? download, IRequest request)
    {
        var archive = await RequireAsync(publicKey, version);

        string type;

        long length;

        using (var zip = ZipFile.OpenRead(archive.File))
        {
            var entry = Find(zip, archive, path);

            length = entry.Length;

            type = Pictures.TryGetValue(Path.GetExtension(path), out var picture)
                 ? picture
                 : LooksLikeText(entry) ? "text/plain; charset=utf-8" : "application/octet-stream";
        }

        var name = path[(path.LastIndexOf('/') + 1)..];

        var response = request.Respond()
                              .Content(new ZipEntryContent(archive.File, $"{archive.Root}/{path}", length, type))
                              .Header("X-Content-Type-Options", "nosniff")
                              .Header("Content-Security-Policy", "default-src 'none'; img-src 'self' data:; style-src 'unsafe-inline'; sandbox")
                              // a version never changes, what it is published under may
                              .Header("Cache-Control", "public, max-age=300");

        if (!string.IsNullOrEmpty(download) || type == "application/octet-stream")
        {
            response.Header("Content-Disposition", $"attachment; filename=\"{Safe(name)}\"");
        }

        return response.Build();
    }

    /// <summary>
    /// A version as a project to download: a .NET 10 project with a
    /// Dockerfile, its documentation and tests, and its license - and none of
    /// the data the lambda keeps.
    /// </summary>
    [ResourceMethod(":publicKey/versions/:version/zip")]
    public async ValueTask<IResponse> Zip(string publicKey, int version, IRequest request)
    {
        var archive = await RequireAsync(publicKey, version);

        logger.LogInformation("Downloaded version {Version} of the published source of lambda {Lambda}", version, publicKey);

        return request.Respond()
                      .Content(Resource.FromFile(archive.File).Type(new ContentType("application/zip")).Build())
                      .Header("Content-Disposition", $"attachment; filename=\"{Safe(publicKey)}-v{version}.zip\"")
                      .Header("Cache-Control", "public, max-age=300")
                      .Build();
    }

    #endregion

    #region Stars

    /// <summary>
    /// Stars a published source, or takes the star back.
    /// </summary>
    /// <remarks>
    /// A POST with the ticket the source was read with, not a link: something
    /// that follows every link it finds stars nothing. Each address stars a
    /// source once, and only so many in a while.
    /// </remarks>
    [ResourceMethod(Method.Post, ":publicKey/star")]
    public StarResponse Star(string publicKey, IRequest request, StarRequest body)
    {
        var client = request.Client.Address;

        var now = DateTime.UtcNow;

        var current = sources.GetStars(publicKey) ?? throw NotPublished(publicKey);

        if (!stars.Accepts(publicKey, body?.Ticket, now))
        {
            throw LambdaException.Invalid("This page is too fresh or too old to star from - read the source again and star it then.");
        }

        var starred = body?.Starred ?? true;

        switch (stars.Decide(client, current.Id, starred, now))
        {
            case StarVerdict.TooMany:
                throw LambdaException.TooMany("That was a lot of stars in a short while. Try again in a few minutes.");

            case StarVerdict.Unchanged:
                return new StarResponse(current.Stars, starred, false);
        }

        var count = sources.Star(publicKey, starred) ?? throw NotPublished(publicKey);

        if (starred)
        {
            logger.LogInformation("Starred the source of lambda {Lambda}, which has {Stars} star(s) now", publicKey, count);
        }
        else
        {
            logger.LogInformation("Took back a star of the source of lambda {Lambda}, which has {Stars} star(s) now", publicKey, count);
        }

        return new StarResponse(count, starred, true);
    }

    #endregion

    #region Helpers

    private async ValueTask<SourceArchive> RequireAsync(string publicKey, int version)
        => await sources.GetArchiveAsync(publicKey, version)
        ?? throw LambdaException.NotFound($"There is no version {version} of a published source at '{publicKey}'.");

    private static LambdaException NotPublished(string publicKey)
        => LambdaException.NotFound($"There is no published source at '{publicKey}'.");

    private static string ZipPath(string publicKey, int version) => $"/api/v1/sources/{publicKey}/versions/{version}/zip";

    /// <summary>
    /// The files of the project, by their path below its folder.
    /// </summary>
    private static IEnumerable<(string Path, ZipArchiveEntry Entry)> Entries(ZipArchive zip, string root)
        => zip.Entries.Where(e => e.FullName.StartsWith(root + "/", StringComparison.Ordinal) && !e.FullName.EndsWith('/'))
                      .Select(e => (e.FullName[(root.Length + 1)..], e));

    private static ZipArchiveEntry Find(ZipArchive zip, SourceArchive archive, string path)
        => zip.GetEntry($"{archive.Root}/{path}")
        ?? throw LambdaException.NotFound($"There is no file '{path}' in version {archive.Version}.");

    private static string? ReadText(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();

        using var buffer = new MemoryStream();

        stream.CopyTo(buffer);

        var bytes = buffer.ToArray();

        return IsText(bytes) ? Strict.GetString(bytes) : null;
    }

    /// <summary>
    /// Whether a file is text, going by how it begins - which is what can be
    /// told without reading the whole of a large one.
    /// </summary>
    private static bool LooksLikeText(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();

        var head = new byte[8192];

        var read = stream.ReadAtLeast(head, head.Length, false);

        if (Array.IndexOf(head, (byte)0, 0, read) >= 0)
        {
            return false;
        }

        // cut anywhere, the last character may be cut in half - so the last
        // few bytes are left out of the question
        var whole = read < head.Length ? read : Math.Max(0, read - 4);

        try
        {
            Strict.GetCharCount(head, 0, whole);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    private static readonly UTF8Encoding Strict = new(false, true);

    /// <summary>
    /// Whether bytes are text: UTF-8 through and through, without the zero
    /// bytes nothing but a binary format has.
    /// </summary>
    private static bool IsText(byte[] content)
    {
        if (Array.IndexOf(content, (byte)0) >= 0)
        {
            return false;
        }

        try
        {
            Strict.GetCharCount(content);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    /// <summary>
    /// A file name that cannot end the header it is written into.
    /// </summary>
    private static string Safe(string name)
        => new([.. name.Select(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '_')]);

    #endregion

}
