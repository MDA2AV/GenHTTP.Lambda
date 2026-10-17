using System.Security.Cryptography;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;

using GenHTTP.Lambda.Configuration;

using GenHTTP.Modules.Authentication;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// The one check that stands between a request and everything an operator can
/// see: the administration panel, the server's figures and its log.
/// </summary>
/// <remarks>
/// GenHTTP's API key authentication with the token in <c>X-Admin-Token</c>, so
/// a request without one is answered with 401 and one with the wrong token
/// with 403. It runs as a concern in front of the resources rather than in
/// their methods, because a method that takes a body cannot read a header any
/// more - the framework releases them once the body is bound.
///
/// An installation without a token has no panel: the routes are not added at
/// all (see <see cref="ApiLayout"/>), so they are not found.
/// </remarks>
public static class AdminAuthentication
{

    /// <summary>
    /// The role of whoever presented the token.
    /// </summary>
    public const string Role = "admin";

    /// <summary>
    /// The header the token is presented in.
    /// </summary>
    public const string Header = "X-Admin-Token";

    // how the API description says it, for each resource behind the token -
    // the framework's API key concern does not describe itself

    public const string HeaderDescription = "The administration token of the installation (`LAMBDA_ADMIN_TOKEN`)";

    public const string MissingDescription = "The token is missing.";

    public const string WrongDescription = "The token is wrong.";

    /// <summary>
    /// The concern that lets a request with the configured token through.
    /// </summary>
    public static IConcernBuilder Create(LambdaOptions options)
    {
        var expected = Encoding.UTF8.GetBytes(options.AdminToken ?? throw new InvalidOperationException("No administration token is configured."));

        return ApiKeyAuthentication.Create()
                                   .WithHeader(Header)
                                   .Authenticator((_, presented) =>
                                   {
                                       var actual = Encoding.UTF8.GetBytes(presented);

                                       // the length is not a secret, but which byte differs is
                                       var allowed = actual.Length == expected.Length && CryptographicOperations.FixedTimeEquals(actual, expected);

                                       return new ValueTask<IUser?>(allowed ? Operator.Instance : null);
                                   });
    }

    /// <summary>
    /// Whoever holds the token.
    /// </summary>
    /// <remarks>
    /// Not the framework's <c>ApiKeyUser</c>, whose display name is the key
    /// itself - and a display name is what ends up in a log line.
    /// </remarks>
    private sealed class Operator : IUser
    {

        public static Operator Instance { get; } = new();

        public string DisplayName => "operator";

        public string[] Roles { get; } = [Role];

    }

}
