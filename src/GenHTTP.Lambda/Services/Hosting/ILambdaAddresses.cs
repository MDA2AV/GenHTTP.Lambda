using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Where the lambdas answer: each at a subdomain of the hosting domain named
/// after its public key, or at a domain of its own.
/// </summary>
public interface ILambdaAddresses
{

    /// <summary>
    /// The domain the lambdas answer below, such as <c>genhttp.run</c>.
    /// </summary>
    string Domain { get; }

    /// <summary>
    /// The hosting domain as an address names it, with its port where it has
    /// one: <c>genhttp.run</c>, or <c>localhost:8080</c>.
    /// </summary>
    string Authority { get; }

    /// <summary>
    /// Where a lambda answers below it, with <c>{key}</c> where its public key
    /// goes: <c>https://{key}.genhttp.run/</c>.
    /// </summary>
    string Template { get; }

    /// <summary>
    /// The address of a lambda below the hosting domain, such as
    /// <c>https://quiz.genhttp.run/</c> - which sends its visitors on to a
    /// domain of its own while it answers at one.
    /// </summary>
    string Of(string publicKey);

    /// <summary>
    /// Where anything linking to a lambda should point: the root of its own
    /// domain while its tier serves one, its address below the hosting domain
    /// otherwise.
    /// </summary>
    string Of(string publicKey, LambdaTier tier, string? domain);

}
