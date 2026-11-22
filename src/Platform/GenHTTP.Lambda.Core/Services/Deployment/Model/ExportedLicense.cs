using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// The license a packed project is under, written into its LICENSE.
/// </summary>
/// <param name="Holder">Who holds the copyright, as the license names them</param>
/// <param name="Page">Where the source is published, if the installation knows its own address</param>
public sealed record ExportedLicense(SourceLicense License, string Holder, string? Page);
