using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.SharedFiles;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A file shared across the installation.")]
public sealed record SharedFileResponse(
    [Summary("What it is called")]
    string Name,
    [Summary("What it is for: `picture`, `font`, `style` or `text`")]
    string Kind,
    [Summary("What it is served as: `image/svg+xml`, `font/woff2`, …")]
    string Type,
    [Summary("How large it is, in bytes")]
    int Bytes,
    [Summary("When it was last uploaded")]
    DateTime Updated,
    [Summary("Where it is found, relative to the installation, whatever it holds by then - what a page links to")]
    string Path,
    [Summary("Where it is found as it is now, which may be cached for good")]
    string VersionedPath,
    [Summary("What it draws; absent where it is no font")]
    FontFaceResponse? Font,
    [Summary("What of the installation uses it, and keeps it from being deleted: `textFont`, `headingFont`, `guide`. Absent where that was not asked.")]
    IReadOnlyList<string>? Uses = null
)
{

    internal static SharedFileResponse Of(SharedFile file)
        => new(file.Name, SharedFileKindNames.Name(file.Type.Kind), file.Type.Type, file.Bytes, file.Updated, file.Path, file.VersionedPath, FontFaceResponse.Of(file.Font));

    internal static SharedFileResponse Of(SharedFileUsage usage)
        => Of(usage.File) with { Uses = [.. usage.Uses.Select(SharedFileUseNames.Name)] };

}
