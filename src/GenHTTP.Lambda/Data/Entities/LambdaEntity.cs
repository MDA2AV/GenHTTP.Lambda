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

    public List<DeploymentEntity> Deployments { get; set; } = [];

}
