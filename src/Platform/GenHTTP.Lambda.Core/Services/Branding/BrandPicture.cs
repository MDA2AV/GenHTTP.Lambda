using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Services.Hosting;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// A picture the installation presents itself with, as it is served.
/// </summary>
/// <param name="Type">What it is, as read from its content: <c>image/png</c>, <c>image/svg+xml</c>, …</param>
/// <param name="Updated">When the operator uploaded it, or when it was drawn</param>
public sealed record BrandPicture(byte[] Content, string Type, DateTime Updated)
{

    /// <summary>
    /// Whether every page a link is previewed with can show it - a social
    /// network draws no SVG.
    /// </summary>
    public bool Raster => Type != BrandChecks.Svg;

}
