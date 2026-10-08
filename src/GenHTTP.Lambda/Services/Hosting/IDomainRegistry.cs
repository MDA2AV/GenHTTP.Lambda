namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// The hosts being served on behalf of a lambda, and the lambda each is
/// served by.
/// </summary>
public interface IDomainRegistry
{

    /// <summary>
    /// How many domains of their own are being served.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// What a host belongs to: a lambda's domain, a subdomain of the hosting
    /// domain or that domain itself - or nothing, for the platform.
    /// </summary>
    /// <param name="host">A host in its normalized form, see <see cref="DomainNames.Normalize"/></param>
    LambdaHost? Find(string host);

    /// <summary>
    /// The domain the lambda with this key is served at, if it is served at one.
    /// </summary>
    string? DomainOf(string publicKey);

    /// <summary>
    /// Reads the served domains again, after anything that changes a domain,
    /// a tier or the key of a lambda.
    /// </summary>
    void Reload();

}
