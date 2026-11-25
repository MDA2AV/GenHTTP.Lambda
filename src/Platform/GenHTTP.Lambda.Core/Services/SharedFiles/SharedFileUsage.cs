namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// A shared file, and what of the installation uses it.
/// </summary>
public sealed record SharedFileUsage(SharedFile File, IReadOnlyList<SharedFileUse> Uses);
