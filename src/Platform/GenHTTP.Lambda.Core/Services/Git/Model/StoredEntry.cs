namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// A file of a commit's tree. Always a regular file: a lambda holds nothing else.
/// </summary>
/// <param name="Path">Where it is</param>
/// <param name="Blob">The blob holding what it holds</param>
public sealed record StoredEntry(string Path, string Blob);
