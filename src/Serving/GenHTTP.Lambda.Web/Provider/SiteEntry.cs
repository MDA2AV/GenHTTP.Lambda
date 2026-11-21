namespace GenHTTP.Lambda.Web;

/// <summary>
/// A page as <c>pages.json</c> has it: its picture, and its words by language.
/// </summary>
public sealed record SiteEntry(string? Image, Dictionary<string, SiteText> Text);
