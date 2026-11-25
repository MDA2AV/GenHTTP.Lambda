namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// Which letters a font is asked for, and which languages of the pages it has
/// too few of them for.
/// </summary>
/// <remarks>
/// A font from the web is often cut down to the letters of one part of Latin:
/// on a page in Polish, the letters it lacks are then drawn in another face in
/// the middle of a word - which whoever chose it, reading the pages in their
/// own language, never sees. So what a font has is read as it is uploaded.
///
/// Only the letters beyond A to Z that the languages write words with, and
/// only of those in Latin letters: a font of Latin letters draws no Japanese
/// and no Korean, which everybody knows, and the system's face draws them
/// well beside it.
/// </remarks>
public static class FontLetters
{
    private const string Latin = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    /// <summary>
    /// The letters of each language beyond those of English, by its code.
    /// </summary>
    private static readonly (string Language, string Letters)[] Alphabets =
    [
        ("de", "ÄÖÜäöüß"),
        ("es", "ÁÉÍÑÓÚÜáéíñóúü¡¿"),
        ("fr", "ÀÂÆÇÈÉÊËÎÏÔÙÛÜŒàâæçèéêëîïôùûüÿœ"),
        ("it", "ÀÈÉÌÒÙàèéìòù"),
        ("nl", "ÉËÏÖÜéëïöü"),
        ("pl", "ĄĆĘŁŃÓŚŹŻąćęłńóśźż"),
        ("pt", "ÀÁÂÃÇÉÊÍÓÔÕÚàáâãçéêíóôõú"),
        ("tr", "ÇĞİÖŞÜçğıöşü")
    ];

    #region Functionality

    /// <summary>
    /// Every letter a font is asked for.
    /// </summary>
    public static string All { get; } = new([.. Alphabets.SelectMany(a => a.Letters).Concat(Latin).Distinct().Order()]);

    /// <summary>
    /// The languages a font made of the given files has too few letters for,
    /// by their codes - English among them where it lacks any of A to Z, as a
    /// font of pictures does. None where a file does not say what it has.
    /// </summary>
    public static IReadOnlyList<string> Lacking(IReadOnlyList<SharedFile> files)
    {
        if (files.Count == 0 || files.Any(f => f.Font?.Letters == null))
        {
            return [];
        }

        var letters = files.SelectMany(f => f.Font!.Letters!).ToHashSet();

        if (!Latin.All(letters.Contains))
        {
            return ["en", .. Alphabets.Select(a => a.Language)];
        }

        return [.. Alphabets.Where(a => !a.Letters.All(letters.Contains)).Select(a => a.Language)];
    }

    #endregion

}
