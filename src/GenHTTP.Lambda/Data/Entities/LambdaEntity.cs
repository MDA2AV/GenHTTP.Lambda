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
    /// Whether the installation maintains this one as an example.
    /// </summary>
    /// <remarks>
    /// Neither sweep touches it: an example nobody has opened for a month is
    /// still wanted, and one deployed yesterday should still be answering.
    /// </remarks>
    public bool IsExample { get; set; }

    public List<DeploymentEntity> Deployments { get; set; } = [];

}
