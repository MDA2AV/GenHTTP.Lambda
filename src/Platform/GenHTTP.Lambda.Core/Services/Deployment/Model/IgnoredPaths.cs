using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// What the <c>.gitignore</c> files of a build folder leave out of it.
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
///
/// The rules are kept by the folder of their file, so a path is held only
/// against the files of the folders it is in, however many others there are.
/// </remarks>
public sealed class IgnoredPaths
{
    private readonly ImmutableDictionary<string, ImmutableList<Rule>> _folders;

    private IgnoredPaths(ImmutableDictionary<string, ImmutableList<Rule>> folders)
    {
        _folders = folders;
    }

    /// <summary>
    /// Nothing left out.
    /// </summary>
    public static IgnoredPaths None { get; } = new(ImmutableDictionary.Create<string, ImmutableList<Rule>>(StringComparer.Ordinal));

    #region Functionality

    /// <summary>
    /// The rules of the given .gitignore files.
    /// </summary>
    /// <param name="files">Each file by its path - "web/.gitignore" for one in web/ - and what it says</param>
    public static IgnoredPaths Of(IEnumerable<(string Path, string Content)> files)
        => files.Aggregate(None, (ignored, file) => ignored.With(file.Path, file.Content));

    /// <summary>
    /// These rules and those of one more .gitignore file, which is read once
    /// and not again for the files that come after it.
    /// </summary>
    /// <param name="path">Where the file is - "web/.gitignore" for one in web/</param>
    /// <param name="content">What it says</param>
    public IgnoredPaths With(string path, string content)
    {
        var rules = Parse(content);

        if (rules.Count == 0)
        {
            return this;
        }

        var slash = path.LastIndexOf('/');

        var folder = slash < 0 ? string.Empty : path[..(slash + 1)];

        // of two files for the same folder, the one given later has the last word
        var before = _folders.GetValueOrDefault(folder, ImmutableList<Rule>.Empty);

        return new IgnoredPaths(_folders.SetItem(folder, before.AddRange(rules)));
    }

    /// <summary>
    /// Whether a file is left out: it, or a folder it is in.
    /// </summary>
    /// <param name="path">Its path, relative to where the paths of the .gitignore files are</param>
    public bool Ignores(string path)
    {
        if (_folders.IsEmpty)
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

        var name = path[(path.LastIndexOf('/') + 1)..];

        // the folders it is in from the top, so a deeper file has the last word
        var at = 0;

        while (true)
        {
            if (_folders.TryGetValue(path[..at], out var rules))
            {
                var relative = path[at..];

                foreach (var rule in rules)
                {
                    if ((!rule.FoldersOnly || folder) && rule.Pattern.IsMatch(rule.Anchored ? relative : name))
                    {
                        ignored = !rule.Negated;
                    }
                }
            }

            var next = path.IndexOf('/', at);

            if (next < 0)
            {
                return ignored == true;
            }

            at = next + 1;
        }
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
                    // a set holds one character at least, so its end is two
                    // further at the earliest - and one that is never closed
                    // matches nothing in git
                    var close = i + 2 < pattern.Length ? pattern.IndexOf(']', i + 2) : -1;

                    if (close < 0)
                    {
                        return null;
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
