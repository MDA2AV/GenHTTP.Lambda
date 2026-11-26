namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// What the operator set a kind of run's limits to.
/// </summary>
/// <param name="Seconds">How long it may work: nothing for what the agent holds it to by default, zero for no limit</param>
/// <param name="Steps">How many steps it may take: nothing for what the agent holds it to by default, zero for no limit</param>
public sealed record LimitSetting(int? Seconds, int? Steps)
{

    /// <summary>Nothing decided: what the agent holds a run to by default.</summary>
    public static readonly LimitSetting Agents = new(null, null);

}
