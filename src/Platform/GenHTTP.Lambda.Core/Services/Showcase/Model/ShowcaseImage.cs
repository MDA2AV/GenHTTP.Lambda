namespace GenHTTP.Lambda.Services.Showcase;

/// <summary>
/// The picture of an entry, to be served.
/// </summary>
public sealed record ShowcaseImage(byte[] Content, string Type, DateTime Updated);
