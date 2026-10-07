using System.Reflection;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The code a lambda starts with: nothing much, or a copy of one of the demos.
/// </summary>
/// <remarks>
/// The files live in the assembly under <c>Resources/Templates</c>. A demo's
/// files are both what the demo runs and what a copy of it starts with, with
/// one difference: lines between a <c>[demo]</c> and a <c>[/demo]</c> marker
/// only belong to the demo. They say it is read only and where to read it,
/// which a copy - an ordinary lambda of somebody's own - must not claim.
/// </remarks>
public static class TemplateCatalog
{
    private const string Placeholder = "{{KEY}}";

    private const string DemoStart = "[demo]";

    private const string DemoEnd = "[/demo]";

    /// <summary>
    /// What a lambda is started from when nothing is chosen.
    /// </summary>
    public const string EmptyId = "empty";

    private static readonly LambdaTemplate[] All =
    [
        new(EmptyId, "Empty"),

        new("demo-crud", "DemoCrud",
            ("Tasks.cs", "DemoCrudTasks"), ("Store.cs", "DemoCrudStore"),
            ("resources/migrations/V1__Create_tasks.sql", "DemoCrudMigration1"),
            ("resources/web/index.html", "DemoCrudPage"), ("resources/web/app.js", "DemoCrudApp"), ("resources/web/base.css", "DemoStyle"),
            (LambdaSource.ProductDoc, "DemoCrudProduct"), (LambdaSource.DecisionsDoc, "DemoCrudDecisions"),
            (LambdaSource.TestingDoc, "DemoCrudTesting"), ("tests/smoke.mjs", "DemoCrudSmoke"),
            ("tests/tasks.json", "DemoCrudTestTasks"), ("tests/seed.mjs", "DemoCrudSeed")),

        new("demo-registration", "DemoRegistration",
            ("Accounts.cs", "DemoRegistrationAccounts"),
            ("resources/migrations/V1__Create_accounts.sql", "DemoRegistrationMigration1"),
            ("resources/web/index.html", "DemoRegistrationLanding"), ("resources/web/members.html", "DemoRegistrationMembers"),
            ("resources/web/session.js", "DemoRegistrationSession"), ("resources/web/base.css", "DemoStyle"),
            (LambdaSource.ProductDoc, "DemoRegistrationProduct"), (LambdaSource.DecisionsDoc, "DemoRegistrationDecisions"),
            (LambdaSource.TestingDoc, "DemoRegistrationTesting"), ("tests/smoke.mjs", "DemoRegistrationSmoke"),
            ("tests/accounts.json", "DemoRegistrationTestAccounts"), ("tests/seed.mjs", "DemoRegistrationSeed")),

        new("demo-game", "DemoGame",
            ("Lobby.cs", "DemoGameLobby"), ("Board.cs", "DemoGameBoard"), ("Protocol.cs", "DemoGameProtocol"),
            ("resources/migrations/V1__Create_outcomes.sql", "DemoGameMigration1"),
            ("resources/web/index.html", "DemoGamePage"), ("resources/web/game.js", "DemoGameScript"), ("resources/web/base.css", "DemoStyle"),
            (LambdaSource.ProductDoc, "DemoGameProduct"), (LambdaSource.DecisionsDoc, "DemoGameDecisions"),
            (LambdaSource.TestingDoc, "DemoGameTesting"), ("tests/smoke.mjs", "DemoGameSmoke")),

        new("demo-files", "DemoFiles",
            ("Gallery.cs", "DemoFilesGallery"),
            ("resources/migrations/V1__Create_uploads.sql", "DemoFilesMigration1"),
            ("resources/web/index.html", "DemoFilesPage"), ("resources/web/app.js", "DemoFilesApp"), ("resources/web/base.css", "DemoStyle"),
            (LambdaSource.ProductDoc, "DemoFilesProduct"), (LambdaSource.DecisionsDoc, "DemoFilesDecisions"),
            (LambdaSource.TestingDoc, "DemoFilesTesting"), ("tests/smoke.mjs", "DemoFilesSmoke")),

        new("demo-live", "DemoLive",
            ("Poll.cs", "DemoLivePoll"),
            ("resources/migrations/V1__Create_votes.sql", "DemoLiveMigration1"),
            ("resources/web/index.html", "DemoLivePage"), ("resources/web/app.js", "DemoLiveApp"), ("resources/web/base.css", "DemoStyle"),
            (LambdaSource.ProductDoc, "DemoLiveProduct"), (LambdaSource.DecisionsDoc, "DemoLiveDecisions"),
            (LambdaSource.TestingDoc, "DemoLiveTesting"), ("tests/smoke.mjs", "DemoLiveSmoke"))
    ];

    #region Functionality

    /// <summary>
    /// Whether a lambda can be started from the given identifier.
    /// </summary>
    public static bool Exists(string id) => Array.Exists(All, t => t.Id == id);

    /// <summary>
    /// The files a lambda starts with, as the blob a version is stored as.
    /// </summary>
    /// <param name="id">What to start from, the empty lambda when nothing or nothing known is given</param>
    /// <param name="publicKey">The key the lambda will be hosted at</param>
    /// <param name="demo">Whether this is the demo itself rather than a copy of it</param>
    public static string ForKey(string? id, string publicKey, bool demo = false)
        => LambdaSource.Serialize(FilesFor(id, publicKey, demo));

    /// <summary>
    /// The files a lambda starts with, the snippet first.
    /// </summary>
    /// <param name="demo">Whether this is the demo itself, which keeps the lines marked as its own</param>
    public static IReadOnlyList<LambdaFile> FilesFor(string? id, string publicKey, bool demo = false)
    {
        var template = Array.Find(All, t => t.Id == id)
                    ?? Array.Find(All, t => t.Id == EmptyId)!;

        var files = new List<LambdaFile>
        {
            new(LambdaSource.EntryName, Fill(template.Source, publicKey, demo))
        };

        foreach (var part in template.Parts)
        {
            files.Add(new LambdaFile(part.Name, Fill(part.Source, publicKey, demo)));
        }

        return files;
    }

    private static string Fill(string source, string publicKey, bool demo)
        => Mark(source, demo).Replace(Placeholder, publicKey, StringComparison.Ordinal);

    /// <summary>
    /// Drops the marker lines, and for a copy what is between them too.
    /// </summary>
    /// <remarks>
    /// A marker is a line of its own, in whatever comment the file type has -
    /// <c>// [demo]</c>, <c>&lt;!-- [demo] --&gt;</c> - so the file reads and
    /// runs as it is either way.
    /// </remarks>
    private static string Mark(string source, bool demo)
    {
        if (!source.Contains(DemoStart, StringComparison.Ordinal))
        {
            return source;
        }

        var kept = new List<string>();

        var inside = false;

        foreach (var line in source.Split('\n'))
        {
            if (line.Contains(DemoStart, StringComparison.Ordinal))
            {
                inside = true;
                continue;
            }

            if (line.Contains(DemoEnd, StringComparison.Ordinal))
            {
                inside = false;
                continue;
            }

            if (!inside || demo)
            {
                kept.Add(line);
            }
        }

        return string.Join('\n', kept);
    }

    #endregion

}

/// <summary>
/// The files of one starting point, read from the assembly the first time they are asked for.
/// </summary>
public sealed class LambdaTemplate(string id, string resource, params (string Name, string Resource)[] parts)
{
    private readonly Lazy<string> _source = new(() => Read(resource), LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly Lazy<IReadOnlyList<TemplatePart>> _parts = new(
        () => [.. parts.Select(p => new TemplatePart(p.Name, p.Resource))],
        LazyThreadSafetyMode.ExecutionAndPublication);

    public string Id => id;

    /// <summary>
    /// The files beside the snippet, in the order they are offered.
    /// </summary>
    public IReadOnlyList<TemplatePart> Parts => _parts.Value;

    /// <summary>
    /// The snippet, still carrying its key placeholder and demo markers.
    /// </summary>
    public string Source => _source.Value;

    /// <summary>
    /// Reads one embedded template file.
    /// </summary>
    /// <param name="resource">Its name in the assembly, without extensions</param>
    /// <param name="kind">
    /// The extension it is stored under, which is the extension of the file it
    /// becomes: a page is kept as a page, so what is read in the editor is what
    /// is served.
    /// </param>
    internal static string Read(string resource, string kind = "cs")
    {
        var assembly = Assembly.GetExecutingAssembly();

        var names = assembly.GetManifestResourceNames();

        var name = Array.Find(names, n => n.EndsWith($".{resource}.{kind}.txt", StringComparison.Ordinal))
                ?? throw new InvalidOperationException($"The template '{resource}.{kind}' is missing from the assembly ({string.Join(", ", names)}).");

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
    // the extension of the file this becomes is also the extension it is
    // stored under, so a part says what it is by what it is called
    private readonly Lazy<string> _source = new(
        () => LambdaTemplate.Read(resource, Path.GetExtension(name).TrimStart('.')),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public string Name => name;

    public string Source => _source.Value;
}
