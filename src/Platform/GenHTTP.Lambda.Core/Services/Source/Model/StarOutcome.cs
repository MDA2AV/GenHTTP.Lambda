namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// What starring a source came to.
/// </summary>
/// <param name="Stars">How many stars it has now</param>
/// <param name="Starred">Whether the visitor's star is given rather than taken back</param>
/// <param name="Changed">Whether anything was counted: a visitor who starred already stars nothing more</param>
public sealed record StarOutcome(int Stars, bool Starred, bool Changed);
