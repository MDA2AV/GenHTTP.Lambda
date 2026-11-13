using System.Buffers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// The programs that are not people - search engines, AI crawlers, link
/// previews, monitors, scanners, HTTP libraries - by the agents they send.
/// </summary>
/// <remarks>
/// Not written here: the patterns are those of crawler-user-agents
/// (github.com/monperrus/crawler-user-agents, MIT), a list kept by many people
/// and checked against browsers of people in its own tests, with a kind for
/// every entry. A guess of our own - "bot" anywhere in the agent - was taken
/// in by a phone called Cubot and missed half of what the list knows.
///
/// The copy of the release is embedded, and the job that keeps the crawlers'
/// addresses current fetches the list again weekly (<see cref="CrawlerJob"/>),
/// so a crawler that appears after a release is known without one. A list
/// that does not read is not taken, and the one before stays.
///
/// The agents come from whoever sends a request, so nothing here may take
/// longer the more cleverly an agent is written. Nearly every pattern of the
/// list is plain text (1,465 of 1,501 when it was added), and those are looked
/// for all at once in one pass over the agent; text that has to begin the
/// agent is compared with its start; and the few real expressions run on the
/// engine that cannot backtrack. One that engine cannot take is left out
/// rather than run on the other.
/// </remarks>
public sealed class CrawlerList
{
    private const string Resource = "GenHTTP.Lambda.Resources.Crawlers.crawler-user-agents.json";

    /// <summary>
    /// The kinds of the list, the one a crawler is counted under first where
    /// it has several - a search engine that also feeds an AI is a search
    /// engine to whoever wants to be found.
    /// </summary>
    public static IReadOnlyList<string> Kinds { get; } =
    [
        "search-engine", "ai-crawler", "social-preview", "advertising", "seo", "monitoring", "feed-reader", "archiver",
        "academic", "scanner", "browser-automation", "http-library"
    ];

    /// <summary>
    /// What a crawler is counted under whose kinds are none of <see cref="Kinds"/>.
    /// </summary>
    public const string OtherKind = "other";

    private Snapshot _snapshot;

    #region Get-/Setters

    /// <summary>
    /// How many patterns are in use.
    /// </summary>
    public int Count => Volatile.Read(ref _snapshot).Patterns.Length;

    /// <summary>
    /// Changes whenever another list is taken, so what was worked out with
    /// the one before can be let go of.
    /// </summary>
    public int Generation => Volatile.Read(ref _snapshot).Generation;

    #endregion

    #region Initialization

    public CrawlerList()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Resource)
                        ?? throw new InvalidOperationException($"The embedded crawler list '{Resource}' is missing.");

        _snapshot = Read(stream, 0) ?? throw new InvalidOperationException("The embedded crawler list does not read.");
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Takes a newer list, as published.
    /// </summary>
    /// <returns>Whether it was taken - not when it does not read, or holds nothing</returns>
    public bool Load(Stream json)
    {
        var current = Volatile.Read(ref _snapshot);

        var loaded = Read(json, current.Generation + 1);

        if (loaded == null)
        {
            return false;
        }

        Volatile.Write(ref _snapshot, loaded);

        return true;
    }

    /// <summary>
    /// The kind of crawler an agent belongs to, or nothing where it is none
    /// the list knows.
    /// </summary>
    public string? KindOf(string agent)
    {
        var snapshot = Volatile.Read(ref _snapshot);

        // most agents are browsers, and need no more than to be found in none
        // of them: one pass for all the text, a look at the start, and the few
        // expressions
        if (!agent.AsSpan().ContainsAny(snapshot.Texts)
            && !snapshot.Patterns.Any(p => p.Prefix != null && agent.StartsWith(p.Prefix, StringComparison.Ordinal))
            && !snapshot.Patterns.Any(p => p.Expression != null && p.Expression.IsMatch(agent)))
        {
            return null;
        }

        // the first that matches says what kind it is, as the list orders them
        foreach (var pattern in snapshot.Patterns)
        {
            if (pattern.Matches(agent))
            {
                return pattern.Kind;
            }
        }

        return OtherKind;
    }

    private static Snapshot? Read(Stream json, int generation)
    {
        List<Pattern> patterns = [];

        try
        {
            using var document = JsonDocument.Parse(json);

            foreach (var entry in document.RootElement.EnumerateArray())
            {
                if (!entry.TryGetProperty("pattern", out var source) || source.GetString() is not { Length: > 0 } text)
                {
                    continue;
                }

                var tags = entry.TryGetProperty("tags", out var listed) && listed.ValueKind == JsonValueKind.Array
                    ? listed.EnumerateArray().Select(t => t.GetString()).ToHashSet()
                    : [];

                var kind = Kinds.FirstOrDefault(tags.Contains) ?? OtherKind;

                if (Literal(text) is { Length: > 0 } literal)
                {
                    patterns.Add(new Pattern(kind, Text: literal));
                }
                else if (text[0] == '^' && Literal(text[1..]) is { Length: > 0 } prefix)
                {
                    patterns.Add(new Pattern(kind, Prefix: prefix));
                }
                else
                {
                    try
                    {
                        patterns.Add(new Pattern(kind, Expression: new Regex(text, Options)));
                    }
                    catch (Exception e) when (e is ArgumentException or NotSupportedException)
                    {
                        // a construct the engine without backtracking does not
                        // have; one crawler fewer rather than a pattern that
                        // could take a request's time
                    }
                }
            }
        }
        catch (JsonException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }

        if (patterns.Count == 0)
        {
            return null;
        }

        var texts = SearchValues.Create([.. patterns.Where(p => p.Text != null).Select(p => p.Text!).Distinct()], StringComparison.Ordinal);

        return new Snapshot([.. patterns], texts, generation);
    }

    /// <summary>
    /// The text a pattern stands for where it is nothing but text, with the
    /// escapes of punctuation taken out - <c>Googlebot\/</c> is
    /// <c>Googlebot/</c> - or nothing where it is an expression.
    /// </summary>
    private static string? Literal(string pattern)
    {
        var text = new StringBuilder(pattern.Length);

        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];

            if (c == '\\')
            {
                // an escaped letter or digit is a class (\d, \s), anything
                // else is the character itself
                if (i + 1 < pattern.Length && !char.IsLetterOrDigit(pattern[i + 1]))
                {
                    text.Append(pattern[++i]);
                    continue;
                }

                return null;
            }

            if (".^$*+?()[]{}|".Contains(c))
            {
                return null;
            }

            text.Append(c);
        }

        return text.ToString();
    }

    private const RegexOptions Options = RegexOptions.NonBacktracking | RegexOptions.CultureInvariant;

    /// <summary>
    /// One pattern of the list: text found anywhere, text the agent begins
    /// with, or an expression.
    /// </summary>
    private sealed record Pattern(string Kind, string? Text = null, string? Prefix = null, Regex? Expression = null)
    {
        public bool Matches(string agent)
            => Text != null ? agent.Contains(Text, StringComparison.Ordinal)
             : Prefix != null ? agent.StartsWith(Prefix, StringComparison.Ordinal)
             : Expression!.IsMatch(agent);
    }

    private sealed record Snapshot(Pattern[] Patterns, SearchValues<string> Texts, int Generation);

    #endregion

}
