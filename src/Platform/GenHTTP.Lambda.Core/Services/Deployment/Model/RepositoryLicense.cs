using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// The license the project of a repository is under, written into its LICENSE.
/// </summary>
/// <param name="Year">The year the license names, which is the year the commit was made in</param>
/// <param name="Holder">Who holds the copyright, as the license names them</param>
public sealed record RepositoryLicense(SourceLicense License, int Year, string Holder);
