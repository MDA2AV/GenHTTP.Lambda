using System.Text;

using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Guide;

/// <summary>
/// What the operator's section of the guide may be: a heading of its own, and
/// a text short enough that the guide stays one tool result an agent reads in
/// full.
/// </summary>
/// <remarks>
/// Said once for every door, with fixed numbers the API may write down: they
/// protect what every agent is handed rather than promise anybody anything.
/// The text is counted in bytes, not characters - a client is handed bytes,
/// and a character of Japanese or Korean is three of them.
/// </remarks>
public static class GuideChecks
{

    /// <summary>
    /// How long the title may be: it is a heading.
    /// </summary>
    public const int MaxTitle = 60;

    /// <summary>
    /// How large the text may be in UTF-8. Every agent reads it with every
    /// call to the guide, beside a platform's guide held under 24 KB - so the
    /// two stay under 30 KB, which a client hands the model as it is.
    /// </summary>
    public const int MaxTextBytes = 6 * 1024;

    #region Functionality

    /// <summary>
    /// The section as it is kept, or why it is not.
    /// </summary>
    /// <param name="taken">The headings of the platform's own guide, which the section may not be called</param>
    public static GuideSection Section(string? title, string? text, IReadOnlySet<string> taken)
        => new(Title(title, taken), Text(text));

    private static string Title(string? title, IReadOnlySet<string> taken)
    {
        var trimmed = title?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            throw LambdaException.Invalid("The section needs a title: it is its heading in the guide.");
        }

        if (trimmed.Length > MaxTitle)
        {
            throw LambdaException.Invalid($"The title must not be longer than {MaxTitle} characters.");
        }

        // one line that reads the same way round as the guide around it
        if (trimmed.Any(c => char.IsControl(c) || c is >= '‪' and <= '‮' or >= '⁦' and <= '⁩'))
        {
            throw LambdaException.Invalid("The title has to be one line of text, without control characters.");
        }

        if (trimmed.StartsWith('#'))
        {
            throw LambdaException.Invalid("The title is the heading already: leave out the #.");
        }

        if (taken.Contains(trimmed))
        {
            throw LambdaException.Invalid($"'{trimmed}' is a section of the platform's guide, which the tools point agents to by its name. Give yours another title.");
        }

        return trimmed;
    }

    private static string Text(string? text)
    {
        var normalized = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Trim();

        if (normalized.Length == 0)
        {
            throw LambdaException.Invalid("The section needs a text. To take it out of the guide, remove it.");
        }

        var bytes = Encoding.UTF8.GetByteCount(normalized);

        if (bytes > MaxTextBytes)
        {
            throw LambdaException.Invalid($"The text must not be larger than {MaxTextBytes / 1024} KB in UTF-8 - it is {bytes:N0} bytes. Every agent reads it with every call to the guide.");
        }

        if (normalized.Any(c => char.IsControl(c) && c is not '\n' and not '\t'))
        {
            throw LambdaException.Invalid("The text must not hold control characters.");
        }

        Structure(normalized);

        return normalized;
    }

    /// <summary>
    /// Refuses a text that would end the section early: a heading of the first
    /// or second level - written with # or ##, or underlined with = or - - or a
    /// code block left open, which would make the rest of the guide code.
    /// </summary>
    /// <remarks>
    /// Read the way Markdown reads it, closely enough: nothing inside a fenced
    /// code block is a heading (a shell comment starts with #), a fence closes
    /// only with the character it opened with, at least as many times, and a
    /// line of dashes after a blank line is a rule rather than an underline.
    /// </remarks>
    private static void Structure(string text)
    {
        var lines = text.Split('\n');

        char? fence = null;
        var length = 0;
        var opened = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = Unindented(lines[i]);

            if (fence is { } mark)
            {
                if (line != null && Run(line, mark) is var closing && closing >= length && line[closing..].Trim().Length == 0)
                {
                    fence = null;
                }

                continue;
            }

            if (line == null)
            {
                continue;
            }

            if (line.Length > 0 && line[0] is '`' or '~' && Run(line, line[0]) is >= 3 and var run)
            {
                fence = line[0];
                length = run;
                opened = i + 1;
                continue;
            }

            if (Atx(line) || (i > 0 && Underline(line) && Paragraph(lines[i - 1])))
            {
                throw LambdaException.Invalid($"Line {i + 1} is a heading of the first or second level, which are the guide's own: headings inside the section take ### or more.");
            }
        }

        if (fence is { } open)
        {
            throw LambdaException.Invalid($"The code block opened on line {opened} is never closed, which would make the rest of the guide code: close it with {new string(open, length)}.");
        }
    }

    /// <summary>
    /// How many times a line starts with the character.
    /// </summary>
    private static int Run(string line, char mark)
    {
        var run = 0;

        while (run < line.Length && line[run] == mark)
        {
            run++;
        }

        return run;
    }

    /// <summary>
    /// The line without the up to three spaces Markdown lets a block start
    /// with - or nothing where it is indented further, which is code.
    /// </summary>
    private static string? Unindented(string line)
    {
        var spaces = 0;

        while (spaces < line.Length && line[spaces] == ' ')
        {
            spaces++;
        }

        return spaces > 3 ? null : line[spaces..];
    }

    private static bool Atx(string line)
    {
        var level = 0;

        while (level < line.Length && line[level] == '#')
        {
            level++;
        }

        return level is 1 or 2 && (line.Length == level || line[level] is ' ' or '\t');
    }

    private static bool Underline(string line)
    {
        var trimmed = line.TrimEnd(' ', '\t');

        return trimmed.Length > 0 && (trimmed.All(c => c == '=') || trimmed.All(c => c == '-'));
    }

    /// <summary>
    /// Whether a line is text an underline below it would make a heading of.
    /// </summary>
    private static bool Paragraph(string line)
    {
        var text = Unindented(line)?.TrimEnd();

        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        // a list item, a quote, a table row or a fence is not a paragraph
        if (text[0] is '>' or '|' or '#' or '`' or '~' || text.StartsWith("- ", StringComparison.Ordinal) || text.StartsWith("* ", StringComparison.Ordinal)
            || text.StartsWith("+ ", StringComparison.Ordinal))
        {
            return false;
        }

        var digits = 0;

        while (digits < text.Length && char.IsAsciiDigit(text[digits]))
        {
            digits++;
        }

        return !(digits > 0 && digits < text.Length && text[digits] is '.' or ')');
    }

    #endregion

}
