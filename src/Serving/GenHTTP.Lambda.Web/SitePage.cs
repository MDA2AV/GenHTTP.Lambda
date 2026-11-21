using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// A page a search engine should find, in one of its languages, and what it
/// should say about it there - with the path of the picture a link preview
/// shows, if it has one of its own.
/// </summary>
/// <param name="Language">The language it is shown in</param>
/// <param name="Path">Its path without a language, as <c>pages.json</c> spells it</param>
/// <param name="Languages">Every language the page is written in, its own included</param>
/// <param name="ImageType">What the picture is, where its name does not say - an owner's picture on the showcase</param>
public sealed record SitePage(string Language, string Path, string Title, string Description, IReadOnlyList<string> Languages, string? Image = null,
                              string? ImageType = null);
