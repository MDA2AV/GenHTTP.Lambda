using System.Reflection;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The examples a new lambda can start from, in the groups the creation
/// assistant offers: a service that answers requests, a socket that stays
/// open, or a whole application to take apart.
/// </summary>
public static class TemplateCatalog
{
    private const string Placeholder = "{{KEY}}";

    /// <summary>
    /// The template a lambda is seeded with when none was asked for.
    /// </summary>
    public const string DefaultId = "rest-service";

    private static readonly LambdaTemplate[] All =
    [
        new("rest-service", "rest", "REST service", "A small book API with an OpenAPI document and a browser to try it out.", "RestService"),
        new("rest-minimal", "rest", "Minimal service", "A handful of inline routes and nothing else.", "RestMinimal"),
        new("rest-webservice", "rest", "Class based service", "The same routes as methods of a class, described by attributes.", "RestWebservice"),
        new("websocket-functional", "websocket", "Functional", "Three callbacks for connect, message and close.", "WebsocketFunctional"),
        new("websocket-reactive", "websocket", "Reactive", "A class the platform calls when something happens.", "WebsocketReactive"),
        new("websocket-imperative", "websocket", "Imperative", "A loop that owns the connection and reads it frame by frame.", "WebsocketImperative"),
        new("chat", "app", "Chat room", "Accounts, a sign in, and a room that stays open. Passwords hashed, messages kept.", "Chat",
            false, ("Accounts.cs", "ChatAccounts"), ("Page.cs", "ChatPage")),
        new("game", "app", "Game with a scoreboard", "A three.js runner in the browser, and a scoreboard this lambda keeps.", "Game",
            false, ("Scores.cs", "GameScores"), ("Page.cs", "GamePage")),

        // the examples. Hidden, because the picker is for somewhere to start
        // from and these are finished things to look at - they are reached by
        // name, from the examples menu and from a clone.
        new("shortener", "app", "Link shortener", "Paste a long address, get a short one. Follows are counted.", "Shortener",
            true, ("Links.cs", "ShortenerLinks"), ("Page.cs", "ShortenerPage")),
        new("guestbook", "app", "Guestbook", "Anyone can sign it, everyone can read it, and it is still there tomorrow.", "Guestbook",
            true, ("Entries.cs", "GuestbookEntries"), ("Page.cs", "GuestbookPage")),
        new("inspector", "app", "Request inspector", "Answers every request with a description of itself.", "Inspector",
            true, ("Page.cs", "InspectorPage")),
        new("arena", "app", "Shared arena", "Everybody in one three dimensional room, with the server deciding where they are.", "Arena",
            true, ("World.cs", "ArenaWorld"), ("Protocol.cs", "ArenaProtocol"), ("Page.cs", "ArenaPage"))
    ];


    private static readonly LambdaTemplateGroup[] GroupList =
    [
        new("rest", "Answers requests", "A service, a page, a document - anything a client asks for and gets back.", [.. All.Where(t => t.Group == "rest")]),
        new("websocket", "Keeps a connection", "A websocket that stays open, in each of the three flavours the module offers.", [.. All.Where(t => t.Group == "websocket")]),
        new("app", "A whole application", "Something finished rather than something to start from: a page, an API and whatever it needs to remember.", [.. All.Where(t => t.Group == "app")])
    ];

    #region Functionality

    /// <summary>
    /// The groups a lambda can be started from, in the order they are offered.
    /// </summary>
    public static IReadOnlyList<LambdaTemplateGroup> Groups => GroupList;

    /// <summary>
    /// Whether the given identifier belongs to a template, hidden or not.
    /// </summary>
    public static bool Exists(string id) => Array.Exists(All, t => t.Id == id);

    /// <summary>
    /// The template a name refers to, or null where nothing does.
    /// </summary>
    public static LambdaTemplate? Find(string? id) => id == null ? null : Array.Find(All, t => t.Id == id);

    /// <summary>
    /// The code of a template, with the public key filled into its comments.
    /// </summary>
    /// <param name="id">The template to read, falling back to the default one</param>
    /// <param name="publicKey">The key the lambda will be hosted at</param>
    /// <remarks>
    /// A template of several files is returned as the blob a version is stored
    /// as, which is what every caller of this wants: something to save.
    /// </remarks>
    public static string ForKey(string? id, string publicKey)
        => LambdaSource.Serialize(FilesFor(id, publicKey));

    /// <summary>
    /// The files a template starts a lambda with, the snippet first.
    /// </summary>
    public static IReadOnlyList<LambdaFile> FilesFor(string? id, string publicKey)
    {
        var template = Array.Find(All, t => t.Id == id)
                    ?? Array.Find(All, t => t.Id == DefaultId)!;

        var files = new List<LambdaFile>
        {
            new(LambdaSource.EntryName, Fill(template.Source, publicKey))
        };

        foreach (var part in template.Parts)
        {
            files.Add(new LambdaFile(part.Name, Fill(part.Source, publicKey)));
        }

        return files;
    }

    private static string Fill(string source, string publicKey)
        => source.Replace(Placeholder, publicKey, StringComparison.Ordinal);

    #endregion

}

/// <summary>
/// One example, read from the assembly the first time it is asked for.
/// </summary>
public sealed class LambdaTemplate(string id, string group, string name, string description, string resource, bool hidden = false, params (string Name, string Resource)[] parts)
{
    private readonly Lazy<string> _source = new(() => Read(resource), LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly Lazy<IReadOnlyList<TemplatePart>> _parts = new(
        () => [.. parts.Select(p => new TemplatePart(p.Name, p.Resource))],
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// The files beside the snippet, in the order they are offered.
    /// </summary>
    public IReadOnlyList<TemplatePart> Parts => _parts.Value;

    public string Id => id;

    /// <summary>
    /// Whether this one is reachable by name but left out of the listing.
    /// </summary>
    /// <remarks>
    /// A page elsewhere - the GenHTTP documentation, say - can send somebody
    /// here with a template chosen to match what they were just reading. That
    /// example belongs to that page rather than to this catalogue, so it is
    /// reachable by name and left out of the assistant, which would otherwise
    /// grow an entry every time anything linked here.
    /// </remarks>
    public bool Hidden => hidden;

    public string Group => group;

    public string Name => name;

    public string Description => description;

    /// <summary>
    /// The code of this template, still carrying its key placeholder.
    /// </summary>
    public string Source => _source.Value;

    internal static string Read(string resource)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var names = assembly.GetManifestResourceNames();

        var name = Array.Find(names, n => n.EndsWith($".{resource}.cs.txt", StringComparison.Ordinal))
                ?? throw new InvalidOperationException($"The template '{resource}' is missing from the assembly ({string.Join(", ", names)}).");

        using var stream = assembly.GetManifestResourceStream(name)!;

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

}

/// <summary>
/// One file of a template beside its snippet, read when it is first wanted.
/// </summary>
public sealed class TemplatePart(string name, string resource)
{
    private readonly Lazy<string> _source = new(() => LambdaTemplate.Read(resource), LazyThreadSafetyMode.ExecutionAndPublication);

    public string Name => name;

    public string Source => _source.Value;
}

/// <summary>
/// The first choice of the creation assistant.
/// </summary>
public sealed record LambdaTemplateGroup(string Id, string Name, string Description, IReadOnlyList<LambdaTemplate> Templates);
