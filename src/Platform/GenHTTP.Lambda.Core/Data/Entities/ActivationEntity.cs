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
    /// Who put it online, one of <see cref="VersionOrigins" />.
    /// </summary>
    public string? Origin { get; set; }

    /// <summary>
    /// When it went offline, or null while it is still online.
    /// </summary>
    public DateTime? Ended { get; set; }

    /// <summary>
    /// Why it went offline, one of <see cref="ActivationEndings" />.
    /// </summary>
    public string? EndedBy { get; set; }

    public LambdaEntity? Lambda { get; set; }

}

/// <summary>The ways a stretch of being online ends.</summary>
public static class ActivationEndings
{

    /// <summary>Another deployment took its place.</summary>
    public const string Replaced = "replaced";

    /// <summary>The owner took it offline.</summary>
    public const string Stopped = "stopped";

    /// <summary>Nobody used it for long enough and the sweep took it down.</summary>
    public const string Expired = "expired";

    /// <summary>The operator took it offline.</summary>
    public const string Admin = "admin";

}
