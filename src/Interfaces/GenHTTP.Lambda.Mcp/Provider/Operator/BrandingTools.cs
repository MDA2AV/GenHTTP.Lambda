using System.ComponentModel.DataAnnotations;

using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Mcp;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// What the operator's agent may change about the installation itself rather
/// than about a lambda on it: its name and its logo.
/// </summary>
/// <remarks>
/// On the operator's endpoint, and there only where the host lets the
/// operator brand the installation (see <see cref="OperatorMcpBuilder"/>).
/// The token is checked in front of it, as for every tool there; it used to
/// be an argument of this tool on the public endpoint, where every agent was
/// shown it and the token landed in the transcript.
///
/// The same changes the panel's Branding section makes and the API's
/// <c>/admin/branding</c>, through the same service, logged in the same words.
/// </remarks>
public sealed class BrandingTools(IBrandingService branding, ILogger<BrandingTools> logger)
{

    [Tool("customize_installation", Title = "Name this installation and give it a logo", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("The name and the logo the installation's pages show in place of GenHTTP Lambda's - in the bar of every page, the titles of the pages and their link previews, and the icon of the browser tab. Nothing about a lambda changes. With neither, it returns how the installation presents itself. An empty logo takes the logo away, which cannot be undone but by sending it again.")]
    public ValueTask<object> CustomizeInstallation([Summary("What the installation is called: one line of at most 40 characters. An empty string gives it back the name GenHTTP Lambda; left out, the name stays as it is."), MaxLength(BrandChecks.MaxName)] string? name = null,
                                                   [Summary("The logo: an SVG's markup as it is, or a PNG, JPEG, GIF, WebP or SVG image base64 encoded - at most 512 KB. A square mark reads best: it stands beside the name in the bar and is the icon of the browser tab, on a light and a dark background. An SVG needs xmlns=\"http://www.w3.org/2000/svg\" on its svg element. An empty string removes the logo; left out, it stays as it is.")] string? logo = null)
        => Refusing.Async(() => ChangeAsync(name, logo));

    private async ValueTask<object> ChangeAsync(string? name, string? logo)
    {
        // both are checked before either is changed, so a refusal changes nothing
        BrandChecks.Name(name);

        var picture = logo != null && logo.Trim().Length > 0 ? Decode(logo) : null;

        if (picture != null)
        {
            BrandChecks.Logo(picture);
        }

        var brand = branding.Current;

        if (name != null)
        {
            (brand, var previous) = await branding.RenameAsync(name);

            logger.BrandingRenamed(previous, brand);
        }

        if (logo != null)
        {
            if (picture == null)
            {
                var had = brand.Logo != null;

                brand = await branding.RemoveLogoAsync();

                if (had)
                {
                    logger.LogoRemoved();
                }
            }
            else
            {
                brand = await branding.ChangeLogoAsync(picture);

                logger.LogoUploaded(brand);
            }
        }

        if (name == null && logo == null)
        {
            logger.LogInformation("Read branding by operator");
        }

        return Describe(brand);
    }

    /// <summary>
    /// The picture from what the agent sent: the markup of an SVG as it is,
    /// anything else base64.
    /// </summary>
    private static byte[] Decode(string logo)
    {
        var text = logo.Trim();

        if (text.StartsWith('<'))
        {
            return System.Text.Encoding.UTF8.GetBytes(text);
        }

        try
        {
            return Convert.FromBase64String(text);
        }
        catch (FormatException)
        {
            throw LambdaException.Invalid("The logo has to be an SVG's markup, or a picture base64 encoded.");
        }
    }

    private static object Describe(Brand brand) => new
    {
        name = brand.Name,
        named = brand.Given != null,
        logo = brand.Logo is { } logo ? new { type = logo.Type, bytes = logo.Content.Length } : null,
        picture = brand.PicturePath,
        shown = brand.Own
            ? "The pages show this name, with the logo beside it - or the first letter of the name on a plate where there is no logo - from the next page anybody opens. Ask the operator to look at the front page in a light and a dark theme."
            : "The pages show GenHTTP Lambda, with its own mark."
    };

}
