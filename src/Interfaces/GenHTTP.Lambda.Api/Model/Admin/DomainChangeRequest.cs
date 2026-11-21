using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The domain a lambda is to answer at.")]
public sealed record DomainChangeRequest(
    [Summary("The domain, as it should answer: `shop.example.com`, or `www.example.com` to answer there. Nothing, or an empty one, removes it.")]
    string? Domain
);
