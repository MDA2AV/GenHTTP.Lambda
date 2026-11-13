using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Puts the content of a public page into the index page, so a crawler that
/// runs no script reads what the page says rather than an empty element.
/// </summary>
/// <remarks>
/// The pages are rendered when the frontend is built, once for every language,
/// into <c>prerender.json</c> next to the index page (see
/// <c>src/Frontend/prerender.mjs</c>). What the build
/// cannot know - the address the page is answered from, how long this
/// installation keeps a lambda, what is in the showcase right now - it renders
/// as placeholders, which are filled in here. The same values go along with
/// the page in <c>#site-facts</c>, so the application draws the same markup in
/// the browser and takes it over instead of drawing it again.
/// </remarks>
public sealed partial class SitePrerender
{
    private const string Root = "<div id=\"root\"></div>";

    private const string Showcase = "/showcase";

    private const string Total = "__LAMBDA_SHOWCASE_TOTAL__";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [GeneratedRegex("__LAMBDA_[A-Z_]+__")]
    private static partial Regex Placeholder();

    [GeneratedRegex("href=\"__LAMBDA_ENTRY_ADDRESS__\"|animation-delay:0ms|__LAMBDA_ENTRY_[A-Z]+__")]
    private static partial Regex EntryPlaceholder();

    #region Get-/Setters

    private string File { get; }

    private string? PublicUrl { get; }

    private ILimitsService Limits { get; }

    private IShowcaseService Showcases { get; }

    private ILambdaAddresses Addresses { get; }

    private ILogger<SitePrerender> Logger { get; }

    private Snapshot? Cached { get; set; }

    #endregion

    #region Initialization

    public SitePrerender(Bundle bundle, string? publicUrl, ILimitsService limits, IShowcaseService showcases, ILambdaAddresses addresses,
                         ILogger<SitePrerender> logger)
    {
        File = bundle.PrerenderFile;
        PublicUrl = publicUrl;
        Limits = limits;
        Showcases = showcases;
        Addresses = addresses;
        Logger = logger;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// The index page with the content of the given page in it, or as it is if
    /// there is none to put there.
    /// </summary>
    public string Render(string markup, SitePage page, IRequest request)
    {
        var prerendered = Read();

        if (prerendered == null || !markup.Contains(Root, StringComparison.Ordinal))
        {
            return markup;
        }

        var origin = PublicUrl ?? RequestOrigin.Of(request);

        var host = Uri.TryCreate(origin, UriKind.Absolute, out var parsed) ? parsed.Authority : origin;

        var limits = Limits.Get();

        var lifetimeHours = (int)limits.OfflineAfter.TotalHours;

        var (before, after) = Around(Addresses.Template);

        var facts = new SiteFacts(
            origin,
            host,
            before,
            after,
            lifetimeHours,
            (int)Math.Round(lifetimeHours / 24.0, MidpointRounding.AwayFromZero),
            (int)limits.RemovedAfter.TotalDays
        );

        string content;

        // the showcase as it is right now, or as it is drawn while it loads
        if (page.Path == Showcase && ListShowcase() is { } listing && RenderShowcase(prerendered, page.Language, listing, facts) is { } showcase)
        {
            content = showcase;
            facts = facts with { Showcase = listing };
        }
        else if (prerendered.Pages.TryGetValue(SiteLanguages.In(page.Language, page.Path), out var rendered))
        {
            content = Fill(rendered, facts);
        }
        else
        {
            return markup;
        }

        // the facts are written as a script's text, where "</script>" would end
        // it early - the default encoder escapes every angle bracket
        var script = $"<script id=\"site-facts\" type=\"application/json\">{JsonSerializer.Serialize(facts, Json)}</script>";

        return markup.Replace(Root, $"<div id=\"root\">{content}</div>\n{script}", StringComparison.Ordinal);
    }

    /// <summary>
    /// The showcase in a language with the entries of its first page in it, or
    /// nothing if the build did not render the shape this listing takes.
    /// </summary>
    /// <remarks>
    /// A single lambda is a shape of its own, since every language counts one
    /// in other words than several - the words are the build's, and only the
    /// number is written in here.
    /// </remarks>
    private static string? RenderShowcase(Prerendered prerendered, string language, ShowcaseListing listing, SiteFacts facts)
    {
        var shape = listing.Entries.Count == 0 ? "empty"
                  : listing.Next != null ? "partial"
                  : listing.Total == 1 ? "one"
                  : "complete";

        if (!prerendered.Showcase.TryGetValue(language, out var shapes) || !shapes.TryGetValue(shape, out var page)
            || !prerendered.Entry.TryGetValue(language, out var entry)
            || (shape != "empty" && !page.Contains(entry, StringComparison.Ordinal)))
        {
            return null;
        }

        // the site's own placeholders first, so nothing an owner wrote into an
        // entry is ever taken for one
        page = Fill(page, facts).Replace(Total, listing.Total.ToString(), StringComparison.Ordinal);

        var entries = string.Concat(listing.Entries.Select((item, index) => RenderEntry(entry, item, index)));

        return page.Replace(entry, entries, StringComparison.Ordinal);
    }

    private static string RenderEntry(string template, ShowcaseListingEntry entry, int index)
        => EntryPlaceholder().Replace(template, match => match.Value switch
        {
            "href=\"__LAMBDA_ENTRY_ADDRESS__\"" => $"href=\"{WebUtility.HtmlEncode(entry.Address)}\"",
            // the same stagger as the page, which starts again with every page it loads
            "animation-delay:0ms" => $"animation-delay:{index % ShowcasePage.Size * 40}ms",
            "__LAMBDA_ENTRY_KEY__" => WebUtility.HtmlEncode(entry.PublicKey),
            "__LAMBDA_ENTRY_TITLE__" => WebUtility.HtmlEncode(entry.Title),
            "__LAMBDA_ENTRY_DESCRIPTION__" => WebUtility.HtmlEncode(entry.Description),
            "__LAMBDA_ENTRY_ADDRESS__" => WebUtility.HtmlEncode(Shown(entry.Address)),
            "__LAMBDA_ENTRY_IMAGE__" => WebUtility.HtmlEncode(entry.ImagePath),
            _ => match.Value
        });

    /// <summary>
    /// The placeholders of the site itself, in one pass.
    /// </summary>
    private static string Fill(string markup, SiteFacts facts)
        => Placeholder().Replace(markup, match => match.Value switch
        {
            "__LAMBDA_ORIGIN__" => WebUtility.HtmlEncode(facts.Origin),
            "__LAMBDA_HOST__" => WebUtility.HtmlEncode(facts.Host),
            "__LAMBDA_BEFORE__" => WebUtility.HtmlEncode(facts.Before),
            "__LAMBDA_AFTER__" => WebUtility.HtmlEncode(facts.After),
            "__LAMBDA_LIFETIME_HOURS__" => facts.LifetimeHours.ToString(),
            "__LAMBDA_OFFLINE_DAYS__" => facts.OfflineDays.ToString(),
            "__LAMBDA_RETENTION_DAYS__" => facts.RetentionDays.ToString(),
            _ => match.Value
        });

    /// <summary>
    /// What a lambda's address is shown with before its key and after it, as
    /// <c>useExampleAddress</c> in the frontend makes it: <c>("", ".genhttp.run")</c>,
    /// or <c>("localhost:8080/lambda/", "")</c> below a path.
    /// </summary>
    private static (string Before, string After) Around(string template)
    {
        var at = template.IndexOf("{key}", StringComparison.Ordinal);

        var before = Regex.Replace(template[..at], "^https?://", string.Empty);
        var after = template[(at + "{key}".Length)..].TrimEnd('/');

        return (before, after);
    }

    /// <summary>
    /// The same as <c>shownAddress</c> in the frontend: the host, without the
    /// scheme and the slash after it.
    /// </summary>
    private static string Shown(string address) => Regex.Replace(address, "^https?://", string.Empty).TrimEnd('/');

    private ShowcaseListing? ListShowcase()
    {
        try
        {
            return ShowcaseListing.First(Showcases, Addresses);
        }
        catch (Exception e)
        {
            // the page still works without it: the browser asks again
            Logger.LogWarning(e, "The showcase could not be listed for the rendered page");
            return null;
        }
    }

    private Prerendered? Read()
    {
        // read again whenever it changes, because "npm run build" updates a
        // running server - and only then, because it is most of a megabyte
        try
        {
            var written = System.IO.File.GetLastWriteTimeUtc(File);

            if (Cached is { } cached && cached.Written == written)
            {
                return cached.Content;
            }

            var content = System.IO.File.Exists(File)
                        ? JsonSerializer.Deserialize<Prerendered>(System.IO.File.ReadAllText(File), Json)
                        : null;

            Cached = new Snapshot(written, content);

            return content;
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            // no build, or a broken one: the pages are still served, empty
            return null;
        }
    }

    private sealed record Snapshot(DateTime Written, Prerendered? Content);

    #endregion

}

/// <summary>
/// What the frontend build rendered: every public page in every language by
/// the path it has in that language, the showcase by language and by the shape
/// of its first page, and one entry of the showcase by language, as it appears
/// in there.
/// </summary>
public sealed record Prerendered(
    Dictionary<string, string> Pages,
    Dictionary<string, Dictionary<string, string>> Showcase,
    Dictionary<string, string> Entry
);

/// <summary>
/// What the server knows and the build could not, as <c>SiteFacts</c> in <c>site.ts</c>.
/// </summary>
/// <param name="Before">What a lambda's address is shown with before its key: "", or "localhost:8080/lambda/"</param>
/// <param name="After">And after it: ".genhttp.run", or ""</param>
public sealed record SiteFacts(string Origin, string Host, string Before, string After, int LifetimeHours, int OfflineDays, int RetentionDays,
                               ShowcaseListing? Showcase = null);
