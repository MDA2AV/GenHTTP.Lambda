using System.Globalization;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// A colour of the installation, as a browser draws it.
/// </summary>
/// <remarks>
/// Three numbers rather than what somebody typed: whatever is written into a
/// page is written from these, so nothing an operator entered ever is.
/// </remarks>
public readonly record struct BrandColor(byte Red, byte Green, byte Blue)
{

    public static readonly BrandColor White = new(255, 255, 255);

    public static readonly BrandColor Black = new(0, 0, 0);

    #region Get-/Setters

    /// <summary>
    /// The colour as it is entered and shown: <c>#1a73e8</c>.
    /// </summary>
    public string Hex => $"#{Red:x2}{Green:x2}{Blue:x2}";

    /// <summary>
    /// The colour as the style sheet of the pages keeps it, so an opacity can
    /// be written behind it: <c>26 115 232</c>.
    /// </summary>
    public string Channels => string.Create(CultureInfo.InvariantCulture, $"{Red} {Green} {Blue}");

    /// <summary>
    /// How bright it is to the eye, from 0 to 1, as the WCAG counts it.
    /// </summary>
    public double Luminance => 0.2126 * Linear(Red) + 0.7152 * Linear(Green) + 0.0722 * Linear(Blue);

    #endregion

    #region Functionality

    /// <summary>
    /// Reads a colour as it is entered: <c>#1a73e8</c>, or <c>#18e</c> for
    /// short - with or without the sign, which an address would have to encode.
    /// </summary>
    public static bool TryParse(string? text, out BrandColor color)
    {
        color = default;

        var digits = text?.Trim();

        if (digits?.StartsWith('#') == true)
        {
            digits = digits[1..];
        }

        if (digits is not { Length: 3 or 6 } || !digits.All(char.IsAsciiHexDigit))
        {
            return false;
        }

        if (digits.Length == 3)
        {
            digits = $"{digits[0]}{digits[0]}{digits[1]}{digits[1]}{digits[2]}{digits[2]}";
        }

        color = new(Channel(digits, 0), Channel(digits, 2), Channel(digits, 4));

        return true;
    }

    /// <summary>
    /// How well one is told from the other, from 1 to 21, as the WCAG counts
    /// it: text needs 4.5, the edge of something to press 3.
    /// </summary>
    public double ContrastTo(BrandColor other)
    {
        var (mine, theirs) = (Luminance, other.Luminance);

        return (Math.Max(mine, theirs) + 0.05) / (Math.Min(mine, theirs) + 0.05);
    }

    /// <summary>
    /// What is seen where this colour is drawn over another with an opacity.
    /// </summary>
    public BrandColor Over(BrandColor below, double opacity)
        => new(Mix(Red, below.Red, opacity), Mix(Green, below.Green, opacity), Mix(Blue, below.Blue, opacity));

    public override string ToString() => Hex;

    private static byte Channel(string digits, int at) => byte.Parse(digits.AsSpan(at, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    private static byte Mix(byte above, byte below, double opacity) => (byte)Math.Round(above * opacity + below * (1 - opacity));

    /// <summary>
    /// A channel as light adds up, from 0 to 1, rather than as it is stored.
    /// </summary>
    internal static double Linear(byte channel)
    {
        var value = channel / 255.0;

        return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
    }

    #endregion

}
