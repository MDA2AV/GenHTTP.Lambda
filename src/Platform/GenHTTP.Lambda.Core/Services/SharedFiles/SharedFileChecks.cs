using System.Text;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// What a shared file may be: a small number of small files, of types that
/// are drawn or read and never run.
/// </summary>
/// <remarks>
/// Said once for every door, with fixed numbers the API and the tools may
/// write down: a place for a logo, a few fonts and a style guide rather than
/// for what an app keeps, and nothing a lambda's owner is promised.
///
/// The end of the name says what a file is, and its content has to be that:
/// the files are served from the platform's own address, and a type taken on
/// trust is how something else is served as if it were one.
/// </remarks>
public static class SharedFileChecks
{

    /// <summary>
    /// How many files there may be. Agents are handed their names with the
    /// guide, so there are no more than fit into it.
    /// </summary>
    public const int MaxFiles = 40;

    /// <summary>
    /// How large one may be: a font of every weight and many scripts, or a
    /// picture for the whole width of a page.
    /// </summary>
    public const int MaxBytes = 2 * 1024 * 1024;

    /// <summary>
    /// How large they may be together.
    /// </summary>
    public const int MaxTotalBytes = 20 * 1024 * 1024;

    /// <summary>
    /// How long a name may be.
    /// </summary>
    public const int MaxName = 60;

    /// <summary>
    /// How large a text may be to be handed to an agent as a tool's answer;
    /// a larger one is fetched by its address.
    /// </summary>
    public const int MaxReadBytes = 32 * 1024;

    private static readonly UTF8Encoding Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    #region Functionality

    /// <summary>
    /// What a file of the given name is, or why it may not be called that.
    /// </summary>
    public static SharedFileType Name(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw LambdaException.Invalid("The file needs a name, such as house-style.md.");
        }

        if (name.Length > MaxName)
        {
            throw LambdaException.Invalid($"The name must not be longer than {MaxName} characters.");
        }

        // an address as it is typed, and a name on any disk
        if (!char.IsAsciiLetterOrDigit(name[0]) || !name.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.'))
        {
            throw LambdaException.Invalid($"'{name}' is no name for a shared file: letters and digits without accents, dashes, underscores and dots, starting with a letter or a digit.");
        }

        return SharedFileType.Of(name) ?? throw LambdaException.Invalid(
            $"'{name}' ends in no type a shared file may have: {string.Join(", ", SharedFileType.All.Keys.Order().Select(k => "." + k))}.");
    }

    /// <summary>
    /// What a font draws, where it is one - or why the content is not what
    /// its name says.
    /// </summary>
    public static FontFace? Content(string name, SharedFileType type, byte[] content)
    {
        if (content.Length == 0)
        {
            throw LambdaException.Invalid("The file is empty.");
        }

        Size(content.Length);

        switch (type.Kind)
        {
            case SharedFileKind.Font:
                return Font(name, type, content);

            case SharedFileKind.Picture when type.Type == SharedFileType.Svg:

                if (!SvgImage.Is(content))
                {
                    throw LambdaException.Invalid($"'{name}' is no SVG drawing: well formed, its root an svg element with xmlns=\"{SvgImage.Namespace}\".");
                }

                return null;

            case SharedFileKind.Picture:

                var drawn = ImageTypes.Sniff(content);

                if (drawn != type.Type)
                {
                    throw LambdaException.Invalid(drawn == null ? $"'{name}' is no PNG, JPEG, GIF or WebP image."
                                                                : $"'{name}' is of type {drawn}: give it the end of that.");
                }

                return null;

            default:

                if (!IsText(content))
                {
                    throw LambdaException.Invalid($"'{name}' is no text in UTF-8.");
                }

                return null;
        }
    }

    /// <summary>
    /// Refuses a file that is larger than one may be, as soon as that is known.
    /// </summary>
    public static void Size(long bytes)
    {
        if (bytes > MaxBytes)
        {
            throw LambdaException.Invalid($"A shared file must not be larger than {MaxBytes / 1024 / 1024} MB.");
        }
    }

    private static FontFace Font(string name, SharedFileType type, byte[] content)
    {
        var format = FontFile.Sniff(content);

        if (format == null)
        {
            throw LambdaException.Invalid($"'{name}' is no WOFF2, WOFF, TrueType or OpenType font{(content.AsSpan().StartsWith("ttcf"u8) ? " but a collection of several: take the one you want out of it" : string.Empty)}.");
        }

        if (format != type.Type)
        {
            throw LambdaException.Invalid($"'{name}' is a font of type {format}: call it .{format["font/".Length..]}.");
        }

        return FontFile.Read(content) ?? throw LambdaException.Invalid($"'{name}' is a damaged font, or a collection of several: a browser could not draw it.");
    }

    private static bool IsText(byte[] content)
    {
        try
        {
            return !Strict.GetString(content).Contains('\0');
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    #endregion

}
