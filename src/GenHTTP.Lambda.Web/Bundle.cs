using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The built frontend of a component: the folder its bundle was built into,
/// with the index page every one of its pages is drawn in, the scripts and
/// styles below <c>assets/</c>, and whatever else it ships - its icons, its
/// table of pages, its prerendered pages.
/// </summary>
/// <remarks>
/// Each component with pages has a bundle of its own (built from its
/// <c>Frontend</c> folder by <c>npm run build</c>), which its project copies
/// into the host's output below <c>bundles/</c>: a component is found by its
/// pages wherever its assembly is, and the hosts carry no frontend of their
/// own but their site's.
///
/// Read from disk as it is asked for, and the index page again whenever the
/// file is newer: a bundle written again where a running server reads it is
/// served without a restart - the host's output once a build copied it
/// there, or the folder a host named with a builder's <c>Bundle(directory)</c>.
/// </remarks>
public sealed class Bundle
{

    #region Get-/Setters

    /// <summary>
    /// The folder the bundle was built into.
    /// </summary>
    public string Directory { get; }

    /// <summary>
    /// Whether there is a built bundle there at all.
    /// </summary>
    public bool Available => System.IO.File.Exists(IndexFile);

    /// <summary>
    /// Whether the bundle has scripts and styles of its own, to be routed
    /// ahead of its pages.
    /// </summary>
    public bool HasAssets => System.IO.Directory.Exists(AssetDirectory);

    /// <summary>
    /// The bundle's table of pages, which names each of them (see SiteMeta).
    /// </summary>
    public string PageFile => Path.Combine(Directory, "pages.json");

    /// <summary>
    /// The pages the build rendered (see SitePrerender).
    /// </summary>
    public string PrerenderFile => Path.Combine(Directory, "prerender.json");

    private string IndexFile => Path.Combine(Directory, "index.html");

    private string AssetDirectory => Path.Combine(Directory, "assets");

    private volatile IndexCopy? _index;

    #endregion

    #region Initialization

    private Bundle(string directory)
    {
        Directory = Path.GetFullPath(directory);
    }

    /// <summary>
    /// The bundle built into the given folder.
    /// </summary>
    public static Bundle In(string directory) => new(directory);

    /// <summary>
    /// The bundle of a component where its project puts it: below
    /// <c>bundles/</c>, beside the assemblies of the host.
    /// </summary>
    /// <param name="name">The name of the component's folder there, such as <c>editor</c></param>
    public static Bundle Beside(string name) => new(Path.Combine(AppContext.BaseDirectory, "bundles", name));

    #endregion

    #region Functionality

    /// <summary>
    /// The shell every page of the bundle is drawn in - or, where it was not
    /// built, a page that says how to build it.
    /// </summary>
    /// <remarks>
    /// Read for every page served, so held in memory and only read again once
    /// the file on disk is newer - which, while the frontend is developed
    /// against a running server, it will be after every build.
    /// </remarks>
    public string Index()
    {
        if (!Available)
        {
            return Placeholder;
        }

        var written = System.IO.File.GetLastWriteTimeUtc(IndexFile);

        if (_index is { } known && known.Written == written)
        {
            return known.Markup;
        }

        var markup = System.IO.File.ReadAllText(IndexFile);

        _index = new IndexCopy(written, markup);

        return markup;
    }

    /// <summary>
    /// The files of the bundle, without falling back to the index page - none
    /// where it was not built, which a server without a frontend serves as well.
    /// </summary>
    public IResourceTree Files()
        => System.IO.Directory.Exists(Directory) ? ResourceTree.FromDirectory(Directory).Build() : VirtualTree.Create().Build();

    /// <summary>
    /// The bundle's scripts and styles, routed before its pages so a request
    /// for one that is not there ends in a 404, and cached for good, since
    /// their names change with their content.
    /// </summary>
    /// <remarks>
    /// A page falling back to the index is right for a route the client
    /// router owns and wrong for a script: a browser holding a stale index
    /// asks for the bundle it was built with, is handed markup with a 200 and
    /// a text/html type, and dies on the first angle bracket with no failed
    /// request to show for it.
    /// </remarks>
    public IHandlerBuilder Assets()
        => GenHTTP.Modules.Files.Assets.From(AssetDirectory)
                  .Add(CacheControl.Immutable());

    /// <summary>
    /// The index page as an answer, with the status given.
    /// </summary>
    public IResponse Answer(IRequest request, ResponseStatus status = ResponseStatus.Ok)
        => request.Respond()
                  .Status(status)
                  .Content(Resource.FromString(Index()).Type(new ContentType("text/html; charset=utf-8")).Build())
                  .Build();

    private sealed record IndexCopy(DateTime Written, string Markup);

    private const string Placeholder = """
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
            </style>
        </head>
        <body>
        <main>
            <h1>The frontend has not been built yet</h1>
            <p>The API is running, but this page's bundle has not been built.</p>
            <pre>npm install
        npm run build</pre>
            <p>Run both at the root of the repository, then build the server again - or run
               <code>npm run dev</code> in the frontend of the component, which proxies
               the rest to this server.</p>
        </main>
        </body>
        </html>
        """;

    #endregion

}
