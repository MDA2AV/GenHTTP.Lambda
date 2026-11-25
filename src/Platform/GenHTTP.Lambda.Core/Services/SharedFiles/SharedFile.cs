using GenHTTP.Lambda.Services.Hosting;

namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// A file whoever runs the installation shares across it: for its own pages,
/// for the guide, and for the apps built on it.
/// </summary>
/// <param name="Name">What it is called, which is the end of its address</param>
/// <param name="Type">What it is for, and what it is served as</param>
/// <param name="Bytes">How large it is</param>
/// <param name="Hash">The SHA-256 of its content, as a hex string - which is what its content is kept under</param>
/// <param name="Updated">When it was last uploaded</param>
/// <param name="Font">What it draws, where it is a font</param>
public sealed record SharedFile(string Name, SharedFileType Type, int Bytes, string Hash, DateTime Updated, FontFace? Font)
{

    /// <summary>
    /// What the files are found below, relative to the platform.
    /// </summary>
    public const string BasePath = $"{PlatformRoutes.ApiBase}/shared/";

    /// <summary>
    /// A short name for its content, which changes whenever that does.
    /// </summary>
    public string Version => Hash[..12];

    /// <summary>
    /// Where it is found by its name, whatever it holds by then.
    /// </summary>
    public string Path => BasePath + Name;

    /// <summary>
    /// Where it is found as it is now, which may be cached for good.
    /// </summary>
    public string VersionedPath => $"{Path}?v={Version}";

}
