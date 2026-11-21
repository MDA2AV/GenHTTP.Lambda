using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Git;

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
