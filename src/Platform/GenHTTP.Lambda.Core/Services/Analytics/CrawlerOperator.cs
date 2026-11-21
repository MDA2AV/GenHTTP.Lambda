using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// An operator whose crawlers can be confirmed by their addresses.
/// </summary>
/// <param name="Name">What its crawlers are counted under, once confirmed</param>
/// <param name="Tokens">How its crawlers name themselves in their agents</param>
/// <param name="Lists">Where it publishes the addresses they crawl from</param>
public sealed record CrawlerOperator(string Name, string Tokens, IReadOnlyList<string> Lists)
{

    /// <summary>
    /// Whether an agent claims to be one of its crawlers - without
    /// backtracking, since agents come from anybody.
    /// </summary>
    public Regex Claims { get; } = new(Tokens, RegexOptions.NonBacktracking | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

}
