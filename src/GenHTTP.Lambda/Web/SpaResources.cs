using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;

using GenHTTP.Modules.Files;
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

    /// <summary>
    /// Whether the bundle's asset directory exists and can be routed ahead of
    /// the application itself.
    /// </summary>
    public bool HasAssets { get; }

    private string Root { get; }

    private string IndexFile { get; }

    private string AssetDirectory { get; }

    #endregion

    #region Initialization

    public SpaResources(LambdaOptions options, ILogger<SpaResources> logger)
    {
        Root = options.WebRoot;
        IndexFile = Path.Combine(Root, "index.html");
        AssetDirectory = Path.Combine(Root, "assets");

        Available = File.Exists(IndexFile);
        HasAssets = Directory.Exists(AssetDirectory);

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

        // ranges because the front page has a video, and Safari will not play
        // one from a server that answers a range with the whole file
        return SinglePageApplication.From(ResourceTree.FromDirectory(Root))
                                    .ServerSideRouting()
                                    .Add(RangeSupport.Create())
                                    .Add(CacheControl.NoCache());
    }

    /// <summary>
    /// The bundle's own files, routed before the application so a request for a
    /// file that is not there ends in a 404.
    ///
    /// Server side routing answers anything it cannot resolve with the index
    /// page, which is right for a route the client router owns and wrong for a
    /// script: a browser holding a stale index asks for the bundle it was built
    /// with, is handed markup with a 200 and a text/html type, and dies on the
    /// first angle bracket with no failed request to show for it.
    /// </summary>
    public IHandlerBuilder CreateAssetHandler()
        => Assets.From(AssetDirectory)
                 .Add(CacheControl.Immutable());

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
                      .Header("Cache-Control", "no-cache")
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
