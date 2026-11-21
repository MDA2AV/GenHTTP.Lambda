namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// What an owner wants published: what is left out stays as it is, and an
/// empty author names nobody.
/// </summary>
public sealed record SourceDraft(string? License, string? Author);
