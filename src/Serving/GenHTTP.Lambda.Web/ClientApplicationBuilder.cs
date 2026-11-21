using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Branding;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.SinglePageApplications;

namespace GenHTTP.Lambda.Web;

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
