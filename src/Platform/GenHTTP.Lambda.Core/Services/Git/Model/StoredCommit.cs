using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Git;

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
