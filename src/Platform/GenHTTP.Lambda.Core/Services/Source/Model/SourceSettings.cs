namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// Whether the source of a lambda is published, as its owner sees it.
/// </summary>
/// <param name="Published">Whether anybody may read it right now</param>
/// <param name="License">The SPDX identifier of the license it is published under</param>
/// <param name="Author">Who holds the copyright, or nothing for "the authors of" the lambda</param>
/// <param name="Stars">How many visitors starred it, kept while it is taken down</param>
/// <param name="PublishedAt">When it was last published</param>
public sealed record SourceSettings(string PublicKey, bool Published, string License, string? Author, int Stars, DateTime PublishedAt,
                                    DateTime Updated);
