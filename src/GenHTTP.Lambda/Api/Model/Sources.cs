using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// A license a source can be published under.
/// </summary>
/// <param name="Id">Its SPDX identifier, which is how it is asked for</param>
/// <param name="Kind">Permissive, Copyleft or PublicDomain - roughly what it asks of whoever reuses the code</param>
/// <param name="Url">Where it is described in full</param>
public sealed record LicenseResponse(string Id, string Name, string Kind, string Url)
{

    public static LicenseResponse Of(SourceLicense license) => new(license.Id, license.Name, license.Kind.ToString(), license.Url);

    public static LicenseResponse Of(string id) => Of(SourceLicenses.Find(id) ?? SourceLicenses.Find(SourceLicenses.Default)!);

}

#region Public

/// <summary>
/// A published source, as the listing shows it.
/// </summary>
/// <param name="Title">What its owner calls it on the showcase, if it is on the showcase</param>
/// <param name="About">What its newest version says it is - the first paragraph of its documentation</param>
/// <param name="Description">What its owner says about it on the showcase, if it is there</param>
/// <param name="Online">Whether the app answers right now</param>
/// <param name="Address">Where the app answers - its own domain while it has one, its address below the hosting domain otherwise</param>
/// <param name="Path">Where its source is read, without a language</param>
/// <param name="ImagePath">Its picture on the showcase, if it has one</param>
/// <param name="LatestVersion">Its newest version</param>
/// <param name="Updated">When its newest version was saved</param>
/// <param name="PublishedAt">When its source was published</param>
public sealed record SourceEntryResponse(
    string PublicKey,
    string? Title,
    string? About,
    string? Description,
    LicenseResponse License,
    int Stars,
    bool Online,
    string Address,
    string Path,
    string? ImagePath,
    int? LatestVersion,
    DateTime? Updated,
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

/// <summary>
/// A published source with its history.
/// </summary>
/// <param name="Author">Who holds the copyright, if the owner named somebody</param>
/// <param name="Holder">Who the license names as holding it - the author, or the authors of the lambda</param>
/// <param name="ActiveVersion">The version online, if one is</param>
/// <param name="Created">When the lambda was made</param>
/// <param name="Versions">Every version kept, newest first: what each changed, never what was asked for</param>
/// <param name="StarTicket">What starring it has to be sent with, good for a day from a second on</param>
/// <param name="GitPath">Where it is cloned with git - every version, read only</param>
public sealed record SourceProjectResponse(
    SourceEntryResponse Source,
    string? Author,
    string Holder,
    int? ActiveVersion,
    DateTime Created,
    IReadOnlyList<SourceVersionResponse> Versions,
    string StarTicket,
    string GitPath
);

/// <param name="Change">What it changed, in a line</param>
/// <param name="Origin">Which door it came through: template, api, agent or git</param>
/// <param name="Online">Whether this is the version online</param>
/// <param name="ZipPath">Where it is downloaded as a project</param>
public sealed record SourceVersionResponse(int Version, DateTime Created, string? Change, string? Origin, bool Online, string ZipPath);

/// <summary>
/// The files of a version, as the project it is packed into has them.
/// </summary>
/// <param name="Root">The folder the project is in, inside the zip</param>
/// <param name="Bytes">How large the zip is</param>
/// <param name="Files">Every file, by its path below the project's folder</param>
/// <param name="ZipPath">Where it is downloaded</param>
public sealed record SourceTreeResponse(string PublicKey, int Version, string Root, long Bytes, IReadOnlyList<SourceFileResponse> Files, string ZipPath);

/// <param name="Kind">code, resource, docs, tests, platform or project</param>
public sealed record SourceFileResponse(string Path, long Size, string Kind);

/// <summary>
/// One file of a version.
/// </summary>
/// <param name="Text">Whether it is text, which is then in Content</param>
/// <param name="Content">The text, while it is text and not too long to show</param>
/// <param name="RawPath">Where it is fetched as it is</param>
public sealed record SourceFileContentResponse(string Path, long Size, string Kind, bool Text, string? Content, string RawPath);

/// <summary>
/// A star given, or taken back.
/// </summary>
/// <param name="Ticket">What the page was handed with the source</param>
/// <param name="Starred">True, or left out, to star it; false to take the star back</param>
public sealed record StarRequest(string? Ticket, bool? Starred);

/// <param name="Stars">How many it has now</param>
/// <param name="Counted">Whether this one changed the count - it does not when the same was done from here before</param>
public sealed record StarResponse(int Stars, bool Starred, bool Counted);

#endregion

#region Owner

/// <summary>
/// Whether a lambda's source is published, as its owner sees it, and what it
/// can be published under.
/// </summary>
/// <param name="Source">What it is published under, or nothing when it never was</param>
/// <param name="Licenses">The licenses on offer, the default first</param>
/// <param name="MaxAuthor">How long the name in the license may be</param>
public sealed record OwnSourceResponse(SourceSettingsResponse? Source, IReadOnlyList<LicenseResponse> Licenses, string Default, int MaxAuthor);

/// <param name="Published">Whether anybody may read it right now</param>
/// <param name="Author">Who holds the copyright, if the owner named somebody</param>
/// <param name="Holder">Who the license names - the author, or the authors of the lambda</param>
/// <param name="Stars">How many visitors starred it, kept while it is taken down</param>
/// <param name="Path">Where it is read, without a language</param>
public sealed record SourceSettingsResponse(bool Published, LicenseResponse License, string? Author, string Holder, int Stars,
                                            DateTime PublishedAt, DateTime Updated, string Path)
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

/// <summary>
/// What a lambda's source is to be published under. What is left out stays as
/// it is; an empty author names nobody.
/// </summary>
/// <param name="License">An SPDX identifier from the licenses on offer - MIT when a source is first published without one</param>
/// <param name="Author">Who holds the copyright, as the license is to name them</param>
public sealed record SourceRequest(string? License, string? Author);

#endregion
