using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// A file of the lambda, as a commit holds it.
/// </summary>
/// <param name="Name">What the lambda calls it</param>
/// <param name="Path">Where it is in the tree</param>
/// <param name="Blob">
/// What it holds - for the snippet what lambda.cs holds, which is not in the tree: the tree holds it as Project.cs
/// </param>
/// <param name="Binary">Whether the lambda keeps it as base64 rather than as text</param>
public sealed record StoredFile(string Name, string Path, string Blob, bool Binary);
