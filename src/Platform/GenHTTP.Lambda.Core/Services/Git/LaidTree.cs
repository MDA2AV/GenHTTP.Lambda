using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// The tree of a commit the platform makes.
/// </summary>
/// <param name="Entries">Every file of the tree</param>
/// <param name="Files">The lambda's files in it</param>
/// <param name="Contents">What the platform's files hold, which may not be kept yet, by their blob</param>
/// <param name="Own">The lambda's files, which may not be kept yet either, by their blob - read where they are needed</param>
public sealed record LaidTree(IReadOnlyList<StoredEntry> Entries, IReadOnlyList<StoredFile> Files, IReadOnlyDictionary<string, byte[]> Contents,
                              IReadOnlyDictionary<string, VersionFile> Own);
