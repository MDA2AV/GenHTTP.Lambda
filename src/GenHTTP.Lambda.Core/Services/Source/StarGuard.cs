using System.Buffers.Text;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// Keeps the stars of a published source about as honest as they can be
/// without accounts.
/// </summary>
/// <remarks>
/// Three things, each cheap, none of them a wall. A star is given with a POST
/// carrying a ticket the page was handed when it was read, which is at least a
/// second old - so a link, a crawler following every address it finds, or a
/// script posting blind stars nothing. One address stars a source once: what
/// it gave is remembered, so starring twice is starring once and taking a star
/// back that was never given takes nothing. And one address changes only so
/// many stars in a while.
///
/// The address is remembered in memory only, and as a keyed hash rather than
/// itself - the key is made when the server starts and never leaves it. A
/// restart forgets who starred what, which is the price of keeping nothing
/// about a visitor on disk: somebody could star again after it. The count
/// itself is in the database.
/// </remarks>
public sealed class StarGuard
{

    /// <summary>
    /// How old a ticket has to be at least - the time it takes a person to
    /// find the star, and a script to be written that waits for it.
    /// </summary>
    private static readonly TimeSpan Youngest = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How long a ticket is good for: a page left open over a day is read again.
    /// </summary>
    private static readonly TimeSpan Oldest = TimeSpan.FromHours(24);

    /// <summary>
    /// How many stars one address may give or take back in a <see cref="Window" />.
    /// </summary>
    private const int Budget = 30;

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How many stars are remembered before the memory starts over, so that a
    /// flood of addresses costs a bounded amount of it.
    /// </summary>
    private const int Remembered = 100_000;

    private readonly byte[] _key = RandomNumberGenerator.GetBytes(32);

    private readonly Dictionary<(string Client, long Source), bool> _given = [];

    private readonly Dictionary<string, (DateTime Since, int Count)> _spent = [];

    private readonly Lock _sync = new();

    #region Tickets

    /// <summary>
    /// A ticket to star the source at a public key, as the page is handed it.
    /// </summary>
    public string Issue(string publicKey, DateTime now)
    {
        var issued = new DateTimeOffset(now).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);

        return $"{issued}.{Sign($"{publicKey}\n{issued}")}";
    }

    /// <summary>
    /// Whether a ticket was issued here for the source, and is neither too
    /// young nor too old.
    /// </summary>
    public bool Accepts(string publicKey, string? ticket, DateTime now)
    {
        var dot = ticket?.IndexOf('.') ?? -1;

        if (ticket == null || dot <= 0 || !long.TryParse(ticket.AsSpan(0, dot), NumberStyles.None, CultureInfo.InvariantCulture, out var issued))
        {
            return false;
        }

        var expected = Sign($"{publicKey}\n{ticket[..dot]}");

        if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(ticket[(dot + 1)..])))
        {
            return false;
        }

        var age = now - DateTimeOffset.FromUnixTimeMilliseconds(issued).UtcDateTime;

        return age >= Youngest && age <= Oldest;
    }

    #endregion

    #region Stars

    /// <summary>
    /// Whether a star from an address counts: it does when the address has not
    /// already done the same, and has not done too much of it lately.
    /// </summary>
    public StarVerdict Decide(IPAddress? client, long source, bool starred, DateTime now)
    {
        var who = client == null ? "unknown" : Sign(client.ToString());

        lock (_sync)
        {
            if (_spent.TryGetValue(who, out var spent) && now - spent.Since < Window && spent.Count >= Budget)
            {
                return StarVerdict.TooMany;
            }

            if (_given.TryGetValue((who, source), out var given) ? given == starred : !starred)
            {
                // starred twice, or taken back without having been given
                return StarVerdict.Unchanged;
            }

            if (_given.Count >= Remembered)
            {
                _given.Clear();
            }

            if (_spent.Count >= Remembered)
            {
                _spent.Clear();
            }

            _given[(who, source)] = starred;

            _spent[who] = spent.Count > 0 && now - spent.Since < Window ? (spent.Since, spent.Count + 1) : (now, 1);

            return StarVerdict.Counted;
        }
    }

    #endregion

    #region Helpers

    private string Sign(string value)
    {
        var hash = HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(value));

        return Base64Url.EncodeToString(hash.AsSpan(0, 18));
    }

    #endregion

}

/// <summary>
/// What becomes of a star.
/// </summary>
public enum StarVerdict
{

    /// <summary>
    /// It is counted.
    /// </summary>
    Counted,

    /// <summary>
    /// The address already did exactly this; the count stays as it is.
    /// </summary>
    Unchanged,

    /// <summary>
    /// The address changed too many stars lately.
    /// </summary>
    TooMany

}
