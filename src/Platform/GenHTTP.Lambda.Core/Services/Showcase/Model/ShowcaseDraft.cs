using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Showcase;

/// <summary>
/// What an owner wants the entry to say. The picture may be left out when
/// there already is one, to keep it.
/// </summary>
public sealed record ShowcaseDraft(string? Title, string? Description, byte[]? Image);
