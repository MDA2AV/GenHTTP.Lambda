using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A page of every lambda on the installation.")]
public sealed record AdminListingResponse(
    [Summary("The lambdas of this page")]
    IReadOnlyList<AdminLambda> Lambdas,
    [Summary("How many lambdas there are - the demos left out, unless they were asked for")]
    int Total,
    [Summary("How many of them are online")]
    int Deployed,
    [Summary("How many the search matched, which is what the pages count from")]
    int Matched,
    [Summary("The page this is, counting from 1")]
    int Page,
    [Summary("How many pages there are")]
    int Pages
);
