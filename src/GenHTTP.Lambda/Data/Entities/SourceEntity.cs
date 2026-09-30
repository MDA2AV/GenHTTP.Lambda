namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// Whether the owner of a lambda published its source code, and under which
/// license.
/// </summary>
/// <remarks>
/// At most one per lambda, which is why the lambda is the key. Taking the
/// source down keeps the row and clears <see cref="Published" />, so the stars
/// it was given are still there when it is published again.
/// </remarks>
public sealed class SourceEntity
{

    public long LambdaId { get; set; }

    /// <summary>
    /// Whether anybody may read it right now.
    /// </summary>
    public bool Published { get; set; }

    /// <summary>
    /// The SPDX identifier of the license it is published under.
    /// </summary>
    public required string License { get; set; }

    /// <summary>
    /// Who holds the copyright, as the license names them - or nothing, for
    /// "the authors of" the lambda.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// How many visitors starred it.
    /// </summary>
    public int Stars { get; set; }

    /// <summary>
    /// What the newest version says the app is, kept for the listing, which
    /// would otherwise read every version it shows.
    /// </summary>
    public string? About { get; set; }

    /// <summary>
    /// The version <see cref="About" /> was read from.
    /// </summary>
    public int? AboutVersion { get; set; }

    /// <summary>
    /// When it was last published.
    /// </summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// When the owner last changed any of it.
    /// </summary>
    public DateTime Updated { get; set; }

    public LambdaEntity? Lambda { get; set; }

}
