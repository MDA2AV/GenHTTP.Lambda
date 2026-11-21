namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// A value too long to send whole: its beginning, and how long it is.
/// </summary>
public sealed record TruncatedText(string Text, int Length);
