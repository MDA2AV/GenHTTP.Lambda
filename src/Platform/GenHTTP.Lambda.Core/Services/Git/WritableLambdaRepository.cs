using GenHTTP.Modules.Git;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// The repository of a lambda as whoever holds its editor key reads it, and pushes to.
/// </summary>
public sealed class WritableLambdaRepository(GitStore store, GitHistory history, long lambdaId, GitIndex index, Func<string, string?> branches,
                                               Func<GitPush, ValueTask> push)
    : LambdaRepository(store, history, lambdaId, index, true, branches), IWritableGitRepository
{

    public ValueTask PushAsync(GitPush changes) => push(changes);

}
