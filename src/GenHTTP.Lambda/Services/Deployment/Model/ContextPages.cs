using System.Text;
using System.Text.RegularExpressions;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// Reads the pages of a version's context - its documentation and its tests -
/// for the places that show what they say without showing all of them.
/// </summary>
public static partial class ContextPages
{

    #region Functionality

    /// <summary>
    /// The text of a page, if the version has it as text.
    /// </summary>
    public static string? Read(IReadOnlyList<LambdaFile> files, string name)
    {
        var file = files.FirstOrDefault(f => f.Name == name);

        return file == null || file.Encoding == "base64" ? null : file.Code;
    }

    /// <summary>
    /// Which of the pages every version is meant to have are not there.
    /// </summary>
    public static IReadOnlyList<string> Missing(IReadOnlyList<LambdaFile> files)
        => [.. LambdaSource.ExpectedContext.Where(page => string.IsNullOrWhiteSpace(Read(files, page)))];

    /// <summary>
    /// The first paragraph of a page, as plain text - what the app is, in the
    /// sentence or two its documentation opens with.
    /// </summary>
    /// <remarks>
    /// Headings, comments, lists, tables, quotes and code are passed over:
    /// what is wanted is the prose a page begins with, the way a repository
    /// is described in a line under its name. Markup is taken out rather than
    /// rendered, since this is shown in a sentence, and cut at a word once it
    /// runs long.
    /// </remarks>
    /// <param name="markdown">The page</param>
    /// <param name="most">How many characters it may come to</param>
    public static string? FirstParagraph(string? markdown, int most = 280)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return null;
        }

        var lines = markdown.ReplaceLineEndings("\n").Split('\n');

        var paragraph = new List<string>();

        var fence = false;
        var comment = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // front matter, as some editors put at the top of a page
            if (i == 0 && line == "---")
            {
                var end = Array.FindIndex(lines, 1, l => l.Trim() == "---");

                if (end > 0)
                {
                    i = end;
                    continue;
                }
            }

            if (comment)
            {
                comment = !line.Contains("-->", StringComparison.Ordinal);
                continue;
            }

            if (line.StartsWith("```", StringComparison.Ordinal) || line.StartsWith("~~~", StringComparison.Ordinal))
            {
                fence = !fence;
                continue;
            }

            if (fence)
            {
                continue;
            }

            if (line.StartsWith("<!--", StringComparison.Ordinal))
            {
                comment = !line.Contains("-->", StringComparison.Ordinal);
                continue;
            }

            if (line.Length == 0)
            {
                if (paragraph.Count > 0)
                {
                    break;
                }

                continue;
            }

            if (IsBlock(line))
            {
                if (paragraph.Count > 0)
                {
                    break;
                }

                continue;
            }

            paragraph.Add(line);
        }

        if (paragraph.Count == 0)
        {
            return null;
        }

        var text = Plain(string.Join(' ', paragraph));

        if (text.Length == 0)
        {
            return null;
        }

        if (text.Length <= most)
        {
            return text;
        }

        var cut = text.LastIndexOf(' ', most - 1);

        return string.Concat(text.AsSpan(0, cut > most / 2 ? cut : most - 1), "…");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// A line that starts something other than a paragraph.
    /// </summary>
    private static bool IsBlock(string line)
        => line.StartsWith('#') || line.StartsWith('>') || line.StartsWith('|') || line.StartsWith("    ", StringComparison.Ordinal)
        || ListItem().IsMatch(line) || Rule().IsMatch(line);

    /// <summary>
    /// A paragraph with its markup taken out.
    /// </summary>
    private static string Plain(string text)
    {
        text = Image().Replace(text, "$1");
        text = Link().Replace(text, "$1");
        text = Tag().Replace(text, string.Empty);

        var builder = new StringBuilder(text.Length);

        foreach (var character in text)
        {
            if (character is not ('*' or '`'))
            {
                builder.Append(character);
            }
        }

        return Spaces().Replace(builder.ToString(), " ").Trim();
    }

    [GeneratedRegex(@"^([-*+]|\d+[.)])\s")]
    private static partial Regex ListItem();

    [GeneratedRegex(@"^([-*_]\s*){3,}$")]
    private static partial Regex Rule();

    [GeneratedRegex(@"!\[([^\]]*)\]\([^)]*\)")]
    private static partial Regex Image();

    [GeneratedRegex(@"\[([^\]]*)\]\([^)]*\)")]
    private static partial Regex Link();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex Tag();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();

    #endregion

}
