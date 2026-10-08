using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
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
/// Pushes are few, but unpacked in memory and compiled, so only two are
/// applied at once on the whole server.
/// </remarks>
public sealed class GitService(IMetaService meta, IFeatureService features, ISourceService sources, GitStore store, GitHistory history, GitPushes pushes,
                                LambdaOptions options) : IGitService
{
    private readonly SemaphoreSlim[] _turns = [.. Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1))];

    private readonly SemaphoreSlim _pushing = new(2, 2);

    #region Functionality

    public async ValueTask<IGitRepository?> OpenAsync(string privateKey, string origin, CancellationToken cancellation = default)
    {
        if (meta.Get(privateKey) is not { } info || meta.GetId(privateKey) is not { } id)
        {
            return null;
        }

        var lambda = Describe(info, id, origin);

        var index = await SyncAsync(lambda, true, cancellation);

        var branches = features.List(privateKey).ToDictionary(f => f.Key, f => f.Branch);

        return new WritableLambdaRepository(store, history, id, index, key => branches.GetValueOrDefault(key), push => PushAsync(lambda, push));
    }

    public async ValueTask<IGitRepository?> ReadAsync(string publicKey, string origin, CancellationToken cancellation = default)
    {
        if (sources.GetProject(publicKey) == null || meta.GetPrivateKey(publicKey) is not { } privateKey)
        {
            return null;
        }

        if (meta.Get(privateKey) is not { } info || meta.GetId(privateKey) is not { } id)
        {
            return null;
        }

        var index = await SyncAsync(Describe(info, id, origin), false, cancellation);

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
        if (lambda.Tier == nameof(LambdaTier.Demo))
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
    /// installation is, and the license its source is published under.
    /// </summary>
    private GitLambda Describe(LambdaInfo lambda, long id, string origin)
    {
        var home = (options.PublicUrl ?? origin).TrimEnd('/');

        var license = sources.Get(lambda.PrivateKey) is { Published: true } source && SourceLicenses.Find(source.License) is { } found
            ? new RepositoryLicense(found, DateTime.UtcNow.Year, SourceLicenses.Holder(source.Author, lambda.PublicKey))
            : null;

        return new GitLambda(id, lambda.PrivateKey, lambda.PublicKey, lambda.Tier, new RepositoryProject(lambda.PublicKey, lambda.Address, home, license),
                             origin.TrimEnd('/'));
    }

    private SemaphoreSlim Turn(long lambdaId) => _turns[(int)((ulong)lambdaId % (ulong)_turns.Length)];

    #endregion

}
