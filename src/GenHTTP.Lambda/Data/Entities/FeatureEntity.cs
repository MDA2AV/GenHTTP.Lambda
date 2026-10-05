namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// A change being worked on beside a lambda: its own files, its own copy of
/// the lambda's data and an address of its own, until it is merged or deleted.
/// </summary>
/// <remarks>
/// Its files and its data are on disk, like the code of the versions. This is
/// what the platform needs to know about it: whose it is, what it is for,
/// which version it branched off, and whether its preview is online.
/// </remarks>
public sealed class FeatureEntity
{

    public long Id { get; set; }

    public long LambdaId { get; set; }

    /// <summary>
    /// What it is addressed by: in the API, and at <c>/features/{Key}/</c>,
    /// where its preview answers. Random and long, since whoever has it can
    /// open the preview.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// What it is, in a few words.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The branch it is in the lambda's git repository, unique per lambda.
    /// </summary>
    /// <remarks>
    /// Given once, when it starts - the branch that was pushed, or its name
    /// as git allows it - and kept when it is renamed, since a clone knows
    /// the branch by this.
    /// </remarks>
    public required string Branch { get; set; }

    /// <summary>
    /// What the user wants from it and why, which the version it is merged
    /// into keeps.
    /// </summary>
    public string? Specification { get; set; }

    /// <summary>
    /// What it changes, in a line, which the version it is merged into keeps.
    /// </summary>
    public string? Change { get; set; }

    /// <summary>
    /// The version it branched off - or, once whoever works on it brought the
    /// changes of a later version in, that version. Only a feature based on
    /// the newest version can be merged.
    /// </summary>
    public int BaseVersion { get; set; }

    /// <summary>
    /// Where it came from, one of <see cref="VersionOrigins" />.
    /// </summary>
    public string? Origin { get; set; }

    public DateTime Created { get; set; }

    /// <summary>
    /// When its files, notes or preview last changed.
    /// </summary>
    public DateTime Modified { get; set; }

    /// <summary>
    /// How many times its preview was put online, which tells a preview built
    /// from one deployment from one built from the next.
    /// </summary>
    public int Preview { get; set; }

    /// <summary>
    /// When its preview was last put online, or null while it is offline.
    /// </summary>
    public DateTime? Previewed { get; set; }

    /// <summary>
    /// How many times its files were saved, starting at one for the files it
    /// began with - so a preview can tell whether it serves what the feature
    /// holds now.
    /// </summary>
    public int Revision { get; set; } = 1;

    /// <summary>
    /// Which save of its files the preview was built from, or null before it
    /// was first put online.
    /// </summary>
    public int? PreviewOf { get; set; }

    public LambdaEntity? Lambda { get; set; }

}
