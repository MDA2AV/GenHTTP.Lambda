namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// A published source with everything its page shows about the lambda.
/// </summary>
/// <param name="Author">Who holds the copyright, if the owner named somebody</param>
/// <param name="ActiveVersion">The version online, if one is</param>
/// <param name="Created">When the lambda was made</param>
/// <param name="Versions">Every version kept, newest first</param>
public sealed record SourceProject(SourceEntry Entry, string? Author, int? ActiveVersion, DateTime Created, IReadOnlyList<SourceVersion> Versions);
