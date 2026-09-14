namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The lambdas the installation keeps online so that somebody who has not
/// written anything yet has something working to look at.
/// </summary>
/// <remarks>
/// They are the templates, deployed. That is deliberate rather than lazy: the
/// examples and the code a new lambda starts from ought to be the same thing,
/// and keeping two sets would mean the one nobody edits quietly rotting while
/// the other is maintained. It also means every example is already covered by
/// the test that compiles every template.
/// </remarks>
public static class ExampleCatalog
{

    /// <summary>
    /// The key an example is hosted at, which has to be stable: it is what
    /// anything linking to an example points at.
    /// </summary>
    public static string KeyFor(string templateId) => $"example-{templateId}";

    /// <summary>
    /// Where each example is worth calling, and how.
    /// </summary>
    /// <remarks>
    /// A lambda is mounted wherever its code says, and most of these mount
    /// nothing at their own root - offering "call it" against the root would
    /// answer every one of them with a 404 and teach a visitor that the
    /// examples are broken. The route is curated with the example because
    /// only the example knows it.
    /// </remarks>
    private static readonly Dictionary<string, (string Path, bool Socket)> Entry = new(StringComparer.Ordinal)
    {
        ["rest-service"] = ("books/", false),
        ["rest-minimal"] = ("api/", false),
        ["rest-webservice"] = ("api/", false),
        ["websocket-functional"] = ("", true),
        ["websocket-reactive"] = ("", true),
        ["websocket-imperative"] = ("", true),
        // these two serve their own page at the root, so the root is the point
        ["chat"] = ("", false),
        ["game"] = ("", false)
    };

    /// <summary>
    /// Every example the installation maintains.
    /// </summary>
    public static IEnumerable<LambdaExample> All
        => TemplateCatalog.Groups.SelectMany(g => g.Templates.Select(t => new LambdaExample(
            t.Id,
            g.Id,
            g.Name,
            t.Name,
            t.Description,
            KeyFor(t.Id),
            Entry.GetValueOrDefault(t.Id).Path ?? string.Empty,
            Entry.GetValueOrDefault(t.Id).Socket,
            t.Hidden
        )));

    /// <summary>
    /// The example a template identifier refers to, if it is one.
    /// </summary>
    public static LambdaExample? Find(string? id) => id == null ? null : All.FirstOrDefault(e => e.Id == id);

}

/// <summary>
/// One example: a template, and the lambda it is kept deployed as.
/// </summary>
/// <param name="Id">The template it comes from, which is also what a clone asks for</param>
/// <param name="GroupId">Which of the two kinds it belongs to</param>
/// <param name="GroupName">What that kind is called</param>
/// <param name="PublicKey">Where it is hosted, which is stable across restarts</param>
/// <param name="TryPath">What to call underneath it, since most mount nothing at their own root</param>
/// <param name="Socket">Whether it is answered by opening a socket rather than by asking for a page</param>
/// <param name="Hidden">Whether it is left out of the menu, as the picker leaves it out</param>
public sealed record LambdaExample(
    string Id,
    string GroupId,
    string GroupName,
    string Name,
    string Description,
    string PublicKey,
    string TryPath,
    bool Socket,
    bool Hidden
);
