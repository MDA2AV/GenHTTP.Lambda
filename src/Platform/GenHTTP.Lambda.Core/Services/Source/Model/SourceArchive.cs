using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// A version of a published source, packed.
/// </summary>
/// <param name="File">Where the zip is</param>
/// <param name="Root">The folder the project is in, inside the zip - named after the lambda</param>
public sealed record SourceArchive(string PublicKey, int Version, string File, string Root);
