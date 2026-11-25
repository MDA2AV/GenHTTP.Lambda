using System.Text;
using System.Xml;

namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Whether something uploaded is an SVG a browser draws as a picture, read
/// from its content.
/// </summary>
/// <remarks>
/// Text, well formed, its root an <c>svg</c> element in the SVG namespace -
/// without which a browser shows no picture at all. Nothing it refers to is
/// fetched and no document type it names is read. Whatever script it holds is
/// never run: a page draws it as an image, and opened on its own it is served
/// with a policy that runs nothing. Shared by every door a drawing comes in
/// at, as <see cref="ImageTypes"/> is.
/// </remarks>
public static class SvgImage
{

    /// <summary>
    /// What an SVG is served as.
    /// </summary>
    public const string Type = "image/svg+xml";

    public const string Namespace = "http://www.w3.org/2000/svg";

    private static readonly UTF8Encoding Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    #region Functionality

    /// <summary>
    /// Whether the content is an SVG a browser draws.
    /// </summary>
    public static bool Is(byte[] content) => Is(content, out var named) && named;

    /// <summary>
    /// Whether the content is an SVG, and whether it names the namespace a
    /// browser needs to draw it - so whoever sent one without can be told.
    /// </summary>
    public static bool Is(byte[] content, out bool named)
    {
        named = false;

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

            named = reader.NamespaceURI == Namespace;

            // the rest of it, so a drawing that is cut off is none either
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
