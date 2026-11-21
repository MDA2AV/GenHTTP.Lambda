namespace GenHTTP.Lambda.Web;

/// <summary>
/// What the frontend build rendered: every public page in every language by
/// the path it has in that language, the showcase by language and by the shape
/// of its first page, and one entry of the showcase by language, as it appears
/// in there - and all of it once more as a branded installation shows it,
/// where the site may be branded, and the pages that differ for each profile
/// the site says something of its own to, by its name.
/// </summary>
public sealed record Prerendered(
    Dictionary<string, string> Pages,
    Dictionary<string, Dictionary<string, string>> Showcase,
    Dictionary<string, string> Entry,
    PrerenderedPages? Branded = null,
    Dictionary<string, PrerenderedProfile>? Profiles = null
)
{

    /// <summary>
    /// The pages as GenHTTP Lambda shows them.
    /// </summary>
    public PrerenderedPages Plain => new(Pages, Showcase);

}
