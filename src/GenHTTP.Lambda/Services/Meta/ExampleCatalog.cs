using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The lambdas the installation keeps online so that somebody who has not
/// written anything yet has something working to look at.
/// </summary>
/// <remarks>
/// These are not the templates, and used to be. A template is somewhere to
/// start from and the editor already hands one over; an example is a finished
/// thing, and showing somebody the code they are about to be given teaches
/// them nothing.
///
/// Each is backed by a template all the same, so that cloning one is the
/// ordinary creation flow with the source chosen. Those templates are hidden:
/// reachable by name, left out of the picker.
/// </remarks>
public static class ExampleCatalog
{

    /// <summary>
    /// The key an example is hosted at, which has to be stable: it is what
    /// anything linking to an example points at.
    /// </summary>
    public static string KeyFor(string templateId) => $"example-{templateId}";

    /// <summary>
    /// Every example, in the order they are offered.
    /// </summary>
    /// <remarks>
    /// The route each one is worth calling is curated with it, because a
    /// lambda is mounted wherever its code says and not all of these mount
    /// something at their own root. Offering the root regardless would answer
    /// a visitor with a 404 and teach them the examples are broken.
    /// </remarks>
    private static readonly LambdaExample[] Catalogue =
    [
        new("shortener", Basic, "Link shortener",
            "Paste a long address and get a short one back. Follows are counted, and the links are there tomorrow.",
            "", false),

        new("guestbook", Basic, "Guestbook",
            "Anyone can sign it and everyone can read it. A page, a form, and a file that outlives the deployment.",
            "", false),

        new("inspector", Basic, "Request inspector",
            "Answers every request with a description of itself: the method, the query, the headers and the body.",
            "echo", false),

        new("chat", Advanced, "Chat room",
            "Accounts and a room that stays open. Passwords hashed, messages kept, and a socket per person.",
            "", false),

        new("game", Advanced, "Game with a scoreboard",
            "A three.js runner in the browser, and a scoreboard this lambda keeps for it.",
            "", false),

        new("arena", Advanced, "Shared arena",
            "Everybody in the same three dimensional room at once, with the server deciding where everyone is.",
            "", false)
    ];

    #region Levels

    /// <summary>Small, finished, and readable in a sitting.</summary>
    public const string Basic = "basic";

    /// <summary>Several files, and something that would be work to build.</summary>
    public const string Advanced = "advanced";

    /// <summary>What a level is called on screen.</summary>
    public static string NameOf(string level) => level == Basic ? "Basic" : "Advanced";

    /// <summary>The levels, in the order they are offered.</summary>
    public static IReadOnlyList<string> Levels => [Basic, Advanced];

    #endregion

    #region Functionality

    /// <summary>
    /// Every example the installation maintains.
    /// </summary>
    public static IEnumerable<LambdaExample> All => Catalogue;

    /// <summary>
    /// The example an identifier refers to, if it is one.
    /// </summary>
    public static LambdaExample? Find(string? id) => id == null ? null : Array.Find(Catalogue, e => e.Id == id);

    /// <summary>
    /// The files an example is made of, which are its template's.
    /// </summary>
    public static IReadOnlyList<LambdaFile> FilesFor(LambdaExample example)
        => TemplateCatalog.FilesFor(example.Id, example.PublicKey);

    #endregion

}

/// <summary>
/// One example: a lambda the installation keeps deployed, and what to say
/// about it.
/// </summary>
/// <param name="Id">Its template, which is also what a clone asks for</param>
/// <param name="Level">Basic or advanced</param>
/// <param name="TryPath">What is worth calling underneath it, empty for its root</param>
/// <param name="Socket">Whether it is reached by opening a socket rather than asking for a page</param>
public sealed record LambdaExample(string Id, string Level, string Name, string Description, string TryPath, bool Socket)
{

    /// <summary>Where it is hosted, which is stable across restarts.</summary>
    public string PublicKey => ExampleCatalog.KeyFor(Id);

}
