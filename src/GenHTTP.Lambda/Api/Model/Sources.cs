using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A license a source can be published under.")]
public sealed record LicenseResponse(
    [Summary("Its SPDX identifier, which is how it is asked for: `MIT`, `Apache-2.0`, `GPL-3.0-or-later` …")]
    string Id,
    [Summary("Its full name")]
    string Name,
    [Summary("`Permissive`, `Copyleft` or `PublicDomain` - roughly what it asks of whoever reuses the code")]
    string Kind,
    [Summary("Where it is described in full")]
    string Url
)
{

    public static LicenseResponse Of(SourceLicense license) => new(license.Id, license.Name, license.Kind.ToString(), license.Url);

    public static LicenseResponse Of(string id) => Of(SourceLicenses.Find(id) ?? SourceLicenses.Find(SourceLicenses.Default)!);

}

#region Public

[Summary("A published source, as the listing shows it.")]
public sealed record SourceEntryResponse(
    [Summary("The public key of the lambda")]
    string PublicKey,
    [Summary("What its owner calls it in the showcase, where it is in the showcase")]
    string? Title,
    [Summary("What its newest version says it is: the first paragraph of its `docs/product.md`")]
    string? About,
    [Summary("What its owner says about it in the showcase, where it is there")]
    string? Description,
    [Summary("The license it is published under")]
    LicenseResponse License,
    [Summary("How many visitors starred it")]
    int Stars,
    [Summary("Whether the app answers right now")]
    bool Online,
    [Summary("Where the app answers - its own domain while it has one, its address below the hosting domain otherwise")]
    string Address,
    [Summary("Where its source is read on this site: `/source/{publicKey}`")]
    string Path,
    [Summary("Its picture in the showcase, where it has one")]
    string? ImagePath,
    [Summary("Its newest version")]
    int? LatestVersion,
    [Summary("When its newest version was saved")]
    DateTime? Updated,
    [Summary("When its source was published")]
    DateTime PublishedAt
)
{

    public static SourceEntryResponse Of(SourceEntry entry, ILambdaAddresses addresses) => new(
        entry.PublicKey,
        entry.Title,
        entry.About,
        entry.Description,
        LicenseResponse.Of(entry.License),
        entry.Stars,
        entry.Online,
        addresses.Of(entry.PublicKey, entry.Tier, entry.Domain),
        $"/source/{entry.PublicKey}",
        entry.Picture is { } picture ? $"/api/v1/showcases/{entry.PublicKey}/image?v={picture.Ticks}" : null,
        entry.LatestVersion,
        entry.Updated,
        entry.PublishedAt
    );

}

[Summary("A published source with its history.")]
public sealed record SourceProjectResponse(
    [Summary("The source, as the listing shows it")]
    SourceEntryResponse Source,
    [Summary("Who holds the copyright, where the owner named somebody")]
    string? Author,
    [Summary("Who the license names as holding it: the author, or the authors of the lambda")]
    string Holder,
    [Summary("The version online, if one is")]
    int? ActiveVersion,
    [Summary("When the lambda was created")]
    DateTime Created,
    [Summary("Every version kept, newest first: what each changed, never what was asked for")]
    IReadOnlyList<SourceVersionResponse> Versions,
    [Summary("What starring it is sent with (`POST …/star`): good from a second after it was handed out, for a day")]
    string StarTicket,
    [Summary("Where it is cloned with git on this site - every version, read only")]
    string GitPath
);

[Summary("A version of a published source.")]
public sealed record SourceVersionResponse(
    [Summary("Its number")]
    int Version,
    [Summary("When it was saved")]
    DateTime Created,
    [Summary("What it changed, in a line")]
    string? Change,
    [Summary("How it was saved: `template`, `api`, `agent` or `git`")]
    string? Origin,
    [Summary("Whether it is the version online")]
    bool Online,
    [Summary("Where it is downloaded as a project")]
    string ZipPath
);

[Summary("The files of a published version, as the project it is packed into has them.")]
public sealed record SourceTreeResponse(
    [Summary("The public key of the lambda")]
    string PublicKey,
    [Summary("The number of the version")]
    int Version,
    [Summary("The folder the project is in, inside the zip")]
    string Root,
    [Summary("How large the zip is, in bytes")]
    long Bytes,
    [Summary("Every file, by its path below the project's folder")]
    IReadOnlyList<SourceFileResponse> Files,
    [Summary("Where the zip is downloaded")]
    string ZipPath
);

[Summary("A file of a published version.")]
public sealed record SourceFileResponse(
    [Summary("Its path below the project's folder")]
    string Path,
    [Summary("How large it is, in bytes")]
    long Size,
    [Summary("What it is: `code`, `resource`, `docs`, `tests`, `platform` (what stands in for the platform) or `project` (what makes it a project)")]
    string Kind
);

[Summary("A file of a published version, with its text.")]
public sealed record SourceFileContentResponse(
    [Summary("Its path below the project's folder")]
    string Path,
    [Summary("How large it is, in bytes")]
    long Size,
    [Summary("What it is: `code`, `resource`, `docs`, `tests`, `platform` or `project`")]
    string Kind,
    [Summary("Whether it is text - which is then in `content`, unless it is longer than a megabyte")]
    bool Text,
    [Summary("Its text, while it is text no longer than a megabyte")]
    string? Content,
    [Summary("Where it is fetched as it is")]
    string RawPath
);

[Summary("A star given, or taken back.")]
public sealed record StarRequest(
    [Summary("The `starTicket` the source was read with")]
    string? Ticket,
    [Summary("`true`, or left out, to star it; `false` to take the star back")]
    bool? Starred
);

[Summary("How a source is starred now.")]
public sealed record StarResponse(
    [Summary("How many stars it has now")]
    int Stars,
    [Summary("Whether this address stars it now")]
    bool Starred,
    [Summary("Whether this request changed the count - it does not where the same was done from here before")]
    bool Counted
);

#endregion

#region Owner

[Summary("Whether a lambda's source is published, as its owner sees it, and what it can be published under.")]
public sealed record OwnSourceResponse(
    [Summary("What it is published under; absent when it never was")]
    SourceSettingsResponse? Source,
    [Summary("The licenses on offer, the default first")]
    IReadOnlyList<LicenseResponse> Licenses,
    [Summary("The license a source is published under unless another is asked for: `MIT`")]
    string Default,
    [Summary("How many characters the name in the license may have")]
    int MaxAuthor
);

[Summary("What a lambda's source is published under.")]
public sealed record SourceSettingsResponse(
    [Summary("Whether anybody may read it right now - it may have been taken down since")]
    bool Published,
    [Summary("The license")]
    LicenseResponse License,
    [Summary("Who holds the copyright, where the owner named somebody")]
    string? Author,
    [Summary("Who the license names: the author, or the authors of the lambda")]
    string Holder,
    [Summary("How many visitors starred it - kept while it is taken down")]
    int Stars,
    [Summary("When it was published")]
    DateTime PublishedAt,
    [Summary("When what it is published under last changed")]
    DateTime Updated,
    [Summary("Where it is read on this site: `/source/{publicKey}`")]
    string Path
)
{

    public static SourceSettingsResponse Of(SourceSettings settings) => new(
        settings.Published,
        LicenseResponse.Of(settings.License),
        settings.Author,
        SourceLicenses.Holder(settings.Author, settings.PublicKey),
        settings.Stars,
        settings.PublishedAt,
        settings.Updated,
        $"/source/{settings.PublicKey}"
    );

}

[Summary("What a lambda's source is to be published under. What is left out stays as it is.")]
public sealed record SourceRequest(
    [Summary("The SPDX identifier of one of the licenses on offer; `MIT` when a source is first published without one")]
    string? License,
    [Summary("Who holds the copyright, as the license is to name them, on one line; empty names nobody")]
    string? Author
);

#endregion
