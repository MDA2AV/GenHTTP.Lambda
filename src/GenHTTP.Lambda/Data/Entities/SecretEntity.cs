namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// One secret of a lambda - an API key, a password, a token - sealed.
/// </summary>
/// <remarks>
/// Only the name is readable here. The value is what <c>SecretCipher</c> made
/// of it, and nothing that reads this table gets the value back without the
/// key of the installation, which is not in the database.
/// </remarks>
public sealed class SecretEntity
{

    public long Id { get; set; }

    public long LambdaId { get; set; }

    /// <summary>
    /// The feature whose copy of the secrets this belongs to, or nothing for
    /// the lambda's own.
    /// </summary>
    public long? FeatureId { get; set; }

    /// <summary>
    /// What the code reads it by, such as <c>STRIPE_KEY</c>.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The sealed value: a format byte, the nonce, the tag and the cipher text.
    /// </summary>
    public required byte[] Value { get; set; }

    public DateTime Created { get; set; }

    /// <summary>
    /// When the value was last replaced.
    /// </summary>
    public DateTime Changed { get; set; }

}
