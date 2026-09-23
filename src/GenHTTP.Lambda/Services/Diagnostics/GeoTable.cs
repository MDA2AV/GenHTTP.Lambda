using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Which country an address was allocated to.
/// </summary>
/// <remarks>
/// Built from the delegation files the five regional registries publish - the
/// same records that say who was given which block. That makes this the
/// registration country of whoever holds the range, which is usually but not
/// always where the person using it is sitting: a VPN, a CDN edge, a hosting
/// provider registered in one country and racked in another all read as the
/// registry's answer rather than the user's. It tells a Portuguese visitor
/// from a Singaporean scanner; it does not place anybody on a map, and
/// nothing here should be described as if it did.
///
/// Deliberately not a lookup against somebody else's service. Doing this by
/// asking a third party would mean handing them the address of every visitor
/// to the installation, which is a worse trade than a coarser answer.
///
/// The table is immutable once built and swapped in whole, so a refresh never
/// leaves a reader looking at half of one.
/// </remarks>
public sealed class GeoTable
{

    #region Get-/Setters

    /// <summary>
    /// How many ranges are loaded. Zero means nothing has been read yet.
    /// </summary>
    public int Ranges => Volatile.Read(ref _snapshot)?.Count ?? 0;

    #endregion

    #region Functionality

    /// <summary>
    /// The country an address belongs to, or nothing if it is unknown, private
    /// or the table has not been loaded.
    /// </summary>
    public string? CountryOf(string? address)
    {
        var table = Volatile.Read(ref _snapshot);

        if (table == null || address == null)
        {
            return null;
        }

        // a forwarded caller is "<claimed> via <peer>"; the claim is the one
        // being described, and the one worth placing
        var cut = address.IndexOf(' ');

        var claim = cut < 0 ? address : address[..cut];

        if (!IPAddress.TryParse(claim, out var parsed))
        {
            return null;
        }

        if (parsed.AddressFamily == AddressFamily.InterNetwork)
        {
            return Find(table.FourStart, table.FourEnd, table.FourCode, ToFour(parsed));
        }

        if (parsed.IsIPv4MappedToIPv6)
        {
            return Find(table.FourStart, table.FourEnd, table.FourCode, ToFour(parsed.MapToIPv4()));
        }

        return Find(table.SixStart, table.SixEnd, table.SixCode, ToSix(parsed));
    }

    /// <summary>
    /// Builds a table from the lines of one or more delegation files.
    /// </summary>
    /// <remarks>
    /// The format is pipe separated and documented by the registries:
    /// <c>registry|cc|type|start|value|date|status|id</c>, where value is a
    /// count of addresses for v4 and a prefix length for v6. Anything that is
    /// not an allocated or assigned range of one of those two types is not a
    /// delegation and is skipped, summary lines included.
    /// </remarks>
    public void Load(IEnumerable<string> lines)
    {
        var four = new List<(uint Start, uint End, string Code)>(140_000);
        var six = new List<(UInt128 Start, UInt128 End, string Code)>(140_000);

        // a couple of hundred distinct codes against hundreds of thousands of
        // rows, so they are shared rather than repeated
        var codes = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var line in lines)
        {
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }

            var parts = line.Split('|');

            if (parts.Length < 7)
            {
                continue;
            }

            var country = parts[1];

            if (country.Length != 2 || parts[6] is not ("allocated" or "assigned"))
            {
                continue;
            }

            if (!codes.TryGetValue(country, out var shared))
            {
                shared = country;
                codes[country] = shared;
            }

            if (parts[2] == "ipv4")
            {
                if (IPAddress.TryParse(parts[3], out var start) && uint.TryParse(parts[4], out var count) && count > 0)
                {
                    var first = ToFour(start);

                    four.Add((first, first + count - 1, shared));
                }
            }
            else if (parts[2] == "ipv6")
            {
                if (IPAddress.TryParse(parts[3], out var start) && int.TryParse(parts[4], out var prefix)
                    && prefix is >= 0 and <= 128)
                {
                    var first = ToSix(start);

                    // the last address in the block, which for a /128 is the
                    // first one
                    var size = prefix == 0 ? UInt128.MaxValue : (UInt128.One << (128 - prefix)) - 1;

                    six.Add((first, first + size, shared));
                }
            }
        }

        four.Sort((a, b) => a.Start.CompareTo(b.Start));
        six.Sort((a, b) => a.Start.CompareTo(b.Start));

        var table = new Snapshot(
            four.Select(r => r.Start).ToArray(), four.Select(r => r.End).ToArray(), four.Select(r => r.Code).ToArray(),
            six.Select(r => r.Start).ToArray(), six.Select(r => r.End).ToArray(), six.Select(r => r.Code).ToArray()
        );

        Volatile.Write(ref _snapshot, table);
    }

    private Snapshot? _snapshot;

    private static string? Find<T>(T[] starts, T[] ends, string[] codes, T address) where T : IComparable<T>
    {
        if (starts.Length == 0)
        {
            return null;
        }

        var at = Array.BinarySearch(starts, address);

        // an exact hit is the first address of its range; otherwise the one
        // before where it would have been inserted is the only range that can
        // contain it, the list being sorted and non overlapping
        var index = at >= 0 ? at : ~at - 1;

        if (index < 0)
        {
            return null;
        }

        return address.CompareTo(ends[index]) <= 0 ? codes[index] : null;
    }

    private static uint ToFour(IPAddress address)
    {
        Span<byte> bytes = stackalloc byte[4];

        address.TryWriteBytes(bytes, out _);

        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    private static UInt128 ToSix(IPAddress address)
    {
        Span<byte> bytes = stackalloc byte[16];

        address.TryWriteBytes(bytes, out _);

        return new UInt128(BinaryPrimitives.ReadUInt64BigEndian(bytes), BinaryPrimitives.ReadUInt64BigEndian(bytes[8..]));
    }

    private sealed record Snapshot(
        uint[] FourStart, uint[] FourEnd, string[] FourCode,
        UInt128[] SixStart, UInt128[] SixEnd, string[] SixCode
    )
    {
        public int Count => FourStart.Length + SixStart.Length;
    }

    #endregion

}
