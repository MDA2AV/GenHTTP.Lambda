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
/// A code is a language, or a language and a region where the site is written
/// in more than one variant of it: "pt" is Portuguese as Brazil writes it,
/// which most readers of Portuguese do, and "pt-pt" is Portuguese as Portugal
/// does.
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

    /// <summary>
    /// Every language, in the order the switcher offers them: alphabetically
    /// by what each calls itself - Bahasa Indonesia, Deutsch, English, and so
    /// on - with the ones in other scripts last.
    /// </summary>
    public static readonly IReadOnlyList<string> All = ["id", "de", "en", "es", "fr", "it", "nl", "pl", "pt", "pt-pt", "tr", "ja", "ko"];

    /// <summary>
    /// How Open Graph names each of them - a language and the region most of
    /// its readers are in.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Locales = new Dictionary<string, string>
    {
        ["id"] = "id_ID",
        ["de"] = "de_DE",
        ["en"] = "en_US",
        ["es"] = "es_ES",
        ["fr"] = "fr_FR",
        ["it"] = "it_IT",
        ["nl"] = "nl_NL",
        ["pl"] = "pl_PL",
        ["pt"] = "pt_BR",
        ["pt-pt"] = "pt_PT",
        ["tr"] = "tr_TR",
        ["ja"] = "ja_JP",
        ["ko"] = "ko_KR"
    };

    /// <summary>
    /// How a language is tagged in the markup, for a search engine and a
    /// screen reader - its code, except where the code leaves out the region.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> Tags = new Dictionary<string, string>
    {
        ["pt"] = "pt-BR",
        ["pt-pt"] = "pt-PT"
    };

    /// <summary>
    /// The variant that also stands for its whole language, for a reader in a
    /// country with no variant of its own here: Portuguese outside of Brazil
    /// and Portugal finds the Brazilian pages, which most of its readers write.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> Whole = new Dictionary<string, string>
    {
        ["pt"] = "pt"
    };

    /// <summary>
    /// Where a language is written after the variant of another country than
    /// the one its primary code stands for: Angola, Mozambique and the other
    /// countries of Portuguese in Africa and Asia write it as Portugal does.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> Regions = new Dictionary<string, string>
    {
        ["pt-pt"] = "pt-pt",
        ["pt-ao"] = "pt-pt",
        ["pt-mz"] = "pt-pt",
        ["pt-cv"] = "pt-pt",
        ["pt-gw"] = "pt-pt",
        ["pt-st"] = "pt-pt",
        ["pt-tl"] = "pt-pt",
        ["pt-mo"] = "pt-pt"
    };

    /// <summary>
    /// Codes some browsers still send for a language that has another one
    /// now: older Android and Java call Indonesian "in".
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> Aliases = new Dictionary<string, string>
    {
        ["in"] = "id"
    };

    public static bool IsLanguage(string? value) => value != null && All.Contains(value);

    /// <summary>
    /// The language as the markup tags it: "de", or "pt-BR" for Portuguese.
    /// </summary>
    public static string TagOf(string language) => Tags.GetValueOrDefault(language, language);

    /// <summary>
    /// What a page in the language answers for among its translations - its
    /// tag, and for the variant standing for a whole language, that language.
    /// </summary>
    public static IEnumerable<string> HreflangsOf(string language)
        => Whole.TryGetValue(language, out var whole) ? [TagOf(language), whole] : [TagOf(language)];

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

        foreach (var tag in Accepted(accepted))
        {
            if (Match(tag) is { } language)
            {
                return language;
            }
        }

        return Default;
    }

    /// <summary>
    /// The language the site has for a tag a browser sent: "pt-PT" is
    /// Portuguese as Portugal writes it, "pt-AO" as well, "pt-BR" and "pt" are
    /// Brazilian, "de-CH" is German - or nothing, for a language it has not.
    /// </summary>
    public static string? Match(string tag)
    {
        var lower = tag.Trim().ToLowerInvariant();

        if (Regions.TryGetValue(lower, out var region))
        {
            return region;
        }

        var primary = lower.Split('-')[0];

        primary = Aliases.GetValueOrDefault(primary, primary);

        return IsLanguage(primary) ? primary : null;
    }

    /// <summary>
    /// The languages of an Accept-Language header, most wanted first:
    /// "de-CH, fr;q=0.8, en;q=0.5" is de-ch, fr, en. A language weighted zero
    /// is one the browser refuses, and "*" says nothing.
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

            ranges.Add((tag, weight, position));
        }

        // as weighted, and as listed where the weights are equal
        return ranges.OrderByDescending(r => r.Weight)
                     .ThenBy(r => r.Position)
                     .Select(r => r.Language);
    }

}
