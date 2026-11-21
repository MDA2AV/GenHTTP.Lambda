using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Asks for a new lambda.")]
public sealed record CreateLambdaRequest(
    [Summary("The public key to claim, which names the address: 3 to 40 characters of a-z, 0-9 and dashes. Left out, a random one is chosen.")]
    string? PublicKey,
    [Summary("Whether the terms of the installation (`terms` of `GET /system`) were accepted. Where it has terms, nothing is created unless this is true.")]
    bool AcceptedTerms,
    [Summary("What the lambda starts as: the `id` of a demo, as `GET /demos` and the starters of `GET /system` list them, or `empty`. Left out, empty.")]
    string? Template = null,
    [Summary("How its editor opens: `Full` (the default) with the code, files and versions, or `Simple` - the app, how it is doing and where to ask for a change, for somebody who is not going to read the code. Case does not matter.")]
    string? View = null
);
