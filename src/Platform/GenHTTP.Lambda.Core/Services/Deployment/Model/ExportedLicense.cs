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
/// The license a packed project is under, written into its LICENSE.
/// </summary>
/// <param name="Holder">Who holds the copyright, as the license names them</param>
/// <param name="Page">Where the source is published, if the installation knows its own address</param>
public sealed record ExportedLicense(SourceLicense License, string Holder, string? Page);
