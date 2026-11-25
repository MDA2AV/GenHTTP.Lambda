namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// The shades the pages draw an accent in, worked out so that what is written
/// in it can be read.
/// </summary>
/// <remarks>
/// An operator enters the colour of their house, and any colour may be one: a
/// yellow nobody can read on white, a navy that vanishes on the dark page. So
/// it is drawn as entered wherever it is a surface - a button, the plate of
/// the monogram, the lights behind the front page - and as a lighter or a
/// darker shade of itself where it is text, no further from the colour than
/// reading it takes: 4.5 to 1 on white and on the dark page, as the WCAG asks
/// for text, and more where the platform's own blue has more. What is written
/// on a button of it is white or dark, whichever can be read.
///
/// Said here once, for every page and for the panel, which shows what a
/// colour will come to before it is saved.
/// </remarks>
public sealed record BrandPalette(BrandShades Light, BrandShades Dark)
{

    /// <summary>
    /// The dark theme's page (<c>ink-950</c> of the kit).
    /// </summary>
    private static readonly BrandColor Night = new(0x20, 0x21, 0x24);

    /// <summary>
    /// How much of the moving background is seen behind the headline, through
    /// the band it is read on.
    /// </summary>
    private const double Behind = 0.4;

    private const double Step = 0.01;

    #region Functionality

    /// <summary>
    /// The shades of an accent.
    /// </summary>
    /// <param name="accent">The colour as it was entered</param>
    public static BrandPalette Of(BrandColor accent) => new(OnWhite(accent), OnNight(accent));

    /// <summary>
    /// What stands in for a second accent where there is none: the colour a
    /// little on from the first and as bright as makes it another light, so
    /// the two of the moving background are told apart and are still of one
    /// house.
    /// </summary>
    /// <remarks>
    /// On from a yellow is an olive, which nobody's house is: its neighbour is
    /// the amber on the other side.
    /// </remarks>
    public static BrandColor Beside(BrandColor accent)
    {
        var seen = PerceivedColor.Of(accent);

        if (seen is { Degrees: >= 70 and <= 125, Lightness: > 0.6 })
        {
            return seen.Turned(-30).Shifted(-0.08).ToColor();
        }

        return seen.Turned(20).Shifted(seen.Lightness < 0.72 ? 0.12 : -0.1).ToColor();
    }

    /// <summary>
    /// What can be read on a surface of a colour: white where that is enough,
    /// the dark of the pages otherwise - and black for the few between them,
    /// on which neither is.
    /// </summary>
    public static BrandColor InkOn(BrandColor surface)
    {
        if (surface.ContrastTo(BrandColor.White) >= 4.5)
        {
            return BrandColor.White;
        }

        return surface.ContrastTo(Night) >= 4.5 ? Night : BrandColor.Black;
    }

    private static BrandShades OnWhite(BrandColor accent)
    {
        var light = Until(accent, -Step, c => c.ContrastTo(BrandColor.White) >= 1.35);

        var line = Until(accent, -Step, c => c.ContrastTo(BrandColor.White) >= 4.5);
        var text = Until(Shifted(line, -0.045), -Step, c => c.ContrastTo(BrandColor.White) >= 5.5);
        var deep = Until(Shifted(text, -0.06), -Step, c => c.ContrastTo(BrandColor.White) >= 7 && c.ContrastTo(light.Over(BrandColor.White, Behind)) >= 4.5);

        // as entered, but for one so pale that a button of it would be a word on the page
        var fill = Until(accent, -Step, c => c.ContrastTo(BrandColor.White) >= 1.25);

        return new(Shifted(line, 0.16), line, text, deep, fill, Pressed(fill), InkOn(fill), light);
    }

    private static BrandShades OnNight(BrandColor accent)
    {
        var light = Until(accent, Step, c => c.ContrastTo(Night) >= 3.2);

        var line = Until(accent, Step, c => c.ContrastTo(Night) >= 4.5);

        // read over the moving background as well - but where that would take
        // the accent out of the text, it is the light that gives way
        var tint = Until(accent, Step, c => c.ContrastTo(Night) >= 8 || (c.ContrastTo(Night) >= 6.5 && Reads(c, light)));

        light = Until(light, -Step, c => Reads(tint, c));

        var fill = Until(accent, Step, c => c.ContrastTo(Night) >= 3);

        // what the dark theme leaves to the shades of the light one is read on a dark page too
        return new(tint, line, line, line, fill, Pressed(fill), InkOn(fill), light);
    }

    private static bool Reads(BrandColor text, BrandColor light) => text.ContrastTo(light.Over(Night, Behind)) >= 4.5;

    /// <summary>
    /// The button under the pointer: a little darker - or lighter, where it is
    /// so dark that nobody would see a difference, or what is written on it
    /// would be harder to read.
    /// </summary>
    private static BrandColor Pressed(BrandColor fill)
    {
        var darker = Shifted(fill, -0.045);

        return PerceivedColor.Of(fill).Lightness >= 0.35 && InkOn(fill).ContrastTo(darker) >= 4.5 ? darker : Shifted(fill, 0.06);
    }

    private static BrandColor Shifted(BrandColor color, double lightness) => PerceivedColor.Of(color).Shifted(lightness).ToColor();

    /// <summary>
    /// The colour itself where it fits, or the first of its shades that does.
    /// </summary>
    private static BrandColor Until(BrandColor color, double step, Func<BrandColor, bool> fits)
    {
        if (fits(color))
        {
            return color;
        }

        var start = PerceivedColor.Of(color);

        var shade = color;

        for (var shift = step; start.Lightness + shift is >= 0 and <= 1; shift += step)
        {
            shade = start.Shifted(shift).ToColor();

            if (fits(shade))
            {
                break;
            }
        }

        return shade;
    }

    #endregion

}
