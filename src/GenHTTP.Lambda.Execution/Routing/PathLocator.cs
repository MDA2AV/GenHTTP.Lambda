using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Protection;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Execution;

/// <summary>
/// Finds the lambda named by the first segment of the path, the way every
/// lambda is reached below <c>/lambda/</c> where the lambdas answer by path.
/// </summary>
/// <remarks>
/// The lambda routes on what comes after its key, so it is stepped past - a
/// lambda answers at the root of its address either way, and relative links
/// work the same below a path as at a host of its own.
///
/// The key is read as it is spelled (<see cref="LambdaKeys.TryRead"/>), never
/// by the rules for claiming one, so a rule made later does not take a lambda
/// from before it offline.
/// </remarks>
public sealed class PathLocator(IMetaService meta, HostingPages pages) : ILambdaLocator
{

    public ResolvedLambda? Locate(IRequest request)
    {
        var target = request.Header.Target;

        if (target.Current is not { } segment || !LambdaKeys.TryRead(segment.Decode(), out var key))
        {
            return null;
        }

        var lambda = meta.Resolve(key);

        if (lambda != null)
        {
            target.Advance();
        }

        return lambda;
    }

    public IResponse Unavailable(IRequest request)
    {
        var key = request.Header.Target.Current?.Decode() ?? string.Empty;

        return pages.Missing(request, $"/{Services.Hosting.PlatformRoutes.Lambdas}/{key}/");
    }

}

/// <summary>
/// Sends the address of a lambda without the slash after its key on to the
/// address with it, before the lambda is asked.
/// </summary>
/// <remarks>
/// At a host of its own a lambda is always asked for at least <c>/</c>; below
/// a path, <c>/lambda/quiz</c> would serve its front page with every relative
/// link in it resolved against <c>/lambda/</c> - so the page would load and
/// none of its scripts, styles or API calls.
/// </remarks>
/// <remarks>
/// It also sends a lambda that has a domain of its own on to it, with the
/// rest of the path and the query: decided here, on the key as the router
/// read it, so that however the path was spelled - <c>/Lambda/shop/</c>,
/// <c>//lambda/shop</c> - the lambda is never served below the platform's
/// address while it has a domain.
/// </remarks>
public sealed class TrailingSlashConcern(IHandler content, IDomainRegistry registry) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Header.Target;

        var key = target.Current is { } segment && LambdaKeys.TryRead(segment.Decode(), out var read) ? read : null;

        if (key != null && registry.DomainOf(key) is { } domain)
        {
            target.Advance();

            var rest = target.Current != null ? "/" + target.AsString(false, true).TrimStart('/') : "/";

            return new(Moved(request, $"https://{domain}{rest}{RequestOrigin.Query(request)}"));
        }

        if (target.Current != null && target.IsLast && !target.HasTrailingSlash)
        {
            // from the key the router read, where it is one, so a path
            // spelled with empty segments is sent to one without them
            var location = key != null ? PlatformRoutes.LambdaBase + key : target.AsString(false);

            return new(Moved(request, location + "/" + RequestOrigin.Query(request)));
        }

        return content.HandleAsync(request);
    }

    private static IResponse Moved(IRequest request, string location)
        => request.Respond()
                  .Status(request.HasType(RequestMethod.Get, RequestMethod.Head) ? ResponseStatus.MovedPermanently : ResponseStatus.PermanentRedirect)
                  .Header("Location", location)
                  .Build();

}

public sealed class TrailingSlashConcernBuilder(IDomainRegistry registry) : IConcernBuilder
{

    public IConcern Build(IHandler content) => new TrailingSlashConcern(content, registry);

}
