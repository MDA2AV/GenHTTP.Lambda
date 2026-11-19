namespace GenHTTP.Lambda.Integrations.Secrets;

/// <summary>
/// Where the API keys and passwords of the lambdas are kept: values that are
/// written and never read back by anybody but the lambda they belong to.
/// </summary>
/// <remarks>
/// <para>
/// The platform hands out a value only to the running lambda it belongs to,
/// and checks the name, the size and how many a lambda may have before it
/// asks here; a provider keeps values and nothing else. What it is asked for
/// to show anybody else is <see cref="List"/>, which holds no value.
/// </para>
/// <para>
/// A lambda reads its secrets while it serves requests, so the platform holds
/// what <see cref="Load"/> answered until something changes it. A provider
/// keeping them on another machine is asked again only then.
/// </para>
/// </remarks>
public interface ISecretProvider : IIntegration
{

    /// <summary>
    /// The secrets of a lambda, or a feature's copy, as they are now - to be
    /// read by the lambda they belong to and nobody else.
    /// </summary>
    ISecretSet Load(DataScope scope);

    /// <summary>
    /// The names of the secrets kept, without their values.
    /// </summary>
    IReadOnlyList<SecretEntry> List(DataScope scope);

    /// <summary>
    /// Keeps a value under a name, replacing the one there.
    /// </summary>
    SecretEntry Store(DataScope scope, string name, string value);

    /// <summary>
    /// Removes a secret, and says whether there was one.
    /// </summary>
    bool Delete(DataScope scope, string name);

    /// <summary>
    /// Replaces the secrets of one scope with those of another.
    /// </summary>
    void Copy(DataScope source, DataScope target);

    /// <summary>
    /// Removes every secret of one scope, such as a feature's copy.
    /// </summary>
    void Delete(DataScope scope);

    /// <summary>
    /// Removes every secret of a lambda, its features' copies included - a
    /// secret stored later starts afresh.
    /// </summary>
    void DeleteAll(long lambdaId);

}

/// <summary>
/// What a lambda reads its secrets from.
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

/// <summary>
/// A secret, as anybody but its lambda sees it: by its name.
/// </summary>
/// <param name="Created">When it was first stored, in UTC</param>
/// <param name="Changed">When its value was last replaced, in UTC</param>
public sealed record SecretEntry(string Name, DateTime Created, DateTime Changed);
