namespace GenHTTP.Lambda.Services.Showcase;

/// <summary>
/// How much an entry may say.
/// </summary>
/// <remarks>
/// Short on purpose. A card on a page of cards is read in the time it takes to
/// scroll past it, and a limit is the kindest way to say that to somebody
/// about to write three paragraphs.
/// </remarks>
public static class ShowcaseLimits
{

    public const int MaxTitle = 60;

    public const int MaxDescription = 280;

    /// <summary>
    /// What an entry should read like, said once for the editor, the API and
    /// the agents alike.
    /// </summary>
    public const string Tone =
        "Write plainly and concretely: what it is and what a visitor can do with it. " +
        "No marketing language, no superlatives, no exclamation marks, no emoji.";

}
