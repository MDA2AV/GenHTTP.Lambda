namespace GenHTTP.Lambda.Services.Showcase;

/// <summary>
/// A lambda as its owner presents it.
/// </summary>
/// <param name="PublicKey">Where it answers</param>
/// <param name="Online">Whether it answers right now - only those are listed</param>
/// <param name="Updated">When the entry last changed, which versions the picture</param>
public sealed record ShowcaseInfo(
    string PublicKey,
    string Title,
    string Description,
    string ImageType,
    int ImageBytes,
    bool Online,
    DateTime Created,
    DateTime Updated
);

/// <summary>
/// One page of the showcase, in the order it is shown.
/// </summary>
/// <param name="Total">How many are listed altogether</param>
public sealed record ShowcasePage(IReadOnlyList<ShowcaseInfo> Entries, int Total);

/// <summary>
/// The picture of an entry, to be served.
/// </summary>
public sealed record ShowcaseImage(byte[] Content, string Type, DateTime Updated);

/// <summary>
/// What an owner wants the entry to say. The picture may be left out when
/// there already is one, to keep it.
/// </summary>
public sealed record ShowcaseDraft(string? Title, string? Description, byte[]? Image);

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
