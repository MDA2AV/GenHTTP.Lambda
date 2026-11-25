using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Branding;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The name, the logo, the colours and the fonts the operator gives the
/// installation, at <c>/admin/branding</c> behind the admin token.
/// </summary>
/// <remarks>
/// Only there where the host lets its operator brand the installation (see
/// <c>LambdaOptions.Branding</c>) and an admin token is set: everywhere else
/// the routes are not found and the API description leaves them out. The
/// panel's Branding section calls these, and so does an agent the operator
/// handed the token - the same thing through the same door.
/// </remarks>
[Tag(ApiDocumentation.Branding)]
[Summary("The name and the logo the installation's pages show in place of GenHTTP Lambda's, and the colours and fonts they are drawn with - for whoever runs it.")]
[Remarks("""
         Where the host of the installation lets its operator brand it; elsewhere these routes are not there. The name and the
         picture beside it show in the bar of every page, in the titles of the pages and their previews, and as the icon of the
         browser tab - the logo, or the first letter of the name on a plate where there is no logo. The colours and the fonts are
         those of every page - the site, the editor, the panel, the published sources - and of nothing a lambda serves. A change
         shows on the next page anybody opens; `GET /system` says what the pages show (`brand`).
         """)]
public sealed class BrandingResource(IBrandingService branding, ILogger<BrandingResource> logger)
{

    [ResourceMethod]
    [Summary("Reads the branding")]
    [return: Summary("The name, the logo, the colours and the fonts, and what they may be")]
    public BrandingResponse Get() => BrandingResponse.Of(branding.Current);

    [ResourceMethod(Method.Put)]
    [Summary("Names the installation")]
    [Remarks("""
             The name its pages show, in place of `GenHTTP Lambda`: one line of at most 40 characters, trimmed. An empty or absent
             name gives it back the platform's name. The logo, the colours and the fonts are left as they are.
             """)]
    [Response(ResponseStatus.BadRequest, "The name is longer than 40 characters, or holds line breaks or other control characters. Nothing was changed.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The branding, as it is now")]
    public async ValueTask<BrandingResponse> Rename(BrandingRequest body)
    {
        var (brand, previous) = await branding.RenameAsync(body.Name);

        logger.BrandingRenamed(previous, brand);

        return BrandingResponse.Of(brand);
    }

    [ResourceMethod(Method.Delete)]
    [Summary("Gives back the platform's name, mark, colours and fonts")]
    [Remarks("Takes the name, the logo, the colours and the fonts away: the pages show GenHTTP Lambda again, with its own mark and as it is drawn. The shared files the fonts were made of stay. Nothing happens where they do already.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    public async ValueTask Reset()
    {
        var had = branding.Current.Own || branding.Current.Look.Own;

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
    [return: Summary("The branding, as it is now")]
    public async ValueTask<BrandingResponse> ChangeLogo(Stream body)
    {
        var content = await Bodies.ReadAsync(body, BrandChecks.MaxLogoBytes, $"The logo must not be larger than {BrandChecks.MaxLogoBytes / 1024} KB.");

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

    [ResourceMethod("preview")]
    [Summary("Works out a look")]
    [Remarks("""
             What the pages would be drawn with in the colours and fonts given, changing nothing. An accent is drawn as it is where it
             is a surface - a button, the plate of the installation's letter, the moving background - and where it is text, as much
             lighter or darker as reading it takes (4.5 to 1 and more). What is written on a button of it is white or dark,
             whichever can be read. A font is told by what its files draw, and which languages of the pages they lack letters for.
             """)]
    [Response(ResponseStatus.BadRequest, "One of the colours is none as `#rrggbb`, or a font cannot be made of the files.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("What it would come to")]
    public async ValueTask<BrandingPreviewResponse> Preview([Summary("The accent as `#rrggbb`, its `#` encoded or left out; nothing for the platform's blue")] string? accent,
                                                            [Summary("The second accent; nothing for the platform's purple")] string? second,
                                                            [Summary("The shared files the font would be made of, with commas between; nothing for the system's")] string? text,
                                                            [Summary("Those of the font of the headings; nothing for the same as the text")] string? headings)
        => BrandingPreviewResponse.Of(await branding.ComposeAsync(BrandChecks.Color(accent, "accent"), BrandChecks.Color(second, "second accent"), Names(text), Names(headings)));

    private static string[] Names(string? files) => files?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

    [ResourceMethod(Method.Put, "colors")]
    [Summary("Chooses the colours")]
    [Remarks("""
             `accent` is what links, buttons and marks are drawn in, and the first light of the moving background behind the front
             page. `second` is what is drawn in the platform's purple otherwise, and the second light - without one, that is the
             colour beside the accent, and the purple stays. Any colour may be one: see `GET /admin/branding/preview` for what becomes of it. The letter that
             stands in for a logo is on a plate of the accent.
             """)]
    [Response(ResponseStatus.BadRequest, "One of them is no colour as `#rrggbb`. Nothing was changed.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The branding, as it is now")]
    public async ValueTask<BrandingResponse> ChangeColors(BrandingColorsRequest body)
    {
        var (brand, previous) = await branding.ColorAsync(BrandChecks.Color(body.Accent, "accent"), BrandChecks.Color(body.Second, "second accent"));

        logger.BrandingColored(previous, brand);

        return BrandingResponse.Of(brand);
    }

    [ResourceMethod(Method.Put, "fonts")]
    [Summary("Chooses the fonts")]
    [Remarks("""
             A font is the shared files it is made of (`PUT /admin/shared/{name}`): one variable font, which draws every weight, or a
             file for each - which weights and whether they are slanted is read from the files. `text` is what the pages are set in
             and `headings` their headings and the installation's name in the bar; code stays in the face it is in. The pages come
             with the font from this installation, so no visitor's browser asks anybody else for it. Letters a font does not have
             are drawn in the system's.
             """)]
    [Response(ResponseStatus.BadRequest, "There is no shared file of one of the names, it is no font, or a font is made of more than 12 files. Nothing was changed.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The branding, as it is now")]
    public async ValueTask<BrandingResponse> ChangeFonts(BrandingFontsRequest body)
    {
        var (brand, previous) = await branding.FontAsync(body.Text ?? [], body.Headings ?? []);

        logger.BrandingFonts(previous, brand);

        return BrandingResponse.Of(brand);
    }

}
