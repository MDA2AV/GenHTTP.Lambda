using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The public pages of a site as one brand shows them: every page by its path
/// in its language, and the showcase by language and by the shape of its
/// first page.
/// </summary>
public sealed record PrerenderedPages(
    Dictionary<string, string> Pages,
    Dictionary<string, Dictionary<string, string>> Showcase
);
