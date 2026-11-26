using UglyToad.PdfPig.Exceptions;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// What an agent reads of a document a person attached - a PDF, or a text,
/// spreadsheet or presentation of an office suite - since a model is shown
/// text and pictures, never the file.
/// </summary>
/// <remarks>
/// <para>
/// A run's agent reads what was attached through <c>read_attachment</c> and
/// nothing else: it has no tools of its own to open a file with. So the text
/// is read out here, in the order a person reads it, and a long document is
/// handed over in parts: one answer holds what a tool result can carry, and
/// says where the next part starts.
/// </para>
/// <para>
/// The files are strangers', read in the server's process: bounded in what
/// they unpack to, how deep they nest and how far they are read, and a file
/// that cannot be read is said to be one rather than failing the call. The
/// old binary formats (.doc, .xls, .ppt) are not read, and so not taken.
/// </para>
/// </remarks>
internal static class DocumentText
{
    public const string Pdf = "application/pdf";

    public const string Word = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public const string Excel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public const string PowerPoint = "application/vnd.openxmlformats-officedocument.presentationml.presentation";

    public const string OpenText = "application/vnd.oasis.opendocument.text";

    public const string OpenSpreadsheet = "application/vnd.oasis.opendocument.spreadsheet";

    public const string OpenPresentation = "application/vnd.oasis.opendocument.presentation";

    /// <summary>Whether a file of this kind is read as a document.</summary>
    public static bool Reads(string mediaType) => mediaType is Pdf or Word or Excel or PowerPoint or OpenText or OpenSpreadsheet or OpenPresentation;

    /// <summary>
    /// A part of a document's text, from a place in it - long work, to be
    /// done away from the thread that serves the request.
    /// </summary>
    /// <param name="from">Where the part starts, as the answer before said</param>
    /// <param name="length">How long a part is at most</param>
    public static DocumentReading Read(string mediaType, byte[] content, int from, int length)
    {
        var text = new BoundedText(from, length);
        var pictures = new List<DocumentPicture>();

        int? pages = null;
        string? note = null;
        var broken = false;

        try
        {
            if (mediaType == Pdf)
            {
                pages = PdfText.Read(content, text, pictures);
            }
            else
            {
                using var package = PackageParts.Open(content);

                pages = mediaType switch
                {
                    Word => OfficeOpenXmlText.Word(package, text),
                    Excel => OfficeOpenXmlText.Workbook(package, text),
                    PowerPoint => OfficeOpenXmlText.Slides(package, text),
                    _ => OpenDocumentText.Read(package, text, mediaType == OpenSpreadsheet)
                };
            }
        }
        catch (PdfDocumentEncryptedException)
        {
            note = "It is protected with a password, so none of it can be read here. Ask the person for a copy without one, or what it says.";
        }
        catch (Exception e) when (e is not (OutOfMemoryException or OperationCanceledException))
        {
            // a reader of strangers' files fails in every way a file can be
            // broken - not a zip, XML cut off, a PDF of a dialect of its own;
            // what was read up to there is still shown
            broken = true;
        }

        var (part, next) = text.Part();

        if (broken)
        {
            note = part.Length > 0 || pictures.Count > 0
                ? "It could not be read to the end - it may be damaged, or made in a way this reader does not know. What could be read is shown."
                : "It could not be read - it may be damaged, or made in a way this reader does not know. Ask the person what it says, or for it in another format.";
        }
        else if (part.Length == 0 && note == null && pictures.Count == 0)
        {
            note = from > 0 ? "The text ends before this place." : "There is no text in it.";
        }

        return new DocumentReading(part, next, pages, pictures, note);
    }

}
