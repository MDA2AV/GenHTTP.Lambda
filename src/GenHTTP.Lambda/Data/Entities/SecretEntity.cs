namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// One secret of a lambda, or of one of its features' copy of them.
/// </summary>
/// <remarks>
/// <see cref="Value" /> is what <c>SecretCipher</c> made of the value: never
/// the value itself, and nothing here can be turned back into it without the
/// secret of the installation.
/// </remarks>
public sealed class SecretEntity
{

    public long Id { get; set; }

    public long LambdaId { get; set; }

    /// <summary>
    /// The feature whose copy this is, or nothing for the lambda's own.
    /// </summary>
    public long? FeatureId { get; set; }

    /// <summary>
    /// What the code asks for it by, such as <c>STRIPE_API_KEY</c>.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The encrypted value.
    /// </summary>
    public required byte[] Value { get; set; }

    public DateTime Created { get; set; }

    /// <summary>
    /// When the value was last set.
    /// </summary>
    public DateTime Updated { get; set; }

    public LambdaEntity? Lambda { get; set; }

    public FeatureEntity? Feature { get; set; }

}
