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
