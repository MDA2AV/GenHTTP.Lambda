using System.Text;
using System.Text.RegularExpressions;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// What the <c>.gitignore</c> files of a development space leave out of it.
/// </summary>
/// <remarks>
/// What a build installs, caches and writes for itself - node_modules, target
/// or a virtual environment, for example - is named differently by every
/// tool, and the space says which it is: in its .gitignore files. So the
/// platform reads those rather than keeping a list of its own. A clone
/// follows them because git does; this is what a zip put back follows,
/// where the files were gathered from a folder somebody built in.
///
/// The rules are git's (<c>gitignore(5)</c>): a pattern without a slash
/// matches a name at any depth, one with a slash is anchored to the folder
/// of its file, a trailing slash matches folders only, <c>!</c> takes a
/// match back, <c>*</c>, <c>?</c>, <c>[...]</c> and <c>**</c> as git reads
/// them, the last match wins and a deeper file overrides one above it - and
/// nothing below a folder that is left out comes back in, since git never
/// looks into one. Matched without backtracking, since the patterns are
/// whatever somebody wrote.
/// </remarks>
public sealed class IgnoredPaths
{
    private readonly IReadOnlyList<(string Folder, IReadOnlyList<Rule> Rules)> _files;

    private IgnoredPaths(IReadOnlyList<(string Folder, IReadOnlyList<Rule> Rules)> files)
    {
        _files = files;
    }

    /// <summary>
    /// Nothing left out.
    /// </summary>
    public static IgnoredPaths None { get; } = new([]);

    #region Functionality

    /// <summary>
    /// The rules of the given .gitignore files.
    /// </summary>
    /// <param name="files">Each file by its path - "web/.gitignore" for one in web/ - and what it says</param>
    public static IgnoredPaths Of(IEnumerable<(string Path, string Content)> files)
    {
        var read = new List<(string Folder, IReadOnlyList<Rule> Rules)>();

        foreach (var (path, content) in files)
        {
            var slash = path.LastIndexOf('/');

            var folder = slash < 0 ? string.Empty : path[..(slash + 1)];

            var rules = Parse(content);

            if (rules.Count > 0)
            {
                read.Add((folder, rules));
            }
        }

        // the files above first, so a deeper one has the last word
        read.Sort((x, y) => x.Folder.Count(c => c == '/').CompareTo(y.Folder.Count(c => c == '/')));

        return new IgnoredPaths(read);
    }

    /// <summary>
    /// Whether a file is left out: it, or a folder it is in.
    /// </summary>
    /// <param name="path">Its path, relative to where the paths of the .gitignore files are</param>
    public bool Ignores(string path)
    {
        if (_files.Count == 0)
        {
            return false;
        }

        var segments = path.Split('/');

        // git never looks into a folder it leaves out, so nothing below it is taken back
        for (var depth = 1; depth < segments.Length; depth++)
        {
            if (Matches(string.Join('/', segments, 0, depth), true))
            {
                return true;
            }
        }

        return Matches(path, false);
    }

    #endregion

    #region Matching

    private bool Matches(string path, bool folder)
    {
        bool? ignored = null;

        foreach (var (where, rules) in _files)
        {
            if (!path.StartsWith(where, StringComparison.Ordinal))
            {
                continue;
            }

            var relative = path[where.Length..];

            if (relative.Length == 0)
            {
                continue;
            }

            var name = relative[(relative.LastIndexOf('/') + 1)..];

            foreach (var rule in rules)
            {
                if (rule.FoldersOnly && !folder)
                {
                    continue;
                }

                if (rule.Pattern.IsMatch(rule.Anchored ? relative : name))
                {
                    ignored = !rule.Negated;
                }
            }
        }

        return ignored == true;
    }

    #endregion

    #region Reading

    private sealed record Rule(Regex Pattern, bool Anchored, bool FoldersOnly, bool Negated);

    private static List<Rule> Parse(string content)
    {
        var rules = new List<Rule>();

        foreach (var raw in content.ReplaceLineEndings("\n").Split('\n'))
        {
            var line = TrimEnd(raw);

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var negated = line.StartsWith('!');

            if (negated)
            {
                line = line[1..];
            }
            else if (line.StartsWith("\\!", StringComparison.Ordinal) || line.StartsWith("\\#", StringComparison.Ordinal))
            {
                line = line[1..];
            }

            var foldersOnly = line.EndsWith('/');

            if (foldersOnly)
            {
                line = line.TrimEnd('/');
            }

            if (line.Length == 0)
            {
                continue;
            }

            // a slash anywhere but at the end ties the pattern to the folder of its file
            var anchored = line.Contains('/');

            if (line.StartsWith('/'))
            {
                line = line[1..];
            }

            if (Translate(line) is { } pattern)
            {
                rules.Add(new Rule(pattern, anchored, foldersOnly, negated));
            }
        }

        return rules;
    }

    /// <summary>
    /// A line without the spaces it ends with, unless one is escaped.
    /// </summary>
    private static string TrimEnd(string line)
    {
        var end = line.Length;

        while (end > 0 && line[end - 1] == ' ' && !(end > 1 && line[end - 2] == '\\'))
        {
            end--;
        }

        return line[..end];
    }

    /// <summary>
    /// A pattern of git's as a regular expression over a whole path.
    /// </summary>
    /// <returns>Nothing for a pattern git would not make sense of either</returns>
    private static Regex? Translate(string pattern)
    {
        var builder = new StringBuilder("^");

        var i = 0;

        while (i < pattern.Length)
        {
            var character = pattern[i];

            if (character == '*' && i + 1 < pattern.Length && pattern[i + 1] == '*'
                && (i == 0 || pattern[i - 1] == '/')
                && (i + 2 == pattern.Length || pattern[i + 2] == '/'))
            {
                if (i + 2 == pattern.Length)
                {
                    // a/** - everything inside
                    builder.Append(".*");
                    i += 2;
                }
                else
                {
                    // **/a and a/**/b - any number of folders, none included
                    builder.Append("(?:.*/)?");
                    i += 3;
                }

                continue;
            }

            switch (character)
            {
                case '*':
                    builder.Append("[^/]*");
                    break;

                case '?':
                    builder.Append("[^/]");
                    break;

                case '\\' when i + 1 < pattern.Length:
                    builder.Append(Regex.Escape(pattern[++i].ToString()));
                    break;

                case '[':
                    var close = pattern.IndexOf(']', i + 2);

                    if (close < 0)
                    {
                        builder.Append("\\[");
                        break;
                    }

                    var inside = pattern[(i + 1)..close];

                    var negated = inside[0] is '!' or '^';

                    if (negated)
                    {
                        inside = inside[1..];
                    }

                    var set = new StringBuilder();

                    foreach (var member in inside)
                    {
                        // a slash is never matched, as git separates names on it first
                        if (member != '/')
                        {
                            set.Append(member is '\\' or ']' or '[' or '^' ? $"\\{member}" : member.ToString());
                        }
                    }

                    builder.Append(negated ? $"[^/{set}]" : $"[{set}]");

                    i = close;
                    break;

                default:
                    builder.Append(Regex.Escape(character.ToString()));
                    break;
            }

            i++;
        }

        builder.Append('$');

        try
        {
            return new Regex(builder.ToString(), RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    #endregion

}
