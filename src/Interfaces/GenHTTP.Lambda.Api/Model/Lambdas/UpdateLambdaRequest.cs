using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The changes to make to a lambda. What is left out stays as it is.")]
public sealed record UpdateLambdaRequest(
    [Summary("The public key to move the lambda to, under the same rules as when it is created")]
    string? PublicKey,
    [Summary("How its editor opens for somebody who has not chosen a view of their own: `Full` or `Simple`")]
    string? View = null
);
