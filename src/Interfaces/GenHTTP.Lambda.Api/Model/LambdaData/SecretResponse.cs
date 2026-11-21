using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A secret, as anybody but the lambda gets to see it: never its value.")]
public sealed record SecretResponse(
    [Summary("What the code reads it by: `Secret.ReadAsync(\"STRIPE_KEY\")`")]
    string Name,
    [Summary("When it was first stored")]
    DateTime Created,
    [Summary("When its value was last replaced")]
    DateTime Changed,
    [Summary("Whether the code reads it by that name")]
    bool Used
);
