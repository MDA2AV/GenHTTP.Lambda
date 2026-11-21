using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Showcase;

/// <summary>
/// One page of the showcase, in the order it is shown.
/// </summary>
/// <param name="Total">How many are listed altogether</param>
public sealed record ShowcasePage(IReadOnlyList<ShowcaseInfo> Entries, int Total)
{

    /// <summary>
    /// How many entries a page of the showcase has, unless asked for another
    /// number: the API's listing and the page rendered on the server alike,
    /// so the browser takes the rendered one over without a jump.
    /// </summary>
    public const int Size = 12;

}
