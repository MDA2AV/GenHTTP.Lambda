using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// The commit a feature's branch points to, and what of the feature it holds.
/// </summary>
/// <param name="Commit">The commit</param>
/// <param name="Revision">The save of the feature's files it holds - a newer save needs a commit of its own</param>
/// <param name="Base">The newest version it contains - a feature moved past it needs a commit bringing that in</param>
public sealed record FeatureTip(string Commit, int Revision, int Base);
