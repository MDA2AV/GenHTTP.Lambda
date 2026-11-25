using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.SharedFiles;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What a font file draws, as read from it.")]
public sealed record FontFaceResponse(
    [Summary("What the font calls itself")]
    string Family,
    [Summary("The lightest weight it draws, from 1 to 1000: 400 is regular, 700 bold")]
    int From,
    [Summary("The heaviest weight it draws: the same, unless it is a variable font")]
    int To,
    [Summary("Whether its letters are slanted")]
    bool Italic
)
{

    internal static FontFaceResponse? Of(FontFace? face) => face == null ? null : new(face.Family, face.From, face.To, face.Italic);

}
