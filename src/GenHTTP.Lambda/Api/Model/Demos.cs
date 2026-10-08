namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// A demo the installation keeps online, and how to read it.
/// </summary>
/// <param name="Shows">What reading its code teaches</param>
/// <param name="ReadWhen">The kinds of request it is the right starting point for</param>
/// <param name="PublicKey">Where it answers</param>
/// <param name="PrivateKey">Its editor key: public, and read only</param>
/// <param name="Address">Where it answers, below the hosting domain</param>
/// <param name="Files">The files it is made of, the snippet first</param>
/// <param name="Live">Whether it is answering right now - they are prepared after startup</param>
public sealed record DemoResponse(
    string Id,
    string Name,
    string Description,
    string Shows,
    string ReadWhen,
    string PublicKey,
    string PrivateKey,
    string Address,
    IReadOnlyList<string> Files,
    bool Live
);
