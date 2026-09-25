using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The lambdas the installation keeps online so that an agent, or anybody
/// else, can read something that already works before writing anything.
/// </summary>
/// <remarks>
/// A demo is an ordinary lambda in the demo tier. Its editor key is its public
/// key and is announced, so it is read with the same tools as any other lambda
/// - its code with its history, its workspace, its logs - and the tier makes
/// all of that read only (see <c>MetaService.EnsureEditable</c>).
///
/// Each is backed by a hidden template of the same name, so that starting a
/// lambda of one's own from a demo is the ordinary creation flow with that
/// template chosen.
///
/// Every demo keeps what it stores in JSON files in its workspace. That is on
/// purpose: it is what every lambda can do today, and a demo showing a
/// database the platform does not offer yet would teach the wrong thing.
/// </remarks>
public static class DemoCatalog
{

    private static readonly LambdaDemo[] Catalogue =
    [
        new("demo-crud", "Records you can list, add, change and remove",
            "Keep track of things", "A list people can add to, change and tick off - tasks, notes, bookmarks or a small inventory.",
            "A task list: a REST API with list, search, read, create, update and delete, stored in a JSON file, with an OpenAPI document, Scalar to try it, and a page that uses it.",
            "A class-based webservice ([ResourceMethod]), status codes (201, 400, 404), validation, a reusable JSON file store with a lock and a size cap, OpenAPI and Scalar, and a front end shipped as assets in a folder.",
            "Anything that keeps a list of things: notes, bookmarks, inventory, a small admin tool."),

        new("demo-registration", "Registration and login",
            "Let people sign up", "Accounts people register and sign in with, and pages only they get to see.",
            "A landing page that asks you to register or sign in, and a members page only a signed in user gets to see.",
            "Accounts in a JSON file with salted PBKDF2 password hashes; sessions in an HttpOnly, SameSite cookie set with Result<T>.Cookie and read with GetCookie, stored only as hashes and expiring; the Authentication module (ApiKeyAuthentication with a cookie extractor) guarding api/; the signed in user injected into routes with UserInjector<T>; and two pages that decide where to send you.",
            "Anything with users: sign up, sign in, sign out, content only members see."),

        new("demo-game", "A multiplayer game over a websocket",
            "A game to play together", "Something several people play at the same time, live in their browsers.",
            "Tic-tac-toe against whoever else opens the page, or against the house when nobody is around. The server keeps the board and decides every move.",
            "A reactive websocket handler (IReactiveHandler), typed JSON messages both ways with ReadPayloadAsync<T> and WritePayloadAsync, matchmaking and per-game state, a server that validates every move, one write at a time per socket, and a single page application talking to it.",
            "Anything live between several browsers: games, shared boards, chat, collaboration."),

        new("demo-files", "Uploads and serving files",
            "Share files and pictures", "People upload pictures or documents, and everybody else can see them.",
            "Upload a picture or a document and everybody sees it in the gallery; you can remove what you uploaded.",
            "Taking a raw request body as a Stream, checking its type and size, storing it in the workspace, serving a workspace folder with Workspace.Files() and nosniff, keeping the metadata in JSON, and why only a fixed list of file types is served back.",
            "Anything users upload to: avatars, attachments, a gallery, a file drop."),

        new("demo-live", "Live updates with server-sent events",
            "Show things as they happen", "A page that updates by itself the moment something changes - votes, scores, a dashboard.",
            "A poll whose bars move for everybody the moment anybody votes, with a count of who is watching.",
            "EventSource.Create() streaming to many browsers, objects sent as JSON messages and a named event beside them, a POST that changes state and notifies every listener, keeping a stream alive with comments, and the browser's EventSource reconnecting on its own.",
            "One-way live data: dashboards, feeds, progress, notifications, scoreboards. Simpler than a websocket when the browser only listens.")
    ];

    #region Functionality

    /// <summary>
    /// Every demo the installation maintains, in the order they are offered.
    /// </summary>
    public static IEnumerable<LambdaDemo> All => Catalogue;

    /// <summary>
    /// The demo an identifier or key refers to, if it is one.
    /// </summary>
    public static LambdaDemo? Find(string? id) => id == null ? null : Array.Find(Catalogue, e => e.Id == id);

    /// <summary>
    /// The files a demo runs, which are its template's with the lines only
    /// the demo itself has.
    /// </summary>
    public static IReadOnlyList<LambdaFile> FilesFor(LambdaDemo demo)
        => TemplateCatalog.FilesFor(demo.Id, demo.Key, demo: true);

    #endregion

}

/// <summary>
/// One demo.
/// </summary>
/// <param name="Id">Its key and the template it comes from, which are the same</param>
/// <param name="Name">What it is, in a few words, for whoever reads its code</param>
/// <param name="Goal">What somebody who wants one would call it, for the page that creates a lambda</param>
/// <param name="Pitch">What they would get, in the same words</param>
/// <param name="Description">What a visitor can do with it</param>
/// <param name="Shows">What reading its code teaches, so an agent can pick the one to read</param>
/// <param name="ReadWhen">The kinds of request it is the right starting point for</param>
public sealed record LambdaDemo(string Id, string Name, string Goal, string Pitch, string Description, string Shows, string ReadWhen)
{

    /// <summary>
    /// Where it is hosted and the key its editor opens with - the same, and
    /// both public, because a demo is there to be read.
    /// </summary>
    public string Key => Id;

}
