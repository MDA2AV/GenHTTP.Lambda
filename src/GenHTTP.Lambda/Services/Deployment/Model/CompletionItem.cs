namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// A single suggestion for the code editor. <see cref="Insert" /> is set for
/// snippets, where the text to insert differs from the label.
/// </summary>
public sealed record CompletionItem(string Label, string Kind, string Detail, string? Insert = null);
