using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Blocking;

/// <summary>
/// What an address is to a block: the range a block of it covers, how that is
/// written, and whether it may be blocked at all.
/// </summary>
/// <remarks>
/// An IPv4 address is blocked as it is. An IPv6 address is blocked with the
/// /64 it belongs to, since that is what one connection's owner is handed -
/// and a device picks a new address within it every day or so by itself, so a
/// block of the one address it used would be over before anybody noticed.
/// </remarks>
public static class NetworkAddress
{

    /// <summary>
    /// The address as it is meant: an IPv4 client reaching a dual stack socket
    /// arrives wearing an IPv6 mapping.
    /// </summary>
    public static IPAddress Normalize(IPAddress address) => address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

    /// <summary>
    /// Whether the address belongs to one of the installation's own networks,
    /// which are never recorded nor blocked (see <c>LambdaOptions.UnblockableNetworks</c>).
    /// </summary>
    public static bool IsOwn(IPAddress address, IReadOnlyList<IPNetwork> networks)
    {
        var normalized = Normalize(address);

        foreach (var network in networks)
        {
            if (network.Contains(normalized))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// What a block of the address covers: the address itself, or its /64.
    /// </summary>
    public static IPNetwork RangeOf(IPAddress address)
    {
        var normalized = Normalize(address);

        if (normalized.AddressFamily == AddressFamily.InterNetwork)
        {
            return new IPNetwork(normalized, 32);
        }

        var bytes = normalized.GetAddressBytes();

        bytes.AsSpan(8).Clear();

        return new IPNetwork(new IPAddress(bytes), 64);
    }

    /// <summary>
    /// How a range is written down and shown: an IPv4 address as it is, an
    /// IPv6 range with its prefix.
    /// </summary>
    public static string Describe(IPNetwork range)
        => range.BaseAddress.AddressFamily == AddressFamily.InterNetwork ? range.BaseAddress.ToString() : range.ToString();

    /// <summary>
    /// Reads what somebody wants blocked: an address, or the /64 of an IPv6
    /// address written as such.
    /// </summary>
    public static bool TryRead(string? text, [NotNullWhen(true)] out IPNetwork? range)
    {
        range = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var trimmed = text.Trim();

        if (IPAddress.TryParse(trimmed, out var address))
        {
            range = RangeOf(address);
            return true;
        }

        if (IPNetwork.TryParse(trimmed, out var network) && network.BaseAddress.AddressFamily == AddressFamily.InterNetworkV6 && network.PrefixLength == 64)
        {
            range = network;
            return true;
        }

        return false;
    }

    /// <summary>
    /// What a block of the given address would cover, or why it cannot be
    /// blocked: it is not an address, or it is on one of the installation's
    /// own networks.
    /// </summary>
    public static IPNetwork ReadBlockable(string? text, IReadOnlyList<IPNetwork> own)
    {
        if (!TryRead(text, out var range))
        {
            throw LambdaException.Invalid($"'{text}' is not an address that can be blocked: an IPv4 or IPv6 address, or the /64 of an IPv6 address.");
        }

        if (IsOwn(range.Value.BaseAddress, own))
        {
            throw LambdaException.Invalid($"{Describe(range.Value)} is on one of the installation's own networks (LAMBDA_UNBLOCKABLE_NETWORKS), which are never blocked: " +
                                          "the build agent and a proxy in front of the server come from there.");
        }

        return range.Value;
    }

    /// <summary>
    /// The number a range is looked up by while requests are served.
    /// </summary>
    /// <remarks>
    /// Worked out from the bytes on the stack rather than by making an
    /// address of the range, since it is asked for every request while any
    /// block is held. An IPv4 address is kept as IPv6 maps it, the prefix of
    /// an IPv6 address in the upper half - so the two never meet.
    /// </remarks>
    public static bool TryKey(IPAddress address, out UInt128 key)
    {
        Span<byte> bytes = stackalloc byte[16];

        if (!address.TryWriteBytes(bytes, out var written))
        {
            key = default;
            return false;
        }

        if (written == 4)
        {
            key = V4(bytes[..4]);
            return true;
        }

        if (bytes[..10].IndexOfAnyExcept((byte)0) < 0 && bytes[10] == 0xFF && bytes[11] == 0xFF)
        {
            key = V4(bytes[12..16]);
            return true;
        }

        key = (UInt128)BinaryPrimitives.ReadUInt64BigEndian(bytes[..8]) << 64;
        return true;
    }

    private static UInt128 V4(ReadOnlySpan<byte> bytes) => ((UInt128)0xFFFF << 32) | BinaryPrimitives.ReadUInt32BigEndian(bytes);

}
