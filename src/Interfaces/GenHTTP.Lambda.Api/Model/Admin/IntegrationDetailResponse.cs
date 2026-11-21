using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("One fact about an integration, such as the server it talks to.")]
public sealed record IntegrationDetailResponse(
    [Summary("What it says, such as `Server`")]
    string Label,
    [Summary("What it is here")]
    string Value
);
