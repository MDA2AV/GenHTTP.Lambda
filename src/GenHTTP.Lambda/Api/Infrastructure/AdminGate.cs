using System.Security.Cryptography;
using System.Text;

using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// The one check that stands between a request and everything an operator can
/// see.
/// </summary>
/// <remarks>
/// A wrong token and an installation with no panel at all answer identically,
/// on purpose: the alternative tells anyone who asks whether this server has an
/// administrator worth guessing a token for.
/// </remarks>
public static class AdminGate
{

    private const string Header = "X-Admin-Token";

    private const string Absent = "This installation has no administration panel.";

    /// <summary>
    /// Lets the request through, or ends it as though there were nothing here.
    /// </summary>
    public static void Require(IRequest request, LambdaOptions options)
    {
        if (!Allows(request, options))
        {
            throw LambdaException.NotFound(Absent);
        }
    }

    /// <summary>
    /// Whether the request carries the configured token.
    /// </summary>
    public static bool Allows(IRequest request, LambdaOptions options)
    {
        if (!options.Administrable)
        {
            return false;
        }

        var presented = request.Header.Headers.GetEntry(Header);

        var expected = Encoding.UTF8.GetBytes(options.AdminToken!);

        var actual = Encoding.UTF8.GetBytes(presented ?? string.Empty);

        // the length is not a secret, but which byte differs is
        return actual.Length == expected.Length && CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    /// <summary>
    /// Lets the request through if it is entitled to figures about the server
    /// itself.
    /// </summary>
    /// <remarks>
    /// An installation that has an administrator keeps these to them. One that
    /// has none has nothing to check a request against, so keeping them back
    /// would only mean nobody could ever see them - there they stay public, as
    /// they were before there was a panel to put them behind.
    /// </remarks>
    public static void RequireForFigures(IRequest request, LambdaOptions options)
    {
        if (options.Administrable && !Allows(request, options))
        {
            throw LambdaException.NotFound(Absent);
        }
    }

}
