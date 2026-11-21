namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// Everything anybody may know about a public key: whether it could be
/// claimed, and whether something is already answering there.
/// </summary>
/// <param name="PublicKey">The key as it would be stored</param>
/// <param name="Valid">Whether the key is one a lambda could have</param>
/// <param name="Exists">Whether a lambda has it</param>
/// <param name="Deployed">Whether that lambda is online</param>
/// <param name="Reason">Why it cannot be claimed, if it cannot</param>
public sealed record KeyStatus(string PublicKey, bool Valid, bool Exists, bool Deployed, string? Reason)
{

    /// <summary>
    /// Whether a new lambda could be created with this key.
    /// </summary>
    public bool Available => Valid && !Exists;

}
