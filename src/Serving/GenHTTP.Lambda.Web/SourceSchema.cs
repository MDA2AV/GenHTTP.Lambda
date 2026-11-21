namespace GenHTTP.Lambda.Web;

/// <summary>
/// What is said about a published source in schema.org terms.
/// </summary>
/// <param name="License">Where its license is described</param>
/// <param name="Author">Who holds the copyright, if the owner named somebody</param>
/// <param name="Updated">When its newest version was saved</param>
/// <param name="Application">Where the app answers, while it is online</param>
public sealed record SourceSchema(string Name, string? Description, string License, string? Author, DateTime? Updated, string? Application);
