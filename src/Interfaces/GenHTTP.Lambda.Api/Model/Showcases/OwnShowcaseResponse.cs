using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Showcase;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The showcase entry of a lambda as its owner sees it, with what may go into one.")]
public sealed record OwnShowcaseResponse(
    [Summary("The entry; absent while the lambda is not in the showcase")]
    ShowcaseResponse? Showcase,
    [Summary("What an entry may hold")]
    ShowcaseLimitsResponse Limits
);
