using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// A single suggestion for the code editor. <see cref="Insert" /> is set for
/// snippets, where the text to insert differs from the label.
/// </summary>
[Summary("A name or a snippet the editor may suggest, whatever the code around it.")]
public sealed record CompletionItem(
    [Summary("What is suggested: a type a lambda may use, an attribute, or the name of a snippet")]
    string Label,
    [Summary("What it is: `class`, `struct`, `interface`, `enum`, `attribute` or `snippet`")]
    string Kind,
    [Summary("Where it comes from - the namespace of a type - or what a snippet does")]
    string Detail,
    [Summary("For a snippet, the code to insert; absent where it is the label itself")]
    string? Insert = null);
