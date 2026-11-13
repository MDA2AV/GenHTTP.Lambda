namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// An address the server refuses, before it does anything else for it.
/// </summary>
/// <remarks>
/// Put here by the operator and taken away by them, or by the job that lets a
/// block go after two weeks (see <c>AddressBlocks</c>). Held in memory while
/// the server runs; this is what it is read back from.
/// </remarks>
public sealed class AddressBlockEntity
{

    public long Id { get; set; }

    /// <summary>
    /// What is refused: an IPv4 address, or the /64 an IPv6 address belongs
    /// to, written as such (<c>2001:db8:1:2::/64</c>).
    /// </summary>
    public required string Address { get; set; }

    /// <summary>
    /// Why, in the operator's words.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// The lambda it was blocked for, by the public key it had then.
    /// </summary>
    /// <remarks>
    /// Not a reference: the block outlives the lambda, which may be deleted
    /// or move to another key.
    /// </remarks>
    public string? PublicKey { get; set; }

    /// <summary>
    /// When it was blocked, which is what it expires from.
    /// </summary>
    public DateTime Created { get; set; }

}
