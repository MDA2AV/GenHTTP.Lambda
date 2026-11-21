using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// The lambda a repository is read or written as.
/// </summary>
/// <param name="Id">The identity its commits are kept under</param>
/// <param name="PrivateKey">Its editor key, to ask the other services with - never told anybody</param>
/// <param name="PublicKey">Its public key, to name it by</param>
/// <param name="ReadOnly">Whether only the installation changes it - a demo - which refuses every push</param>
/// <param name="Blocked">Whether the operator blocked it, which refuses every push</param>
/// <param name="Project">What the platform's files of a commit made now say</param>
/// <param name="Origin">The address it was reached at, for the addresses a push answers with</param>
public sealed record GitLambda(long Id, string PrivateKey, string PublicKey, bool ReadOnly, RepositoryProject Project, string Origin, bool Blocked = false);
