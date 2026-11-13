using System.Net;

namespace GenHTTP.Lambda.Services.Blocking;

/// <summary>
/// The addresses the server refuses before it does anything else for them.
/// </summary>
public interface IAddressBlocks
{

    /// <summary>
    /// How long a block holds before it is let go of by itself.
    /// </summary>
    static TimeSpan Kept => TimeSpan.FromDays(14);

    /// <summary>
    /// Whether a block covers the address.
    /// </summary>
    /// <remarks>
    /// Asked of every request, and answered from memory - at once while no
    /// block is held at all.
    /// </remarks>
    bool Covers(IPAddress? address);

    /// <summary>
    /// Counts a request from the address that was refused against its block.
    /// </summary>
    void Refused(IPAddress address);

    /// <summary>
    /// Every block, newest first.
    /// </summary>
    IReadOnlyList<AddressBlock> List();

    /// <summary>
    /// Blocks the given addresses - an IPv6 address with its /64 - for
    /// <see cref="Kept"/> from now. One blocked already starts its time again,
    /// with this reason.
    /// </summary>
    /// <param name="reason">Why, in the operator's words</param>
    /// <param name="publicKey">The lambda they are blocked for, if any</param>
    /// <returns>The blocks made or renewed</returns>
    IReadOnlyList<AddressBlock> Block(IReadOnlyList<string> addresses, string? reason, string? publicKey);

    /// <summary>
    /// Lets go of a block. False when there was none of that id.
    /// </summary>
    bool Remove(long id);

    /// <summary>
    /// Lets go of every block older than <see cref="Kept"/>.
    /// </summary>
    /// <returns>How many there were</returns>
    int Expire(DateTime now);

}
