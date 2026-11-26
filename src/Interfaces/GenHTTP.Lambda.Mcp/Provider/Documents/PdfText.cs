using System.Text;

using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// The text of a PDF, page by page - and a page without any, a scan, as the
/// picture it is, which the model reads as it reads any picture.
/// </summary>
/// <remarks>
/// <para>
/// A menu, a flyer or a form people attach is as often a scan as a document
/// with text in it, and a scan read as text is nothing at all. So a page that
/// holds no letters or digits is shown as its largest picture, in the part of
/// the answer its page begins in - the JPEG as the PDF keeps it, anything else
/// as a PNG where the reader can make one. At most a few pictures go into one
/// answer, since each is large: a part ends before the page whose picture
/// would be one too many, and the next part begins with it.
/// </para>
/// <para>
/// No page is drawn: what is not a picture (a vector drawing, a chart made of
/// lines) is not shown. Pages are read until the part asked for is full.
/// </para>
/// </remarks>
internal static class PdfText
{

    /// <summary>How many pictures of pages one answer holds at most.</summary>
    public const int MostPictures = 3;

    /// <summary>The largest picture shown: what a model takes of one, with room to spare.</summary>
    public const int LargestPicture = 3_500_000;

    /// <summary>A smaller picture on a page without text is a mark or an icon, not the page.</summary>
    private const int SmallestSide = 200;

    /// <summary>How wide a gap between two letters is a space, as a part of their size.</summary>
    private const double SpaceGap = 0.12;

    private static readonly ParsingOptions Options = new()
    {
        UseLenientParsing = true,
        SkipMissingFonts = true
    };

    /// <returns>How many pages it has</returns>
    /// <exception cref="UglyToad.PdfPig.Exceptions.PdfDocumentEncryptedException">It cannot be opened without a password</exception>
    public static int Read(byte[] content, BoundedText text, List<DocumentPicture> pictures)
    {
        using var document = PdfDocument.Open(content, Options);

        var pages = document.NumberOfPages;

        for (var number = 1; number <= pages && !text.Full; number++)
        {
            var page = document.GetPage(number);

            var start = text.Position;

            text.Heading($"Page {number}");

            var words = Words(page);

            if (words.Any(char.IsLetterOrDigit))
            {
                text.Write(words);
                continue;
            }

            if (start < text.From)
            {
                text.Append("[No text on this page - a scan or a picture. It was shown with the part this page begins in.]");
                continue;
            }

            var picture = PictureOf(page);

            if (picture == null)
            {
                text.Append("[No text on this page, and no picture on it that can be shown.]");
                continue;
            }

            if (picture.Value.Content.Length > LargestPicture)
            {
                text.Append("[No text on this page - a scan or a picture, too large to be shown here.]");
                continue;
            }

            if (pictures.Count == MostPictures)
            {
                text.Cut(start);
                break;
            }

            pictures.Add(new DocumentPicture(picture.Value.Content, picture.Value.MediaType, number));

            text.Append($"[No text on this page - a scan or a picture: picture {pictures.Count} of this answer.]");
        }

        return pages;
    }

    /// <summary>
    /// The text of a page, its letters in the order the page draws them: a
    /// line where the baseline moves or the text goes back to the left, a
    /// space where a gap is one.
    /// </summary>
    /// <remarks>
    /// A PDF need not keep its spaces as letters - justified text set by TeX
    /// keeps none, only the distance - and a reader that takes the letters
    /// as given runs whole lines into one word. A gap of more than an eighth
    /// of the letter's size is a space; within a word, letters sit closer
    /// than that, kerned or not.
    /// </remarks>
    private static string Words(Page page)
    {
        var text = new StringBuilder();

        Letter? last = null;

        foreach (var letter in page.Letters)
        {
            var blank = string.IsNullOrWhiteSpace(letter.Value);

            if (last != null)
            {
                var size = Math.Max(last.PointSize, 1);

                if (Math.Abs(letter.StartBaseLine.Y - last.StartBaseLine.Y) > size * 0.5 || letter.StartBaseLine.X < last.StartBaseLine.X - size)
                {
                    text.Append('\n');
                }
                else if (!blank && text.Length > 0 && text[^1] is not (' ' or '\n') && letter.StartBaseLine.X - last.EndBaseLine.X > size * SpaceGap)
                {
                    text.Append(' ');
                }
            }

            if (blank)
            {
                if (text.Length > 0 && text[^1] is not (' ' or '\n'))
                {
                    text.Append(' ');
                }
            }
            else
            {
                text.Append(letter.Value);
            }

            last = letter;
        }

        return text.ToString();
    }

    /// <summary>
    /// The largest picture of a page, as a JPEG or a PNG - or nothing, where
    /// it has none large enough or none either could be made of.
    /// </summary>
    private static (byte[] Content, string MediaType)? PictureOf(Page page)
    {
        var largest = page.GetImages()
                          .Where(i => i.WidthInSamples >= SmallestSide && i.HeightInSamples >= SmallestSide)
                          .OrderByDescending(i => (long)i.WidthInSamples * i.HeightInSamples)
                          .FirstOrDefault();

        if (largest == null)
        {
            return null;
        }

        // a JPEG is kept in a PDF as the file it is (DCTDecode)
        var raw = largest.RawMemory.Span;

        if (raw.Length > 3 && raw[0] == 0xFF && raw[1] == 0xD8 && raw[2] == 0xFF)
        {
            return (raw.ToArray(), "image/jpeg");
        }

        return largest.TryGetPng(out var png) && png is { Length: > 0 } ? (png, "image/png") : null;
    }

}
