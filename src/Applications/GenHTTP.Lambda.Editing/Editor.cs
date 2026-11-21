using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Branding;
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
