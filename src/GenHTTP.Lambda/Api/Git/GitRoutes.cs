using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Git;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.Git;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Redirects;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api.Git;

/// <summary>
/// Every lambda as a git repository: at <c>/editor/{privateKey}/{name}.git</c>
/// for whoever holds its editor key, to clone and push to, and at
/// <c>/source/{publicKey}.git</c> for anybody, to clone, while its source is
/// published.
/// </summary>
/// <remarks>
/// The address of the editor and of the published source with <c>.git</c>
/// added, the way repositories are addressed elsewhere: a clone is named
/// after what precedes it. Below the editor's that is any name - the public
/// key is what the platform offers - since it only names the folder, and a
/// clone made before the key changed goes on pushing to the same lambda. Both
/// answer without the <c>.git</c> as well, and opened in a browser they lead
/// to their page.
///
/// Only the paths git asks for are answered here; anything else below
/// <c>/editor/</c> and <c>/source/</c> is the pages', and passes on to them.
/// A repository that is not there is answered as such - never with a page,
/// which git would take for a broken server.
///
/// What the server writes - a pack, or the answer to a push, which is where
/// the push is applied - is written away from the reactor (see
/// <see cref="OffloadedContent"/>). Fetching is logged like a download,
/// pushing by the service, which knows what it did.
/// </remarks>
public sealed class GitRoutes : IHandler
{
    private const string Chosen = "lambda.git.repository";

    private static readonly string[][] Endpoints = [["info", "refs"], ["git-upload-pack"], ["git-receive-pack"]];

    private readonly IHandler _git;

    private readonly IGitService _service;

    private readonly IMetaService _meta;

    private readonly ILogger _logger;

    #region Initialization

    public GitRoutes(IGitService service, IMetaService meta, LambdaOptions options, ILogger<GitRoutes> logger)
    {
        _service = service;
        _meta = meta;
        _logger = logger;

        _git = GitServer.Create()
                        .Repository(request => new ValueTask<IGitRepository?>(request.Properties.TryGet<IGitRepository>(Chosen, out var chosen) ? chosen : null))
                        .Agent("genhttp-lambda")
                        .MaximumPushSize(options.GitMaxPushBytes)
                        // read once more rather than held, beyond this, for every clone at once
                        .ContentCache(16 * 1024 * 1024)
                        .Build();
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => _git.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (Parse(request.Header.Target) is not { } route)
        {
            return null;
        }

        if (route.Skip == 0)
        {
            // the repository's own address, opened in a browser
            return request.Header.Method == RequestMethod.Get || request.Header.Method == RequestMethod.Head
                ? await Redirect.To(route.Owner ? $"/editor/{route.Key}" : $"/source/{route.Key}", true).Build().HandleAsync(request)
                : null;
        }

        var origin = RequestOrigin.Of(request);

        var repository = route.Owner
            ? await _service.OpenAsync(route.Key, origin)
            : await _service.ReadAsync(route.Key, origin);

        if (repository == null)
        {
            return Missing(request, route.Owner ? "There is no lambda with this editor key (or it has been deleted)." : "No source is published at this address.");
        }

        request.Properties[Chosen] = repository;

        request.Header.Target.Advance(route.Skip);

        var response = await _git.HandleAsync(request);

        if (response == null)
        {
            return Missing(request, "This is no address of a git repository.");
        }

        if (route.Fetching && request.Header.Query.GetEntry("service") == "git-upload-pack")
        {
            if (route.Owner)
            {
                _logger.LogInformation("Fetched lambda {Lambda} with git", _meta.PublicKeyOf(route.Key));
            }
            else
            {
                _logger.LogInformation("Fetched source of lambda {Lambda} with git", route.Key);
            }
        }

        return response.Content is { } content
            ? response.Rebuild().Content(new OffloadedContent(content)).Build()
            : response;
    }

    #endregion

    #region Routing

    /// <summary>
    /// A request for a repository.
    /// </summary>
    /// <param name="Owner">Whether it is the editor's, read and written with the editor key</param>
    /// <param name="Key">The editor key, or the public key of a published source</param>
    /// <param name="Skip">How many segments come before what git asks for - none for the repository's own address</param>
    /// <param name="Fetching">Whether git asks what there is, which a clone, a fetch and a push begin with</param>
    private sealed record Route(bool Owner, string Key, int Skip, bool Fetching);

    private static Route? Parse(IRequestTarget target)
    {
        var area = target.Current?.Decode();

        if (area is not ("editor" or "source") || target.Next(1)?.Decode() is not { Length: > 0 } second)
        {
            return null;
        }

        var rest = new List<string>();

        for (var offset = 2; target.Next(offset) is { } segment && rest.Count < 4; offset++)
        {
            rest.Add(segment.Decode());
        }

        if (area == "source")
        {
            var named = second.EndsWith(".git", StringComparison.Ordinal);

            var key = named ? second[..^4] : second;

            if (named && rest.Count == 0)
            {
                return new Route(false, key, 0, false);
            }

            return Endpoint(rest) is { } endpoint ? new Route(false, key, 2, endpoint == 0) : null;
        }

        // below the editor's key: what git asks for, after a name or not
        if (Endpoint(rest) is { } direct)
        {
            return new Route(true, second, 2, direct == 0);
        }

        if (rest.Count == 1 && rest[0].EndsWith(".git", StringComparison.Ordinal))
        {
            return new Route(true, second, 0, false);
        }

        return rest.Count > 1 && Endpoint(rest[1..]) is { } named2 ? new Route(true, second, 3, named2 == 0) : null;
    }

    /// <summary>
    /// Which of the paths git asks for these segments are, if they are one.
    /// </summary>
    private static int? Endpoint(List<string> segments)
    {
        for (var i = 0; i < Endpoints.Length; i++)
        {
            if (segments.SequenceEqual(Endpoints[i], StringComparer.Ordinal))
            {
                return i;
            }
        }

        return null;
    }

    private static IResponse Missing(IRequest request, string message)
        => request.Respond()
                  .Status(ResponseStatus.NotFound)
                  .Content(Resource.FromString(message + "\n").Type(ContentType.TextPlain).Build())
                  .Build();

    #endregion

}

/// <summary>
/// Puts the repositories into the layout of the platform.
/// </summary>
public sealed class GitRoutesBuilder(IGitService service, IMetaService meta, LambdaOptions options, ILogger<GitRoutes> logger) : IHandlerBuilder<GitRoutesBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    public GitRoutesBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build() => Concerns.Chain(_concerns, new GitRoutes(service, meta, options, logger));

}
