using System.Text.Json;

using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// Keeps the commits a lambda is read as with git, below
/// <c>{data}/git/{lambda}</c>: each commit as it was made or pushed, the
/// files they hold, and which commit is which version and feature.
/// </summary>
/// <remarks>
/// A commit is its bytes - its id is their hash - so it is made once and kept,
/// never made again from the version: a newer platform would make it a little
/// differently, and every clone would see its history rewritten. Pushed ones
/// are kept as they came, which is what a client that pushed them expects.
///
/// The files are kept once each, by the id git gives them, so a version that
/// changes one file adds one file. They are what the versions hold already,
/// and kept here all the same: a commit has to be served as it was for as
/// long as it is reachable, and the files of a feature change in place.
///
/// Written through a file beside the one it replaces, so a write cut short
/// leaves the one before. What nothing reaches any more is collected once a
/// version is pruned or a feature goes; until then it is merely kept.
/// </remarks>
public sealed class GitStore(LambdaOptions options)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    #region Index

    public GitIndex ReadIndex(long lambda)
    {
        var file = Path.Combine(Root(lambda), "index.json");

        if (!File.Exists(file))
        {
            return GitIndex.Empty();
        }

        var stored = JsonSerializer.Deserialize<StoredIndex>(File.ReadAllText(file), Json);

        return new GitIndex(stored?.Versions ?? [], stored?.Features ?? [], stored?.Moved ?? []);
    }

    public void WriteIndex(long lambda, GitIndex index)
        => Write(Path.Combine(Root(lambda), "index.json"), JsonSerializer.SerializeToUtf8Bytes(new StoredIndex(index.Versions, index.Features, index.Moved), Json));

    private sealed record StoredIndex(Dictionary<int, string>? Versions, Dictionary<string, FeatureTip>? Features, Dictionary<string, string>? Moved);

    #endregion

    #region Commits

    public StoredCommit? ReadCommit(long lambda, GitObjectId id)
    {
        var file = CommitFile(lambda, id);

        try
        {
            return File.Exists(file) ? JsonSerializer.Deserialize<StoredCommit>(File.ReadAllText(file), Json) : null;
        }
        catch (FileNotFoundException)
        {
            // collected between looking and reading
            return null;
        }
    }

    public void WriteCommit(long lambda, GitObjectId id, StoredCommit commit)
        => Write(CommitFile(lambda, id), JsonSerializer.SerializeToUtf8Bytes(commit, Json));

    #endregion

    #region Blobs

    public bool HasBlob(long lambda, GitObjectId id) => File.Exists(BlobFile(lambda, id));

    /// <summary>
    /// Keeps what a file holds, unless it is kept already.
    /// </summary>
    public void WriteBlob(long lambda, GitObjectId id, ReadOnlySpan<byte> content)
    {
        var file = BlobFile(lambda, id);

        if (!File.Exists(file))
        {
            Write(file, content);
        }
    }

    /// <summary>
    /// What a file holds, read in place: it is bounded by what a version may
    /// hold, and read where the response is written, away from the reactor.
    /// </summary>
    public byte[] ReadBlob(long lambda, GitObjectId id)
    {
        try
        {
            return File.ReadAllBytes(BlobFile(lambda, id));
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new InvalidOperationException($"The file {id} of lambda #{lambda} is no longer kept.", e);
        }
    }

    #endregion

    #region Collection

    /// <summary>
    /// Removes the commits and the files nothing reaches any more.
    /// </summary>
    /// <remarks>
    /// Reached is a version that is still there, the commit laying it out
    /// anew where it was made before the platform laid lambdas out as it does
    /// now, the tip of a feature that is still there, and what lies below a
    /// tip up to the versions it starts from. Below a version nothing is
    /// followed: the versions before it are kept for as long as they are
    /// versions, and a version that was pruned is gone from the history - a
    /// clone is told the history starts later.
    /// </remarks>
    /// <returns>How many commits were removed</returns>
    public int Collect(long lambda, GitIndex index)
    {
        var root = Root(lambda);

        var commits = Path.Combine(root, "commits");

        if (!Directory.Exists(commits))
        {
            return 0;
        }

        var versions = index.Versions.Values.ToHashSet(StringComparer.Ordinal);

        var live = new HashSet<string>(StringComparer.Ordinal);

        var blobs = new HashSet<string>(StringComparer.Ordinal);

        var moves = versions.Where(index.Moved.ContainsKey).Select(v => index.Moved[v]);

        var pending = new Stack<string>(versions.Concat(moves).Concat(index.Features.Values.Select(f => f.Commit)));

        while (pending.TryPop(out var id))
        {
            if (!live.Add(id) || ReadCommit(lambda, GitObjectId.Parse(id)) is not { } commit)
            {
                continue;
            }

            foreach (var entry in commit.Tree)
            {
                blobs.Add(entry.Blob);
            }

            foreach (var file in commit.Files ?? [])
            {
                blobs.Add(file.Blob);
            }

            if (commit.Version != null)
            {
                continue;
            }

            foreach (var parent in commit.Parse().Parents)
            {
                var hex = parent.ToString();

                // a version below a feature is reached as long as it is one
                if (!versions.Contains(hex) && ReadCommit(lambda, parent) is { Version: not null })
                {
                    continue;
                }

                pending.Push(hex);
            }
        }

        // what laid out a commit that is gone is gone too
        foreach (var gone in index.Moved.Where(m => !live.Contains(m.Value)).Select(m => m.Key).ToList())
        {
            index.Moved.Remove(gone);
        }

        var removed = 0;

        foreach (var file in Directory.EnumerateFiles(commits, "*.json"))
        {
            if (!live.Contains(Path.GetFileNameWithoutExtension(file)))
            {
                Delete(file);
                removed++;
            }
        }

        var stored = Path.Combine(root, "blobs");

        if (Directory.Exists(stored))
        {
            foreach (var file in Directory.EnumerateFiles(stored, "*", SearchOption.AllDirectories))
            {
                var id = Path.GetFileName(Path.GetDirectoryName(file)) + Path.GetFileName(file);

                if (!blobs.Contains(id))
                {
                    Delete(file);
                }
            }
        }

        return removed;
    }

    #endregion

    #region Helpers

    private string Root(long lambda) => Path.Combine(options.GitDirectory, lambda.ToString());

    private string CommitFile(long lambda, GitObjectId id) => Path.Combine(Root(lambda), "commits", $"{id}.json");

    private string BlobFile(long lambda, GitObjectId id)
    {
        var hex = id.ToString();

        return Path.Combine(Root(lambda), "blobs", hex[..2], hex[2..]);
    }

    private static void Write(string file, ReadOnlySpan<byte> content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);

        var staging = $"{file}.{Guid.NewGuid():n}.writing";

        File.WriteAllBytes(staging, content);

        File.Move(staging, file, true);
    }

    private static void Delete(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (IOException)
        {
            // being read right now: the next collection takes it
        }
    }

    #endregion

}
