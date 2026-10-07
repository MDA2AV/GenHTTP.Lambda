using System.Text;

using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// A lambda as the tree of a commit, and the tree of a pushed commit as a
/// lambda again.
/// </summary>
/// <remarks>
/// The tree is the project the lambda is exported as, made for a repository
/// (see <see cref="ProjectPacker.Repository"/>): the lambda's files where
/// <see cref="ProjectPaths"/> puts them, and the platform's around them.
///
/// The way back takes the lambda's files and checks the platform's: those
/// are the same in every commit until the platform changes them, so a pushed
/// commit has each as one of its parents has it - otherwise somebody changed
/// what has no effect on the lambda, and is told so rather than ignored. A
/// <c>Project.cs</c> that is a parent's comes back as the lambda.cs that
/// parent was made from, so a push that leaves the code alone leaves it alone
/// to the byte; one that changed it is taken out of the class again.
/// </remarks>
public static class ProjectTree
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    #region Laying out

    /// <summary>
    /// The tree of a commit the platform makes for the given files.
    /// </summary>
    /// <param name="previous">
    /// The commit it follows, whose Project.cs is kept where lambda.cs did not change - a push may have written it
    /// in a way of its own, and putting the snippet back into the class again would change that for nothing
    /// </param>
    public static LaidTree Lay(IReadOnlyList<LambdaFile> files, RepositoryProject project, StoredCommit? previous)
    {
        var entries = new List<StoredEntry>();

        var stored = new List<StoredFile>();

        var contents = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        var snippet = files.FirstOrDefault(f => f.Name == LambdaSource.EntryName)?.Code ?? string.Empty;

        var snippetBytes = Encoding.UTF8.GetBytes(snippet);

        var snippetBlob = GitObjectId.ForBlob(snippetBytes).ToString();

        contents[snippetBlob] = snippetBytes;

        // the class it was put into before, if the snippet is what it was
        var kept = previous?.Files?.FirstOrDefault(f => f.Name == LambdaSource.EntryName && f.Blob == snippetBlob) is { } same
                 ? previous.At(same.Path)
                 : null;

        var binary = files.Where(f => f.Encoding == "base64").Select(f => f.Name).ToHashSet(StringComparer.Ordinal);

        foreach (var file in ProjectPacker.Repository(project, files))
        {
            string blob;

            if (file.Name == LambdaSource.EntryName && kept != null)
            {
                blob = kept.Blob;
            }
            else
            {
                blob = GitObjectId.ForBlob(file.Content).ToString();

                contents[blob] = file.Content;
            }

            entries.Add(new StoredEntry(file.Path, blob));

            if (file.Name != null)
            {
                stored.Add(new StoredFile(file.Name, file.Path, file.Name == LambdaSource.EntryName ? snippetBlob : blob, binary.Contains(file.Name)));
            }
        }

        // the lambda's order, which the project does not keep
        var order = files.Select((f, i) => (f.Name, i)).ToDictionary(x => x.Name, x => x.i, StringComparer.Ordinal);

        stored.Sort((x, y) => order[x.Name].CompareTo(order[y.Name]));

        entries.Sort((x, y) => string.CompareOrdinal(x.Path, y.Path));

        return new LaidTree(entries, stored, contents);
    }

    #endregion

    #region Reading back

    /// <summary>
    /// The lambda a pushed tree holds, or why it holds none.
    /// </summary>
    /// <param name="tree">The files of the commit</param>
    /// <param name="parents">The commits it follows, as far as they are kept</param>
    /// <param name="read">Reads a kept file</param>
    public static async ValueTask<ReadTree> ReadAsync(IReadOnlyList<GitFile> tree, IReadOnlyList<StoredCommit> parents, Func<GitObjectId, byte[]> read)
    {
        // laid out as the platform did once: what was in assets/ then would
        // be read as code, and the lambda would serve nothing
        if (parents.Any(p => !p.IsCurrent))
        {
            return ReadTree.Refused("This builds on a commit made before the platform laid the lambda's files out anew: what it reads and serves is in resources/, not assets/. "
                                  + "Rebase onto origin/main (git pull --rebase, or git rebase origin/main) - git moves your changes along - and push again.");
        }

        foreach (var file in tree)
        {
            if (file.Mode != GitFileMode.Regular)
            {
                var what = file.Mode == GitFileMode.Symlink ? "a link" : "executable";

                return ReadTree.Refused($"'{file.Path}' is {what}, and a lambda holds plain files only. "
                                      + (file.Mode == GitFileMode.Symlink ? "Commit the file itself instead." : $"git update-index --chmod=-x {file.Path}, and commit again."));
            }
        }

        if (Generated(tree, parents) is { } complaint)
        {
            return ReadTree.Refused(complaint);
        }

        var parentFiles = parents.SelectMany(p => p.Files ?? []).ToList();

        var files = new List<LambdaFile>();

        var stored = new List<StoredFile>();

        var contents = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        string? project = null;

        foreach (var entry in tree)
        {
            var path = ProjectPaths.Classify(entry.Path);

            if (path.Kind != ProjectPathKind.Lambda)
            {
                continue;
            }

            var blob = entry.Id!.Value.ToString();

            var known = parentFiles.FirstOrDefault(f => f.Path == entry.Path && (f.Name == LambdaSource.EntryName || f.Blob == blob))
                     ?? parentFiles.FirstOrDefault(f => f.Path == entry.Path);

            if (path.Name == LambdaSource.EntryName)
            {
                var bytes = (await entry.ReadAsync()).ToArray();

                if (!TryText(bytes, out var text))
                {
                    return ReadTree.Refused("Project.cs is not UTF-8 text.");
                }

                project = text;

                // a parent's Project.cs is that parent's lambda.cs, to the byte
                var same = parents.Select(p => (Entry: p.At(ProjectPaths.Snippet), Snippet: p.Files?.FirstOrDefault(f => f.Name == LambdaSource.EntryName)))
                                  .FirstOrDefault(p => p.Entry?.Blob == blob && p.Snippet != null);

                string snippet;

                if (same.Snippet != null)
                {
                    snippet = Encoding.UTF8.GetString(read(GitObjectId.Parse(same.Snippet.Blob)));
                }
                else
                {
                    var unwrapped = ProjectSnippet.Unwrap(text);

                    if (unwrapped.Snippet == null)
                    {
                        return ReadTree.Refused(unwrapped.Complaint!);
                    }

                    snippet = unwrapped.Snippet;
                }

                var snippetBytes = Encoding.UTF8.GetBytes(snippet);

                var snippetBlob = GitObjectId.ForBlob(snippetBytes).ToString();

                contents[snippetBlob] = snippetBytes;

                files.Add(new LambdaFile(LambdaSource.EntryName, snippet));
                stored.Add(new StoredFile(LambdaSource.EntryName, entry.Path, snippetBlob, false));

                continue;
            }

            // named as the lambda named it where a parent had it, as the path says otherwise
            var name = known?.Name is { } before && before != LambdaSource.EntryName ? before : path.Name!;

            var content = (await entry.ReadAsync()).ToArray();

            if (LambdaSource.IsCompiled(name))
            {
                if (!TryText(content, out var text))
                {
                    return ReadTree.Refused($"'{entry.Path}' is not UTF-8 text.");
                }

                files.Add(new LambdaFile(name, text));
                stored.Add(new StoredFile(name, entry.Path, blob, false));
            }
            else if (known is { Binary: true } && known.Blob == blob || !TryText(content, out var asText))
            {
                files.Add(new LambdaFile(name, Convert.ToBase64String(content), "base64"));
                stored.Add(new StoredFile(name, entry.Path, blob, true));
            }
            else
            {
                files.Add(new LambdaFile(name, asText));
                stored.Add(new StoredFile(name, entry.Path, blob, false));
            }
        }

        if (project == null)
        {
            return ReadTree.Refused("Project.cs holds the code of the lambda and cannot be removed.");
        }

        var ordered = Ordered(files, parents.FirstOrDefault()?.Files);

        if (LambdaSource.Validate(ordered) is { } invalid)
        {
            // named as the lambda names it, which is where the clone has it but for the C# at the top
            return ReadTree.Refused(invalid);
        }

        var order = ordered.Select((f, i) => (f.Name, i)).ToDictionary(x => x.Name, x => x.i, StringComparer.Ordinal);

        stored.Sort((x, y) => order[x.Name].CompareTo(order[y.Name]));

        return new ReadTree(ordered, stored, contents, project, null);
    }

    /// <summary>
    /// Why the platform's files of a pushed tree are not those of a parent, if they are not.
    /// </summary>
    /// <remarks>
    /// Each on its own, so a merge may take one from either side; a file that
    /// is in no parent is one too many, and one missing that every parent has
    /// is missing.
    /// </remarks>
    private static string? Generated(IReadOnlyList<GitFile> tree, IReadOnlyList<StoredCommit> parents)
    {
        if (parents.Count == 0)
        {
            return null;
        }

        var paths = tree.Select(f => f.Path)
                        .Concat(parents.SelectMany(p => p.Tree.Select(e => e.Path)))
                        .Where(p => ProjectPaths.Classify(p).Kind == ProjectPathKind.Generated)
                        .Distinct(StringComparer.Ordinal)
                        .Order(StringComparer.Ordinal);

        foreach (var path in paths)
        {
            var mine = tree.FirstOrDefault(f => f.Path == path)?.Id?.ToString();

            if (parents.Any(p => p.At(path)?.Blob == mine))
            {
                continue;
            }

            if (mine == null)
            {
                return $"'{path}' is the platform's and cannot be removed: it is no part of the lambda. git checkout origin/main -- {path}, and commit again.";
            }

            return parents.All(p => p.At(path) == null)
                ? $"'{path}' is no file of the lambda: the platform writes the files around it. Remove it (git rm --cached {path}), and commit again."
                : $"'{path}' is the platform's: changing it changes nothing of the lambda, so the change is refused. Undo it (git checkout origin/main -- {path}), and commit again.";
        }

        return null;
    }

    /// <summary>
    /// The files in the order the lambda had them: lambda.cs first, then the
    /// files the commit before had in its order, then the new ones the way a
    /// zip of a lambda is read - so an unchanged lambda is the same to the byte.
    /// </summary>
    private static List<LambdaFile> Ordered(List<LambdaFile> files, IReadOnlyList<StoredFile>? before)
    {
        var known = (before ?? []).Select((f, i) => (f.Name, i)).ToDictionary(x => x.Name, x => x.i, StringComparer.Ordinal);

        return files.OrderBy(f => f.Name == LambdaSource.EntryName ? 0 : known.ContainsKey(f.Name) ? 1 : 2)
                    .ThenBy(f => known.GetValueOrDefault(f.Name, int.MaxValue))
                    .ThenBy(f => f.IsCompiled ? 0 : f.IsCode ? 1 : 2)
                    .ThenBy(f => f.Name, StringComparer.Ordinal)
                    .ToList();
    }

    /// <summary>
    /// Text where it is UTF-8 without NUL characters, kept as it is - a byte
    /// order mark included, so it is written back as it came.
    /// </summary>
    private static bool TryText(byte[] bytes, out string text)
    {
        try
        {
            text = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            text = string.Empty;
            return false;
        }

        // a byte order mark stays the character it stands for, so it is written back
        return !text.Contains('\0');
    }

    #endregion

}

/// <summary>
/// The tree of a commit the platform makes.
/// </summary>
/// <param name="Entries">Every file of the tree</param>
/// <param name="Files">The lambda's files in it</param>
/// <param name="Contents">What the files hold that may not be kept yet, by their blob</param>
public sealed record LaidTree(IReadOnlyList<StoredEntry> Entries, IReadOnlyList<StoredFile> Files, IReadOnlyDictionary<string, byte[]> Contents);

/// <summary>
/// The lambda a pushed tree holds.
/// </summary>
/// <param name="Files">The lambda's files, in its order - or nothing where the tree holds none</param>
/// <param name="Stored">The same as a commit keeps them</param>
/// <param name="Contents">What is kept for them besides the tree: the lambda.cs taken out of Project.cs</param>
/// <param name="Project">Project.cs as pushed, to say where in it a problem the compiler found is</param>
/// <param name="Complaint">Why the tree is no lambda</param>
public sealed record ReadTree(IReadOnlyList<LambdaFile>? Files, IReadOnlyList<StoredFile> Stored, IReadOnlyDictionary<string, byte[]> Contents,
                                string? Project, string? Complaint)
{

    public static ReadTree Refused(string complaint) => new(null, [], new Dictionary<string, byte[]>(), null, complaint);

}
