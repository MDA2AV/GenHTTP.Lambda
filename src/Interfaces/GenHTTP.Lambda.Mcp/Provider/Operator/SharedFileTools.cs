using System.ComponentModel.DataAnnotations;
using System.Text;

using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.SharedFiles;

using GenHTTP.Modules.Mcp;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// The files the operator shares across the installation, as their agent
/// manages them: what the panel's Shared files section does, through the same
/// service and the same refusals.
/// </summary>
public sealed class SharedFileTools(ISharedFileService files, ISharedFiles library, LambdaOptions options, ILogger<SharedFileTools> logger)
{

    [Tool("list_shared_files", Title = "List the shared files", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("The files shared across the installation - fonts and a logo for its own pages, a style sheet, a house style for the agents that build on it - each with its address, what a font draws, and what uses it: the branding's fonts, the guide's operator section. With what a file may be.")]
    public ValueTask<object> ListSharedFiles(IRequest request)
    {
        var home = Home(request);

        return Refusing.Async(async () =>
        {
            var listed = await files.ListAsync();

            logger.LogInformation("Listed shared files by operator");

            return new
            {
                files = listed.Select(f => Describe(f, home)),
                bytes = listed.Sum(f => (long)f.File.Bytes),
                limits = new
                {
                    files = SharedFileChecks.MaxFiles,
                    bytes = SharedFileChecks.MaxBytes,
                    totalBytes = SharedFileChecks.MaxTotalBytes,
                    ends = SharedFileType.All.Keys
                }
            };
        });
    }

    [Tool("read_shared_file", Title = "Read a shared file", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("What a shared text, style sheet or SVG says, up to 32 KB. Anything else is fetched by its address.")]
    public ValueTask<object> ReadSharedFile([Summary("What the file is called, from list_shared_files."), MaxLength(SharedFileChecks.MaxName)] string name)
        => Refusing.Async(async () =>
        {
            var text = await library.ReadTextAsync(name);

            logger.LogInformation("Read shared file {Name} by operator", name);

            return ToolResult.Text(text);
        });

    [Tool("write_shared_file", Title = "Share a file", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("Shares a file, or replaces the one of that name: whatever uses it has the new one from then on. A picture, a font, a style sheet or a text - the end of its name says which, and its content has to be that; no pages, no scripts. Public: anybody with its address reads it. A font or a picture is too large to write out here: whoever can make requests sends it to PUT /api/v1/admin/shared/{name} as the body, with the operator's token; the operator uploads it in the panel otherwise. A font's license has to allow it on the web. The agents that build here are told of it in platform_guide, sharedFiles.")]
    public ValueTask<object> WriteSharedFile(IRequest request,
                                             [Summary("What it is called: letters and digits without accents, dashes, underscores and dots, such as house-style.md."), MaxLength(SharedFileChecks.MaxName)] string name,
                                             [Summary("What a text, a style sheet or an SVG says.")] string? text = null,
                                             [Summary("The content base64 encoded, instead of text.")] string? base64 = null)
    {
        var home = Home(request);

        return Refusing.Async(async () =>
        {
            var (file, replaced) = await files.WriteAsync(name, Content(text, base64));

            logger.SharedFileWritten(file.File, replaced);

            return new { ok = true, replaced, file = Describe(file, home) };
        });
    }

    [Tool("delete_shared_file", Title = "Delete a shared file", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Deletes a shared file for good: an app that links to it goes without. Refused while the branding or the guide uses it. Ask the operator first.")]
    public ValueTask<object> DeleteSharedFile([Summary("What the file is called, from list_shared_files."), MaxLength(SharedFileChecks.MaxName)] string name)
        => Refusing.Async(async () =>
        {
            var deleted = await files.DeleteAsync(name);

            logger.SharedFileDeleted(deleted);

            return new { ok = true, deleted = deleted.Name };
        });

    #region Implementation

    private string Home(IRequest request) => options.PublicUrl ?? RequestOrigin.Of(request);

    private static byte[] Content(string? text, string? base64)
    {
        if ((text == null) == (base64 == null))
        {
            throw LambdaException.Invalid("Send either text or base64.");
        }

        if (text != null)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        try
        {
            return Convert.FromBase64String(base64!);
        }
        catch (FormatException)
        {
            throw LambdaException.Invalid("That is not base64.");
        }
    }

    private static object Describe(SharedFileUsage usage, string home) => new
    {
        name = usage.File.Name,
        kind = SharedFileKindNames.Name(usage.File.Type.Kind),
        type = usage.File.Type.Type,
        bytes = usage.File.Bytes,
        updated = usage.File.Updated,
        address = home + usage.File.Path,
        font = usage.File.Font is { } font ? new { family = font.Family, from = font.From, to = font.To, italic = font.Italic } : null,
        uses = usage.Uses.Select(SharedFileUseNames.Name)
    };

    #endregion

}
