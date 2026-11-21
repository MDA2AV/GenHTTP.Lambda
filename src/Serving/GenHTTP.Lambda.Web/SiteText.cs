namespace GenHTTP.Lambda.Web;

/// <summary>
/// The name and description of a page in one language, and the picture of
/// its link preview in that language, if it has one drawn in it.
/// </summary>
public sealed record SiteText(string Title, string Description, string? Image = null);
