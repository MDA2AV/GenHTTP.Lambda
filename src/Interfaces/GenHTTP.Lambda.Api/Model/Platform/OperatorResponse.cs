using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Who runs an installation, as far as its pages have to say so.")]
public sealed record OperatorResponse(
    [Summary("Where to write to them, about moving an app to production for one; absent where they name nobody")]
    string? Contact,
    [Summary("Where a lambda that does harm is reported; absent where there is nowhere")]
    string? Abuse,
    [Summary("Whether the site has its terms, privacy policy and imprint at `/terms`, `/privacy` and `/imprint`")]
    bool Legal,
    [Summary("The page of the site where a visitor has an app built, with the build box - `/build`, or `/` where the front page has it")]
    string Start
);
