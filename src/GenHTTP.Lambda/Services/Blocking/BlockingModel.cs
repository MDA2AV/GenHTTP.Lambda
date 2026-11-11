namespace GenHTTP.Lambda.Services.Blocking;

/// <summary>
/// An address the server refuses, as the operator sees it.
/// </summary>
/// <param name="Address">An IPv4 address, or the /64 of an IPv6 one</param>
/// <param name="Reason">Why, in the operator's words</param>
/// <param name="PublicKey">The lambda it was blocked for, by the key it had then</param>
/// <param name="Expires">When the block is let go of by itself</param>
/// <param name="Refused">How many requests it refused since the server came up</param>
/// <param name="LastRefused">When it last refused one, since the server came up</param>
public sealed record AddressBlock(long Id, string Address, string? Reason, string? PublicKey, DateTime Created, DateTime Expires,
                                  long Refused, DateTime? LastRefused);
