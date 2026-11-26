namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// A file the person sent with a message, kept by the platform.
/// </summary>
/// <remarks>
/// The agent reads it from the platform like everything else - through MCP or
/// the REST API, by its id - and can save it into the app by its id rather
/// than carrying its bytes, so a picture does not have to pass through the
/// model to become the app's logo. An earlier one is offered so that "the
/// logo from before" can be found.
/// </remarks>
/// <param name="Id">What the platform knows it by</param>
/// <param name="Name">What it was called on the person's device</param>
/// <param name="MediaType">What kind of file it is</param>
/// <param name="Size">How many bytes it holds</param>
/// <param name="Sha256">The SHA-256 of what it holds, as lower case hex</param>
/// <param name="Earlier">Whether it came with an earlier message of the conversation rather than this one</param>
/// <param name="Path">Where the REST API answers with it, below the platform's address (<see cref="AgentAccess.Api"/>)</param>
public sealed record AgentAttachment(string Id, string Name, string MediaType, long Size, string Sha256, bool Earlier, string Path);
