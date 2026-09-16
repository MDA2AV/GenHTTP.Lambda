using System.Net;

using MaxMind.Db;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Where an address is, as closely as a free database will say, and whose
/// network it is on.
/// </summary>
/// <remarks>
/// The country table next door answers from the registries and is exact about
/// what it knows: who was delegated a block. This is the other kind of answer
/// - a guess, made by somebody else, from measurements and inference - and it
/// is right about most consumer connections, wrong about most infrastructure,
/// and worth having anyway because "Lisbon, on MEO" is a different kind of
/// fact from "PT".
///
/// The files are memory mapped rather than read: they are a search trie and
/// the format is built to be walked in place, so a hundred and fifty megabyte
/// database costs page cache the kernel can drop under pressure, not heap
/// that has to be paid for whether or not anybody looks anything up.
///
/// Data by DB-IP, under CC BY 4.0, which is why the panel says so.
/// </remarks>
public sealed class GeoPlaces : IDisposable
{

    #region Get-/Setters

    /// <summary>
    /// Whether a database is open and can be asked.
    /// </summary>
    public bool Ready => Volatile.Read(ref Cities) != null || Volatile.Read(ref Networks) != null;

    /// <summary>
    /// The month the open databases were published for, if it is known.
    /// </summary>
    public string? Edition { get; private set; }

    private Reader? Cities;

    private Reader? Networks;

    /// <summary>
    /// Answers already worked out, by address.
    /// </summary>
    /// <remarks>
    /// A lookup walks a trie and builds a dictionary of what it finds, which
    /// measured at twenty-five microseconds - not much, and still a hundred
    /// times what it costs to remember the answer. Callers repeat: a scanner
    /// is one address and five hundred requests. Bounded, and cleared whole
    /// when it fills rather than evicted one by one, because the cost of
    /// being wrong here is one lookup.
    /// </remarks>
    private System.Collections.Concurrent.ConcurrentDictionary<string, Place?> Known { get; } = new(StringComparer.Ordinal);

    private const int MostKnown = 8192;

    /// <summary>
    /// How many addresses a second may be looked up for the first time.
    /// </summary>
    /// <remarks>
    /// The cache makes this cost per caller rather than per request, which is
    /// the right shape until somebody arrives from more addresses than the
    /// cache holds. A distributed scan does exactly that: every request is a
    /// first sighting, nothing is ever reused, and the cheapest part of
    /// serving a request becomes the most expensive. Past this many in a
    /// second the lookup is simply not done and the line has no place on it -
    /// which is a better answer than a server that slows down in proportion
    /// to how hard it is being scanned.
    /// </remarks>
    private const int MostFreshPerSecond = 250;

    private long Window;

    private int Fresh;

    /// <summary>
    /// How many lookups have been skipped because they arrived too fast.
    /// </summary>
    public long Skipped;

    #endregion

    #region Functionality

    /// <summary>
    /// Opens what is present. Either file may be missing; the answers are
    /// simply thinner.
    /// </summary>
    public void Open(string? cityFile, string? networkFile, string? edition)
    {
        var cities = OpenOne(cityFile);
        var networks = OpenOne(networkFile);

        // swapped in, then the old pair closed, so a lookup in flight during a
        // monthly refresh finishes against the file it started on
        var oldCities = Interlocked.Exchange(ref Cities, cities);
        var oldNetworks = Interlocked.Exchange(ref Networks, networks);

        Edition = edition;

        // last month's answers are not this month's
        Known.Clear();

        oldCities?.Dispose();
        oldNetworks?.Dispose();
    }

    /// <summary>
    /// What is known about an address, or nothing.
    /// </summary>
    public Place? Find(string? address)
    {
        if (address == null)
        {
            return null;
        }

        if (Known.TryGetValue(address, out var remembered))
        {
            return remembered;
        }

        if (!Spare())
        {
            Interlocked.Increment(ref Skipped);

            // deliberately not remembered: this is a pass, not an answer, and
            // the same address asked for again when things are quiet should
            // get a real one
            return null;
        }

        var found = Look(address);

        if (Known.Count >= MostKnown)
        {
            Known.Clear();
        }

        Known[address] = found;

        return found;
    }

    /// <summary>
    /// Whether there is room in this second for another first sighting.
    /// </summary>
    private bool Spare()
    {
        var second = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;

        if (Volatile.Read(ref Window) != second)
        {
            Volatile.Write(ref Window, second);
            Volatile.Write(ref Fresh, 0);
        }

        return Interlocked.Increment(ref Fresh) <= MostFreshPerSecond;
    }

    private Place? Look(string address)
    {

        // a forwarded caller is "<claimed> via <peer>"; the claim is the one
        // being placed
        var cut = address.IndexOf(' ');

        if (!IPAddress.TryParse(cut < 0 ? address : address[..cut], out var parsed))
        {
            return null;
        }

        string? city = null, region = null, country = null, network = null;

        try
        {
            /*
             * Read into shapes that name the four fields wanted rather than
             * into a dictionary of everything there is.
             *
             * A city record carries its name in every language the database
             * ships, and its region, and its continent, and a set of
             * coordinates. Decoding the lot to read two strings off it cost
             * forty microseconds and six kilobytes of garbage per address;
             * the reader skips what nothing asks for.
             */
            if (Volatile.Read(ref Cities) is { } cities && cities.Find<CityRecord>(parsed) is { } found)
            {
                city = found.City?.Names?.English;
                country = found.Country?.IsoCode;
                region = found.Subdivisions is { Count: > 0 } ? found.Subdivisions[0].Names?.English : null;
            }

            if (Volatile.Read(ref Networks) is { } networks && networks.Find<NetworkRecord>(parsed) is { } owner)
            {
                network = owner.Organisation;
            }
        }
        catch (Exception)
        {
            // a truncated or half written database answers nothing rather than
            // taking a request down with it
            return null;
        }

        return city == null && region == null && network == null ? null : new Place(city, region, country, network);
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref Cities, null)?.Dispose();
        Interlocked.Exchange(ref Networks, null)?.Dispose();

        Known.Clear();
    }

    private static Reader? OpenOne(string? path)
    {
        try
        {
            return path != null && File.Exists(path) ? new Reader(path, FileAccessMode.MemoryMapped) : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    #endregion

}

/*
 * Only the fields that end up on a log line are declared. Anything the
 * database holds and these do not name is stepped over rather than decoded,
 * which is where the cost of a lookup went: a city record carries its name in
 * every language the database ships, its region, its continent and a pair of
 * coordinates, and reading two strings off all of that cost forty
 * microseconds and six kilobytes of garbage an address.
 *
 * Internal rather than nested and private, because the reader generates its
 * decoding code against them and cannot see inside a class to do it.
 */

internal sealed class Names
{
    [MapKey("en")]
    public string? English { get; set; }
}

internal sealed class NamedPlace
{
    [MapKey("names")]
    public Names? Names { get; set; }
}

internal sealed class CountryOf
{
    [MapKey("iso_code")]
    public string? IsoCode { get; set; }
}

internal sealed class CityRecord
{
    [MapKey("city")]
    public NamedPlace? City { get; set; }

    [MapKey("country")]
    public CountryOf? Country { get; set; }

    [MapKey("subdivisions")]
    public List<NamedPlace>? Subdivisions { get; set; }
}

internal sealed class NetworkRecord
{
    [MapKey("autonomous_system_organization")]
    public string? Organisation { get; set; }
}

/// <summary>
/// What a database says about one address.
/// </summary>
/// <param name="Network">
/// The organisation the range belongs to - an ISP for a person, a hosting
/// company or a cloud for everything else, which is often the more telling of
/// the two.
/// </param>
public sealed record Place(string? City, string? Region, string? Country, string? Network)
{

    /// <summary>
    /// One line of it, for a log that has one column to spend.
    /// </summary>
    public string Describe()
    {
        var where = string.Join(", ", new[] { City, Region, Country }.Where(p => !string.IsNullOrEmpty(p)).Distinct());

        if (string.IsNullOrEmpty(Network))
        {
            return where;
        }

        return where.Length == 0 ? Network : $"{where} · {Network}";
    }

}
