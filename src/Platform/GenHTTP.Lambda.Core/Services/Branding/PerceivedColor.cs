namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// A colour by how it is seen - how light, how strong, which hue (OKLCH) -
/// rather than by what a screen mixes it from.
/// </summary>
/// <remarks>
/// Made lighter or darker by its numbers of red, green and blue, a colour
/// changes its hue and its strength along the way: a dark yellow becomes an
/// olive, a light blue a lavender. Here its lightness is a number of its own,
/// so a shade of an accent is still that accent.
/// </remarks>
internal readonly record struct PerceivedColor(double Lightness, double Chroma, double Hue)
{

    /// <summary>
    /// Its place on the wheel of hues, from 0 to 360: red is near 30, yellow
    /// 100, green 145, blue 260.
    /// </summary>
    public double Degrees => (Hue * 180 / Math.PI + 360) % 360;

    #region Functionality

    public static PerceivedColor Of(BrandColor color)
    {
        var (r, g, b) = (BrandColor.Linear(color.Red), BrandColor.Linear(color.Green), BrandColor.Linear(color.Blue));

        var l = Math.Cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
        var m = Math.Cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
        var s = Math.Cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b);

        var lightness = 0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s;
        var a = 1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s;
        var c = 0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s;

        return new(lightness, Math.Sqrt(a * a + c * c), Math.Atan2(c, a));
    }

    /// <summary>
    /// The colour a screen draws for it: itself, or - where no screen can be
    /// that strong at that lightness - the strongest of its hue that can.
    /// </summary>
    public BrandColor ToColor()
    {
        if (TryMix(Chroma, out var exact))
        {
            return exact;
        }

        var (possible, impossible) = (0.0, Chroma);

        TryMix(0, out var mixed);

        while (impossible - possible > 0.0005)
        {
            var chroma = (possible + impossible) / 2;

            if (TryMix(chroma, out var color))
            {
                (possible, mixed) = (chroma, color);
            }
            else
            {
                impossible = chroma;
            }
        }

        return mixed;
    }

    /// <summary>
    /// The same colour, lighter or darker.
    /// </summary>
    public PerceivedColor Shifted(double lightness) => this with { Lightness = Math.Clamp(Lightness + lightness, 0, 1) };

    /// <summary>
    /// The colour beside it on the wheel, as many degrees on.
    /// </summary>
    public PerceivedColor Turned(double degrees) => this with { Hue = Hue + degrees * Math.PI / 180 };

    private bool TryMix(double chroma, out BrandColor color)
    {
        var (a, c) = (chroma * Math.Cos(Hue), chroma * Math.Sin(Hue));

        var l = Cube(Lightness + 0.3963377774 * a + 0.2158037573 * c);
        var m = Cube(Lightness - 0.1055613458 * a - 0.0638541728 * c);
        var s = Cube(Lightness - 0.0894841775 * a - 1.2914855480 * c);

        var r = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
        var g = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
        var b = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;

        color = new(Stored(r), Stored(g), Stored(b));

        return Possible(r) && Possible(g) && Possible(b);
    }

    private static double Cube(double value) => value * value * value;

    private static bool Possible(double linear) => linear is >= -0.0005 and <= 1.0005;

    private static byte Stored(double linear)
    {
        var value = Math.Clamp(linear, 0, 1);

        return (byte)Math.Round(255 * (value <= 0.0031308 ? 12.92 * value : 1.055 * Math.Pow(value, 1 / 2.4) - 0.055));
    }

    #endregion

}
