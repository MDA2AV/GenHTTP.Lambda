namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// What a font file says it draws, read from the file itself.
/// </summary>
/// <param name="Family">What the font calls itself, for whoever chooses among several</param>
/// <param name="From">The lightest weight it draws, from 1 to 1000: 400 is regular, 700 bold</param>
/// <param name="To">The heaviest - the same, unless it is a variable font, which draws every weight between</param>
/// <param name="Italic">Whether its letters are slanted</param>
/// <param name="Letters">Those of <see cref="FontLetters.All"/> it has - or nothing where it does not say in a way that is read here</param>
public sealed record FontFace(string Family, int From, int To, bool Italic, string? Letters)
{

    /// <summary>
    /// Whether it draws more than one weight.
    /// </summary>
    public bool Variable => From != To;

}
