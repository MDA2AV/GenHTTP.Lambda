using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What an accent comes to in one theme, each as `#rrggbb`: itself where it is a surface, and as much lighter or darker where it is text as reading it takes.")]
public sealed record BrandingShadesResponse(
    [Summary("Text and marks on a dark page")]
    string Tint,
    [Summary("Edges, marks and links")]
    string Line,
    [Summary("Text on a light page")]
    string Text,
    [Summary("Large text on a light page, read over the moving background")]
    string Deep,
    [Summary("A button")]
    string Fill,
    [Summary("The button under the pointer")]
    string Hover,
    [Summary("What is written on the button")]
    string Ink
)
{

    internal static BrandingShadesResponse? Of(BrandShades? shades)
        => shades == null ? null : new(shades.Tint.Hex, shades.Line.Hex, shades.Text.Hex, shades.Deep.Hex, shades.Fill.Hex, shades.Hover.Hex, shades.Ink.Hex);

}
