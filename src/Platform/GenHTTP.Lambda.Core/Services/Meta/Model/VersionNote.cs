using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// Why a version was written, as whoever wrote it tells it.
/// </summary>
/// <remarks>
/// Both parts are optional and neither is checked against the code: they are
/// the account of the author, kept beside what the author did. The origin is
/// not theirs to give - it is filled in by the door the version came through.
/// </remarks>
/// <param name="Specification">What the user wants from the version and why, in their words where possible</param>
/// <param name="Change">What the version changed, in a line</param>
/// <param name="Origin">Which door it came through</param>
/// <param name="Author">Who wrote it, where the platform knows it - the build agent, on a job it issued a ticket for</param>
public sealed record VersionNote(string? Specification = null, string? Change = null, VersionOrigin? Origin = null, VersionAuthor? Author = null)
{

    /// <summary>
    /// How long a specification may be before the rest of it is cut.
    /// </summary>
    public const int MaxSpecification = 4000;

    /// <summary>
    /// How long the line about the change may be.
    /// </summary>
    public const int MaxChange = 500;

}
