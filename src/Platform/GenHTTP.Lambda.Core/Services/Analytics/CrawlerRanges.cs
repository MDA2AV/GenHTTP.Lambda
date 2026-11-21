using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// The crawlers that can be told from impostors: those whose operators publish
/// the addresses they crawl from.
/// </summary>
/// <remarks>
/// Anybody can send <c>Googlebot</c> as their agent, and scrapers do, to be let
/// in where Google is. So a visit is only counted as Google's when it came
/// from an address Google publishes for its crawlers; one that claims to be
/// Google from anywhere else is counted as a claim that was not confirmed.
/// Google, Bing, OpenAI, Apple and Perplexity publish theirs, as JSON in the
/// same shape, fetched and kept current by <see cref="CrawlerJob"/>.
/// The others are not named at all: the list says what kind of crawler they
/// are (<see cref="CrawlerList"/>), which is as much as an agent can be trusted
/// to say.
///
/// The address checked is the one the connection came from, never the one a
/// forwarding header claims: the header is written by the sender, and would
/// let anybody claim Google's address with Google's name. An installation
/// behind a proxy therefore confirms nothing - its connections come from the
/// proxy - and says so by counting every claim as unconfirmed.
/// </remarks>
public sealed class CrawlerRanges
{

    /// <summary>
    /// The operators whose crawlers are confirmed: how their crawlers name
    /// themselves, and where they publish the addresses those crawl from.
    /// </summary>
    /// <remarks>
    /// The names are those each operator documents for the crawlers its lists
    /// cover. A list going away, or moving, leaves its operator unconfirmed
    /// until this is changed - the job logs a failed fetch.
    /// </remarks>
    public static IReadOnlyList<CrawlerOperator> Operators { get; } =
    [
        new("Google", @"Googlebot|Google-InspectionTool|GoogleOther|Storebot-Google|AdsBot-Google|Mediapartners-Google|APIs-Google|FeedFetcher-Google|Google-Read-Aloud|Google-Site-Verification|Google-CloudVertexBot|Google-Safety",
            [
                "https://developers.google.com/static/search/apis/ipranges/googlebot.json",
                "https://developers.google.com/static/search/apis/ipranges/special-crawlers.json",
                "https://developers.google.com/static/search/apis/ipranges/user-triggered-fetchers.json"
            ]),
        new("Bing", @"bingbot|BingPreview|msnbot|adidxbot", ["https://www.bing.com/toolbox/bingbot.json"]),
        new("OpenAI", @"GPTBot|OAI-SearchBot|ChatGPT-User",
            ["https://openai.com/gptbot.json", "https://openai.com/searchbot.json", "https://openai.com/chatgpt-user.json"]),
        new("Apple", @"Applebot", ["https://search.developer.apple.com/applebot.json"]),
        new("Perplexity", @"PerplexityBot|Perplexity-User",
            ["https://www.perplexity.com/perplexitybot.json", "https://www.perplexity.com/perplexity-user.json"])
    ];

    /// <summary>
    /// What a claim is counted under that was not confirmed: from another
    /// address, from one that is not known, or before any list was read.
    /// </summary>
    public const string Unconfirmed = "unconfirmed";

    private readonly Dictionary<string, Ranges> _ranges = [];

    private readonly Lock _lock = new();

    #region Get-/Setters

    /// <summary>
    /// The operators whose addresses are known, whose crawlers are confirmed.
    /// </summary>
    public IReadOnlyList<string> Confirmed
    {
        get
        {
            lock (_lock)
            {
                return [.. Operators.Select(o => o.Name).Where(_ranges.ContainsKey)];
            }
        }
    }

    #endregion

    #region Functionality

    /// <summary>
    /// The operator whose crawler an agent claims to be, or nothing.
    /// </summary>
    public static CrawlerOperator? Claimed(string agent) => Operators.FirstOrDefault(o => o.Claims.IsMatch(agent));

    /// <summary>
    /// What a crawler that claims to be the operator's is counted under: the
    /// operator's name where it came from one of its addresses, unconfirmed
    /// otherwise.
    /// </summary>
    /// <param name="client">Who asked, as the log names them: an address, or "claimed via peer"</param>
    public string Confirm(CrawlerOperator claimed, string? client)
    {
        var peer = PeerOf(client);

        if (peer == null)
        {
            return Unconfirmed;
        }

        Ranges? ranges;

        lock (_lock)
        {
            _ranges.TryGetValue(claimed.Name, out ranges);
        }

        return ranges != null && ranges.Contain(peer) ? claimed.Name : Unconfirmed;
    }

    /// <summary>
    /// Takes the addresses an operator publishes, from every list it
    /// publishes them in.
    /// </summary>
    /// <returns>Whether any were read - lists that do not read leave the ones before</returns>
    public bool Load(string name, IEnumerable<Stream> lists)
    {
        var four = new List<(uint Start, uint End)>();
        var six = new List<(UInt128 Start, UInt128 End)>();

        foreach (var list in lists)
        {
            try
            {
                using var document = JsonDocument.Parse(list);

                foreach (var prefix in document.RootElement.GetProperty("prefixes").EnumerateArray())
                {
                    foreach (var property in prefix.EnumerateObject())
                    {
                        if (property.Value.GetString() is { } text && IPNetwork.TryParse(text, out var network))
                        {
                            Add(network, four, six);
                        }
                    }
                }
            }
            catch (Exception e) when (e is JsonException or InvalidOperationException or KeyNotFoundException)
            {
                // one list of an operator that does not read is not a reason
                // to forget the others
            }
        }

        if (four.Count == 0 && six.Count == 0)
        {
            return false;
        }

        lock (_lock)
        {
            _ranges[name] = new Ranges([.. four], [.. six]);
        }

        return true;
    }

    /// <summary>
    /// The address a connection came from, out of the way the log names a
    /// caller - never the address a forwarding header claimed.
    /// </summary>
    private static IPAddress? PeerOf(string? client)
    {
        if (client == null)
        {
            return null;
        }

        var via = client.LastIndexOf(" via ", StringComparison.Ordinal);

        var peer = via < 0 ? client : client[(via + 5)..];

        if (!IPAddress.TryParse(peer, out var address))
        {
            return null;
        }

        return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
    }

    private static void Add(IPNetwork network, List<(uint, uint)> four, List<(UInt128, UInt128)> six)
    {
        Span<byte> bytes = stackalloc byte[16];

        if (network.BaseAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            network.BaseAddress.TryWriteBytes(bytes, out _);

            var start = BinaryPrimitives.ReadUInt32BigEndian(bytes);
            var size = network.PrefixLength == 0 ? uint.MaxValue : (1u << (32 - network.PrefixLength)) - 1;

            four.Add((start, start + size));
        }
        else
        {
            network.BaseAddress.TryWriteBytes(bytes, out _);

            var start = BinaryPrimitives.ReadUInt128BigEndian(bytes);
            var size = network.PrefixLength == 0 ? UInt128.MaxValue : (UInt128.One << (128 - network.PrefixLength)) - 1;

            six.Add((start, start + size));
        }
    }

    /// <summary>
    /// The addresses of one operator. A few thousand at most, so they are
    /// walked rather than indexed.
    /// </summary>
    private sealed record Ranges((uint Start, uint End)[] Four, (UInt128 Start, UInt128 End)[] Six)
    {
        public bool Contain(IPAddress address)
        {
            Span<byte> bytes = stackalloc byte[16];

            address.TryWriteBytes(bytes, out _);

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                var value = BinaryPrimitives.ReadUInt32BigEndian(bytes);

                return Four.Any(r => value >= r.Start && value <= r.End);
            }

            var six = BinaryPrimitives.ReadUInt128BigEndian(bytes);

            return Six.Any(r => six >= r.Start && six <= r.End);
        }
    }

    #endregion

}

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
