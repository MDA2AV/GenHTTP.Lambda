namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// Something an integration lets the operator offer and people choose for a
/// run, such as the model it runs on or how much it thinks.
/// </summary>
/// <remarks>
/// The platform never reads what an option means: it lets the operator say
/// which values are offered, which one a run takes where nobody chose, and
/// which need the installation's password - and it hands the integration the
/// value chosen (<see cref="AgentRun.Options"/>), records it with what the
/// run wrote and counts runs by it. So another integration brings other
/// options without the platform changing.
/// </remarks>
/// <param name="Name">What it is called on the wire and in the settings: lower case letters, digits and dashes, kept for good</param>
/// <param name="Label">What the pages call it, in English</param>
/// <param name="Values">What may be chosen, in the order the pages list them</param>
/// <param name="Default">The value a run takes where the operator decided nothing: one of <paramref name="Values"/></param>
public sealed record AgentOption(string Name, string Label, IReadOnlyList<AgentOptionValue> Values, string Default)
{

    /// <summary>
    /// The value of the given name, or nothing.
    /// </summary>
    public AgentOptionValue? Value(string name) => Values.FirstOrDefault(v => v.Name == name);

}
