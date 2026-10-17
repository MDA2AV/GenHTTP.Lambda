using System.IO.Compression;
using System.Text;

using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Hosting;
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
/// the lambda's database, workspace or secrets. Listed under the tag
/// <see cref="LambdaSourceResource"/> describes.
/// </remarks>
[Tag(ApiDocumentation.OpenSource)]
public sealed class SourceResource(ISourceService sources, ILambdaAddresses addresses, ILogger<SourceResource> logger)
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

    [ResourceMethod]
    [Summary("Lists the published sources, a page at a time")]
    [Remarks("Public. Every lambda whose owner published its code, whether or not it is online.")]
    [return: Summary("A page of sources, and where the next one starts")]
    public Page<SourceEntryResponse> List([Summary("Words that the key, the title or what it is about all have to contain")] string? search,
                                          [Summary("`stars` (the default), the most starred first; `updated`, the most recently saved first; or `published`, the most recently published first")] string? order,
                                          [Summary("How many sources to leave out, from the start")] int skip = 0,
                                          [Summary("How many sources to answer with, 1 to 48")] int take = PageSize)
    {
        var from = Math.Max(0, skip);

        var ordered = order?.ToLowerInvariant() switch
        {
            "updated" => SourceOrder.Updated,
            "published" or "new" => SourceOrder.Published,
            _ => SourceOrder.Stars
        };

        var page = sources.List(search, ordered, from, take);

        return Page<SourceEntryResponse>.Of([.. page.Entries.Select(e => SourceEntryResponse.Of(e, addresses))], from, page.Total);
    }

    [ResourceMethod(":publicKey")]
    [Summary("Reads a published source")]
    [Remarks("What it is, under which license, where it runs, every version kept and what each changed - and a `starTicket` to star it with.")]
    [Response(ResponseStatus.NotFound, "There is no published source at this public key - it does not exist, or its owner has not published it, which are answered alike.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The source, its versions and a ticket to star it with")]
    public SourceProjectResponse Get([Summary("The public key of the lambda")] string publicKey)
    {
        var project = sources.GetProject(publicKey) ?? throw NotPublished(publicKey);

        var entry = SourceEntryResponse.Of(project.Entry, addresses);

        var versions = project.Versions.Select(v => new SourceVersionResponse(v.Version, v.Created, v.Change, v.Origin,
                                                                              v.Version == project.ActiveVersion && entry.Online,
                                                                              ZipPath(publicKey, v.Version)));

        return new SourceProjectResponse(entry, project.Author, SourceLicenses.Holder(project.Author, publicKey), project.ActiveVersion,
                                         project.Created, [.. versions], sources.IssueTicket(publicKey), $"/source/{publicKey}.git");
    }

    #endregion

    #region Versions

    [ResourceMethod(":publicKey/versions/:version")]
    [Summary("Lists the files of a published version")]
    [Remarks("""
             As the project it is packed into has them - the export of that version, without its data. Packed on the first
             request for the version and kept, so the first answer may take a moment for a large lambda.
             """)]
    [Response(ResponseStatus.NotFound, "There is no published source at this public key, or it has no such version.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The files of the version")]
    public async ValueTask<SourceTreeResponse> Tree([Summary("The public key of the lambda")] string publicKey, [Summary(ApiDocumentation.VersionNumber)] int version)
    {
        var archive = await RequireAsync(publicKey, version);

        using var zip = ZipFile.OpenRead(archive.File);

        var files = Entries(zip, archive.Root).Select(e => new SourceFileResponse(e.Path, e.Entry.Length, SourceKinds.Of(e.Path)))
                                              .OrderBy(f => f.Path, StringComparer.Ordinal)
                                              .ToList();

        return new SourceTreeResponse(publicKey, version, archive.Root, new FileInfo(archive.File).Length, files, ZipPath(publicKey, version));
    }

    [ResourceMethod(":publicKey/versions/:version/files/:path")]
    [Summary("Reads a file of a published version")]
    [Remarks("Its text, while it is UTF-8 text no longer than a megabyte, and either way where to fetch it as it is (`rawPath`).")]
    [Response(ResponseStatus.NotFound, "There is no published source at this public key, it has no such version, or the version has no such file.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The file, with its text where it is text")]
    public async ValueTask<SourceFileContentResponse> File([Summary("The public key of the lambda")] string publicKey, [Summary(ApiDocumentation.VersionNumber)] int version,
                                                           [Summary("Its path below the project's folder, encoded as one segment: `docs%2Fproduct.md`")] string path)
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

    /// <remarks>
    /// Served from the origin of the site, so a page or a script a lambda
    /// ships is sent as text here, and whatever is sent is sandboxed and never
    /// sniffed. That is what lets the documentation show the pictures beside
    /// it without letting a published source run anything on this site.
    /// </remarks>
    [ResourceMethod(":publicKey/versions/:version/raw/:path")]
    [Summary("Fetches a file of a published version as it is")]
    [Remarks("""
             A picture as a picture, anything else as plain text or as bytes - never as something a browser would run: a page
             or a script is sent as text, under a sandboxing content security policy. Bytes, and anything asked for with
             `download`, are sent to be saved.
             """)]
    [Response(ResponseStatus.Ok, "The file: a picture, `text/plain` or `application/octet-stream`", ContentType = "*/*")]
    [ResponseHeader(ResponseStatus.Ok, "Content-Disposition", "`attachment`, with the name of the file - where it is bytes or `download` was asked for")]
    [Response(ResponseStatus.NotFound, "There is no published source at this public key, it has no such version, or the version has no such file.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<IResponse> Raw([Summary("The public key of the lambda")] string publicKey, [Summary(ApiDocumentation.VersionNumber)] int version,
                                          [Summary("Its path below the project's folder, encoded as one segment")] string path,
                                          [Summary("Anything but empty sends it to be saved rather than shown: `?download=1`")] string? download, IRequest request)
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

    [ResourceMethod(":publicKey/versions/:version/zip")]
    [Summary("Downloads a published version as a project")]
    [Remarks("A .NET 10 project with a `Dockerfile`, its documentation and tests, and its `LICENSE` - and none of the data the lambda keeps.")]
    [Response(ResponseStatus.Ok, "The project, as a zip archive", ContentType = ApiDocumentation.Zip)]
    [ResponseHeader(ResponseStatus.Ok, "Content-Disposition", "`attachment`, named after the public key and the version: `quiz-v3.zip`")]
    [Response(ResponseStatus.NotFound, "There is no published source at this public key, or it has no such version.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<IResponse> Zip([Summary("The public key of the lambda")] string publicKey, [Summary(ApiDocumentation.VersionNumber)] int version, IRequest request)
    {
        var archive = await RequireAsync(publicKey, version);

        logger.LogInformation("Downloaded source of lambda {Lambda} version {Version}", publicKey, version);

        return request.Respond()
                      .Content(Resource.FromFile(archive.File).Type(new ContentType("application/zip")).Build())
                      .Header("Content-Disposition", $"attachment; filename=\"{Safe(publicKey)}-v{version}.zip\"")
                      .Header("Cache-Control", "public, max-age=300")
                      .Build();
    }

    #endregion

    #region Stars

    /// <remarks>
    /// A POST with the ticket the source was read with, not a link: something
    /// that follows every link it finds stars nothing.
    /// </remarks>
    [ResourceMethod(Method.Post, ":publicKey/star")]
    [Summary("Stars a published source, or takes the star back")]
    [Remarks("""
             With the `starTicket` the source was read with, which is good from a second after it was handed out for a day.
             Each address stars a source once - starring it again changes nothing, and `counted` says so - and an address may
             star only so many sources in a while.
             """)]
    [Response(ResponseStatus.BadRequest, "The ticket is missing, too fresh or too old. Read the source again and star it then.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.TooManyRequests, "This address starred a lot in a short while. Try again in a few minutes.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "There is no published source at this public key - it does not exist, or its owner has not published it, which are answered alike.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("How it is starred now")]
    public StarResponse Star([Summary("The public key of the lambda")] string publicKey, IRequest request, StarRequest body)
    {
        var outcome = sources.Star(publicKey, body?.Ticket, request.Client.Address, body?.Starred ?? true)
                   ?? throw NotPublished(publicKey);

        if (outcome.Changed)
        {
            if (outcome.Starred)
            {
                logger.LogInformation("Starred source of lambda {Lambda} stars {Stars}", publicKey, outcome.Stars);
            }
            else
            {
                logger.LogInformation("Unstarred source of lambda {Lambda} stars {Stars}", publicKey, outcome.Stars);
            }
        }

        return new StarResponse(outcome.Stars, outcome.Starred, outcome.Changed);
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
