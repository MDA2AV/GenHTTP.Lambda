using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// The text of a Word document, an Excel workbook or a PowerPoint
/// presentation (Office Open XML: .docx, .xlsx, .pptx).
/// </summary>
/// <remarks>
/// <para>
/// Read in the order a person reads it: paragraphs as lines, tables with a tab
/// between cells, a sheet's rows with its cells in their columns, slides in the
/// order the presentation shows them. What is not read: headers, footers,
/// comments, speaker notes, formulas (their values are), deleted or moved-away
/// text, and the second copy a document keeps of a shape for older readers
/// (<c>mc:Fallback</c>).
/// </para>
/// <para>
/// Elements are known by their local name only, so the strict flavour of the
/// format, whose namespaces differ, is read as well. A paragraph or a row is
/// read as a whole, everything else as it streams by; nesting is followed so
/// deep and no deeper, since a stranger's file could nest until the stack
/// gives out.
/// </para>
/// </remarks>
internal static class OfficeOpenXmlText
{
    private const int Deepest = 64;

    /// <summary>What is never text a person reads, wherever it appears.</summary>
    private static readonly HashSet<string> Skipped = ["Fallback", "pPr", "rPr", "moveFrom", "del", "delText", "instrText", "rPh", "extLst"];

    #region Documents

    /// <returns>How many pages it had when it was last saved, where it says</returns>
    public static int? Word(PackageParts package, BoundedText text)
    {
        var main = Main(package, "word/document.xml");

        using (var reader = package.Read(main))
        {
            if (reader != null)
            {
                Flow(reader, text);
            }
        }

        return Count(package, "docProps/app.xml", "Pages");
    }

    /// <returns>How many slides it has</returns>
    public static int? Slides(PackageParts package, BoundedText text)
    {
        var main = Main(package, "ppt/presentation.xml");

        var targets = package.Relationships(main).ToDictionary(r => r.Id, r => r.Target);

        var slides = new List<string>();

        using (var reader = package.Read(main))
        {
            while (reader != null && reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "sldId" && Reference(reader) is { } id && targets.TryGetValue(id, out var slide))
                {
                    slides.Add(slide);
                }
            }
        }

        for (var i = 0; i < slides.Count && !text.Full; i++)
        {
            text.Heading($"Slide {i + 1}");

            using var reader = package.Read(slides[i]);

            if (reader != null)
            {
                Flow(reader, text);
            }
        }

        return slides.Count;
    }

    /// <summary>
    /// Paragraphs as lines, the cells of a table with a tab between them and
    /// a row as a line - Word's and PowerPoint's elements share the names.
    /// </summary>
    private static void Flow(XmlReader reader, BoundedText text)
    {
        var cells = 0;

        reader.MoveToContent();

        while (!reader.EOF && !text.Full)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.LocalName)
                {
                    case "p":
                        {
                            var line = Inline((XElement)XNode.ReadFrom(reader));

                            if (cells > 0)
                            {
                                text.Append(line.ReplaceLineEndings(" ").Trim() + " ");
                            }
                            else
                            {
                                text.Write(line);
                                text.Line();
                            }

                            continue;
                        }
                    case "tc" when !reader.IsEmptyElement:
                        cells++;
                        break;
                    case var name when Skipped.Contains(name):
                        reader.Skip();
                        continue;
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                switch (reader.LocalName)
                {
                    case "tc":
                        cells = Math.Max(0, cells - 1);
                        text.Tab();
                        break;
                    case "tr":
                        text.Line();
                        break;
                }
            }

            reader.Read();
        }
    }

    #endregion

    #region Workbooks

    /// <returns>How many sheets it has</returns>
    public static int? Workbook(PackageParts package, BoundedText text)
    {
        var main = Main(package, "xl/workbook.xml");

        var related = package.Relationships(main);

        var targets = related.ToDictionary(r => r.Id, r => r.Target);

        var shared = SharedStrings(package, related.Where(r => r.Kind == "sharedStrings").Select(r => (string?)r.Target).FirstOrDefault());

        var sheets = new List<(string Name, string Part)>();

        using (var reader = package.Read(main))
        {
            while (reader != null && reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "sheet" && Reference(reader) is { } id && targets.TryGetValue(id, out var part))
                {
                    sheets.Add((reader.GetAttribute("name") ?? $"Sheet {sheets.Count + 1}", part));
                }
            }
        }

        foreach (var (name, part) in sheets)
        {
            if (text.Full)
            {
                break;
            }

            text.Heading($"Sheet: {name}");

            using var reader = package.Read(part);

            if (reader == null)
            {
                continue;
            }

            reader.MoveToContent();

            while (!reader.EOF && !text.Full)
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "row")
                {
                    Row((XElement)XNode.ReadFrom(reader), shared, text);
                    continue;
                }

                reader.Read();
            }
        }

        return sheets.Count;
    }

    /// <summary>
    /// The cells of a row in their columns, a tab between two - so a table
    /// reads as one, gaps and all.
    /// </summary>
    private static void Row(XElement row, IReadOnlyList<string> shared, BoundedText text)
    {
        var column = 0;
        var next = 0;

        foreach (var cell in row.Elements().Where(e => e.Name.LocalName == "c"))
        {
            // a cell that does not say where it is follows the one before
            var at = Column(cell.Attribute("r")?.Value) ?? next;

            next = at + 1;

            var value = Value(cell, shared);

            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            // a cell far off to the right is not worth a page of tabs
            for (var gap = Math.Min(at - column, 50); gap > 0; gap--)
            {
                text.Tab();
            }

            text.Append(value.ReplaceLineEndings(" "));

            column = at;
        }

        text.Line();
    }

    private static string? Value(XElement cell, IReadOnlyList<string> shared)
    {
        var value = cell.Elements().FirstOrDefault(e => e.Name.LocalName == "v")?.Value;

        return cell.Attribute("t")?.Value switch
        {
            "s" => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index < shared.Count ? shared[index] : null,
            "inlineStr" => cell.Elements().FirstOrDefault(e => e.Name.LocalName == "is") is { } inline ? Inline(inline) : null,
            "b" => value == "1" ? "TRUE" : value == "0" ? "FALSE" : value,
            _ => value
        };
    }

    /// <summary>The column a reference names, counted from zero: <c>C7</c> is two.</summary>
    private static int? Column(string? reference)
    {
        if (string.IsNullOrEmpty(reference))
        {
            return null;
        }

        var column = 0;

        foreach (var c in reference)
        {
            if (!char.IsAsciiLetter(c))
            {
                break;
            }

            column = column * 26 + (char.ToUpperInvariant(c) - 'A' + 1);

            if (column > 16384)
            {
                return null;
            }
        }

        return column > 0 ? column - 1 : null;
    }

    /// <summary>
    /// The texts the cells of a workbook share, by index - up to what a
    /// part of an answer could show of them.
    /// </summary>
    private static List<string> SharedStrings(PackageParts package, string? part)
    {
        var shared = new List<string>();

        using var reader = part == null ? null : package.Read(part);

        if (reader == null)
        {
            return shared;
        }

        long kept = 0;

        reader.MoveToContent();

        while (!reader.EOF)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "si")
            {
                var value = Inline((XElement)XNode.ReadFrom(reader));

                kept += value.Length;

                shared.Add(kept > PackageParts.MostUnpacked / 8 ? "" : value);
                continue;
            }

            reader.Read();
        }

        return shared;
    }

    #endregion

    #region Parts

    /// <summary>
    /// The text of a paragraph, a run or a cell: its text elements, tabs as
    /// tabs, breaks as line breaks.
    /// </summary>
    private static string Inline(XElement element)
    {
        var text = new StringBuilder();

        Inline(element, text, 0);

        return text.ToString();
    }

    private static void Inline(XElement element, StringBuilder text, int depth)
    {
        if (depth > Deepest)
        {
            return;
        }

        foreach (var child in element.Elements())
        {
            switch (child.Name.LocalName)
            {
                case "t":
                    text.Append(child.Value);
                    break;
                case "tab":
                    text.Append('\t');
                    break;
                case "br" or "cr":
                    text.Append('\n');
                    break;
                case "p":
                    // a paragraph within one - in a text box
                    text.Append(' ');
                    Inline(child, text, depth + 1);
                    break;
                case var name when Skipped.Contains(name):
                    break;
                default:
                    Inline(child, text, depth + 1);
                    break;
            }
        }
    }

    /// <summary>The document's main part, as the package names it.</summary>
    private static string Main(PackageParts package, string usual)
        => package.Relationships("").Where(r => r.Kind == "officeDocument").Select(r => (string?)r.Target).FirstOrDefault() ?? usual;

    /// <summary>The id an element refers to another part by (<c>r:id</c>), whichever namespace says it.</summary>
    private static string? Reference(XmlReader reader)
    {
        if (reader.MoveToFirstAttribute())
        {
            do
            {
                if (reader.LocalName == "id" && reader.NamespaceURI.Length > 0)
                {
                    var id = reader.Value;

                    reader.MoveToElement();

                    return id;
                }
            }
            while (reader.MoveToNextAttribute());

            reader.MoveToElement();
        }

        return null;
    }

    /// <summary>A number the document's properties keep, such as its pages.</summary>
    private static int? Count(PackageParts package, string part, string name)
    {
        using var reader = package.Read(part);

        while (reader != null && reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == name)
            {
                return int.TryParse(reader.ReadElementContentAsString(), NumberStyles.None, CultureInfo.InvariantCulture, out var count) && count > 0 ? count : null;
            }
        }

        return null;
    }

    #endregion

}
