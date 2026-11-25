namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// What an accent comes to on the pages of one theme: the shades they draw
/// in it, by the names the kit's style sheet has for them.
/// </summary>
/// <param name="Tint">Text and marks in the accent on a dark page (<c>accent-400</c>)</param>
/// <param name="Line">The edges, the marks and the links of a page (<c>accent-500</c>)</param>
/// <param name="Text">Text in the accent on a light page (<c>accent-600</c>)</param>
/// <param name="Deep">Large text in the accent, read over the moving background (<c>accent-700</c>)</param>
/// <param name="Fill">A button filled with it</param>
/// <param name="Hover">The same button under the pointer</param>
/// <param name="Ink">What is written on the button</param>
/// <param name="Light">The light it makes in the moving background</param>
public sealed record BrandShades(BrandColor Tint, BrandColor Line, BrandColor Text, BrandColor Deep, BrandColor Fill, BrandColor Hover, BrandColor Ink, BrandColor Light);
