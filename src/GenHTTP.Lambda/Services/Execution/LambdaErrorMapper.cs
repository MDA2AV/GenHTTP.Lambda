using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.ErrorHandling;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Protection;

using GenHTTP.Modules.IO;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Execution;

/// <summary>
/// Turns whatever a lambda throws into a response its visitors can read.
/// </summary>
/// <remarks>
/// Lambdas are free to install their own error handling; this only steps in
/// when nothing else did. The message of an exception is shown because the
/// author of the lambda is also the one debugging it, but stack traces stay on
/// the server.
/// </remarks>
public sealed class LambdaErrorMapper(ILogger<LambdaErrorMapper> logger) : IErrorMapper<Exception>
{

    #region Functionality

    public ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error, ByteString? acceptedFormat)
    {
        if (error is ProviderException provided)
        {
            return new ValueTask<IResponse?>(Render(request, acceptedFormat?.ToString(), provided.Status, provided.Status.ToString(), provided.Message, provided.Modifications));
        }

        var lambda = request.GetLambda();

        // the path alone reads as one of the platform's when the lambda was
        // reached at a domain of its own
        logger.LogWarning(error, "Lambda '{PublicKey}' failed while handling {Method} {Host}{Path}",
            lambda?.PublicKey ?? "?", request.Header.Method, request.GetDomain()?.Name ?? "", request.Header.Path);

        return new ValueTask<IResponse?>(Render(request, acceptedFormat?.ToString(), ResponseStatus.InternalServerError, "Lambda Error",
            $"The lambda threw {error.GetType().Name}: {error.Message}", null));
    }

    public ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler, ByteString? acceptedFormat)
        => new(Render(request, acceptedFormat?.ToString(), ResponseStatus.NotFound, "Not Found", "This lambda does not serve the requested path.", null));

    /// <summary>
    /// A page for a visitor of a lambda - markup for a browser, JSON for
    /// anything else - that says what went wrong without saying anything
    /// about the platform around it.
    /// </summary>
    /// <param name="accepted">
    /// The Accept header, as read before the handler ran. Never read here: once a
    /// route has taken the body, asking the request for a header throws - which
    /// turned every ProviderException of a route with a body into a 500.
    /// </param>
    internal static IResponse Render(IRequest request, string? accepted, ResponseStatus status, string title, string message, Action<IResponseBuilder>? modifications)
    {
        var wantsMarkup = accepted == null || accepted.Contains("text/html", StringComparison.OrdinalIgnoreCase);

        var response = request.Respond().Status(status);

        if (wantsMarkup)
        {
            response.Content(Markup(status, title, message), ContentType.TextHtml);
        }
        else
        {
            response.Content(Json(status, title, message), ContentType.ApplicationJson);
        }

        modifications?.Invoke(response);

        return response.Build();
    }

    private static string Json(ResponseStatus status, string title, string message)
        => $$"""{"status":{{(int)status}},"title":{{Quote(title)}},"message":{{Quote(message)}}}""";

    private static string Quote(string value) => System.Text.Json.JsonSerializer.Serialize(value);

    private static string Markup(ResponseStatus status, string title, string message) => $$"""
        <!doctype html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{{(int)status}} {{WebUtility.HtmlEncode(title)}}</title>
            <style>
                :root { color-scheme: dark light; }
                body { margin: 0; min-height: 100vh; display: grid; place-items: center;
                       font: 15px/1.6 ui-sans-serif, system-ui, sans-serif; }
                main { max-width: 36rem; padding: 2rem; }
                h1 { font-size: 1.3rem; margin: 0 0 .5rem; }
                p { margin: 0; opacity: .85; white-space: pre-wrap; }
                small { display: block; margin-top: 1.5rem; opacity: .55; }
            </style>
        </head>
        <body>
        <main>
            <h1>{{(int)status}} &middot; {{WebUtility.HtmlEncode(title)}}</h1>
            <p>{{WebUtility.HtmlEncode(message)}}</p>
            <small>Served by GenHTTP Lambda</small>
        </main>
        </body>
        </html>
        """;

    #endregion

}
