using System.Text;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// The text read out of a document, up to the part an answer shows.
/// </summary>
/// <remarks>
/// A document is read from its start every time, since nothing is kept
/// between calls, but no further than the end of the part asked for (and one
/// character beyond, to know whether more follows) - so a long document costs
/// what was read of it, never all of it. Lines end without the spaces and tabs
/// a layout leaves, and never more than one empty line follows another.
/// </remarks>
internal sealed class BoundedText(int from, int length)
{
    private readonly StringBuilder text = new();

    private int end = from + length;

    #region Get-/Setters

    /// <summary>Where the part asked for starts.</summary>
    public int From => from;

    /// <summary>How much was read so far: where the next thing appended starts.</summary>
    public int Position => text.Length;

    /// <summary>Whether everything the part shows has been read.</summary>
    public bool Full => text.Length > end;

    #endregion

    #region Writing

    public void Append(string? value)
    {
        if (string.IsNullOrEmpty(value) || Full)
        {
            return;
        }

        var room = end + 1 - text.Length;

        text.Append(value.Length > room ? value[..room] : value);
    }

    /// <summary>What may hold line breaks of its own, each one a line.</summary>
    public void Write(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        var lines = value.ReplaceLineEndings("\n").Split('\n');

        for (var i = 0; i < lines.Length && !Full; i++)
        {
            if (i > 0)
            {
                Line();
            }

            Append(lines[i]);
        }
    }

    /// <summary>Between two cells of a table, without the spaces the cell ended with.</summary>
    public void Tab()
    {
        if (Full)
        {
            return;
        }

        while (text.Length > 0 && text[^1] == ' ')
        {
            text.Length--;
        }

        text.Append('\t');
    }

    public void Line()
    {
        if (Full)
        {
            return;
        }

        var keep = text.Length;

        while (keep > 0 && text[keep - 1] is ' ' or '\t')
        {
            keep--;
        }

        text.Length = keep;

        var breaks = 0;

        while (breaks < 2 && keep - breaks > 0 && text[keep - breaks - 1] == '\n')
        {
            breaks++;
        }

        if (keep > 0 && breaks < 2)
        {
            text.Append('\n');
        }
    }

    /// <summary>A page, a sheet or a slide begins, after an empty line.</summary>
    public void Heading(string title)
    {
        Line();
        Line();
        Append($"## {title}");
        Line();
    }

    /// <summary>
    /// Ends the part shown where something begins that does not fit into it
    /// any more, so the next part starts with it.
    /// </summary>
    public void Cut(int at) => end = Math.Max(from, at);

    #endregion

    #region Reading

    /// <summary>
    /// The part asked for, and where the next one starts - or nothing, where
    /// the document ends in this one.
    /// </summary>
    public (string Text, int? Next) Part()
    {
        if (from >= text.Length)
        {
            return ("", null);
        }

        var stop = Math.Min(end, text.Length);

        if (stop < text.Length)
        {
            // a part ends with a line where one ends in its second half, so
            // the next does not begin in the middle of a sentence
            for (var at = stop - 1; at > from + (stop - from) / 2 && text[stop] != '\n' && text[stop - 1] != '\n'; at--)
            {
                if (text[at] == '\n')
                {
                    stop = at + 1;
                    break;
                }
            }

            // and never between the two halves of a character outside the basic plane
            if (stop > from && char.IsHighSurrogate(text[stop - 1]))
            {
                stop--;
            }
        }

        return (text.ToString(from, stop - from).Trim('\n'), text.Length > stop ? stop : null);
    }

    #endregion

}
