namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// The meta data of a single lambda. The code itself lives in the storage service.
/// </summary>
public sealed class LambdaEntity
{

    public long Id { get; set; }

    /// <summary>
    /// The key the lambda is publicly hosted at (<c>/lambda/{PublicKey}</c>).
    /// </summary>
    public required string PublicKey { get; set; }

    /// <summary>
    /// The secret key the editor of this lambda is reachable with.
    /// </summary>
    public required string PrivateKey { get; set; }

    public LambdaTier Tier { get; set; }

    /// <summary>
    /// The domain the lambda answers at besides its path, such as
    /// <c>shop.example.com</c>.
    /// </summary>
    /// <remarks>
    /// Kept in the form it is matched in (see <c>DomainNames</c>) and only
    /// honoured in the premium tier - a lambda demoted keeps it, unserved,
    /// until it is promoted again.
    /// </remarks>
    public string? Domain { get; set; }

    /// <summary>
    /// The version that is currently deployed, if any.
    /// </summary>
    public int? ActiveVersion { get; set; }

    public DateTime Created { get; set; }

    public DateTime Modified { get; set; }

    /// <summary>
    /// When the active version was put online. Null while nothing is deployed.
    /// </summary>
    /// <remarks>
    /// Kept apart from the creation date of the version it runs: the same
    /// version can be taken down and put back up, and each time starts a fresh
    /// lifetime rather than continuing the one the code was written in.
    /// </remarks>
    public DateTime? Deployed { get; set; }

    /// <summary>
    /// When somebody last asked this lambda for something.
    /// </summary>
    /// <remarks>
    /// Written by the maintenance pass from counters kept in memory, so it
    /// trails real use by up to one interval - which is the right trade for a
    /// figure whose only job is to decide what has been abandoned. Null for a
    /// lambda nobody has called since the column existed.
    /// </remarks>
    public DateTime? LastSeen { get; set; }

    public List<DeploymentEntity> Deployments { get; set; } = [];

}
