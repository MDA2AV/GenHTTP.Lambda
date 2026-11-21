namespace GenHTTP.Lambda.Web;

/// <summary>
/// The pages of a site that differ for one profile from the pages every
/// profile shares - as GenHTTP Lambda shows them, and as a brand does where
/// the site may be branded. A page not here is the shared one; nothing at
/// all is still a rendering, of a profile that differs nowhere.
/// </summary>
public sealed record PrerenderedProfile(
    Dictionary<string, string> Pages,
    Dictionary<string, Dictionary<string, string>> Showcase,
    PrerenderedPages? Branded = null
)
{

    /// <summary>
    /// The pages as GenHTTP Lambda shows them.
    /// </summary>
    public PrerenderedPages Plain => new(Pages, Showcase);

}
