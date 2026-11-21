using System.Text;
using System.Xml;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// What the operator may give the installation to present itself with: a name
/// that fits the bar of a page, and a logo a browser draws.
/// </summary>
/// <remarks>
/// Said once for every door - the panel, the API, the MCP tool - and with
/// fixed numbers, which the API and the tool may write down: they protect
/// the pages every visitor loads rather than being a promise to anybody.
/// </remarks>
public static class BrandChecks
{

    /// <summary>
    /// How long a name may be: room in the bar of a phone beside its links,
    /// and in the title of a tab.
    /// </summary>
    public const int MaxName = 40;

    /// <summary>
    /// How large a logo may be. Every page loads it, once, beside the name
    /// and as the icon of the tab - a mark rather than a photograph.
    /// </summary>
    public const int MaxLogoBytes = 512 * 1024;

    /// <summary>
    /// What an SVG is served as.
    /// </summary>
    public const string Svg = "image/svg+xml";

    private const string SvgNamespace = "http://www.w3.org/2000/svg";

    /// <summary>
    /// Every kind of picture a logo may be.
    /// </summary>
    public static IReadOnlyList<string> Types { get; } = [.. ImageTypes.Raster, Svg];

    private static readonly UTF8Encoding Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    #region Functionality

    /// <summary>
    /// The name as it is kept, or nothing for the platform's own: trimmed,
    /// one line, and short enough for the bar.
    /// </summary>
    public static string? Name(string? name)
    {
        var trimmed = name?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > MaxName)
        {
            throw LambdaException.Invalid($"The name must not be longer than {MaxName} characters.");
        }

        // nothing that is no letter at all, or that turns the letters around
        // it the other way - a name is read in the bar and in every title
        if (trimmed.Any(c => char.IsControl(c) || c is >= '‪' and <= '‮' or >= '⁦' and <= '⁩'))
        {
            throw LambdaException.Invalid("The name has to be one line of text, without control characters.");
        }

        return trimmed;
    }

    /// <summary>
    /// What the logo is, read from its content: a PNG, JPEG, GIF or WebP image,
    /// or an SVG drawing.
    /// </summary>
    public static string Logo(byte[] content)
    {
        if (content.Length == 0)
        {
            throw LambdaException.Invalid("The logo is empty.");
        }

        if (content.Length > MaxLogoBytes)
        {
            throw LambdaException.Invalid($"The logo must not be larger than {MaxLogoBytes / 1024} KB.");
        }

        return ImageTypes.Sniff(content) ?? (IsSvg(content) ? Svg : throw LambdaException.Invalid(
            "The logo has to be a PNG, JPEG, GIF, WebP or SVG image."));
    }

    /// <summary>
    /// Whether the content is an SVG a browser draws as a picture: text, well
    /// formed, its root an <c>svg</c> element in the SVG namespace - without
    /// which a browser shows no picture at all.
    /// </summary>
    /// <remarks>
    /// Nothing it refers to is fetched and no document type it names is
    /// read. Whatever script it holds is never run: a page draws it as an
    /// image, and opened on its own it is served with a policy that runs
    /// nothing (see <c>SystemResource.Logo</c>).
    /// </remarks>
    private static bool IsSvg(byte[] content)
    {
        string text;

        try
        {
            text = Strict.GetString(content);
        }
        catch (DecoderFallbackException)
        {
            return false;
        }

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };

        try
        {
            using var reader = XmlReader.Create(new StringReader(text.TrimStart('﻿')), settings);

            if (reader.MoveToContent() != XmlNodeType.Element || reader.LocalName != "svg")
            {
                return false;
            }

            if (reader.NamespaceURI != SvgNamespace)
            {
                throw LambdaException.Invalid($"The logo is an SVG without xmlns=\"{SvgNamespace}\" on its svg element, which a browser does not draw as a picture.");
            }

            // the rest of it, so a drawing that is cut off is refused too
            while (reader.Read())
            {
            }

            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }

    #endregion

}
