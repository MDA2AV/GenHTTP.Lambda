namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// One page of the lambdas on the installation.
/// </summary>
/// <param name="Lambdas">The page that was asked for</param>
/// <param name="Matched">How many the search found, which is what the pages are counted from</param>
/// <param name="Total">How many there are in total, search or no search</param>
/// <param name="Deployed">How many of the total are online</param>
public sealed record LambdaPage(
    IReadOnlyList<LambdaOverview> Lambdas,
    int Matched,
    int Total,
    int Deployed
);
