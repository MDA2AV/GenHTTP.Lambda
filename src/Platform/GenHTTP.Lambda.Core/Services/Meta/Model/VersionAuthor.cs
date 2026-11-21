using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Building;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// Who wrote a version or what a draft holds, and on which model and effort.
/// </summary>
/// <remarks>
/// Known for the build agent only, from the job its server issued a ticket
/// for - never from what the runner or the agent says about itself. The model
/// is the name the operator offered it by (<c>opus</c>), not the model of
/// Anthropic's the runner ran it as, which may change with the runner.
/// </remarks>
/// <param name="Writer">Who wrote it</param>
/// <param name="Model">The model it ran on</param>
/// <param name="Effort">The effort it ran at</param>
public sealed record VersionAuthor(VersionWriter Writer, AgentModel Model, AgentEffort Effort);
