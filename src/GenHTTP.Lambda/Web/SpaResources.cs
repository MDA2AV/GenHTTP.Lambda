using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.SinglePageApplications;
using GenHTTP.Modules.Files;

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
    /// Serves the built files, answering for the ones that are not there rather
    /// than letting the application below take the request.
    /// </summary>
    /// <remarks>
    /// Server side routing answers anything it does not recognise with the
    /// index page, which is right for a route and wrong for a file: a build
    /// names its chunks by hash, so a tab left open across a deployment asks
    /// for one that no longer exists, and gets HTML with a 200 where it
    /// expected a script. The browser then fails to parse it as a module, the
    /// import never settles and the page waits forever - which is what a
    /// visitor sees as a spinner that never stops. A 404 here says what
    /// happened and lets the application recover.
    /// </remarks>
    public IHandlerBuilder? CreateAssetHandler()
    {
        if (!Available)
        {
            return null;
        }

        var assets = Path.Combine(Root, "assets");

        // created rather than checked for: a build always writes one, and a
        // deployment that somehow did not should still report a missing file
        // as missing rather than falling through to the page below
        Directory.CreateDirectory(assets);

        return Assets.From(ResourceTree.FromDirectory(assets));
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
                       background: #202124; color: #e8eaed; font: 15px/1.6 Roboto, Arial, sans-serif; }
                main { max-width: 34rem; padding: 2rem; }
                h1 { font-size: 1.4rem; margin: 0 0 .5rem; }
                code { background: #292a2d; border: 1px solid #3c4043; padding: .15rem .4rem; }
                pre { background: #292a2d; border: 1px solid #3c4043; padding: 1rem; overflow-x: auto; }
                a { color: #8ab4f8; }
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
