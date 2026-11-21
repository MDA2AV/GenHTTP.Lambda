namespace GenHTTP.Lambda.Integrations.Secrets;

/// <summary>
/// What a lambda reads its secrets from, held in memory once loaded.
/// </summary>
public interface ISecretSet
{

    /// <summary>
    /// How many secrets there are.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// The value of a secret, or nothing where there is none of that name.
    /// </summary>
    string? Read(string name);

}
