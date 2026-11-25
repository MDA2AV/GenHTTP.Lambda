using GenHTTP.Lambda.Infrastructure;

namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// What a shared file is served as, by the end of its name.
/// </summary>
/// <param name="Kind">What it is for</param>
/// <param name="Type">What it is served as: <c>image/png</c>, <c>font/woff2</c>, …</param>
public sealed record SharedFileType(SharedFileKind Kind, string Type)
{

    #region Functionality

    /// <summary>
    /// Every type a shared file may be, by the end of its name.
    /// </summary>
    /// <remarks>
    /// What pages are drawn with and what agents read - and neither pages nor
    /// scripts: the files are served from the platform's own address, which
    /// nobody's page may run on. A script an app needs is the app's own.
    /// </remarks>
    public static IReadOnlyDictionary<string, SharedFileType> All { get; } = new Dictionary<string, SharedFileType>(StringComparer.OrdinalIgnoreCase)
    {
        ["png"] = new(SharedFileKind.Picture, "image/png"),
        ["jpg"] = new(SharedFileKind.Picture, "image/jpeg"),
        ["jpeg"] = new(SharedFileKind.Picture, "image/jpeg"),
        ["gif"] = new(SharedFileKind.Picture, "image/gif"),
        ["webp"] = new(SharedFileKind.Picture, "image/webp"),
        ["svg"] = new(SharedFileKind.Picture, Svg),
        ["woff2"] = new(SharedFileKind.Font, "font/woff2"),
        ["woff"] = new(SharedFileKind.Font, "font/woff"),
        ["ttf"] = new(SharedFileKind.Font, "font/ttf"),
        ["otf"] = new(SharedFileKind.Font, "font/otf"),
        ["css"] = new(SharedFileKind.Style, "text/css; charset=utf-8"),
        ["md"] = new(SharedFileKind.Text, "text/markdown; charset=utf-8"),
        ["txt"] = new(SharedFileKind.Text, "text/plain; charset=utf-8"),
        ["csv"] = new(SharedFileKind.Text, "text/csv; charset=utf-8"),
        ["json"] = new(SharedFileKind.Text, "application/json; charset=utf-8")
    };

    /// <summary>
    /// What an SVG is served as.
    /// </summary>
    public const string Svg = SvgImage.Type;

    /// <summary>
    /// Whether it is text, which an agent may be handed as it is: what is
    /// written, what is styled, and a drawing, whose markup is text as well.
    /// </summary>
    public bool Readable => Kind is SharedFileKind.Text or SharedFileKind.Style || Type == Svg;

    /// <summary>
    /// What a file of the given name is served as, or nothing for an end no
    /// shared file has.
    /// </summary>
    public static SharedFileType? Of(string name) => All.GetValueOrDefault(Path.GetExtension(name).TrimStart('.'));

    #endregion

}
