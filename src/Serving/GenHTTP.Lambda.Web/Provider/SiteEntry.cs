using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// A page as <c>pages.json</c> has it: its picture, and its words by language.
/// </summary>
public sealed record SiteEntry(string? Image, Dictionary<string, SiteText> Text);
