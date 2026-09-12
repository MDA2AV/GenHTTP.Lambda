using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.SinglePageApplications;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Serves the single page application from the web root and hands its index
/// page to whoever needs to answer with it - the landing page, the editor and
/// the "no lambda here" page all live in the same bundle.
/// </summary>
public sealed class SpaResources
{

    #region Get-/Setters

    /// <summary>
    /// Whether a built frontend was found in the web root.
    /// </summary>
    public bool Available { get; }

    private string Root { get; }

    private string IndexFile { get; }

    #endregion

    #region Initialization

    public SpaResources(LambdaOptions options, ILogger<SpaResources> logger)
    {
        Root = options.WebRoot;
        IndexFile = Path.Combine(Root, "index.html");

        Available = File.Exists(IndexFile);

        if (!Available)
        {
            logger.LogWarning("No frontend found at '{Root}', serving the placeholder page instead", Root);
        }
    }

    #endregion

    #region Functionality

    /// <summary>
    /// The handler serving the application, with unknown paths falling back to
    /// its index so the client side router can take over.
    /// </summary>
    public IHandlerBuilder CreateHandler()
    {
        if (!Available)
        {
            return Content.From(Placeholder());
        }

        return SinglePageApplication.From(ResourceTree.FromDirectory(Root))
                                    .ServerSideRouting();
    }

    /// <summary>
    /// Answers with the application itself, using the given status - so a
    /// missing lambda stays a 404 while still rendering a proper page.
    /// </summary>
    public async ValueTask<IResponse> RenderAsync(IRequest request, ResponseStatus status)
    {
        var markup = Available ? await File.ReadAllTextAsync(IndexFile) : PlaceholderMarkup;

        return request.Respond()
                      .Status(status)
                      .Content(markup, ContentType.TextHtml)
                      .Build();
    }

    /// <summary>
    /// Whether the client would rather see a page than a JSON document.
    /// </summary>
    public bool PrefersMarkup(IRequest request)
    {
        var accepted = request.Header.Headers.GetEntry("Accept");

        return accepted != null && accepted.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static GenHTTP.Api.Content.IO.IResource Placeholder()
        => Resource.FromString(PlaceholderMarkup).Type(ContentType.TextHtml).Build();

    private const string PlaceholderMarkup = """
        <!doctype html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>GenHTTP Lambda</title>
            <style>
                :root { color-scheme: dark; }
                body { margin: 0; min-height: 100vh; display: grid; place-items: center;
                       background: #0e1116; color: #e6edf3; font: 15px/1.6 ui-sans-serif, system-ui, sans-serif; }
                main { max-width: 34rem; padding: 2rem; }
                h1 { font-size: 1.4rem; margin: 0 0 .5rem; }
                code { background: #1b212b; border-radius: .3rem; padding: .15rem .4rem; }
                pre { background: #1b212b; border-radius: .5rem; padding: 1rem; overflow-x: auto; }
                a { color: #7ab7ff; }
            </style>
        </head>
        <body>
        <main>
            <h1>The frontend has not been built yet</h1>
            <p>The API is running, but there is no single page application in the web root.</p>
            <pre>cd src/Frontend
        npm install
        npm run build</pre>
            <p>During development, run <code>npm run dev</code> instead and open
               <a href="http://localhost:5173/">localhost:5173</a> - it proxies the API to this server.</p>
        </main>
        </body>
        </html>
        """;

    #endregion

}
