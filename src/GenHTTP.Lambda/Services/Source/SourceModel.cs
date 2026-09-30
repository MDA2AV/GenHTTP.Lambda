using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// Whether the source of a lambda is published, as its owner sees it.
/// </summary>
/// <param name="Published">Whether anybody may read it right now</param>
/// <param name="License">The SPDX identifier of the license it is published under</param>
/// <param name="Author">Who holds the copyright, or nothing for "the authors of" the lambda</param>
/// <param name="Stars">How many visitors starred it, kept while it is taken down</param>
/// <param name="PublishedAt">When it was last published</param>
public sealed record SourceSettings(string PublicKey, bool Published, string License, string? Author, int Stars, DateTime PublishedAt,
                                    DateTime Updated);

/// <summary>
/// What an owner wants published: what is left out stays as it is, and an
/// empty author names nobody.
/// </summary>
public sealed record SourceDraft(string? License, string? Author);

/// <summary>
/// How the listing is ordered.
/// </summary>
public enum SourceOrder
{

    /// <summary>
    /// The most starred first, and among the same the most recently changed.
    /// </summary>
    Stars,

    /// <summary>
    /// The one whose newest version is the newest first.
    /// </summary>
    Updated,

    /// <summary>
    /// The one published last first.
    /// </summary>
    Published

}

/// <summary>
/// A published source, as the listing shows it.
/// </summary>
/// <param name="Title">What its owner calls it on the showcase, if it is on the showcase</param>
/// <param name="About">What its newest version says it is: the first paragraph of its product page</param>
/// <param name="Description">What its owner says about it on the showcase, if it is there</param>
/// <param name="Online">Whether the app answers right now</param>
/// <param name="Updated">When its newest version was saved</param>
/// <param name="Picture">When its showcase picture last changed, if it has one - which names the version of it</param>
/// <param name="PictureType">What that picture is, as it was sniffed</param>
public sealed record SourceEntry(
    string PublicKey,
    string? Title,
    string? About,
    string? Description,
    string License,
    int Stars,
    bool Online,
    LambdaTier Tier,
    string? Domain,
    int? LatestVersion,
    DateTime? Updated,
    DateTime PublishedAt,
    DateTime? Picture,
    string? PictureType
);

/// <summary>
/// One page of the listing.
/// </summary>
/// <param name="Total">How many match altogether</param>
public sealed record SourceListing(IReadOnlyList<SourceEntry> Entries, int Total);

/// <summary>
/// A published source with everything its page shows about the lambda.
/// </summary>
/// <param name="Author">Who holds the copyright, if the owner named somebody</param>
/// <param name="ActiveVersion">The version online, if one is</param>
/// <param name="Created">When the lambda was made</param>
/// <param name="Versions">Every version kept, newest first</param>
public sealed record SourceProject(SourceEntry Entry, string? Author, int? ActiveVersion, DateTime Created, IReadOnlyList<SourceVersion> Versions);

/// <summary>
/// A version as the history of a published source lists it: when, and what
/// it changed - never what was asked for in whose words, which stays with the
/// owner.
/// </summary>
/// <param name="Origin">Which door it came through: template, api, agent</param>
public sealed record SourceVersion(int Version, DateTime Created, string? Change, string? Origin);

/// <summary>
/// A version of a published source, packed.
/// </summary>
/// <param name="File">Where the zip is</param>
/// <param name="Root">The folder the project is in, inside the zip - named after the lambda</param>
public sealed record SourceArchive(string PublicKey, int Version, string File, string Root);

/// <summary>
/// How many stars a published source has.
/// </summary>
/// <param name="Id">The identity it is filed under</param>
public sealed record SourceStars(long Id, int Stars);

/// <summary>
/// A name for somebody who shares the address of a published source.
/// </summary>
/// <param name="Updated">When its newest version was saved, for the sitemap</param>
public sealed record SourceAddress(string PublicKey, DateTime? Updated);

/// <summary>
/// What a file of a packed project is to somebody reading it.
/// </summary>
public static class SourceKinds
{

    /// <summary>The lambda's code: Project.cs, and every other file of its own.</summary>
    public const string Code = "code";

    /// <summary>What it ships to be served: its front end, its migrations.</summary>
    public const string Asset = "asset";

    /// <summary>What was written about it.</summary>
    public const string Docs = "docs";

    /// <summary>How it is tested.</summary>
    public const string Tests = "tests";

    /// <summary>What stands in for the platform.</summary>
    public const string Platform = "platform";

    /// <summary>What makes it a project: the host, the project file, the Dockerfile, the license.</summary>
    public const string Project = "project";

    /// <summary>
    /// The kind of a file, by where the packer put it.
    /// </summary>
    /// <param name="path">Its path below the project's folder</param>
    public static string Of(string path)
    {
        if (path.StartsWith("assets/", StringComparison.Ordinal))
        {
            return Asset;
        }

        if (path.StartsWith("docs/", StringComparison.Ordinal))
        {
            return Docs;
        }

        if (path.StartsWith("tests/", StringComparison.Ordinal))
        {
            return Tests;
        }

        if (path.StartsWith("Platform/", StringComparison.Ordinal))
        {
            return Platform;
        }

        if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && path != "Program.cs")
        {
            return Code;
        }

        return Project;
    }

}
