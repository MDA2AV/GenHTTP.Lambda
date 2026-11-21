using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// One file of the project a lambda's repository holds.
/// </summary>
/// <param name="Path">Where it is in the project</param>
/// <param name="Content">What it holds, where the platform made it - its own files, and the snippet in its class</param>
/// <param name="Name">The file of the lambda it is made from, or nothing for one the platform puts around it</param>
/// <param name="File">The file of the lambda it is, as it is kept - for every one but the snippet</param>
public sealed record ProjectFile(string Path, byte[]? Content, string? Name, VersionFile? File = null);
