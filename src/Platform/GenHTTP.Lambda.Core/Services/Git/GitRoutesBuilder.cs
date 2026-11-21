using GenHTTP.Api.Content;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// The repositories of an area, for the component it belongs to.
/// </summary>
public sealed class GitRoutesBuilder(GitArea area, IGitService service, IMetaService meta, LambdaOptions options, ILogger<GitRoutes> logger) : IHandlerBuilder<GitRoutesBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    public GitRoutesBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build() => Concerns.Chain(_concerns, new GitRoutes(area, service, meta, options, logger));

}
