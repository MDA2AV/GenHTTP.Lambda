namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// One stretch of time during which a version of a lambda was online.
/// </summary>
/// <remarks>
/// Opened by a deployment and closed by whatever ends it - the next
/// deployment, the owner taking it down, the maintenance sweep or the
/// operator. Nothing else ever changes one, so the table reads as the history
/// of what was being served and why it stopped.
/// </remarks>
public sealed class ActivationEntity
{

    public long Id { get; set; }

    public long LambdaId { get; set; }

    /// <summary>
    /// The version that was online. It may since have been pruned from the
    /// version history, which is why this is a number and not a reference.
    /// </summary>
    public int Version { get; set; }

    public DateTime Started { get; set; }

    /// <summary>
    /// Who put it online.
    /// </summary>
    public VersionOrigin? Origin { get; set; }

    /// <summary>
    /// When it went offline, or null while it is still online.
    /// </summary>
    public DateTime? Ended { get; set; }

    /// <summary>
    /// Why it went offline.
    /// </summary>
    public ActivationEnding? EndedBy { get; set; }

    public LambdaEntity? Lambda { get; set; }

}
