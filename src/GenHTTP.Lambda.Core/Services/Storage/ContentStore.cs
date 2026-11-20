using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// The store of a lambda: the content of every file of its versions, its
/// features and their previews, once each, below <c>code/{lambda}/blobs/</c>,
/// and the manifests that say which content each of them holds.
/// </summary>
/// <remarks>
/// Content is named by its SHA-256, so a file that is the same in many
/// versions - a picture, a library, a page nobody changed - is kept once and
/// written once, and a version is a manifest naming what it holds. What is
/// known about content without reading it - its size, its length as text,
/// the ids git gives it - is a row of <c>blobs</c> beside it, so how git names
/// content is git's concern and decides nothing about how it is kept.
///
/// Content is written beside the store, away from the reactor, and moved in
/// in a moment; a manifest is written once everything it names is there. A
/// lambda's turn here (see <see cref="Turns"/>) is held for those moments
/// only - moving in, freshening, writing a manifest - and for collecting
/// what nothing names any more. So content that a save is about to name was
/// either freshly written or freshened under the turn, and a collection,
/// which only takes what is older than <see cref="Fresh"/>, cannot take it
/// before the manifest naming it is written. The turn comes last: whatever
/// holds it takes no other.
/// </remarks>
public sealed class ContentStore(LambdaOptions options, IDbContextFactory<LambdaDbContext> databases, ContentReads reads)
{

    /// <summary>
    /// How long content no manifest names is kept all the same: what a save
    /// keeps before the turn it is written in, which no manifest names yet.
    /// </summary>
    public static readonly TimeSpan Fresh = TimeSpan.FromHours(1);

    /// <summary>
    /// How much content is kept in place rather than away from the reactor.
    /// </summary>
    private const long InPlace = 256 * 1024;

    private readonly Turns _turns = new();

    #region Reading

    /// <summary>
    /// The files a manifest names, or nothing where there is no manifest.
    /// </summary>
    /// <exception cref="InvalidDataException">What is there is no manifest</exception>
    public IReadOnlyList<VersionFile>? Read(long lambdaId, string manifest)
    {
        string text;

        try
        {
            text = File.ReadAllText(manifest);
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            return null;
        }

        return [.. Manifest.Read(text).Select(e => Kept(lambdaId, e))];
    }

    private VersionFile Kept(long lambdaId, Manifest.Entry entry)
        => VersionFile.Stored(entry.Name, entry.Size, entry.Encoding == "base64", entry.Length, new StoredContent(PathOf(lambdaId, entry.Blob), entry.Blob, reads, false));

    /// <summary>
    /// Whether a file's content is kept by the store of this lambda.
    /// </summary>
    public bool Keeps(long lambdaId, VersionFile file)
        => file.Content is StoredContent { Staged: false } stored && IsBelow(stored.Path, Root(lambdaId));

    #endregion

    #region Keeping

    /// <summary>
    /// Keeps the content of the files that this store does not keep yet, and
    /// freshens what it keeps of the others.
    /// </summary>
    /// <returns>The same files, each as the store keeps it, and whether any content was new to it</returns>
    public async ValueTask<(IReadOnlyList<VersionFile> Files, bool Added)> KeepAsync(long lambdaId, IReadOnlyList<VersionFile> files, CancellationToken cancellation = default)
    {
        var arriving = Enumerable.Range(0, files.Count).Where(i => !Keeps(lambdaId, files[i])).ToList();

        // read, hashed and written beside the store - away from the reactor
        // where the content of a request, a push or an archive is large, and
        // in place where it is a file or two of code, which takes less than
        // the hop
        var prepared = arriving.Count == 0
            ? []
            : arriving.Sum(i => files[i].Size) <= InPlace
                ? Prepare(lambdaId, files, arriving)
                : await Offload.Run(() => Prepare(lambdaId, files, arriving), cancellation);

        var result = files.ToArray();

        var known = new Dictionary<string, ContentFacts>(StringComparer.Ordinal);

        var added = false;

        using var turn = await _turns.TakeAsync(lambdaId);

        var now = DateTime.UtcNow;

        try
        {
            for (var i = 0; i < files.Count; i++)
            {
                if (Keeps(lambdaId, files[i]))
                {
                    Freshen(((StoredContent)files[i].Content).Path, now, files[i]);
                }
            }

            foreach (var (index, facts, source) in prepared)
            {
                var file = files[index];

                var target = PathOf(lambdaId, facts.Hash);

                if (File.Exists(target))
                {
                    Freshen(target, now, file);
                    Discard(source);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);

                    if (source != null)
                    {
                        File.Move(source, target);
                    }
                    else
                    {
                        // there when it was prepared and collected since: the content is at hand still
                        file.Content.CopyTo(target);
                    }

                    File.SetLastWriteTimeUtc(target, now);

                    added = true;
                }

                known[facts.Hash] = facts;

                if (file.Content is StoredContent { Staged: true } staged)
                {
                    staged.Kept(target, reads);
                }

                result[index] = VersionFile.Stored(file.Name, facts.Size, file.Binary, file.Chars ?? facts.Chars, new StoredContent(target, facts.Hash, reads, false));
            }

            await DescribeAsync(lambdaId, result, known);
        }
        finally
        {
            foreach (var (_, _, source) in prepared)
            {
                Discard(source);
            }
        }

        return (result, added);
    }

    /// <summary>
    /// Hashes the content of what arrives, and writes what the store does not
    /// have yet beside it, to be moved in under the turn.
    /// </summary>
    /// <returns>For each file: what it is, and the file to move in - or nothing where the store has it</returns>
    private List<(int Index, ContentFacts Facts, string? Source)> Prepare(long lambdaId, IReadOnlyList<VersionFile> files, List<int> arriving)
    {
        var prepared = new List<(int, ContentFacts, string?)>(arriving.Count);

        try
        {
            foreach (var index in arriving)
            {
                var file = files[index];

                if (file.Content is StoredContent stored)
                {
                    // staged beside the store, or kept by another lambda's: read
                    // through once to be hashed, and moved or copied in
                    var facts = stored.Facts ?? ContentFacts.Of(stored.Path);

                    if (stored.Staged)
                    {
                        prepared.Add((index, facts, stored.Path));
                        continue;
                    }

                    var copy = File.Exists(PathOf(lambdaId, facts.Hash)) ? null : Writing(lambdaId);

                    if (copy != null)
                    {
                        File.Copy(stored.Path, copy);
                    }

                    prepared.Add((index, facts, copy));
                    continue;
                }

                ContentFacts held;

                string? written = null;

                if (file.Content is BytesContent bytes)
                {
                    held = ContentFacts.Of(bytes.Span);

                    if (!File.Exists(PathOf(lambdaId, held.Hash)))
                    {
                        written = Writing(lambdaId);
                        file.Content.CopyTo(written);
                    }
                }
                else
                {
                    var content = file.Content.ReadBytes();

                    held = ContentFacts.Of(content);

                    if (!File.Exists(PathOf(lambdaId, held.Hash)))
                    {
                        written = Writing(lambdaId);
                        File.WriteAllBytes(written, content);
                    }
                }

                prepared.Add((index, held, written));
            }
        }
        catch
        {
            foreach (var (_, _, source) in prepared)
            {
                if (source != null && !IsStaged(source, files))
                {
                    Discard(source);
                }
            }

            throw;
        }

        return prepared;
    }

    /// <summary>
    /// Writes the rows that say what kept content is, for whatever has none.
    /// </summary>
    /// <param name="known">What is known of content that was just kept, so it is not read again</param>
    private async ValueTask DescribeAsync(long lambdaId, IReadOnlyList<VersionFile> files, Dictionary<string, ContentFacts> known)
    {
        var hashes = files.Select(f => f.Hash).Distinct(StringComparer.Ordinal).ToList();

        await using var database = await databases.CreateDbContextAsync();

        var described = await database.Blobs.AsNoTracking()
                                       .Where(b => b.LambdaId == lambdaId && hashes.Contains(b.Hash))
                                       .Select(b => b.Hash)
                                       .ToListAsync();

        var missing = hashes.Except(described, StringComparer.Ordinal).ToList();

        if (missing.Count == 0)
        {
            return;
        }

        foreach (var hash in missing)
        {
            // a row lost, or one made before there were rows: read off the content once
            var facts = known.GetValueOrDefault(hash) ?? ContentFacts.Of(PathOf(lambdaId, hash));

            database.Blobs.Add(new BlobEntity
            {
                LambdaId = lambdaId,
                Hash = facts.Hash,
                Size = facts.Size,
                Chars = facts.Chars,
                GitSha1 = facts.GitSha1,
                GitSha256 = facts.GitSha256
            });
        }

        await database.SaveChangesAsync();
    }

    /// <summary>
    /// Writes a manifest of the files, which the store has to keep, replacing
    /// one that is there unless told otherwise.
    /// </summary>
    /// <param name="replace">Whether a manifest that is there is replaced; where not, it is left and this one is not written</param>
    /// <returns>Whether it was written</returns>
    public async ValueTask<bool> WriteAsync(long lambdaId, string manifest, IReadOnlyList<VersionFile> files, bool replace = true)
    {
        var content = Manifest.Write(files);

        Directory.CreateDirectory(Path.GetDirectoryName(manifest)!);

        var staging = $"{manifest}.{Guid.NewGuid():n}.writing";

        using var turn = await _turns.TakeAsync(lambdaId);

        try
        {
            foreach (var file in files)
            {
                if (!Keeps(lambdaId, file) || !File.Exists(((StoredContent)file.Content).Path))
                {
                    throw new InvalidOperationException($"'{file.Name}' is not kept by the store of lambda #{lambdaId}, so no manifest names it.");
                }
            }

            File.WriteAllBytes(staging, content);

            try
            {
                File.Move(staging, manifest, replace);
            }
            catch (IOException) when (!replace && File.Exists(manifest))
            {
                return false;
            }

            return true;
        }
        finally
        {
            Discard(staging);
        }
    }

    #endregion

    #region Git

    /// <summary>
    /// The files this store keeps whose git blob ids are among those given,
    /// by their id - freshened, since a push is about to name them.
    /// </summary>
    /// <remarks>
    /// Without a name and binary where they are not text: whoever asks names them.
    /// </remarks>
    public async ValueTask<IReadOnlyDictionary<string, VersionFile>> FindAsync(long lambdaId, IReadOnlyCollection<string> gitIds)
    {
        var found = new Dictionary<string, VersionFile>(StringComparer.Ordinal);

        if (gitIds.Count == 0)
        {
            return found;
        }

        using var turn = await _turns.TakeAsync(lambdaId);

        await using var database = await databases.CreateDbContextAsync();

        var rows = await database.Blobs.AsNoTracking()
                                 .Where(b => b.LambdaId == lambdaId && gitIds.Contains(b.GitSha1))
                                 .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var row in rows)
        {
            var path = PathOf(lambdaId, row.Hash);

            try
            {
                File.SetLastWriteTimeUtc(path, now);
            }
            catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
            {
                // its row outlived it: read from what is pushed instead
                continue;
            }

            found[row.GitSha1] = VersionFile.Stored(string.Empty, row.Size, row.Chars == null, row.Chars, new StoredContent(path, row.Hash, reads, false));
        }

        return found;
    }

    /// <summary>
    /// The git blob id of each of the files, by the SHA-256 of what it holds.
    /// </summary>
    /// <remarks>
    /// From the rows for what the store keeps, and worked out of the content
    /// for anything else - and for kept content whose row went missing, which
    /// gets one again.
    /// </remarks>
    public async ValueTask<IReadOnlyDictionary<string, string>> GitIdsAsync(long lambdaId, IReadOnlyList<VersionFile> files)
    {
        var ids = new Dictionary<string, string>(StringComparer.Ordinal);

        var kept = files.Where(f => Keeps(lambdaId, f)).Select(f => f.Hash).Distinct(StringComparer.Ordinal).ToList();

        if (kept.Count > 0)
        {
            await using var database = await databases.CreateDbContextAsync();

            foreach (var row in await database.Blobs.AsNoTracking()
                                              .Where(b => b.LambdaId == lambdaId && kept.Contains(b.Hash))
                                              .Select(b => new { b.Hash, b.GitSha1 })
                                              .ToListAsync())
            {
                ids[row.Hash] = row.GitSha1;
            }

            var undescribed = files.Where(f => Keeps(lambdaId, f) && !ids.ContainsKey(f.Hash)).ToList();

            if (undescribed.Count > 0)
            {
                using var turn = await _turns.TakeAsync(lambdaId);

                await DescribeAsync(lambdaId, undescribed, []);

                await using var again = await databases.CreateDbContextAsync();

                var hashes = undescribed.Select(f => f.Hash).ToList();

                foreach (var row in await again.Blobs.AsNoTracking()
                                               .Where(b => b.LambdaId == lambdaId && hashes.Contains(b.Hash))
                                               .Select(b => new { b.Hash, b.GitSha1 })
                                               .ToListAsync())
                {
                    ids[row.Hash] = row.GitSha1;
                }
            }
        }

        foreach (var file in files.Where(f => !ids.ContainsKey(f.Hash)))
        {
            ids[file.Hash] = file.Content is StoredContent stored ? ContentFacts.Of(stored.Path).GitSha1 : ContentFacts.Of(file.Content.ReadBytes()).GitSha1;
        }

        return ids;
    }

    #endregion

    #region Collection

    /// <summary>
    /// Removes the content no manifest names and nothing freshened lately,
    /// with its row.
    /// </summary>
    /// <param name="named">The content the manifests of the lambda name, read under the turn so none is written meanwhile</param>
    /// <returns>How many files were removed, and whether content no manifest names was too fresh to go</returns>
    public async ValueTask<(int Removed, bool Lingering)> CollectAsync(long lambdaId, Func<HashSet<string>> named)
    {
        var root = Root(lambdaId);

        using var turn = await _turns.TakeAsync(lambdaId);

        var keep = named();

        var present = new HashSet<string>(StringComparer.Ordinal);

        var removed = 0;

        var lingering = false;

        var stale = DateTime.UtcNow - Fresh;

        if (Directory.Exists(root))
        {
            foreach (var folder in Directory.EnumerateDirectories(root))
            {
                var prefix = Path.GetFileName(folder);

                foreach (var file in Directory.EnumerateFiles(folder))
                {
                    var hash = prefix + Path.GetFileName(file);

                    var writing = prefix == WritingFolder;

                    if (!writing && keep.Contains(hash))
                    {
                        present.Add(hash);
                        continue;
                    }

                    if (File.GetLastWriteTimeUtc(file) > stale)
                    {
                        lingering = true;

                        if (!writing)
                        {
                            present.Add(hash);
                        }

                        continue;
                    }

                    if (Discard(file) && !writing)
                    {
                        removed++;
                    }
                }

                if (!Directory.EnumerateFileSystemEntries(folder).Any())
                {
                    try
                    {
                        Directory.Delete(folder);
                    }
                    catch (IOException)
                    {
                        // written to meanwhile
                    }
                }
            }
        }

        await using var database = await databases.CreateDbContextAsync();

        var rows = await database.Blobs.AsNoTracking().Where(b => b.LambdaId == lambdaId).Select(b => b.Hash).ToListAsync();

        var gone = rows.Where(r => !present.Contains(r)).ToList();

        if (gone.Count > 0)
        {
            await database.Blobs.Where(b => b.LambdaId == lambdaId && gone.Contains(b.Hash)).ExecuteDeleteAsync();
        }

        return (removed, lingering);
    }

    /// <summary>
    /// Removes the rows that say what a lambda's store keeps, for a lambda that goes.
    /// </summary>
    /// <remarks>
    /// The key cascades in the schema, but only where the connection has
    /// foreign keys switched on - said here so it does not depend on that.
    /// </remarks>
    public async ValueTask ForgetAsync(long lambdaId)
    {
        await using var database = await databases.CreateDbContextAsync();

        await database.Blobs.Where(b => b.LambdaId == lambdaId).ExecuteDeleteAsync();
    }

    #endregion

    #region Helpers

    /// <summary>
    /// The folder below the store that content is written into before it is moved in.
    /// </summary>
    private const string WritingFolder = "writing";

    public string Root(long lambdaId) => Path.Combine(options.CodeDirectory, lambdaId.ToString(), "blobs");

    private string PathOf(long lambdaId, string hash) => Path.Combine(Root(lambdaId), hash[..2], hash[2..]);

    private string Writing(long lambdaId)
    {
        var folder = Path.Combine(Root(lambdaId), WritingFolder);

        Directory.CreateDirectory(folder);

        return Path.Combine(folder, Guid.NewGuid().ToString("n"));
    }

    /// <summary>
    /// Makes kept content fresh, so no collection takes it before the
    /// manifest about to name it is written.
    /// </summary>
    private static void Freshen(string path, DateTime now, VersionFile file)
    {
        try
        {
            File.SetLastWriteTimeUtc(path, now);
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            // what it was read from was removed and collected meanwhile - by
            // fifty saves landing in between, or a feature saved over
            throw LambdaException.Conflict($"'{file.Name}' is no longer kept as it was when this was read. Read it again, and save again.");
        }
    }

    private static bool IsBelow(string path, string root)
        => path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal);

    private static bool IsStaged(string path, IReadOnlyList<VersionFile> files)
        => files.Any(f => f.Content is StoredContent { Staged: true } staged && staged.Path == path);

    private static bool Discard(string? file)
    {
        if (file == null)
        {
            return false;
        }

        try
        {
            File.Delete(file);
            return true;
        }
        catch (IOException)
        {
            // being read right now: the next collection takes it
            return false;
        }
    }

    #endregion

}
