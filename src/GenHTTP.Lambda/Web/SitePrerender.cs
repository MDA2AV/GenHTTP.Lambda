using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Showcase;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Puts the content of a public page into the index page, so a crawler that
/// runs no script reads what the page says rather than an empty element.
/// </summary>
/// <remarks>
/// The pages are rendered when the frontend is built, into <c>prerender.json</c>
/// next to the index page (see <c>src/Frontend/prerender.mjs</c>). What the build
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

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [GeneratedRegex("__LAMBDA_[A-Z_]+__")]
    private static partial Regex Placeholder();

    [GeneratedRegex("href=\"__LAMBDA_ENTRY_PATH__\"|animation-delay:0ms|__LAMBDA_ENTRY_[A-Z]+__")]
    private static partial Regex EntryPlaceholder();

    #region Get-/Setters

    private string File { get; }

    private LambdaOptions Options { get; }

    private IShowcaseService Showcases { get; }

    private ILogger<SitePrerender> Logger { get; }

    private Snapshot? Cached { get; set; }

    #endregion

    #region Initialization

    public SitePrerender(LambdaOptions options, IShowcaseService showcases, ILogger<SitePrerender> logger)
    {
        File = Path.Combine(options.WebRoot, "prerender.json");
        Options = options;
        Showcases = showcases;
        Logger = logger;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// The index page with the content of the page at the given path in it,
    /// or as it is if there is none to put there.
    /// </summary>
    public async ValueTask<string> RenderAsync(string markup, string path, IRequest request)
    {
        var prerendered = Read();

        if (prerendered == null || !markup.Contains(Root, StringComparison.Ordinal))
        {
            return markup;
        }

        var origin = Options.PublicUrl ?? RequestOrigin.Of(request);

        var host = Uri.TryCreate(origin, UriKind.Absolute, out var parsed) ? parsed.Authority : origin;

        var lifetimeHours = (int)Options.DeploymentLifetime.TotalHours;

        var facts = new SiteFacts(
            origin,
            host,
            lifetimeHours,
            (int)Math.Round(lifetimeHours / 24.0, MidpointRounding.AwayFromZero),
            (int)Options.Retention.TotalDays
        );

        string content;

        // the showcase as it is right now, or as it is drawn while it loads
        if (path == Showcase && await ListShowcaseAsync() is { } listing && RenderShowcase(prerendered, listing, facts) is { } showcase)
        {
            content = showcase;
            facts = facts with { Showcase = listing };
        }
        else if (prerendered.Pages.TryGetValue(path, out var page))
        {
            content = Fill(page, facts);
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
    /// The showcase with the entries of its first page in it, or nothing if the
    /// build did not render the shape this listing takes.
    /// </summary>
    private static string? RenderShowcase(Prerendered prerendered, ShowcaseListingResponse listing, SiteFacts facts)
    {
        var shape = listing.Entries.Count == 0 ? "empty" : listing.Next == null ? "complete" : "partial";

        if (!prerendered.Showcase.TryGetValue(shape, out var page) || (shape != "empty" && !page.Contains(prerendered.Entry, StringComparison.Ordinal)))
        {
            return null;
        }

        // the site's own placeholders first, so nothing an owner wrote into an
        // entry is ever taken for one
        page = Fill(page, facts).Replace("__LAMBDA_SHOWCASE_TOTAL__ lambdas", Counted(listing.Total), StringComparison.Ordinal);

        var entries = string.Concat(listing.Entries.Select((entry, index) => RenderEntry(prerendered.Entry, entry, index)));

        return page.Replace(prerendered.Entry, entries, StringComparison.Ordinal);
    }

    private static string RenderEntry(string template, ShowcaseResponse entry, int index)
        => EntryPlaceholder().Replace(template, match => match.Value switch
        {
            "href=\"__LAMBDA_ENTRY_PATH__\"" => $"href=\"{WebUtility.HtmlEncode(entry.Path)}\"",
            // the same stagger as the page, which starts again with every page it loads
            "animation-delay:0ms" => $"animation-delay:{index % ShowcaseResource.PageSize * 40}ms",
            "__LAMBDA_ENTRY_KEY__" => WebUtility.HtmlEncode(entry.PublicKey),
            "__LAMBDA_ENTRY_TITLE__" => WebUtility.HtmlEncode(entry.Title),
            "__LAMBDA_ENTRY_DESCRIPTION__" => WebUtility.HtmlEncode(entry.Description),
            "__LAMBDA_ENTRY_PATH__" => WebUtility.HtmlEncode(Shown(entry.Path)),
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
            "__LAMBDA_LIFETIME_HOURS__" => facts.LifetimeHours.ToString(),
            "__LAMBDA_OFFLINE_DAYS__" => facts.OfflineDays.ToString(),
            "__LAMBDA_RETENTION_DAYS__" => facts.RetentionDays.ToString(),
            _ => match.Value
        });

    /// <summary>
    /// The same as <c>counted</c> on the showcase page.
    /// </summary>
    private static string Counted(int total) => total == 1 ? "1 lambda" : $"{total} lambdas";

    /// <summary>
    /// The same as <c>shownAddress</c> in the frontend: a domain bare, a path as it is.
    /// </summary>
    private static string Shown(string address)
        => address.StartsWith('/') ? address : Regex.Replace(address, "^https?://", string.Empty).TrimEnd('/');

    private async ValueTask<ShowcaseListingResponse?> ListShowcaseAsync()
    {
        try
        {
            return await new ShowcaseResource(Showcases).List(0, null);
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
/// What the frontend build rendered: every public page by its path, the
/// showcase by the shape of its first page, and one entry of the showcase as
/// it appears in there.
/// </summary>
public sealed record Prerendered(Dictionary<string, string> Pages, Dictionary<string, string> Showcase, string Entry);

/// <summary>
/// What the server knows and the build could not, as <c>SiteFacts</c> in <c>site.ts</c>.
/// </summary>
public sealed record SiteFacts(string Origin, string Host, int LifetimeHours, int OfflineDays, int RetentionDays, ShowcaseListingResponse? Showcase = null);
