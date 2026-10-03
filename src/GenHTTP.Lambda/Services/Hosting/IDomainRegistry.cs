namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// The domains being served, and the lambda each is served by.
/// </summary>
public interface IDomainRegistry
{

    /// <summary>
    /// How many domains are being served.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// The lambda a domain is served by, if it is served by one.
    /// </summary>
    bool TryFind(string domain, out long lambdaId);

    /// <summary>
    /// Reads the served domains again, after anything that changes a domain
    /// or a tier.
    /// </summary>
    void Reload();

}
