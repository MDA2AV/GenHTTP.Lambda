using System.Text.Json;
using System.Text.Json.Nodes;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// What the page of a published source tells a search engine in schema.org
/// terms, inside the page.
/// </summary>
/// <remarks>
/// Said of the code on the page, in every installation alike: no publisher,
/// since the code is its author's and an installation is not obliged to
/// describe itself to a search engine. A site that does says so on its own
/// front page.
/// </remarks>
public static class StructuredData
{

    /// <summary>
    /// What the page of a published source tells a search engine about the
    /// code on it: that it is code, in which language, under which license,
    /// who holds it, and the app it is the source of.
    /// </summary>
    /// <param name="site">The public address of the installation</param>
    /// <param name="path">The page's path, in its language</param>
    public static string RenderSource(SourceSchema schema, string site, string path, string language)
    {
        var code = new JsonObject
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "SoftwareSourceCode",
            ["name"] = schema.Name,
            ["url"] = site + path,
            ["codeRepository"] = site + path,
            ["inLanguage"] = SiteLanguages.TagOf(language),
            ["programmingLanguage"] = new JsonObject { ["@type"] = "ComputerLanguage", ["name"] = "C#" },
            ["runtimePlatform"] = ".NET",
            ["license"] = schema.License
        };

        if (schema.Description != null)
        {
            code["description"] = schema.Description;
        }

        if (schema.Author != null)
        {
            code["author"] = new JsonObject { ["@type"] = "Person", ["name"] = schema.Author };
        }

        if (schema.Updated is { } updated)
        {
            code["dateModified"] = updated.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        }

        if (schema.Application is { } application)
        {
            code["targetProduct"] = new JsonObject
            {
                ["@type"] = "WebApplication",
                ["name"] = schema.Name,
                ["url"] = application.StartsWith('/') ? site + application : application,
                ["applicationCategory"] = "WebApplication",
                ["operatingSystem"] = "Any"
            };
        }

        return $"<script type=\"application/ld+json\">{code.ToJsonString(new JsonSerializerOptions())}</script>";
    }

}

/// <summary>
/// What is said about a published source in schema.org terms.
/// </summary>
/// <param name="License">Where its license is described</param>
/// <param name="Author">Who holds the copyright, if the owner named somebody</param>
/// <param name="Updated">When its newest version was saved</param>
/// <param name="Application">Where the app answers, while it is online</param>
public sealed record SourceSchema(string Name, string? Description, string License, string? Author, DateTime? Updated, string? Application);
