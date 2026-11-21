namespace GenHTTP.Lambda.Web;

/// <summary>
/// The public pages of a site as one brand shows them: every page by its path
/// in its language, and the showcase by language and by the shape of its
/// first page.
/// </summary>
public sealed record PrerenderedPages(
    Dictionary<string, string> Pages,
    Dictionary<string, Dictionary<string, string>> Showcase
);
