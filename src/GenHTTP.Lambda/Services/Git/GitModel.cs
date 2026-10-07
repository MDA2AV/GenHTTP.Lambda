using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// Which commit each version of a lambda is, and which is the tip of each of
/// its features - what the references of its repository are made from.
/// </summary>
/// <param name="Versions">The commit of each version, by its number</param>
/// <param name="Features">The tip of each feature, by its key</param>
/// <param name="Moved">
/// The commits made before the platform laid a lambda out as it does now, which were a tip, and the commit on top of each
/// that lays the same files out anew - see <see cref="GitLayouts"/>
/// </param>
public sealed record GitIndex(Dictionary<int, string> Versions, Dictionary<string, FeatureTip> Features, Dictionary<string, string> Moved)
{

    public static GitIndex Empty() => new([], [], []);

    /// <summary>
    /// What main points to: the commit of the newest version, or the one
    /// laying it out anew, where it was made before the platform laid a
    /// lambda out as it does now.
    /// </summary>
    public GitObjectId? Main => Versions.Count == 0 ? null : GitObjectId.Parse(Current(Versions[Versions.Keys.Max()]));

    /// <summary>
    /// The commit that stands for a commit now: the one laying it out anew,
    /// where there is one, and the commit itself otherwise.
    /// </summary>
    public string Current(string commit) => Moved.GetValueOrDefault(commit, commit);

    /// <summary>
    /// Whether a commit is part of the history of the versions: a version, or
    /// the commit laying one out anew - which is all the published source has.
    /// </summary>
    public bool IsOfVersions(GitObjectId commit)
    {
        if (VersionOf(commit) != null)
        {
            return true;
        }

        var hex = commit.ToString();

        return Versions.Values.Any(v => Moved.GetValueOrDefault(v) == hex);
    }

    /// <summary>
    /// The version a commit is, or nothing for one that is none.
    /// </summary>
    public int? VersionOf(GitObjectId commit)
    {
        var hex = commit.ToString();

        foreach (var (version, id) in Versions)
        {
            if (id == hex)
            {
                return version;
            }
        }

        return null;
    }

}

/// <summary>
/// The commit a feature's branch points to, and what of the feature it holds.
/// </summary>
/// <param name="Commit">The commit</param>
/// <param name="Revision">The save of the feature's files it holds - a newer save needs a commit of its own</param>
/// <param name="Base">The newest version it contains - a feature moved past it needs a commit bringing that in</param>
public sealed record FeatureTip(string Commit, int Revision, int Base);

/// <summary>
/// A commit as it is kept: exactly as it was made or pushed, its tree, and
/// what each file of the tree is to the lambda.
/// </summary>
/// <param name="Data">The commit, base64 - its bytes are its identity, so they are never made again</param>
/// <param name="Tree">Every file of its tree</param>
/// <param name="Files">
/// The files of the lambda it holds, in the lambda's order - or nothing for a commit somebody pushed that was no
/// state of the lambda, because it was only on the way to one
/// </param>
/// <param name="Version">The version it was saved as, if it was one - which ends what it keeps of the history below it</param>
/// <param name="Layout">
/// How the lambda's files are laid out in its tree, one of <see cref="GitLayouts"/> - nothing for a commit made before there was
/// more than one
/// </param>
public sealed record StoredCommit(string Data, IReadOnlyList<StoredEntry> Tree, IReadOnlyList<StoredFile>? Files, int? Version = null,
                                  int? Layout = null)
{

    /// <summary>
    /// Whether its tree lays the lambda out as the platform does now, so a
    /// commit on top of it can be read as one.
    /// </summary>
    public bool IsCurrent => Layout == GitLayouts.Current;

    public GitCommit Parse() => GitCommit.Parse(Convert.FromBase64String(Data));

    /// <summary>
    /// The file of the tree at a path, if there is one.
    /// </summary>
    public StoredEntry? At(string path) => Tree.FirstOrDefault(e => e.Path == path);

}

/// <summary>
/// How the platform has laid a lambda out in the trees of its commits.
/// </summary>
/// <remarks>
/// A commit is made once and kept, so the commits made before the platform
/// laid a lambda out differently keep the layout they were made in: the
/// files the lambda served in <c>assets/</c>, where they are in
/// <c>resources/</c> now, and the platform's files of then. They are the
/// history and are served as they are. Only a commit somebody builds on - the
/// tip of main or of a feature - is laid out anew, by a commit on top of it
/// holding the same files the way they are laid out now, which a clone
/// fetches like any other and rebases onto - git follows the files it moves.
/// A push is read in today's layout, so one that builds on a commit of
/// before is refused, saying so.
/// </remarks>
public static class GitLayouts
{

    /// <summary>The files to serve in <c>assets/</c>: what a commit kept without a layout of its own was laid out in.</summary>
    public const int Assets = 1;

    /// <summary>The files to serve in <c>resources/</c>, and the rest of the code where the lambda has it.</summary>
    public const int Current = 2;

}

/// <summary>
/// A file of a commit's tree. Always a regular file: a lambda holds nothing else.
/// </summary>
/// <param name="Path">Where it is</param>
/// <param name="Blob">The blob holding what it holds</param>
public sealed record StoredEntry(string Path, string Blob);

/// <summary>
/// A file of the lambda, as a commit holds it.
/// </summary>
/// <param name="Name">What the lambda calls it</param>
/// <param name="Path">Where it is in the tree</param>
/// <param name="Blob">
/// What it holds - for the snippet what lambda.cs holds, which is not in the tree: the tree holds it as Project.cs
/// </param>
/// <param name="Binary">Whether the lambda keeps it as base64 rather than as text</param>
public sealed record StoredFile(string Name, string Path, string Blob, bool Binary);

/// <summary>
/// The lambda a repository is read or written as.
/// </summary>
/// <param name="Id">The identity its commits are kept under</param>
/// <param name="PrivateKey">Its editor key, to ask the other services with - never told anybody</param>
/// <param name="PublicKey">Its public key, to name it by</param>
/// <param name="Tier">Its tier</param>
/// <param name="Project">What the platform's files of a commit made now say</param>
/// <param name="Origin">The address it was reached at, for the addresses a push answers with</param>
public sealed record GitLambda(long Id, string PrivateKey, string PublicKey, string Tier, RepositoryProject Project, string Origin);
