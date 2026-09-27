using System.Globalization;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The languages the site is written in, and which of them a visitor gets.
/// </summary>
/// <remarks>
/// Every public page lives once per language, under its own prefix: /de/build
/// is the German build page for everybody, whatever their browser asks for.
/// That is what lets a search engine list each language, since a crawler
/// only ever sees what an address says. Only the addresses without a prefix
/// pick a language - the one chosen on the site, remembered in a cookie, or
/// else the best match of the Accept-Language header - and send the visitor
/// on to it.
///
/// The frontend lists the same codes in <c>src/i18n/languages.ts</c>.
/// </remarks>
public static class SiteLanguages
{

    public const string Default = "en";

    /// <summary>
    /// Where a choice made on the site is remembered.
    /// </summary>
    public const string Cookie = "lang";

    public static readonly IReadOnlyList<string> All = ["en", "de", "es", "pt", "fr", "it"];

    /// <summary>
    /// How Open Graph names each of them - a language and the region most of
    /// its readers are in.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Locales = new Dictionary<string, string>
    {
        ["en"] = "en_US",
        ["de"] = "de_DE",
        ["es"] = "es_ES",
        ["pt"] = "pt_BR",
        ["fr"] = "fr_FR",
        ["it"] = "it_IT"
    };

    public static bool IsLanguage(string? value) => value != null && All.Contains(value);

    /// <summary>
    /// The language a path is in, if it has one: "/de/build" is German, "/de"
    /// as well, and "/build" is in none.
    /// </summary>
    public static string? Of(string path)
    {
        var end = path.IndexOf('/', 1);

        var first = end < 0 ? path[1..] : path[1..end];

        return IsLanguage(first) ? first : null;
    }

    /// <summary>
    /// The path without its language: "/de/build" is "/build", "/de" is "/".
    /// </summary>
    public static string Without(string path)
    {
        if (Of(path) is not { } language)
        {
            return path;
        }

        var rest = path[(language.Length + 1)..];

        return rest.Length == 0 ? "/" : rest;
    }

    /// <summary>
    /// The path in a language: "/build" in German is "/de/build", "/" is "/de".
    /// </summary>
    public static string In(string language, string path)
    {
        var bare = Without(path);

        return bare == "/" ? $"/{language}" : $"/{language}{bare}";
    }

    /// <summary>
    /// The language for a visitor who asked for no particular one: the one they
    /// chose here before, or the first their browser accepts, or the default.
    /// </summary>
    /// <param name="chosen">The value of the cookie a choice is remembered in</param>
    /// <param name="accepted">The Accept-Language header, as sent</param>
    public static string Negotiate(string? chosen, string? accepted)
    {
        if (IsLanguage(chosen))
        {
            return chosen!;
        }

        foreach (var language in Accepted(accepted))
        {
            if (IsLanguage(language))
            {
                return language;
            }
        }

        return Default;
    }

    /// <summary>
    /// The primary languages of an Accept-Language header, most wanted first:
    /// "de-CH, fr;q=0.8, en;q=0.5" is German, French, English. A language
    /// weighted zero is one the browser refuses, and "*" says nothing.
    /// </summary>
    private static IEnumerable<string> Accepted(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return [];
        }

        var ranges = new List<(string Language, double Weight, int Position)>();

        foreach (var (range, position) in header.Split(',').Select((range, position) => (range, position)))
        {
            var parts = range.Split(';');

            var tag = parts[0].Trim().ToLowerInvariant();

            if (tag.Length == 0 || tag == "*")
            {
                continue;
            }

            var weight = 1.0;

            foreach (var parameter in parts.Skip(1))
            {
                var pair = parameter.Trim();

                if (pair.StartsWith("q=", StringComparison.OrdinalIgnoreCase)
                    && double.TryParse(pair[2..], NumberStyles.Float, CultureInfo.InvariantCulture, out var q))
                {
                    weight = q;
                }
            }

            if (weight <= 0)
            {
                continue;
            }

            ranges.Add((tag.Split('-')[0], weight, position));
        }

        // as weighted, and as listed where the weights are equal
        return ranges.OrderByDescending(r => r.Weight)
                     .ThenBy(r => r.Position)
                     .Select(r => r.Language);
    }

}
