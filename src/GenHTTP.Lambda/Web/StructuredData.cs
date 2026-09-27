using System.Text.Json;
using System.Text.Json.Nodes;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// What the front page tells a search engine about the site in schema.org
/// terms: the name to show it under, who runs it and what it is.
/// </summary>
/// <remarks>
/// Only what a search engine does something with. The name of the site is
/// what it shows above a result, and the organization carries the logo. The
/// application earns no rich result without ratings, which there are none of,
/// but it says in so many words that this is a free tool for developers.
///
/// Always under genhttp.dev, whatever address the page was asked for at: this
/// describes the site, and that is where the site is. The site is written in
/// every language it has; the application is described in the language of the
/// front page it is found on, which is where it links to.
/// </remarks>
public static class StructuredData
{
    private const string Home = "https://genhttp.dev/";

    private const string Discord = "https://discord.gg/PRkwKrnrB4";

    private const string Organization = Home + "#organization";

    public static string Render(string site, string description, string language)
    {
        var graph = new JsonObject
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = new JsonArray
            {
                new JsonObject
                {
                    ["@type"] = "Organization",
                    ["@id"] = Organization,
                    ["name"] = "GenHTTP",
                    ["url"] = Home,
                    ["logo"] = new JsonObject
                    {
                        ["@type"] = "ImageObject",
                        ["url"] = Home + "icon-512.png",
                        ["width"] = 512,
                        ["height"] = 512
                    },
                    ["email"] = "solutions@genhttp.dev",
                    ["sameAs"] = new JsonArray(Discord)
                },
                new JsonObject
                {
                    ["@type"] = "WebSite",
                    ["@id"] = Home + "#website",
                    ["name"] = site,
                    ["url"] = Home,
                    ["inLanguage"] = new JsonArray([.. SiteLanguages.All.Select(l => JsonValue.Create(l))]),
                    ["publisher"] = new JsonObject { ["@id"] = Organization }
                },
                new JsonObject
                {
                    ["@type"] = "WebApplication",
                    ["@id"] = Home + "#application",
                    ["name"] = site,
                    ["url"] = Home + language,
                    ["inLanguage"] = language,
                    ["description"] = description,
                    ["applicationCategory"] = "DeveloperApplication",
                    ["operatingSystem"] = "Any",
                    ["browserRequirements"] = "Requires JavaScript",
                    ["offers"] = new JsonObject
                    {
                        ["@type"] = "Offer",
                        ["price"] = "0",
                        ["priceCurrency"] = "USD"
                    },
                    ["publisher"] = new JsonObject { ["@id"] = Organization }
                }
            }
        };

        // the default encoder escapes angle brackets, so nothing in here can
        // end the script it is written into
        return $"<script type=\"application/ld+json\">{graph.ToJsonString(new JsonSerializerOptions())}</script>";
    }

}
