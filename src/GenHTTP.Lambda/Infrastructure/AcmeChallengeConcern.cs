using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Answers the HTTP challenges of an ACME client from the web root it writes
/// them into, so certificates can be issued while the server keeps running.
/// </summary>
/// <remarks>
/// In front of the router rather than inside the platform's routes: a
/// certificate for a lambda's own domain is asked for at that domain, and
/// there every path would otherwise belong to the lambda.
///
/// The client writes a file per challenge to
/// <c>{root}/.well-known/acme-challenge/{token}</c>, and the certificate
/// authority asks for exactly that path. Nothing else under the root is served,
/// and a token is only ever the characters a token can have - so there is no
/// path to walk out of the folder with.
/// </remarks>
public sealed class AcmeChallengeConcern(IHandler content, string root) : IConcern
{
    private const string Prefix = "/.well-known/acme-challenge/";

    /// <summary>
    /// Tokens are base64url and far shorter than this in practice.
    /// </summary>
    private const int MaxToken = 256;

    private readonly string _challenges = Path.Combine(root, ".well-known", "acme-challenge");

    #region Get-/Setters

    public IHandler Content => content;

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Header.Method != RequestMethod.Get)
        {
            return await content.HandleAsync(request);
        }

        var path = request.Header.Path.ToString();

        if (!path.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return await content.HandleAsync(request);
        }

        var token = path[Prefix.Length..];

        var file = IsToken(token) ? Path.Combine(_challenges, token) : null;

        if (file == null || !File.Exists(file))
        {
            // the path is the ACME client's, never a lambda's or the site's,
            // so a challenge that is not there is simply not there
            return request.Respond()
                          .Status(ResponseStatus.NotFound)
                          .Content("No such challenge.", ContentType.TextPlain)
                          .Build();
        }

        return request.Respond()
                      .Content(await File.ReadAllTextAsync(file), ContentType.TextPlain)
                      .Header("Cache-Control", "no-store")
                      .Build();
    }

    private static bool IsToken(string token)
    {
        if (token.Length is 0 or > MaxToken)
        {
            return false;
        }

        foreach (var character in token)
        {
            if (!(char.IsAsciiLetterOrDigit(character) || character is '-' or '_'))
            {
                return false;
            }
        }

        return true;
    }

    #endregion

}

public sealed class AcmeChallengeConcernBuilder(string root) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new AcmeChallengeConcern(content, root);
}
