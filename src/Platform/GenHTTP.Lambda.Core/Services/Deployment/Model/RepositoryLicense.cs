using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Integrations.Sqlite;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// The license the project of a repository is under, written into its LICENSE.
/// </summary>
/// <param name="Year">The year the license names, which is the year the commit was made in</param>
/// <param name="Holder">Who holds the copyright, as the license names them</param>
public sealed record RepositoryLicense(SourceLicense License, int Year, string Holder);
