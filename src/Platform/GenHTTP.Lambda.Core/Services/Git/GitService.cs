using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// Serves every lambda as a git repository.
/// </summary>
/// <remarks>
/// Finds the lambda, takes its turn, and leaves the rest to the units beside
/// it: <see cref="GitHistory"/> writes down the commits the versions and the
/// features are, <see cref="GitPushes"/> makes versions and features of what
/// is pushed, <see cref="GitStore"/> keeps the commits and their files, and
/// <see cref="ProjectTree"/> says what a lambda is as a tree and back.
///
/// A lambda is read and written one request at a time: making its commits
/// and applying a push both decide what the next commit follows. The turn is
/// waited for asynchronously, since it is held across reading every version
/// of a lambda read for the first time, and across compiling what is pushed.
/// Pushes are few, but every file they bring is read and what they make is
/// compiled, so only two are applied at once on the whole server.
/// </remarks>
public sealed class GitService(IMetaService meta, IFeatureService features, ISourceService sources, GitStore store, GitHistory history, GitPushes pushes,
                                IBrandingService branding, LambdaOptions options) : IGitService
{
    private readonly SemaphoreSlim[] _turns = [.. Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1))];

    private readonly SemaphoreSlim _pushing = new(2, 2);

    #region Functionality

    public async ValueTask<IGitRepository?> OpenAsync(string privateKey, string origin, VersionAuthor? author = null, CancellationToken cancellation = default)
    {
        if (await meta.GetAsync(privateKey, cancellation) is not { } info || await meta.GetIdAsync(privateKey, cancellation) is not { } id)
        {
            return null;
        }

        // a push of a run of the agent saves as the run, as every door does
        var lambda = await DescribeAsync(info, id, origin, cancellation) with { Author = author };

        var index = await SyncAsync(lambda, true, cancellation);

        var branches = (await features.ListAsync(privateKey, cancellation)).ToDictionary(f => f.Key, f => f.Branch);

        return new WritableLambdaRepository(store, history, id, index, key => branches.GetValueOrDefault(key), push => PushAsync(lambda, push));
    }

    public async ValueTask<IGitRepository?> ReadAsync(string publicKey, string origin, CancellationToken cancellation = default)
    {
        if (await sources.GetProjectAsync(publicKey, cancellation) == null || await meta.GetPrivateKeyAsync(publicKey, cancellation) is not { } privateKey)
        {
            return null;
        }

        if (await meta.GetAsync(privateKey, cancellation) is not { } info || await meta.GetIdAsync(privateKey, cancellation) is not { } id)
        {
            return null;
        }

        var index = await SyncAsync(await DescribeAsync(info, id, origin, cancellation), false, cancellation);

        return new LambdaRepository(store, history, id, index, false, _ => null);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Brings the commits of a lambda up to date, in its turn and away from the reactor.
    /// </summary>
    private async ValueTask<GitIndex> SyncAsync(GitLambda lambda, bool withFeatures, CancellationToken cancellation)
    {
        var turn = Turn(lambda.Id);

        await turn.WaitAsync(cancellation);

        try
        {
            return await Offload.Run(() => history.SyncAsync(lambda, withFeatures), cancellation);
        }
        finally
        {
            turn.Release();
        }
    }

    /// <summary>
    /// Applies a push, in the lambda's turn and after the commits were
    /// brought up to date in it - so it is checked against what is there now,
    /// not against what was there when the client fetched.
    /// </summary>
    /// <remarks>
    /// Already away from the reactor: the server applies a push where it
    /// writes its answer, which the route hands to the pool.
    /// </remarks>
    private async ValueTask PushAsync(GitLambda lambda, GitPush push)
    {
        if (lambda.Blocked)
        {
            pushes.RefuseAll(lambda, push, LambdaGuard.Blocked(lambda.PublicKey));
            return;
        }

        if (lambda.ReadOnly)
        {
            pushes.RefuseAll(lambda, push, LambdaGuard.ReadOnly(lambda.PublicKey));
            return;
        }

        await _pushing.WaitAsync();

        try
        {
            var turn = Turn(lambda.Id);

            await turn.WaitAsync();

            try
            {
                var index = await history.SyncAsync(lambda, true);

                await pushes.PushAsync(lambda, index, push);
            }
            finally
            {
                turn.Release();
            }
        }
        finally
        {
            _pushing.Release();
        }
    }

    /// <summary>
    /// The lambda as its repository says it: where it runs, where the
    /// installation is and what it is called, and the license its source is
    /// published under.
    /// </summary>
    /// <remarks>
    /// The name is the installation's as it is when a commit is made, as the
    /// address is: a commit is kept as it was made, and a push is compared
    /// with the commits it builds on, so renaming the installation changes
    /// the platform's files of the commits made after it and refuses no push.
    /// </remarks>
    private async ValueTask<GitLambda> DescribeAsync(LambdaInfo lambda, long id, string origin, CancellationToken cancellation)
    {
        var home = (options.PublicUrl ?? origin).TrimEnd('/');

        var license = await sources.GetAsync(lambda.PrivateKey, cancellation) is { Published: true } source && SourceLicenses.Find(source.License) is { } found
            ? new RepositoryLicense(found, DateTime.UtcNow.Year, SourceLicenses.Holder(source.Author, lambda.PublicKey))
            : null;

        return new GitLambda(id, lambda.PrivateKey, lambda.PublicKey, lambda.Policy.ReadOnly, new RepositoryProject(lambda.PublicKey, lambda.Address, home, license, branding.Current.Name),
                             origin.TrimEnd('/'), lambda.Blocked != null);
    }

    private SemaphoreSlim Turn(long lambdaId) => _turns[(int)((ulong)lambdaId % (ulong)_turns.Length)];

    #endregion

}
