using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A demo the installation keeps online, and how to read it.")]
public sealed record DemoResponse(
    [Summary("What it is started from: the `template` of `POST /lambdas`")]
    string Id,
    [Summary("What it is called")]
    string Name,
    [Summary("What it is")]
    string Description,
    [Summary("What reading its code teaches")]
    string Shows,
    [Summary("The kinds of request it is the right starting point for")]
    string ReadWhen,
    [Summary("Where it answers: its public key")]
    string PublicKey,
    [Summary("Its editor key: public, and read only - every change is refused")]
    string PrivateKey,
    [Summary("Where it answers, below the hosting domain")]
    string Address,
    [Summary("The files it is made of, the snippet first")]
    IReadOnlyList<string> Files,
    [Summary("Whether it answers right now - the demos are prepared after the server started")]
    bool Live
);
