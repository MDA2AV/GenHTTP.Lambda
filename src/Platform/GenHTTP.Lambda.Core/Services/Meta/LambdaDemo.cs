namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// One demo.
/// </summary>
/// <param name="Id">Its key and the template it comes from, which are the same</param>
/// <param name="Name">What it is, in a few words, for whoever reads its code</param>
/// <param name="Goal">What somebody who wants one would call it, for the page that creates a lambda</param>
/// <param name="Pitch">What they would get, in the same words</param>
/// <param name="Description">What a visitor can do with it</param>
/// <param name="Shows">What reading its code teaches, so an agent can pick the one to read</param>
/// <param name="ReadWhen">The kinds of request it is the right starting point for</param>
/// <param name="Secrets">
/// The secrets it is given, each a random value nobody knows - so its secrets
/// are switched on and listed like any owner's, and read by its code
/// </param>
/// <param name="Database">
/// Whether it keeps its records in a database - which the demo, and every
/// lambda started as a copy of it, has switched on from the start
/// </param>
/// <param name="License">
/// The license its source is published under on /source, which the
/// installation does for every demo - they are there to be read and built on
/// </param>
public sealed record LambdaDemo(string Id, string Name, string Goal, string Pitch, string Description, string Shows, string ReadWhen,
                                IReadOnlyList<string>? Secrets = null, bool Database = false, string License = "MIT")
{

    /// <summary>
    /// Where it is hosted and the key its editor opens with - the same, and
    /// both public, because a demo is there to be read.
    /// </summary>
    public string Key => Id;

}
