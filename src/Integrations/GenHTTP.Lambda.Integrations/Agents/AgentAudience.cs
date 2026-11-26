namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// Whom a run works for, and how to speak to them.
/// </summary>
/// <remarks>
/// Everything the person reads - the agent's short lines while it works, its
/// answer, a question, the line a version is described by - is written in
/// their language and in the words of their view. The view is the one they
/// asked from: somebody on the workspace or in the simple view of the editor
/// does not write code, and is told about drafts, putting them online,
/// records and keys and passwords; somebody in the full view is told about
/// features, merging, the database and secrets.
/// </remarks>
/// <param name="Language">The language of the page they asked from, as a code such as <c>de</c> or <c>pt-pt</c> - to fall back on where the message does not tell</param>
/// <param name="View">The words they read</param>
/// <param name="Installation">Whom the installation is for</param>
/// <param name="InstallationName">What the installation is called, as its pages name it</param>
public sealed record AgentAudience(string Language, AgentView View, AgentInstallationKind Installation, string InstallationName);
