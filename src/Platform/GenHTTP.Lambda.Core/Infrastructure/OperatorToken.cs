using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Whether somebody presented the admin token of the installation - the one
/// check every door that lets the operator in makes the same way.
/// </summary>
/// <remarks>
/// The REST API asks for it in a header (<c>AdminAuthentication</c>), an MCP
/// tool as an argument; both compare here, so neither reaches into the
/// other. In fixed time: the length of the token is not a secret, but which
/// byte differs is.
/// </remarks>
public static class OperatorToken
{

    /// <summary>
    /// Whether the given value is the admin token - never, on an installation
    /// that has none.
    /// </summary>
    public static bool Matches(LambdaOptions options, string? presented)
    {
        if (!options.Administrable || presented == null)
        {
            return false;
        }

        var expected = Encoding.UTF8.GetBytes(options.AdminToken!);
        var actual = Encoding.UTF8.GetBytes(presented);

        return actual.Length == expected.Length && CryptographicOperations.FixedTimeEquals(actual, expected);
    }

}
