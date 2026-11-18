using GenHTTP.Lambda.Integrations.Files;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// What a name in a workspace comes to, and the room what is in one takes -
/// the same for what a lambda writes and what its owner or an agent puts there.
/// </summary>
/// <remarks>
/// A name is read as the file system of the server reads a path: slashes
/// separate its folders, <c>.</c> and <c>..</c> are walked, and whatever
/// leaves the workspace is refused - "../../etc/passwd" is a name too. Worked
/// out against a root that exists nowhere, so it never touches the disk, and
/// what comes out is relative, with slashes, as the integration takes it.
/// </remarks>
internal static class WorkspacePaths
{

    private static readonly string Anchor = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.Combine(Path.GetTempPath(), "genhttp-lambda-workspace")))
                                          + Path.DirectorySeparatorChar;

    #region Names

    /// <summary>
    /// What a name comes to inside a workspace, or nothing where it leaves it.
    /// </summary>
    /// <remarks>
    /// The caller has refused an empty name already, saying so its own way.
    /// The workspace itself is not inside it either: a name that comes to its
    /// root names nothing in it.
    /// </remarks>
    public static string? Resolve(string name)
    {
        var resolved = Path.GetFullPath(Path.Combine(Anchor, name));

        if (!resolved.StartsWith(Anchor, StringComparison.Ordinal) || resolved.Length == Anchor.Length)
        {
            return null;
        }

        return resolved[Anchor.Length..].Replace('\\', '/');
    }

    /// <summary>
    /// The folders a path goes into that are not there yet.
    /// </summary>
    /// <param name="folders">The folders the workspace has</param>
    public static int Missing(string path, ISet<string> folders)
    {
        var missing = 0;

        for (var at = path.LastIndexOf('/'); at > 0; at = path.LastIndexOf('/', at - 1))
        {
            if (!folders.Contains(path[..at]))
            {
                missing++;
            }
        }

        return missing;
    }

    #endregion

    #region Room

    /// <summary>
    /// The room everything in a workspace takes, counted as its quota is:
    /// every file in whole blocks, at least one, and a block for every folder.
    /// </summary>
    public static long Measure(IReadOnlyList<FileEntry> entries)
    {
        var used = 0L;

        foreach (var entry in entries)
        {
            used += entry.Folder ? WorkspaceLimits.Block : WorkspaceLimits.Footprint(entry.Length);
        }

        return used;
    }

    /// <summary>
    /// The folders of a listing, by their paths.
    /// </summary>
    public static HashSet<string> Folders(IReadOnlyList<FileEntry> entries)
        => entries.Where(e => e.Folder).Select(e => e.Path).ToHashSet(StringComparer.Ordinal);

    #endregion

}
