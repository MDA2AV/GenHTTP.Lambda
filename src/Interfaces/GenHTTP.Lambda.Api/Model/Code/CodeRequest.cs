using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The code something should be worked out from, and where in it to look.
/// </summary>
/// <remarks>
/// One shape for every question about code, whether or not it needs a
/// position: the analyses that read a single file take the one named by
/// <paramref name="File" />, those that read the lambda take them all.
/// </remarks>
[Summary("The files a question about code is asked of, and where in them to look.")]
public sealed record CodeRequest(
    [Summary("The files of the lambda, `lambda.cs` first, as they would be saved")]
    IReadOnlyList<LambdaFile>? Files,
    [Summary("The file a question about one file is about; left out, the first")]
    string? File = null,
    [Summary("The line of the position, counting from zero")]
    int Line = 0,
    [Summary("The column of the position, counting from zero")]
    int Column = 0
);
