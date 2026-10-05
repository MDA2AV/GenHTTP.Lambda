using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// Writes down the commits a lambda is read as: one for every version and
/// every save of a feature the platform made, when its repository is read.
/// </summary>
/// <remarks>
/// Made when somebody reads the repository rather than when a version is
/// saved, so saving - the editor, an agent, the API - stays as it was, and a
/// lambda nobody clones costs nothing. A commit is made from what it stands
/// for alone - the version, its time, its change - and kept from then on, so
/// it is the same commit for everybody who reads it later.
///
/// A version's commit follows the commit of the version before it. A
/// feature's follows the feature's tip, or the version it starts from; one
/// whose base was moved past what its tip holds - its owner or an agent
/// brought a newer version in - follows that version as well, so the branch
/// says it holds it. A feature that holds what its version holds is that
/// version's commit, until it changes.
///
/// Called in the lambda's turn to be read (see <see cref="GitService"/>), and
/// away from the reactor: the first read of a lambda with a long history
/// reads every version.
/// </remarks>
public sealed class GitHistory(GitStore store, IMetaService meta, IFeatureService features, ILogger<GitHistory> logger)
{

    /// <summary>
    /// Who the commits the platform makes are by.
    /// </summary>
    private const string Author = "GenHTTP Lambda";

    #region Bringing up to date

    /// <summary>
    /// Makes the commits that are missing, forgets those of versions and
    /// features that are gone, and says which commit is what now.
    /// </summary>
    /// <param name="withFeatures">Whether the features are read too - only for whoever holds the editor key</param>
    public async ValueTask<GitIndex> SyncAsync(GitLambda lambda, bool withFeatures)
    {
        var index = store.ReadIndex(lambda.Id);

        var changed = false;

        var dropped = false;

        var versions = meta.GetVersions(lambda.PrivateKey).OrderBy(v => v.Version).ToList();

        var existing = versions.Select(v => v.Version).ToHashSet();

        // the commits of versions pruned since, which the next commit still follows
        var before = new SortedDictionary<int, string>(index.Versions);

        foreach (var gone in index.Versions.Keys.Where(v => !existing.Contains(v)).ToList())
        {
            index.Versions.Remove(gone);
            dropped = true;
        }

        foreach (var version in versions)
        {
            if (index.Versions.ContainsKey(version.Version))
            {
                continue;
            }

            // the commit of the version before it - or, where that was pruned
            // before anybody read it, of the newest before it that was read
            var parent = before.Where(v => v.Key < version.Version).Select(v => v.Value).LastOrDefault();

            string code;

            try
            {
                code = meta.GetVersion(lambda.PrivateKey, version.Version).Code;
            }
            catch (LambdaException)
            {
                // pruned since the versions were listed: it is no part of the history any more
                continue;
            }

            var previous = parent != null ? store.ReadCommit(lambda.Id, GitObjectId.Parse(parent)) : null;

            var made = await CommitAsync(lambda, LambdaSource.Parse(code), parent != null ? [parent] : [], previous,
                                         OneLine(version.Change) ?? $"Version {version.Version}", version.Created, version.Version);

            index.Versions[version.Version] = made;
            before[version.Version] = made;

            changed = true;
        }

        if (withFeatures)
        {
            var open = features.List(lambda.PrivateKey);

            var keys = open.Select(f => f.Key).ToHashSet(StringComparer.Ordinal);

            foreach (var gone in index.Features.Keys.Where(k => !keys.Contains(k)).ToList())
            {
                index.Features.Remove(gone);
                dropped = true;
            }

            foreach (var feature in open)
            {
                changed |= await SyncAsync(lambda, index, feature);
            }
        }

        if (changed || dropped)
        {
            store.WriteIndex(lambda.Id, index);
        }

        if (dropped)
        {
            var removed = store.Collect(lambda.Id, index);

            if (removed > 0)
            {
                logger.LogDebug("Collected {Count} commits of lambda #{LambdaId}", removed, lambda.Id);
            }
        }

        return index;
    }

    /// <summary>
    /// Makes the commit a feature needs, if it needs one.
    /// </summary>
    /// <returns>Whether its tip moved</returns>
    private async ValueTask<bool> SyncAsync(GitLambda lambda, GitIndex index, FeatureInfo feature)
    {
        index.Features.TryGetValue(feature.Key, out var tip);

        if (tip != null && tip.Revision == feature.Revision && tip.Base >= feature.Base)
        {
            return false;
        }

        if (!index.Versions.TryGetValue(feature.Base, out var based))
        {
            // the version it starts from is kept like the one online, so this
            // is a version saved after the versions were read - the next read
            return false;
        }

        string code;

        try
        {
            code = features.Get(lambda.PrivateKey, feature.Key).Code;
        }
        catch (LambdaException)
        {
            // merged or deleted since the features were listed
            return false;
        }

        var own = tip != null && store.ReadCommit(lambda.Id, GitObjectId.Parse(tip.Commit)) is { Version: null };

        List<string> parents;

        if (!own)
        {
            // nothing of its own yet: as long as it holds what its version
            // holds, it is that version's commit
            if (code == meta.GetVersion(lambda.PrivateKey, feature.Base).Code)
            {
                index.Features[feature.Key] = new FeatureTip(based, feature.Revision, feature.Base);
                return true;
            }

            parents = [based];
        }
        else
        {
            parents = tip!.Base < feature.Base ? [tip.Commit, based] : [tip.Commit];
        }

        var previous = store.ReadCommit(lambda.Id, GitObjectId.Parse(parents[0]));

        var message = parents.Count > 1
            ? $"Brings version {feature.Base} into the feature '{feature.Name}'"
            : OneLine(feature.Change) ?? feature.Name;

        var commit = await CommitAsync(lambda, LambdaSource.Parse(code), parents, previous, message, feature.Modified, null);

        index.Features[feature.Key] = new FeatureTip(commit, feature.Revision, feature.Base);

        return true;
    }

    #endregion

    #region Commits

    /// <summary>
    /// Makes a commit of the platform's, keeps it, and says which it is.
    /// </summary>
    private async ValueTask<string> CommitAsync(GitLambda lambda, IReadOnlyList<LambdaFile> files, IReadOnlyList<string> parents, StoredCommit? previous,
                                                string message, DateTime when, int? version)
    {
        var at = new DateTimeOffset(DateTime.SpecifyKind(when, DateTimeKind.Utc));

        // the year a published source's license names is the year of the commit
        var project = lambda.Project.License is { } license ? lambda.Project with { License = license with { Year = at.Year } } : lambda.Project;

        var laid = ProjectTree.Lay(files, project, previous);

        foreach (var (blob, content) in laid.Contents)
        {
            store.WriteBlob(lambda.Id, GitObjectId.Parse(blob), content);
        }

        var tree = await TreeOf(lambda.Id, laid.Entries).ComputeIdAsync();

        var builder = GitCommit.Create()
                               .Tree(tree)
                               .Author(new GitSignature(Author, Email(lambda), at))
                               .Message(message);

        foreach (var parent in parents)
        {
            builder.Parent(GitObjectId.Parse(parent));
        }

        var commit = builder.Build();

        store.WriteCommit(lambda.Id, commit.Id, new StoredCommit(Convert.ToBase64String(commit.Data.Span), laid.Entries, laid.Files, version));

        return commit.Id.ToString();
    }

    /// <summary>
    /// Keeps a commit somebody pushed, exactly as it came.
    /// </summary>
    /// <param name="read">What the lambda makes of it, or nothing for one that was only on the way</param>
    /// <param name="version">The version it was saved as, if it was</param>
    public async ValueTask<StoredCommit> KeepAsync(long lambdaId, GitCommit commit, IReadOnlyList<GitFile> tree, ReadTree? read, int? version)
    {
        var entries = new List<StoredEntry>(tree.Count);

        foreach (var file in tree)
        {
            var id = file.Id ?? GitObjectId.ForBlob((await file.ReadAsync()).Span);

            if (!store.HasBlob(lambdaId, id))
            {
                store.WriteBlob(lambdaId, id, (await file.ReadAsync()).Span);
            }

            entries.Add(new StoredEntry(file.Path, id.ToString()));
        }

        foreach (var (blob, content) in read?.Contents ?? new Dictionary<string, byte[]>())
        {
            store.WriteBlob(lambdaId, GitObjectId.Parse(blob), content);
        }

        var stored = new StoredCommit(Convert.ToBase64String(commit.Data.Span), entries, read?.Files != null ? read.Stored : null, version);

        store.WriteCommit(lambdaId, commit.Id, stored);

        return stored;
    }

    /// <summary>
    /// The files of a kept commit, as the server reads them.
    /// </summary>
    public GitTree TreeOf(long lambdaId, IReadOnlyList<StoredEntry> entries)
    {
        var tree = GitTree.Create();

        foreach (var entry in entries)
        {
            var id = GitObjectId.Parse(entry.Blob);

            tree.Add(entry.Path, () => new ValueTask<ReadOnlyMemory<byte>>(store.ReadBlob(lambdaId, id)), GitFileMode.Regular, id);
        }

        return tree.Build();
    }

    #endregion

    #region Helpers

    /// <summary>
    /// The address the commits of the platform are by: the installation's.
    /// </summary>
    private static string Email(GitLambda lambda)
        => $"lambda@{(Uri.TryCreate(lambda.Project.Home, UriKind.Absolute, out var home) ? home.Host : "localhost")}";

    private static string? OneLine(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return string.Join(' ', text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    #endregion

}
