using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Deployment.Model;

[Summary("A replacement of text within one file.")]
public sealed record FileEdit(
    [Summary("The name of the file to change, which has to be text")]
    string File,
    [Summary("The text to replace, exactly as it is in the file. It has to occur exactly once - include more of what surrounds it where it occurs more often.")]
    string Find,
    [Summary("What to put in its place; empty removes it")]
    string Replace);
