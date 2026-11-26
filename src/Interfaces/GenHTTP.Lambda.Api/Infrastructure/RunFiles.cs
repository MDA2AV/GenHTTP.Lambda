using System.Text;

using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Agents;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// How a file that travelled with a run is answered: shown where it is a
/// picture, saved otherwise - and never run on the platform's own address.
/// </summary>
/// <remarks>
/// What a person attached and what the agent handed back are somebody's
/// bytes, served from the platform's origin, where the editor and the panel
/// are. So nothing is answered as what a browser would run: a picture of a
/// kind that holds no script is shown, every other file - an SVG, a page, a
/// text - is a stream of bytes to be saved, and all of it under a policy that
/// runs nothing and loads nothing, with the browser told not to guess.
/// </remarks>
internal static class RunFiles
{

    private static readonly HashSet<string> Shown = new(StringComparer.Ordinal) { "image/png", "image/jpeg", "image/gif", "image/webp" };

    public static IResponse Serve(IRequest request, (RunFile File, byte[] Content) found)
    {
        var (file, content) = found;

        var shown = Shown.Contains(file.Type);

        var response = request.Respond()
                              .Content(content, new ContentType(shown ? file.Type : "application/octet-stream"))
                              .Header("X-Content-Type-Options", "nosniff")
                              .Header("Content-Security-Policy", "default-src 'none'; sandbox")
                              .Header("Cache-Control", "private, max-age=3600")
                              .Header("Referrer-Policy", "no-referrer");

        if (!shown)
        {
            response.Header("Content-Disposition", $"attachment; filename=\"{Ascii(file.Name)}\"; filename*=UTF-8''{Uri.EscapeDataString(file.Name)}");
        }

        return response.Build();
    }

    private static string Ascii(string name)
    {
        var plain = new StringBuilder();

        foreach (var c in name)
        {
            plain.Append(c is >= ' ' and <= '~' and not '"' and not '\\' ? c : '_');
        }

        return plain.ToString();
    }

}
