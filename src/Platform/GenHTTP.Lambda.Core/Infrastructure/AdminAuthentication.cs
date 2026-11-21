using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Documentation.Security;

using GenHTTP.Lambda.Configuration;

using GenHTTP.Modules.Authentication;

namespace GenHTTP.Lambda.Infrastructure;

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
/// all (the API's layout, the operator's MCP endpoint), so they are not found.
/// In the platform rather than in a door, so every door that lets the
/// operator in - the REST API, the operator's MCP endpoint - checks the token
/// the same way, and neither reaches into the other.
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

    /// <summary>
    /// How the API description names the token.
    /// </summary>
    /// <remarks>
    /// The concern describes itself to the API description: every operation
    /// behind it requires this scheme, and says how a request without the
    /// token and one with the wrong token are answered. Named for what it is,
    /// since Scalar asks for it once by that name for all of them.
    /// </remarks>
    public static readonly SecurityScheme Scheme = SecurityScheme.ApiKey(Header, ApiKeyLocation.Header, "The administration token of the installation (`LAMBDA_ADMIN_TOKEN`).")
                                                   with { Name = "adminToken" };

    /// <summary>
    /// The concern that lets a request with the configured token through.
    /// </summary>
    public static IConcernBuilder Create(LambdaOptions options)
    {
        if (!options.Administrable)
        {
            throw new InvalidOperationException("No administration token is configured.");
        }

        // compared where every door that lets the operator in compares it
        return ApiKeyAuthentication.Create()
                                   .WithHeader(Header)
                                   .Documentation(Scheme)
                                   .Authenticator((_, presented) => new ValueTask<IUser?>(OperatorToken.Matches(options, presented) ? Operator.Instance : null));
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
