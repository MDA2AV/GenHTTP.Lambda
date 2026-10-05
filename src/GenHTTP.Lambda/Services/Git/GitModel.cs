using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// Which commit each version of a lambda is, and which is the tip of each of
/// its features - what the references of its repository are made from.
/// </summary>
/// <param name="Versions">The commit of each version, by its number</param>
/// <param name="Features">The tip of each feature, by its key</param>
public sealed record GitIndex(Dictionary<int, string> Versions, Dictionary<string, FeatureTip> Features)
{

    public static GitIndex Empty() => new([], []);

    /// <summary>
    /// The commit of the newest version.
    /// </summary>
    public GitObjectId? Newest => Versions.Count == 0 ? null : GitObjectId.Parse(Versions[Versions.Keys.Max()]);

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
public sealed record StoredCommit(string Data, IReadOnlyList<StoredEntry> Tree, IReadOnlyList<StoredFile>? Files, int? Version = null)
{

    public GitCommit Parse() => GitCommit.Parse(Convert.FromBase64String(Data));

    /// <summary>
    /// The file of the tree at a path, if there is one.
    /// </summary>
    public StoredEntry? At(string path) => Tree.FirstOrDefault(e => e.Path == path);

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
