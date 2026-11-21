using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Branding;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.SinglePageApplications;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// A bundle whose pages are all drawn in the browser - the editor, the panel -
/// served below the path its component is mounted at.
/// </summary>
/// <remarks>
/// Every path the bundle has no file for is one of its client routes and gets
/// the index page, uncached so a new build is picked up; its scripts and
/// styles are cached for good. Nothing about such a page is for a search
/// engine, so none of them is named on the server: the bundle names them in
/// the browser, and says they are not to be indexed. Given the installation's
/// brand, its index page names the installation as its operator did, and
/// shows their icon (see <see cref="PageBranding"/>).
/// </remarks>
public static class ClientApplication
{

    /// <summary>
    /// Serves the given bundle.
    /// </summary>
    public static ClientApplicationBuilder From(Bundle bundle) => new(bundle);

}

public sealed class ClientApplicationBuilder(Bundle bundle) : IHandlerBuilder<ClientApplicationBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly List<(string Name, string Value)> _headers = [];

    private IBrandingService? _branding;

    #region Functionality

    /// <summary>
    /// How the installation presents itself, which the index page follows -
    /// GenHTTP Lambda's, as it was built, without.
    /// </summary>
    public ClientApplicationBuilder Branding(IBrandingService branding)
    {
        _branding = branding;
        return this;
    }

    /// <summary>
    /// A header every page of the bundle is answered with - that it is not to
    /// be indexed, say, or which referrer it sends.
    /// </summary>
    public ClientApplicationBuilder Header(string name, string value)
    {
        _headers.Add((name, value));
        return this;
    }

    public ClientApplicationBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        IHandlerBuilder content;

        if (!bundle.Available)
        {
            content = Content.From(Resource.FromString(bundle.Index()).Type(ContentType.TextHtml));
        }
        else
        {
            // the index page made again as it is asked for only where the
            // operator may change what it says; elsewhere it is the file
            var files = _branding is { Changeable: true } ? new BrandedTree(bundle.Files(), new BrandedIndex(bundle, _branding)) : bundle.Files();

            var pages = SinglePageApplication.From(files)
                                             .ServerSideRouting()
                                             .Add(CacheControl.NoCache());

            content = bundle.HasAssets
                    ? Layout.Create().Add("assets", bundle.Assets()).Add(pages)
                    : pages;
        }

        return Concerns.Chain([.. _concerns, new HeadersConcernBuilder(_headers)], content.Build());
    }

    #endregion

}

/// <summary>
/// Adds the same headers to every answer.
/// </summary>
public sealed class HeadersConcern(IHandler content, IReadOnlyList<(string Name, string Value)> headers) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var response = await content.HandleAsync(request);

        if (response != null)
        {
            var rebuilt = response.Rebuild();

            foreach (var (name, value) in headers)
            {
                rebuilt.Header(name, value);
            }
        }

        return response;
    }

}

public sealed class HeadersConcernBuilder(IReadOnlyList<(string Name, string Value)> headers) : IConcernBuilder
{

    public IConcern Build(IHandler content) => new HeadersConcern(content, headers);

}
