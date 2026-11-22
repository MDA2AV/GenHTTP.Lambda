namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// One thing that could be written at a position, and what it is.
/// </summary>
public sealed record ResolvedCompletion(string Label, string Kind, string Detail, string? Documentation);
