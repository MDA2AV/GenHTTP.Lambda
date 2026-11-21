using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Showcase;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What a showcase entry should say.")]
public sealed record ShowcaseRequest(
    [Summary("What it is called, on one line")]
    string? Title,
    [Summary("What it is, and what a visitor can do with it")]
    string? Description,
    [Summary("The picture, base64 encoded: a PNG, JPEG, GIF or WebP image. Required for a new entry; left out, the one there is kept.")]
    string? Image
);
