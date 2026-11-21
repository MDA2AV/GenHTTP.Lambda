using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("An address the server refuses, before it does anything else for it - apart from the administration panel.")]
public sealed record AddressBlockResponse(
    [Summary("What `DELETE /admin/blocks/{id}` takes")]
    long Id,
    [Summary("An IPv4 address, or the /64 of an IPv6 address: `2001:db8:1:2::/64`")]
    string Address,
    [Summary("Why, in the operator's words")]
    string? Reason,
    [Summary("The lambda it was blocked for, by the public key it had then; it may have gone since")]
    string? Lambda,
    [Summary("When it was blocked")]
    DateTime Created,
    [Summary("When it is let go of by itself, two weeks after it was blocked")]
    DateTime Expires,
    [Summary("How many requests it refused since the server came up")]
    long Refused,
    [Summary("When it last refused one, since the server came up")]
    DateTime? LastRefused
);
