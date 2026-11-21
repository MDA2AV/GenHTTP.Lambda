namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// How many stars a published source has.
/// </summary>
/// <param name="Id">The identity it is filed under</param>
public sealed record SourceStars(long Id, int Stars);
