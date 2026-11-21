using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The name and the logo the operator gives the installation, at
/// <c>/admin/branding</c> behind the admin token.
/// </summary>
/// <remarks>
/// Only there where the host lets its operator brand the installation (see
/// <c>LambdaOptions.Branding</c>) and an admin token is set: everywhere else
/// the routes are not found and the API description leaves them out. The
/// panel's Branding section calls these, and so does an agent the operator
/// handed the token - the same thing through the same door.
/// </remarks>
[Tag(ApiDocumentation.Branding)]
[Summary("The name and the logo the installation's pages show in place of GenHTTP Lambda's - for whoever runs it.")]
[Remarks("""
         Where the host of the installation lets its operator brand it; elsewhere these routes are not there. The name and the
         picture beside it show in the bar of every page, in the titles of the pages and their previews, and as the icon of the
         browser tab - the logo, or the first letter of the name on a plate where there is no logo. A change shows on the next
         page anybody opens; `GET /system` says what the pages show (`brand`).
         """)]
public sealed class BrandingResource(IBrandingService branding, ILogger<BrandingResource> logger)
{

    [ResourceMethod]
    [Summary("Reads the name and the logo")]
    [return: Summary("The name and the logo, and what they may be")]
    public BrandingResponse Get() => BrandingResponse.Of(branding.Current);

    [ResourceMethod(Method.Put)]
    [Summary("Names the installation")]
    [Remarks("""
             The name its pages show, in place of `GenHTTP Lambda`: one line of at most 40 characters, trimmed. An empty or absent
             name gives it back the platform's name. The logo is left as it is.
             """)]
    [Response(ResponseStatus.BadRequest, "The name is longer than 40 characters, or holds line breaks or other control characters. Nothing was changed.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The name and the logo, as they are now")]
    public async ValueTask<BrandingResponse> Rename(BrandingRequest body)
    {
        var (brand, previous) = await branding.RenameAsync(body.Name);

        logger.BrandingRenamed(previous, brand);

        return BrandingResponse.Of(brand);
    }

    [ResourceMethod(Method.Delete)]
    [Summary("Gives back the platform's name and mark")]
    [Remarks("Takes the name and the logo away: the pages show GenHTTP Lambda again, with its own mark. Nothing happens where they do already.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    public async ValueTask Reset()
    {
        var had = branding.Current.Own;

        await branding.ResetAsync();

        if (had)
        {
            logger.BrandingReset();
        }
    }

    [ResourceMethod(Method.Put, "logo")]
    [Summary("Uploads the logo")]
    [Remarks("""
             The body is the picture as it is: a PNG, JPEG, GIF, WebP or SVG image, at most 512 KB, told apart by its content
             rather than by the `Content-Type` it is sent with. It replaces the logo there was. A square mark reads best: it stands
             beside the name in the bar, at the height of a line, and is the icon of the browser tab. An SVG has to name the SVG
             namespace on its `svg` element, or a browser draws nothing; it is shown as a picture, so nothing in it runs.
             """)]
    [RequestBody("The picture", ContentType = "image/*")]
    [Response(ResponseStatus.BadRequest, "The body is empty, larger than 512 KB, or no PNG, JPEG, GIF, WebP or SVG image. Nothing was changed.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The name and the logo, as they are now")]
    public async ValueTask<BrandingResponse> ChangeLogo(Stream body)
    {
        var content = await ReadAsync(body);

        var brand = await branding.ChangeLogoAsync(content);

        logger.LogoUploaded(brand);

        return BrandingResponse.Of(brand);
    }

    [ResourceMethod(Method.Delete, "logo")]
    [Summary("Takes the logo away")]
    [Remarks("The name stays. Where the installation is named, the pages draw its first letter on a plate beside it; where it is not, the platform's mark. Nothing happens where there is no logo.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    public async ValueTask RemoveLogo()
    {
        var had = branding.Current.Logo != null;

        await branding.RemoveLogoAsync();

        if (had)
        {
            logger.LogoRemoved();
        }
    }

    /// <summary>
    /// The body, read no further than a logo may go - so a larger one is
    /// refused rather than held.
    /// </summary>
    private static async ValueTask<byte[]> ReadAsync(Stream body)
    {
        using var buffer = new MemoryStream();

        var chunk = new byte[16 * 1024];

        int read;

        while ((read = await body.ReadAsync(chunk)) > 0)
        {
            if (buffer.Length + read > BrandChecks.MaxLogoBytes)
            {
                throw LambdaException.Invalid($"The logo must not be larger than {BrandChecks.MaxLogoBytes / 1024} KB.");
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

}
