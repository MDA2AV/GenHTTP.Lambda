using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Git;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.Layouting;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Editing;

/// <summary>
/// The editor, at <c>/editor</c>: the control center of a lambda behind its
/// editor key, the page that creates one, and the lambda's git repository for
/// whoever holds the key.
/// </summary>
/// <remarks>
/// A bundle of its own (Frontend/), which talks to the platform through the
/// REST API alone. The same in every installation.
/// </remarks>
/// <example>
/// <code>
/// Layout.Create().Add("editor", Editor.Create(platform));
/// </code>
/// </example>
public static class Editor
{

    /// <summary>
    /// The editor over the given platform.
    /// </summary>
    public static EditorBuilder Create(LambdaPlatform platform) => new(platform);

}

/// <summary>
/// Configures the editor.
/// </summary>
public sealed class EditorBuilder(LambdaPlatform platform) : IHandlerBuilder<EditorBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private Web.Bundle _bundle = Web.Bundle.Beside("editor");

    #region Functionality

    /// <summary>
    /// Where the editor's bundle was built to, where it is not beside the
    /// host - a frontend being worked on, or a test's.
    /// </summary>
    public EditorBuilder Bundle(string directory)
    {
        _bundle = Web.Bundle.In(directory);
        return this;
    }

    public EditorBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var repositories = ActivatorUtilities.CreateInstance<GitRoutesBuilder>(platform.Services, GitArea.Owned);

        /*
         * The editor key is in the address of every page, so none of them
         * names it to anybody else: no referrer for whatever it links to or
         * loads - a lambda most of all, which may be answered from this very
         * host where lambdas answer by path - and nothing for a search engine.
         */
        var pages = ClientApplication.From(_bundle)
                                     .Header("X-Robots-Tag", "noindex, nofollow")
                                     .Header("Referrer-Policy", "no-referrer");

        // the repositories first: only the paths git asks for are theirs, and
        // everything else below the key is a page of the editor
        var editor = Layout.Create()
                           .Add(repositories)
                           .Add(pages);

        return Concerns.Chain(_concerns, editor.Build());
    }

    #endregion

}
