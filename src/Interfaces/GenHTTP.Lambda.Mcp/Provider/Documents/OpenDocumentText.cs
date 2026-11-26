using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// The text of an OpenDocument text, spreadsheet or presentation (.odt, .ods,
/// .odp), as LibreOffice and others save them.
/// </summary>
/// <remarks>
/// <para>
/// All three keep their content in <c>content.xml</c>: paragraphs and headings
/// as lines, a table row as a line with a tab between its cells, a sheet or a
/// slide under a heading. Speaker notes, comments, footnotes and tracked
/// changes are left out.
/// </para>
/// <para>
/// A spreadsheet says how often a row or a cell repeats rather than repeating
/// it - often a million times, for the empty rest of a sheet - so nothing
/// empty is repeated and nothing else more than a few times. Elements are
/// known by their local name, as in <see cref="OfficeOpenXmlText"/>, and
/// nesting is followed only so deep.
/// </para>
/// </remarks>
internal static class OpenDocumentText
{
    private const int Deepest = 64;

    /// <summary>How often a row or a cell that holds something is repeated at most.</summary>
    private const int MostRepeated = 20;

    private static readonly HashSet<string> Skipped = ["notes", "annotation", "note", "tracked-changes", "automatic-styles", "font-face-decls", "forms", "scripts"];

    /// <returns>How many sheets or slides it has, where it has any</returns>
    public static int? Read(PackageParts package, BoundedText text, bool spreadsheet)
    {
        using var reader = package.Read("content.xml");

        if (reader == null)
        {
            return null;
        }

        var sheets = 0;
        var slides = 0;

        reader.MoveToContent();

        while (!reader.EOF && !text.Full)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.LocalName)
                {
                    case "p" or "h":
                        text.Write(Inline((XElement)XNode.ReadFrom(reader)));
                        text.Line();
                        continue;
                    case "table-row":
                        Row((XElement)XNode.ReadFrom(reader), text);
                        continue;
                    case "table" when spreadsheet:
                        sheets++;
                        text.Heading($"Sheet: {Attribute(reader, "name") ?? sheets.ToString(CultureInfo.InvariantCulture)}");
                        break;
                    case "page":
                        slides++;
                        text.Heading($"Slide {slides}");
                        break;
                    case var name when Skipped.Contains(name):
                        reader.Skip();
                        continue;
                }
            }

            reader.Read();
        }

        return spreadsheet ? sheets : slides > 0 ? slides : null;
    }

    private static void Row(XElement row, BoundedText text)
    {
        var lines = new StringBuilder();

        // the tabs owed before the next cell that holds something: one for
        // every empty cell, and one between it and the cell before - so the
        // cells stay in their columns, and nothing trails a row
        var waiting = 0;

        foreach (var cell in row.Elements().Where(e => e.Name.LocalName is "table-cell" or "covered-table-cell"))
        {
            var repeated = Repeated(cell, "number-columns-repeated");

            var value = string.Join(' ', cell.Elements().Where(e => e.Name.LocalName is "p" or "h").Select(p => Inline(p).ReplaceLineEndings(" ").Trim()));

            if (value.Length == 0)
            {
                waiting += repeated;
                continue;
            }

            for (var i = 0; i < Math.Min(repeated, MostRepeated); i++)
            {
                lines.Append('\t', Math.Min(waiting, 50));
                lines.Append(value);
                waiting = 1;
            }
        }

        if (lines.Length == 0)
        {
            text.Line();
            return;
        }

        var line = lines.ToString();

        for (var i = 0; i < Math.Min(Repeated(row, "number-rows-repeated"), MostRepeated) && !text.Full; i++)
        {
            text.Append(line);
            text.Line();
        }
    }

    private static int Repeated(XElement element, string name)
        => int.TryParse(element.Attributes().FirstOrDefault(a => a.Name.LocalName == name)?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var count) && count > 0 ? count : 1;

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

        foreach (var node in element.Nodes())
        {
            if (node is XText plain)
            {
                text.Append(plain.Value);
                continue;
            }

            if (node is not XElement child)
            {
                continue;
            }

            switch (child.Name.LocalName)
            {
                case "s":
                    text.Append(' ', Math.Clamp(Repeated(child, "c"), 1, 100));
                    break;
                case "tab":
                    text.Append('\t');
                    break;
                case "line-break":
                    text.Append('\n');
                    break;
                case "p" or "h":
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

    private static string? Attribute(XmlReader reader, string name)
    {
        if (reader.MoveToFirstAttribute())
        {
            do
            {
                if (reader.LocalName == name)
                {
                    var value = reader.Value;

                    reader.MoveToElement();

                    return value;
                }
            }
            while (reader.MoveToNextAttribute());

            reader.MoveToElement();
        }

        return null;
    }

}
