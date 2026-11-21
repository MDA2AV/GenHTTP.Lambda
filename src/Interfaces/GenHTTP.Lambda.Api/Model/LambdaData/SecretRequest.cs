using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The value to store under a name.")]
public sealed record SecretRequest(
    [Summary("The value, up to 32 KB - sent once, and never shown again")]
    string Value
);
