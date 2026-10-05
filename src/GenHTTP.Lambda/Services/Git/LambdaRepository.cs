using GenHTTP.Modules.Git;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// A lambda as git reads it: main is the newest version, a tag each version,
/// and - for whoever holds the editor key - a branch each feature.
/// </summary>
/// <remarks>
/// Made for one request, from which commit was what when it began; the
/// commits and their files are read from where they are kept as the server
/// asks for them.
///
/// The published source is the same history without the features: they are
/// the owner's work in progress, so neither their branches nor their commits
/// are there - a commit is only found where it is a version.
/// </remarks>
public class LambdaRepository(GitStore store, GitHistory history, long lambdaId, GitIndex index, bool owner, Func<string, string?> branches) : IGitRepository
{

    public ValueTask<GitReferences> GetReferencesAsync()
    {
        var references = new GitReferences().Head("main");

        if (index.Newest is { } newest)
        {
            references.Branch("main", newest);
        }

        foreach (var (version, commit) in index.Versions.OrderBy(v => v.Key))
        {
            references.Tag($"v{version}", GitObjectId.Parse(commit));
        }

        if (owner)
        {
            foreach (var (key, tip) in index.Features)
            {
                if (branches(key) is { } branch)
                {
                    references.Branch(branch, GitObjectId.Parse(tip.Commit));
                }
            }
        }

        return new(references);
    }

    public ValueTask<GitCommit?> GetCommitAsync(GitObjectId id)
    {
        if (!owner && index.VersionOf(id) == null)
        {
            return new((GitCommit?)null);
        }

        return new(store.ReadCommit(lambdaId, id)?.Parse());
    }

    public ValueTask<GitTree> GetTreeAsync(GitCommit commit)
    {
        var stored = store.ReadCommit(lambdaId, commit.Id) ?? throw new InvalidOperationException($"Commit {commit.Id} of lambda #{lambdaId} is no longer kept.");

        return new(history.TreeOf(lambdaId, stored.Tree));
    }

}

/// <summary>
/// The repository of a lambda as whoever holds its editor key reads it, and pushes to.
/// </summary>
public sealed class WritableLambdaRepository(GitStore store, GitHistory history, long lambdaId, GitIndex index, Func<string, string?> branches,
                                               Func<GitPush, ValueTask> push)
    : LambdaRepository(store, history, lambdaId, index, true, branches), IWritableGitRepository
{

    public ValueTask PushAsync(GitPush changes) => push(changes);

}
