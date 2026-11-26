namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// A file that travelled with a run: one the person attached to their
/// message, or one the agent handed back.
/// </summary>
/// <remarks>
/// What it holds is a file of the files integration's, named by its id;
/// this is what it is. An attachment is uploaded before the message it goes
/// with and belongs to nothing until that message is sent - one that never
/// is goes after an hour. Afterwards it belongs to the run and to its
/// lambda, so a later message about the same app can refer to it.
/// </remarks>
public sealed class AgentFileEntity
{

    /// <summary>32 lower case hex digits, random: knowing it is what reading it takes.</summary>
    public required string Id { get; set; }

    public AgentFileDirection Direction { get; set; }

    /// <summary>The run it travelled with, once it did.</summary>
    public string? RunId { get; set; }

    /// <summary>The lambda of that run, once it has one.</summary>
    public long? LambdaId { get; set; }

    public required string Name { get; set; }

    public required string MediaType { get; set; }

    public long Size { get; set; }

    /// <summary>The SHA-256 of what it holds, as lower case hex.</summary>
    public required string Sha256 { get; set; }

    /// <summary>A line about it from the agent, for one it handed back.</summary>
    public string? Description { get; set; }

    public DateTime Created { get; set; }

}
