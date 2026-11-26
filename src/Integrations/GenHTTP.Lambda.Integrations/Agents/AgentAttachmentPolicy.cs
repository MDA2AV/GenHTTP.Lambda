namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// The files a message to the agent may come with, which the pages say before
/// anything is sent.
/// </summary>
/// <remarks>
/// The kinds the agent can make sense of - pictures it can look at, texts it
/// can read. The platform keeps the files (<see cref="AgentAttachment"/>) and
/// may hold them to less room of its own.
/// </remarks>
/// <param name="MediaTypes">The kinds of file, as media types or ranges of them: <c>image/png</c>, <c>image/*</c>, <c>text/*</c></param>
/// <param name="MaxBytes">The most one file may take</param>
/// <param name="MaxCount">The most files one message may come with</param>
public sealed record AgentAttachmentPolicy(IReadOnlyList<string> MediaTypes, long MaxBytes, int MaxCount)
{

    /// <summary>
    /// Whether a file of the given media type may be attached.
    /// </summary>
    public bool Takes(string mediaType)
    {
        var type = mediaType.Split(';', 2)[0].Trim().ToLowerInvariant();

        foreach (var taken in MediaTypes)
        {
            var pattern = taken.Trim().ToLowerInvariant();

            if (pattern == type || (pattern.EndsWith("/*", StringComparison.Ordinal) && type.StartsWith(pattern[..^1], StringComparison.Ordinal)))
            {
                return true;
            }
        }

        return false;
    }

}
