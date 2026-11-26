namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// What an agent can do, and what the operator may choose for it - so the
/// pages offer what it does and leave out what it does not, and the operator
/// knows what it is.
/// </summary>
/// <remarks>
/// An integration says what it cannot do rather than the platform assuming
/// what every agent can: one that takes no attachments has no button for
/// them, one that cannot stop a run has no button for that, one without the
/// web promises nobody the web.
/// </remarks>
/// <param name="Disclosure">What it is, who runs it and where it sends what - for the operator's privacy statement</param>
/// <param name="Options">What the operator offers and people choose for a run, such as the model it runs on; none where there is nothing to choose</param>
/// <param name="Attachments">The files a message may come with, or nothing where it takes none</param>
/// <param name="ReturnsFiles">Whether it may hand files back to the person (<see cref="AgentReturned"/>)</param>
/// <param name="Web">What of the web it reads</param>
/// <param name="Cancels">Whether a run can be stopped (<see cref="IAgentProvider.CancelAsync"/>)</param>
/// <param name="Asks">Whether a run may end with a question for the person (<see cref="AgentEnding.Asked"/>), which their next message answers</param>
/// <param name="Limits">Which limits it holds a run to</param>
public sealed record AgentCapabilities(AgentDisclosure Disclosure, IReadOnlyList<AgentOption> Options, AgentAttachmentPolicy? Attachments,
                                       bool ReturnsFiles, AgentWebAccess Web, bool Cancels, bool Asks, AgentLimitSupport Limits)
{

    /// <summary>
    /// The option of the given name, or nothing.
    /// </summary>
    public AgentOption? Option(string name) => Options.FirstOrDefault(o => o.Name == name);

}
