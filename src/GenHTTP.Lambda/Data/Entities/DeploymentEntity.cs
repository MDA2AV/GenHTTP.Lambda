namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// An immutable version of the code of a lambda. Saving or deploying creates a
/// new record, existing ones are never updated.
/// </summary>
public sealed class DeploymentEntity
{

    public long Id { get; set; }

    public long LambdaId { get; set; }

    /// <summary>
    /// The version number, starting at one and counting up per lambda.
    /// </summary>
    public int Version { get; set; }

    public DateTime Created { get; set; }

    public LambdaEntity? Lambda { get; set; }

}
