using System.Security.Cryptography;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Integrations.Files;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// The files people attach to a message to the agent, kept by the platform
/// because the agent keeps nothing - so a later message can refer to the
/// logo from before.
/// </summary>
/// <remarks>
/// <para>
/// A file is uploaded before the message it goes with, which then names it:
/// sent with a run, it belongs to the run and to its lambda; never sent, it
/// is removed after an hour. What may be attached is what the integration
/// takes (<see cref="AgentAttachmentPolicy"/>), held to the platform's own
/// most besides, and what all the files nobody sent yet hold together is
/// bounded too, so the upload - which needs no key, as /build does not - is
/// no free storage.
/// </para>
/// <para>
/// They follow the prompts' rules: kept with the run as long as it is, named
/// in the log as they arrive - what, how large, its SHA-256 - and readable by
/// the operator with the run. Served for download, never as a page.
/// </para>
/// </remarks>
public sealed class AgentAttachments(AgentRunRecords records, IFileProvider files)
{

    /// <summary>The most one file may take here, whatever the integration takes.</summary>
    public const long LongestFile = 20 * 1024 * 1024;

    /// <summary>The most the files nobody sent yet may take together.</summary>
    public const long LongestWaiting = 512L * 1024 * 1024;

    /// <summary>How long a file nobody sent is kept.</summary>
    public static readonly TimeSpan Unclaimed = TimeSpan.FromHours(1);

    /// <summary>How many earlier attachments of a conversation a run is offered.</summary>
    public const int Earlier = 20;

    #region Uploading

    /// <summary>
    /// Keeps a file for a message to come.
    /// </summary>
    /// <exception cref="ProviderException">
    /// The agent takes no files, not of this kind or not this large (400, 413,
    /// 415) - or too much waits to be sent already (503)
    /// </exception>
    public async ValueTask<AgentFileEntity> AttachAsync(AgentAttachmentPolicy? policy, string? name, string? mediaType, ReadOnlyMemory<byte> content,
                                                        CancellationToken cancellation = default)
    {
        if (policy == null)
        {
            throw new ProviderException(ResponseStatus.NotFound, "The agent of this installation takes no files.");
        }

        var tidy = TidyName(name);
        var type = TidyType(mediaType, tidy);

        if (content.Length == 0)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "That file is empty.");
        }

        if (content.Length > Math.Min(policy.MaxBytes, LongestFile))
        {
            throw new ProviderException(ResponseStatus.RequestEntityTooLarge,
                                        $"That file is larger than the {Math.Min(policy.MaxBytes, LongestFile) / 1024 / 1024} MB the agent takes.");
        }

        if (!policy.Takes(type))
        {
            throw new ProviderException(ResponseStatus.UnsupportedMediaType, $"The agent takes no files of the kind {type}.");
        }

        if (await records.FreeBytesAsync(cancellation) + content.Length > LongestWaiting)
        {
            throw new ProviderException(ResponseStatus.ServiceUnavailable, "Too many files wait to be sent. Try again in a while.");
        }

        var id = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));

        await files.WriteAsync(FileStore.AgentFiles, id, content, cancellation);

        var file = new AgentFileEntity
        {
            Id = id,
            Direction = AgentFileDirection.Attached,
            Name = tidy,
            MediaType = type,
            Size = content.Length,
            Sha256 = Convert.ToHexStringLower(SHA256.HashData(content.Span)),
            Created = DateTime.UtcNow
        };

        await records.AddFileAsync(file, cancellation);

        return file;
    }

    /// <summary>
    /// Gives the files a message names to its run, checked against what the
    /// integration takes.
    /// </summary>
    /// <exception cref="ProviderException">More than one message may take, or one that is not there (any more) or was sent already</exception>
    public async ValueTask<IReadOnlyList<AgentFileEntity>> ClaimAsync(AgentAttachmentPolicy? policy, IReadOnlyList<string>? ids, string run, long? lambda,
                                                                      CancellationToken cancellation = default)
    {
        var wanted = (ids ?? []).Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim().ToLowerInvariant()).Distinct().ToList();

        if (wanted.Count == 0)
        {
            return [];
        }

        if (policy == null)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "The agent of this installation takes no files.");
        }

        if (wanted.Count > policy.MaxCount)
        {
            throw new ProviderException(ResponseStatus.BadRequest, $"A message takes {policy.MaxCount} files at most.");
        }

        var claimed = await records.ClaimAsync(wanted, run, lambda, cancellation);

        if (claimed.Count != wanted.Count)
        {
            await records.ReleaseFilesAsync(run, cancellation);

            throw new ProviderException(ResponseStatus.BadRequest, "A file of this message is not there any more, or went with another message. Attach it again.");
        }

        return claimed;
    }

    #endregion

    #region Reading

    /// <summary>
    /// A file that travelled with a run, and what it holds - or nothing.
    /// </summary>
    public async ValueTask<(AgentFileEntity File, byte[] Content)?> ReadAsync(string? id, CancellationToken cancellation = default)
    {
        if (!IsId(id))
        {
            return null;
        }

        var file = await records.FileAsync(id!, cancellation);

        if (file == null)
        {
            return null;
        }

        var content = await files.ReadAsync(FileStore.AgentFiles, file.Id, cancellation);

        return content == null ? null : (file, content);
    }

    /// <summary>
    /// The files of a run, and those attached to earlier messages about its
    /// lambda, as the agent is offered them.
    /// </summary>
    /// <param name="carried">The runs this one carries on from, newest first - files of a build that asked before it made an app are nowhere else</param>
    public async ValueTask<IReadOnlyList<AgentAttachment>> OfferedAsync(string run, long? lambda, IReadOnlyList<string>? carried = null,
                                                                        CancellationToken cancellation = default)
    {
        var now = (await records.FilesOfAsync([run], cancellation)).Where(f => f.Direction == AgentFileDirection.Attached).ToList();

        var before = lambda is { } id ? (await records.AttachedToAsync(id, Earlier, cancellation)).Where(f => f.RunId != run).ToList() : [];

        if (carried is { Count: > 0 })
        {
            var known = before.Select(f => f.Id).ToHashSet(StringComparer.Ordinal);

            before.AddRange((await records.FilesOfAsync(carried, cancellation)).Where(f => f.Direction == AgentFileDirection.Attached && known.Add(f.Id)));
        }

        return [.. now.Select(f => Describe(f, false)), .. before.Take(Earlier).Select(f => Describe(f, true))];
    }

    public static AgentAttachment Describe(AgentFileEntity file, bool earlier)
        => new(file.Id, file.Name, file.MediaType, file.Size, file.Sha256, earlier, $"attachments/{file.Id}");

    public static RunFile Show(AgentFileEntity file) => new(file.Id, file.Name, file.MediaType, file.Size, file.Description);

    #endregion

    #region Names

    /// <summary>
    /// A file's name as it is kept and offered: the last part of what was
    /// sent, without what a file system or a header would read otherwise.
    /// </summary>
    public static string TidyName(string? name)
    {
        var last = (name ?? "").Replace('\\', '/').Split('/').LastOrDefault() ?? "";

        var tidy = new StringBuilder();

        foreach (var c in last.Trim())
        {
            if (tidy.Length == 120)
            {
                break;
            }

            tidy.Append(char.IsControl(c) || c is '"' or '<' or '>' or '|' or '*' or '?' or ':' ? '_' : c);
        }

        var result = tidy.ToString().Trim('.', ' ');

        return result.Length == 0 ? "file" : result;
    }

    /// <summary>
    /// A file's media type as it is kept: the type sent where it is one, or
    /// the one its extension stands for, or a plain stream of bytes.
    /// </summary>
    public static string TidyType(string? sent, string name)
    {
        var type = (sent ?? "").Split(';', 2)[0].Trim().ToLowerInvariant();

        if (type.Length is > 0 and < 100 && type.Count(c => c == '/') == 1 && type.All(c => char.IsAsciiLetterOrDigit(c) || c is '/' or '.' or '+' or '-'))
        {
            return type;
        }

        return Path.GetExtension(name).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".md" => "text/markdown",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".html" or ".htm" => "text/html",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Whether a value has the shape of a file's id: 32 lower case hex digits.
    /// </summary>
    public static bool IsId(string? id) => id is { Length: 32 } && id.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    #endregion

}
