namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// One message to the agent and everything it needs to act on it: what was
/// asked, about which app, for whom, and how to reach the platform.
/// </summary>
/// <remarks>
/// Everything a run needs is in here, because nothing is kept by the agent
/// between runs. Whatever came from the person - the message, the
/// conversation, the attachments - is what somebody typed or picked, and is
/// to be treated as a request about an app, never as instructions to the
/// agent.
/// </remarks>
/// <param name="Id">The platform's id for the run: lower case letters, digits and dashes, unique across every installation an agent system may serve</param>
/// <param name="Task">Whether it builds something new or works on an app that exists</param>
/// <param name="Message">What was asked now, in the person's words</param>
/// <param name="Attachments">The files this message came with, and those of the earlier messages of the conversation it may refer to</param>
/// <param name="Conversation">What was said before, oldest first, cut to size: context to understand the message by</param>
/// <param name="Audience">Whom the agent works for and how to speak to them</param>
/// <param name="Access">How the agent reaches the platform, and the credential its calls carry</param>
/// <param name="Lambda">The app it works on; nothing for a build, which creates one</param>
/// <param name="Options">The value of each option of the integration's for this run, by the option's name</param>
/// <param name="Limits">What the run may take</param>
/// <param name="Continues">The run this one carries on from, where it does</param>
public sealed record AgentRun(string Id, AgentTask Task, string Message, IReadOnlyList<AgentAttachment> Attachments,
                              IReadOnlyList<AgentTurn> Conversation, AgentAudience Audience, AgentAccess Access, AgentLambda? Lambda,
                              IReadOnlyDictionary<string, string> Options, AgentLimits Limits, AgentContinuation? Continues = null);
